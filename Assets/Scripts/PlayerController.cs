using FixRushGame;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    [Tooltip("Movement speed of the player in units per second.")]
    [SerializeField] Vector3 _spawnPointA = Vector3.zero;
    [SerializeField] Vector3 _spawnPointB = Vector3.zero;
    [SerializeField] float _moveSpeed = 5f;
    [Space(10)]
    [Header("References")]
    [SerializeField] Transform _cameraTransform;
    [SerializeField] GameObject _playerPrefab;
    
    Rigidbody _rb;

    InputSystem_Actions _inputActions;
    InputAction _moveAction;

    GameObject _playerA;
    GameObject _playerB;
    GameObject _selected;

    public Interactable FocusedObj { get; set; }

    private void Awake()
    {
        _inputActions = new InputSystem_Actions();
    }
    private void OnEnable()
    {
        CheckCameraTransformRef();
        SetUpPlayer();
        EnablePlayerInputs();
    }
    private void OnDisable()
    {
        DisablePlayerInputs();
    }
    private void FixedUpdate()
    {
        MovePlayer();
    }

    /// <summary>
    /// Enables player input actions.
    /// </summary>
    void EnablePlayerInputs()
    {
        _moveAction = _inputActions.Player.Move;
        _inputActions.Player.Enable();

        _inputActions.Player.ToggleModel.performed += ToggleSelected;
        _inputActions.Player.ToggleModel.Enable();

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

        _inputActions.Player.ToggleModel.performed -= ToggleSelected;
        _inputActions.Player.ToggleModel.Disable();

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

    void SetUpPlayer()
    {
        if ( _playerA != null || _playerB != null)
        {
            Destroy(_playerA);
            Destroy(_playerB);
        }
        
        // Model A
        _playerA = Instantiate(_playerPrefab, _spawnPointA, Quaternion.identity, transform);
        _playerA.gameObject.tag = "Player";
        // Model B
        _playerB = Instantiate(_playerPrefab, _spawnPointB, Quaternion.identity, transform);
        _playerB.gameObject.tag = "Player";

        // Set model A as active player
        SelectModel(_playerA);
    }

    void SelectModel(GameObject model)
    {
        _selected = model;
        _rb = _selected.GetComponent<Rigidbody>();
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
        _selected.transform.rotation = 
            Quaternion.Slerp(_selected.transform.rotation, Quaternion.LookRotation(new Vector3(move.x, 0, move.z), Vector3.up), 10 * Time.fixedDeltaTime);
    }

    void ToggleSelected(InputAction.CallbackContext ctx)
    {
        if (_selected == _playerA) SelectModel(_playerB);
        else SelectModel(_playerA);
    }

    void Interact(InputAction.CallbackContext ctx)
    {
        if (FocusedObj == null) return;

        FocusedObj.Interact(ctx);
    }

    private void OnDrawGizmos()
    {
        if(_playerPrefab.TryGetComponent<BoxCollider>(out var c))
        {
            Gizmos.color = UnityEngine.Color.yellow;
            Gizmos.DrawWireCube(_spawnPointA, c.size);
            Gizmos.DrawWireCube(_spawnPointB, c.size);
        }
    }
}
