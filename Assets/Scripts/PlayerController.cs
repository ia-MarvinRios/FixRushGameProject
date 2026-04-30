using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FixRush;
using UnityEngine.VFX;

public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] internal PlayerSettings Player;

    [Header("References")]
    [SerializeField] internal GameObject Model1;
    [SerializeField] internal GameObject Model2;
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private PlayerNetworkHandler _networkHandler;
    [SerializeField] internal Transform ObjRoot;
    
    private VisualEffect _particulasEffects;



    private InputSystem_Actions _inputActions;
    private InputAction _moveAction;

    private Camera _camera;

    private Vector2 _moveInput;
    private Vector3 _moveDirection;
    private Vector3 _velocity;
    private Vector3 _currentVelocity;
    private float _acceleration = 10f;
    private float _deceleration = 15f;

    private bool _paused = false;

    internal Avatar Avatar;

    private readonly List<GameObject> _focusCandidates = new();
    internal GameObject FocusedObj = null;
    internal GameObject GrabbedObj = null;
    private Coroutine _holdCoroutine;
    private Coroutine _stillCoroutine;
    private bool _isHolding = false;
    private float _remainingTime;

    private void OnEnable()
    {
        if (!_networkHandler.PhotonViewIsMine)
        {
            return;
        }

        _inputActions = new InputSystem_Actions();
        _camera = Camera.main;

        EnableAllInputs();
    }

    private void Start()
    {
        _particulasEffects = GetComponent<VisualEffect>();
    }
    private void FixedUpdate()
    {
        if (!_networkHandler.PhotonViewIsMine) { return; }

        ApplyGravity();
        Move();
        UpdateFocused();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_networkHandler.PhotonViewIsMine) { return; }

        // Items logic
        if (other.TryGetComponent(out IInteractable interactable))
        {
            _focusCandidates.Add(((MonoBehaviour)interactable).gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!_networkHandler.PhotonViewIsMine) { return; }

        // Items logic
        if (other.TryGetComponent(out IInteractable interactable))
        {
            _focusCandidates.Remove(((MonoBehaviour)interactable).gameObject);
        }
    }

    #region INPUTS

    private void EnableAllInputs()
    {
        _moveAction = _inputActions.Player.Move;
        _moveAction.Enable();

        // Interaction
        _inputActions.Player.Interact.started += HandleInteractionInput;
        _inputActions.Player.Interact.performed += HandleInteractionInput;
        _inputActions.Player.Interact.canceled += HandleInteractionInput;
        _inputActions.Player.Interact.Enable();

        // Switch fixer
        if (PhotonManager.Instance.OfflineMode)
        {
            _inputActions.Player.SwitchFixer.performed += HandleSwitchFixer;
            _inputActions.Player.SwitchFixer.Enable();
        }

        // UI
        _inputActions.UI.Escape.performed += HandleEscapeInput;
        _inputActions.UI.Escape.Enable();
    }
    internal void DisableAllInputs()
    {
        _moveAction.Disable();

        // Interaction
        _inputActions.Player.Interact.started -= HandleInteractionInput;
        _inputActions.Player.Interact.performed -= HandleInteractionInput;
        _inputActions.Player.Interact.canceled -= HandleInteractionInput;
        _inputActions.Player.Interact.Disable();

        // Switch fixer
        if (PhotonManager.Instance.OfflineMode)
        {
            _inputActions.Player.SwitchFixer.performed -= HandleSwitchFixer;
            _inputActions.Player.SwitchFixer.Disable();
        }

        // UI
        _inputActions.UI.Escape.performed -= HandleEscapeInput;
        _inputActions.UI.Escape.Disable();
    }

    private void SetActionMapEnabled(InputActionMap map, bool enabled)
    {
        if (enabled) { map.Enable(); }
        else { map.Disable(); }
    }

    // UI Inputs

    private void HandleEscapeInput(InputAction.CallbackContext context)
    {
        // Toggle Player map inputs
        _paused = !_paused;
        SetActionMapEnabled(_inputActions.Player, !_paused);

        // Update UI
        InGameUI.Instance.TogglePauseMenu();
    }

    private void HandleSwitchFixer(InputAction.CallbackContext context)
    {
        Debug.Log("Switching fixer...");
    }

    #endregion

    #region INTERACTIONS

    // Interaction Inputs

    private void HandleInteractionInput(InputAction.CallbackContext context)
    {
        if (FocusedObj == null)
        {
            // Drop the currently grabbed object if there's no focused object and the player is trying to interact
            if (GrabbedObj != null && context.performed)
            {
                DropObject(GrabbedObj);
            }

            return;
        }

        // Get the IInteractable component from the focused object
        IInteractable interactable = FocusedObj?.GetComponent<IInteractable>();

        if (interactable == null) return;

        // Call the appropriate handler based on the interaction type
        switch (interactable.InteractionType)
        {
            case IInteractable.Type.Simple:
                HandleSimple(context, this, interactable);
                break;
            case IInteractable.Type.Hold:
                HandleHold(context, this, interactable);
                break;
            case IInteractable.Type.Still:
                HandleStill(context, this, interactable);
                break;
        }
    }

    void HandleSimple(InputAction.CallbackContext ctx, PlayerController player, IInteractable interactable)
    {
        if (!ctx.performed) return;

        interactable?.Interact(player);
    }

    void HandleHold(InputAction.CallbackContext ctx, PlayerController player, IInteractable interactable)
    {
        if (ctx.started)
        {
            _isHolding = true;

            _holdCoroutine = StartCoroutine(HoldInteractionCoroutine(player, interactable));

            Debug.Log("Started Hold... HoldTime: " + interactable.HoldTime);
        }

        if (ctx.canceled)
        {
            _isHolding = false;

            if (_holdCoroutine != null)
                StopCoroutine(_holdCoroutine);

            //OnCancelInteraction?.Invoke(p.FocusedObj, p);

            Debug.Log("Stopped Hold...");
        }
    }

    IEnumerator HoldInteractionCoroutine(PlayerController player, IInteractable interactable)
    {
        _remainingTime = interactable.HoldTime;

        while (_isHolding && _remainingTime > 0f)
        {
            _remainingTime -= Time.deltaTime;
            yield return null;
        }

        if (_isHolding)
        {
            // Done Holding (OnInteract pending to be developed...)
            interactable?.Interact(player);
        }

        _isHolding = false;
        _holdCoroutine = null;
    }


    void HandleStill(InputAction.CallbackContext ctx, PlayerController player, IInteractable interactable)
    {
        if (!ctx.performed || player == null)
            return;

        if (_stillCoroutine != null)
            StopCoroutine(_stillCoroutine);

        _stillCoroutine = StartCoroutine(StillInteractionCoroutine(player, interactable));

        Debug.Log("Started Still interaction...");
    }

    IEnumerator StillInteractionCoroutine(PlayerController player, IInteractable interactable)
    {
        float remainingTime = interactable.HoldTime;

        while (player != null && remainingTime > 0f)
        {
            remainingTime -= Time.deltaTime;
            yield return null;
        }

        // Canceled
        if (player == null)
        {
            //OnCancelInteraction?.Invoke(this, null);
            Debug.Log("Still canceled");
        }
        else
        {
            // Done
            interactable?.Interact(player);
            Debug.Log("Still completed");
        }

        _stillCoroutine = null;
    }

    internal void PickUpObject(GameObject obj)
    {
        _focusCandidates.Remove(obj);

        _networkHandler.PickUpRequest(obj);
    }

    internal void DropObject(GameObject obj)
    {
        _networkHandler.DropRequest(obj);
    }

    #endregion

    private void ApplyGravity()
    {
        if (_characterController.isGrounded && _velocity.y < 0)
            _velocity.y = -2f;

        _velocity += Physics.gravity * Time.fixedDeltaTime;

        _characterController.Move(_velocity * Time.fixedDeltaTime);
    }

    private void Move()
    {
        // Process movement input
        _moveInput = _moveAction.ReadValue<Vector2>();
        _moveDirection = _camera.transform.forward.normalized * _moveInput.y + _camera.transform.right.normalized * _moveInput.x;
        _moveDirection.y = 0;

        if (_moveDirection.magnitude > 0)
        {
            // Accelerates the player smoothly
            _currentVelocity = Vector3.MoveTowards(
                _currentVelocity,
                _moveDirection * Player.MoveSpeed,
                _acceleration * Time.deltaTime
            );
        }
        else
        {
            // Slows down the player smoothly when there's no input
            _currentVelocity = Vector3.MoveTowards(
                _currentVelocity,
                Vector3.zero,
                _deceleration * Time.deltaTime
            );
        }

        _characterController.Move(_currentVelocity * Time.deltaTime);

        // Rotate
        if (_moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(_moveDirection),
                10 * Time.fixedDeltaTime
            );
        }

        // Animations
        if (Avatar != null) { Avatar.ProcessAnimations(); }
    }

    protected void UpdateFocused()
    {
        if (_characterController == null)
        {
            FocusedObj = null;
            return;
        }

        const float FOV_THRESHOLD = 0.5f;
        const float DIST_WEIGHT = 0.1f;

        float bestScore = float.MinValue;
        GameObject best = null;

        Vector3 origin = transform.position;
        Vector3 forward = transform.forward;

        List<GameObject> candidates = _focusCandidates;

        for (int i = 0; i < candidates.Count; i++)
        {
            GameObject interactable = candidates[i];
            if (!interactable) continue;
            if (!interactable.activeSelf) continue;

            Vector3 toObj = interactable.transform.position - origin;
            float distance = toObj.magnitude;
            if (distance <= 0.001f) continue;

            Vector3 dir = toObj / distance;
            float dot = Vector3.Dot(forward, dir);

            if (dot < FOV_THRESHOLD)
                continue;

            float score = dot - distance * DIST_WEIGHT;

            if (score > bestScore)
            {
                bestScore = score;
                best = interactable;
            }
        }

        FocusedObj = best;
    }

   

    /// <summary>
    /// Particulas que se activaran cuando se este trabajando en una reparación, Se espera un bool, true = play, false = stop
    /// </summary>
    /// <param name="status"></param>
    public void OnWorkParticle(bool status)
    {
        if (status)
        {
            _particulasEffects.Stop();
        }
        else
        {
            _particulasEffects.Play();
        }
    }

    public void ShowWorkParticle(bool show)
    {
        _networkHandler.SyncWorkParticles(show);
    }



    private void OnDrawGizmos()
    {
        if (FocusedObj != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, FocusedObj.transform.position);
        }
    }
}
