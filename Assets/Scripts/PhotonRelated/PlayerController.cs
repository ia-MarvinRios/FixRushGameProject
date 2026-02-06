using UnityEngine;
using UnityEngine.InputSystem;

public enum InteractionType
{
    Simple,
    Hold,
    Still
}

public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] float _moveSpeed = 5f;
    [Space(10)]
    [Header("References")]
    [SerializeField] GameObject _playerPrefab;
    
    InputSystem_Actions _inputActions;
    InputAction _moveAction;
    Transform _cameraTransform;

    Rigidbody _rb;
    GameObject _playerA;
    GameObject _playerB;
    GameObject _selected;

    AuxPlayer _auxA;
    AuxPlayer _auxB;

    internal GameObject PlayerPrefab { get => _playerPrefab; }
    internal AuxPlayer SelAux { get; private set; }

    private void Awake()
    {
        _inputActions = new InputSystem_Actions();
    }

    #region INPUT HANDLING
    /// <summary>
    /// Enables player input actions.
    /// </summary>
    internal void EnablePlayerInputs(bool isSingleplayer)
    {
        _moveAction = _inputActions.Player.Move;
        _inputActions.Player.Enable();

        if (isSingleplayer)
        {
            _inputActions.Player.ToggleModel.performed += ToggleSelected;
            _inputActions.Player.ToggleModel.Enable();
        }

        _inputActions.Player.Interact.started += Interact;
        _inputActions.Player.Interact.performed += Interact;
        _inputActions.Player.Interact.canceled += Interact;
        _inputActions.Player.Interact.Enable();
    }
    /// <summary>
    /// Disables player input actions.
    /// </summary>
    internal void DisablePlayerInputs(bool isSingleplayer)
    {
        _inputActions.Player.Disable();

        if (isSingleplayer)
        {
            _inputActions.Player.ToggleModel.performed -= ToggleSelected;
            _inputActions.Player.ToggleModel.Disable();
        }

        _inputActions.Player.Interact.started -= Interact;
        _inputActions.Player.Interact.performed -= Interact;
        _inputActions.Player.Interact.canceled -= Interact;
        _inputActions.Player.Interact.Disable();
    }
    #endregion

    public void CheckCameraTransformRef()
    {
        if (_cameraTransform != null) { return; }

        Debug.LogWarning("Camera Transform reference not assigned, trying to use main camera instead...");
        _cameraTransform = Camera.main.transform;

        if (_cameraTransform != null) { return; }
        Debug.LogError("Failed to reference Main Camera on current scene.");
    }

    public void SetUpPlayer(GameObject[] models)
    {
        if (_playerA != null || _playerB != null)
        {
            Destroy(_playerA);
            Destroy(_playerB);
        }

        if (models.Length < 2)
        {
            _playerA = models[0];
            _auxA = _playerA.GetComponent<AuxPlayer>();

            SelectModel(_playerA, _auxA);

            return;
        }

        _playerA = models[0];
        _auxA = _playerA.GetComponent<AuxPlayer>();
        _playerB = models[1];
        _auxB = _playerB.GetComponent<AuxPlayer>();

        SelectModel(_playerA, _auxA);
    }

    void SelectModel(GameObject model, AuxPlayer aux)
    {
        _selected = model;
        SelAux = aux;

        _rb = _selected.GetComponent<Rigidbody>();
    }

    void ToggleSelected(InputAction.CallbackContext context)
    {
        if (_selected == _playerA) SelectModel(_playerB, _auxB);
        else SelectModel(_playerA, _auxA);
    }

    public void Move()
    {
        Vector2 input = _moveAction.ReadValue<Vector2>().normalized;
        Vector3 camFwd = _cameraTransform.forward.normalized;
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

    
    public void Interact(InputAction.CallbackContext context)
    {
        /*
        switch (InteractionType)
        {

            case InteractionType.Simple:
                HandleSimple(ctx, entity);
                break;

            case InteractionType.Hold:
                HandleHold(ctx, entity);
                break;

            case InteractionType.Still:
                HandleStill(ctx, entity);
                break;
        }
        */
    }

    /*
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
    */
}
