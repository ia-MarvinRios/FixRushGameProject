using FixRushGame;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : Player
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

    GameObject _playerA;
    GameObject _playerB;
    GameObject _selected;

    AuxPlayer _auxA;
    AuxPlayer _auxB;

    internal override GameObject GrabbedObj
    {
        get => SelAux.GrabbedObj;
        set => SelAux.GrabbedObj = value;
    }
    internal override Interactable FocusedObj
    {
        get => SelAux != null ? SelAux.FocusedObj : null;
        set
        {
            if (SelAux != null)
                SelAux.FocusedObj = value;
        }
    }

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
        UpdateFocused();
        MoveGrabbedObjs();
    }

    /// <summary>
    /// Enables player input actions.
    /// </summary>
    protected override void EnablePlayerInputs()
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
    protected override void DisablePlayerInputs()
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

    /// <summary>
    /// Initializes and spawns player models at their designated spawn points, replacing any existing player instances.
    /// Sets the primary player model as the active player.
    /// </summary>
    /// <remarks>This method destroys any previously spawned player models before instantiating new ones. Both
    /// player models are assigned the "Player" tag and positioned at their respective spawn points. The primary player
    /// model is set as the active player using the SelectModel method. This method should be called when resetting or
    /// starting a new player session to ensure correct player setup.</remarks>
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
        _auxA = _playerA.GetComponent<AuxPlayer>();
        // Model B
        _playerB = Instantiate(_playerPrefab, _spawnPointB, Quaternion.identity, transform);
        _playerB.gameObject.tag = "Player";
        _auxB = _playerB.GetComponent<AuxPlayer>();

        // Set model A as active player
        SelectModel(_playerA, _auxA);
    }

    void SelectModel(GameObject model, AuxPlayer aux)
    {
        _selected = model;
        SelAux = aux;

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
    void MoveGrabbedObjs()
    {
        if(_auxA.GrabbedObj != null)
        {
            _auxA.GrabbedObj.transform.position = _playerA.transform.position + new Vector3(0, 2f, 0);
        }
        if (_auxB.GrabbedObj != null)
        {
            _auxB.GrabbedObj.transform.position = _playerB.transform.position + new Vector3(0, 2f, 0);
        }
    }

    void ToggleSelected(InputAction.CallbackContext ctx)
    {
        if (_selected == _playerA) SelectModel(_playerB, _auxB);
        else SelectModel(_playerA, _auxA);
    }

    void Interact(InputAction.CallbackContext ctx)
    {
        // No focused obj
        if (FocusedObj == null)
        {
            // Return on performed
            if (!ctx.performed) return;

            // Drop grabbed
            if (GrabbedObj != null)
            {
                GrabbedObj.GetComponent<Pickable>().Drop();
            }

            return;
        }

        // There's focused obj then pass ctx
        FocusedObj.Interact(ctx, SelAux);
    }

    private void OnDrawGizmos()
    {
        if(_playerPrefab.TryGetComponent<BoxCollider>(out var c))
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(_spawnPointA, c.size);
            Gizmos.DrawWireCube(_spawnPointB, c.size);
        }

        if (FocusedObj != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(_selected.transform.position, FocusedObj.transform.position);
        }
    }
}
