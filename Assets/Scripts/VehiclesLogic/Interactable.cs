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
        [Tooltip("Waiting time to complete a hold interaction. Ignored if the action is set to a different type.")]
        [Range(0, 20)] public float HoldTime = 0;
        [Header("Trigger Settings")]
        [SerializeField] float _triggerRadius = 0.5f;

        SphereCollider _c;
        GameObject _entity;
        Coroutine _holdCoroutine;
        bool _isHolding = false;

        public delegate void InteractableDelegate(GameObject obj, GameObject entity);
        public delegate void CancelInteractionDelegate(GameObject obj, GameObject entity);
        public static event InteractableDelegate OnInteract;
        public static event CancelInteractionDelegate OnCancelInteraction;

        public float TriggerRadius { get { return _triggerRadius; } set { _triggerRadius = value; } }

        private void Awake()
        {
            _c = GetComponent<SphereCollider>();
        }

        private void Start()
        {
            _c.isTrigger = true;
            _c.radius = _triggerRadius;
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                _entity = other.gameObject;
                _entity.GetComponentInParent<PlayerController>().FocusedObj = this;

                // I'm gonna use a TryGetComponent check when online multiplayer so don't touch this part for now.
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                _entity.GetComponentInParent<PlayerController>().FocusedObj = null;
                _entity = null;

                // I'm gonna use a TryGetComponent check when online multiplayer so don't touch this part for now.
            }
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
                    if (ctx.performed)
                        OnInteract?.Invoke(gameObject, _entity);
                    break;

                case InteractionType.Hold:
                    if (ctx.started)
                    {
                        _isHolding = true;

                        if (_holdCoroutine != null)
                            StopCoroutine(_holdCoroutine);

                        _holdCoroutine = StartCoroutine(HoldInteractionCoroutine());
                    }

                    if (ctx.canceled)
                    {
                        _isHolding = false;
                        OnCancelInteraction?.Invoke(gameObject, _entity);
                    }
                    break;
            }
        }

        IEnumerator HoldInteractionCoroutine()
        {
            float remainingTime = HoldTime;

            while (_isHolding && remainingTime > 0f)
            {
                remainingTime -= Time.deltaTime;
                yield return null;
            }

            if (_isHolding)
            {
                // Done Holding (OnInteract pending to be developed...)
                OnInteract?.Invoke(gameObject, _entity);
            }

            _isHolding = false;
            _holdCoroutine = null;
        }
    }
}
