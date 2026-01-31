using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FixRushGame
{
    public enum InteractionType
    {
        Simple,
        Hold,
        Still
    }

    [RequireComponent(typeof(SphereCollider))]
    public class Interactable : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [Tooltip("Simple: Instant interaction.\n Hold: Requires holdtime to be completed.\n Still: Will be performed only if the player is in range and canceled if not.")]
        public InteractionType InteractionType;
        [Tooltip("Waiting time to complete a hold interaction. Ignored if the action is set to simple type.")]
        [Range(0, 20)] public float HoldTime = 0;
        [Header("Trigger Settings")]
        [SerializeField] float _triggerRadius = 0.5f;

        SphereCollider _c;
        Player _entity;
        Coroutine _holdCoroutine;
        Coroutine _stillCoroutine;
        InWorldTooltip tooltip;
        bool _isHolding = false;
        float _remainingTime = 0;

        public delegate void InteractableDelegate(Interactable obj, Player entity);
        public delegate void CancelInteractionDelegate(Interactable obj, Player entity);
        public static event InteractableDelegate OnInteract;
        public static event CancelInteractionDelegate OnCancelInteraction;

        public float TriggerRadius { get { return _triggerRadius; } set { _triggerRadius = value; } }

        private void Awake()
        {
            if(_c == null) { _c = GetComponent<SphereCollider>(); }
        }

        private void Start()
        {
            SetUpTrigger();
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        private void OnTriggerEnter(Collider other)
        {
            Player i = other.GetComponentInParent<Player>();
            if (i != null)
            {
                _entity = i;
                i.FocusCandidates.Add(this);

                ShowTooltip();
            }
            
        }
        private void OnTriggerStay(Collider other)
        {
        }

        private void OnTriggerExit(Collider other)
        {
            Player i = other.GetComponentInParent<Player>();
            if (i != null)
            {
                i.FocusCandidates.Remove(this);
                _entity = null;

                // Still Interaction checker
                if (_stillCoroutine != null)
                {
                    StopCoroutine(_stillCoroutine);
                    _stillCoroutine = null;
                    OnCancelInteraction?.Invoke(this, i);
                    Debug.Log("Still canceled");
                }

                HideTooltip();
            }
        }

        void SetUpTrigger()
        {
            _c.radius = _triggerRadius;
            _c.isTrigger = true;
        }

        public void SetInteractable(InteractionType interactionType, float holdTime, float triggerRadius)
        {
            InteractionType = interactionType;
            TriggerRadius = triggerRadius;
            HoldTime = holdTime;
        }

        public void Interact(InputAction.CallbackContext ctx)
        {
            switch (InteractionType){

                case InteractionType.Simple:
                    HandleSimple(ctx);
                    break;

                case InteractionType.Hold:
                    HandleHold(ctx);
                    break;

                case InteractionType.Still:
                    HandleStill(ctx);
                    break;
            }
        }

        public void ShowTooltip()
        {
            if (tooltip != null) return;

            switch (InteractionType)
            {

                case InteractionType.Simple:
                    tooltip = InWorldCanvas.Instance.InstatiateItem(0).GetComponent<InWorldTooltip>();
                    StartCoroutine(MoveTooltip());
                    break;

                case InteractionType.Hold:
                    break;

                case InteractionType.Still:
                    break;
            }
        }

        public void HideTooltip()
        {
            if (tooltip == null) return;

            Destroy(tooltip.gameObject);
            tooltip = null;
        }

        IEnumerator MoveTooltip()
        {
            while (tooltip != null)
            {
                Collider c = GetComponent<Collider>();
                tooltip.transform.position = transform.position + Vector3.up * c.bounds.size.y/2;
                yield return new WaitForSeconds(0.1f);
            }
        }

        void HandleSimple(InputAction.CallbackContext ctx)
        {
            if (ctx.performed && _entity != null)
            {
                OnInteract?.Invoke(_entity.FocusedObj, _entity);
            }
        }

        void HandleHold(InputAction.CallbackContext ctx)
        {
            if (ctx.started)
            {
                _isHolding = true;

                _holdCoroutine = StartCoroutine(HoldInteractionCoroutine());

                Debug.Log("Started Hold... HoldTime: " + HoldTime);
            }

            if (ctx.canceled)
            {
                _isHolding = false;

                if (_holdCoroutine != null)
                    StopCoroutine(_holdCoroutine);

                OnCancelInteraction?.Invoke(_entity.FocusedObj, _entity);

                Debug.Log("Stopped Hold...");
            }
        }

        IEnumerator HoldInteractionCoroutine()
        {
            _remainingTime = HoldTime;

            while (_isHolding && _remainingTime > 0f)
            {
                _remainingTime -= Time.deltaTime;
                yield return null;
            }

            if (_isHolding)
            {
                // Done Holding (OnInteract pending to be developed...)
                OnInteract?.Invoke(_entity.FocusedObj, _entity);
            }

            _isHolding = false;
            _holdCoroutine = null;
        }

        void HandleStill(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed || _entity == null)
                return;

            if (_stillCoroutine != null)
                StopCoroutine(_stillCoroutine);

            _stillCoroutine = StartCoroutine(StillInteractionCoroutine());

            Debug.Log("Started Still interaction...");
        }

        IEnumerator StillInteractionCoroutine()
        {
            float remainingTime = HoldTime;

            while (_entity != null && remainingTime > 0f)
            {
                remainingTime -= Time.deltaTime;
                yield return null;
            }

            // Canceled
            if (_entity == null)
            {
                OnCancelInteraction?.Invoke(this, null);
                Debug.Log("Still canceled");
            }
            else
            {
                // Done
                OnInteract?.Invoke(this, _entity);
                Debug.Log("Still completed");
            }

            _stillCoroutine = null;
        }

        private void OnValidate()
        {
            _c = GetComponent<SphereCollider>();
            SetUpTrigger();
        }
    }
}
