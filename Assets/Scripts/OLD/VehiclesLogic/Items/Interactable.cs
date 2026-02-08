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
        public FixRushGame.InteractionType InteractionType;
        [Tooltip("Waiting time to complete a hold interaction. Ignored if the action is set to simple type.")]
        [Range(0, 20)] public float HoldTime = 0;
        [Header("Trigger Settings")]
        [SerializeField] float _triggerRadius = 0.5f;

        SphereCollider _c;
        Coroutine _holdCoroutine;
        Coroutine _stillCoroutine;
        InWorldTooltip tooltip;
        bool _isHolding = false;
        float _remainingTime = 0;

        public delegate void InteractableDelegate(Interactable obj, AuxPlayer entity);
        public delegate void CancelInteractionDelegate(Interactable obj, AuxPlayer entity);
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
            HideTooltip();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out AuxPlayer aux))
            {
                aux.FocusCandidates.Add(this);
                ShowTooltip();
            }
        }
        private void OnTriggerStay(Collider other)
        {
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out AuxPlayer aux))
            {
                aux.FocusCandidates.Remove(this);

                if (_stillCoroutine != null)
                {
                    StopCoroutine(_stillCoroutine);
                    _stillCoroutine = null;
                    OnCancelInteraction?.Invoke(this, aux);
                }

                HideTooltip();
            }
        }

        void SetUpTrigger()
        {
            _c.radius = _triggerRadius;
            _c.isTrigger = true;
        }

        public void SetInteractable(FixRushGame.InteractionType interactionType, float holdTime, float triggerRadius)
        {
            InteractionType = interactionType;
            TriggerRadius = triggerRadius;
            HoldTime = holdTime;
        }

        public void Interact(InputAction.CallbackContext ctx, AuxPlayer entity)
        {
            switch (InteractionType){

                case FixRushGame.InteractionType.Simple:
                    HandleSimple(ctx, entity);
                    break;

                case FixRushGame.InteractionType.Hold:
                    HandleHold(ctx, entity);
                    break;

                case FixRushGame.InteractionType.Still:
                    HandleStill(ctx, entity);
                    break;
            }
        }

        public void ShowTooltip()
        {
            if (tooltip != null) return;

            switch (InteractionType)
            {

                case FixRushGame.InteractionType.Simple:
                    tooltip = InWorldCanvas.Instance.InstatiateItem(0).GetComponent<InWorldTooltip>();
                    StartCoroutine(MoveTooltip());
                    break;

                case FixRushGame.InteractionType.Hold:
                    break;

                case FixRushGame.InteractionType.Still:
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

        void HandleSimple(InputAction.CallbackContext ctx, AuxPlayer p)
        {
            if (!ctx.performed) return;

            OnInteract?.Invoke(this, p);
        }

        void HandleHold(InputAction.CallbackContext ctx, AuxPlayer p)
        {
            if (ctx.started)
            {
                _isHolding = true;

                _holdCoroutine = StartCoroutine(HoldInteractionCoroutine(p));

                Debug.Log("Started Hold... HoldTime: " + HoldTime);
            }

            if (ctx.canceled)
            {
                _isHolding = false;

                if (_holdCoroutine != null)
                    StopCoroutine(_holdCoroutine);

                OnCancelInteraction?.Invoke(p.FocusedObj, p);

                Debug.Log("Stopped Hold...");
            }
        }

        IEnumerator HoldInteractionCoroutine(AuxPlayer p)
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
                OnInteract?.Invoke(p.FocusedObj, p);
            }

            _isHolding = false;
            _holdCoroutine = null;
        }

        void HandleStill(InputAction.CallbackContext ctx, AuxPlayer p)
        {
            if (!ctx.performed || p == null)
                return;

            if (_stillCoroutine != null)
                StopCoroutine(_stillCoroutine);

            _stillCoroutine = StartCoroutine(StillInteractionCoroutine(p));

            Debug.Log("Started Still interaction...");
        }

        IEnumerator StillInteractionCoroutine(AuxPlayer p)
        {
            float remainingTime = HoldTime;

            while (p != null && remainingTime > 0f)
            {
                remainingTime -= Time.deltaTime;
                yield return null;
            }

            // Canceled
            if (p == null)
            {
                OnCancelInteraction?.Invoke(this, null);
                Debug.Log("Still canceled");
            }
            else
            {
                // Done
                OnInteract?.Invoke(this, p);
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
