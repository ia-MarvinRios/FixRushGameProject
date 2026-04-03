using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] internal PlayerSettings Player;

    [Header("References")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private PlayerNetworkHandler _networkHandler;

    private InputSystem_Actions _inputActions;
    private InputAction _moveAction;
    private InputAction _lookAction;

    private Vector2 _moveInput;
    private Vector3 _moveDirection;

    private bool _paused = false;

    internal Avatar Avatar;

    private void OnEnable()
    {
        if (!_networkHandler.PhotonViewIsMine)
        {
            return;
        }

        Debug.Log("Doing weird stuff from gameobject with name: " + gameObject.name);
        _inputActions = new InputSystem_Actions();

        EnableAllInputs();
    }

    private void FixedUpdate()
    {
        if (!_networkHandler.PhotonViewIsMine) { return; }

        Move();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_networkHandler.PhotonViewIsMine) { return; }

        // Items logic
    }

    #region INPUTS
    private void EnableAllInputs()
    {
        _moveAction = _inputActions.Player.Move;
        _moveAction.Enable();

        // UI
        _inputActions.UI.Escape.performed += HandleEscapeInput;
        _inputActions.UI.Escape.Enable();
    }
    internal void DisableAllInputs()
    {
        _moveAction.Disable();

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

    #endregion

    private void Move()
    {
        _moveInput = _moveAction.ReadValue<Vector2>();

        /*
        _moveDirection =
            _playerCamera.transform.forward.normalized * _moveInput.y +
            _playerCamera.transform.right.normalized * _moveInput.x;
        _moveDirection.y = 0f;

        _rigidbody.linearVelocity = _moveDirection * Player.MoveSpeed;
        */

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
}
