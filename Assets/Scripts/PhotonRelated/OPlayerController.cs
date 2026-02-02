using FixRushGame;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class OPlayerController : MonoBehaviourPunCallbacks
{
    [Header("Player Settings")]
    [Tooltip("Movement speed of the player in units per second.")]
    [SerializeField] float _moveSpeed = 5f;
    [Space(10)]
    [Header("References")]
    [SerializeField] Transform _cameraTransform;
    [SerializeField] Rigidbody _rb;

    InputSystem_Actions _inputActions;
    InputAction _moveAction;

    public Interactable FocusedObj { get; set; }

    private void Awake()
    {
        if (!photonView.IsMine) return;
        _inputActions = new InputSystem_Actions();
    }
    public override void OnEnable()
    {
        if (!photonView.IsMine) return;
        CheckCameraTransformRef();
        EnablePlayerInputs();
    }
    public override void OnDisable()
    {
        if (!photonView.IsMine) return;
        DisablePlayerInputs();
    }
    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;
        MovePlayer();
    }

    /// <summary>
    /// Enables player input actions.
    /// </summary>
    void EnablePlayerInputs()
    {
        _moveAction = _inputActions.Player.Move;
        _inputActions.Player.Enable();

        _inputActions.Player.Interact.started += Interact;
        _inputActions.Player.Interact.performed += Interact;
        _inputActions.Player.Interact.canceled += Interact;
        _inputActions.Player.Interact.Enable();
    }
    /// <summary>
    /// Disables player input actions.
    /// </summary>
    void DisablePlayerInputs()
    {
        _inputActions.Player.Disable();

        _inputActions.Player.Interact.started -= Interact;
        _inputActions.Player.Interact.performed -= Interact;
        _inputActions.Player.Interact.canceled -= Interact;
        _inputActions.Player.Interact.Disable();
    }

    void CheckCameraTransformRef()
    {
        if (_cameraTransform != null) { return; }

        Debug.LogWarning("Camera Transform reference not assigned, trying to use main camera instead...");
        _cameraTransform = Camera.main.transform;

        if (_cameraTransform != null) { return; }
        Debug.LogError("Failed to reference Main Camera on current scene.");
    }

    void MovePlayer()
    {
        Vector2 input    = _moveAction.ReadValue<Vector2>().normalized;
        Vector3 camFwd   = _cameraTransform.forward.normalized;
        Vector3 camRight = _cameraTransform.right.normalized;

        Vector3 move = camFwd * input.y + camRight * input.x;

        _rb.linearVelocity = new Vector3(
            move.x * _moveSpeed,
            _rb.linearVelocity.y,
            move.z * _moveSpeed
            );

        if (input == Vector2.zero) return;
        _rb.transform.rotation = 
            Quaternion.Slerp(_rb.transform.rotation, Quaternion.LookRotation(new Vector3(move.x, 0, move.z), Vector3.up), 10 * Time.fixedDeltaTime);
    }

    void Interact(InputAction.CallbackContext ctx)
    {
        if (FocusedObj == null) return;

        //FocusedObj.Interact(ctx);
    }
}
