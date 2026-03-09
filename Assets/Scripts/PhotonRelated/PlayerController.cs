using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] Vector3 _spawnPointA = Vector3.zero;
    [SerializeField] Vector3 _spawnPointB = Vector3.zero;
    [Space(10)]
    [Header("References")]
    [SerializeField] GameObject _playerPrefab;

    PlayerNetwork _pNetwork;
    
    InputSystem_Actions _inputActions;
    InputAction _moveAction;
    Transform _cameraTransform;

    Rigidbody _rb;
    GameObject _playerA;
    GameObject _playerB;
    GameObject _selected;

    AuxPlayer _auxA;
    AuxPlayer _auxB;

    Coroutine _holdCoroutine;
    Coroutine _stillCoroutine;

    bool _isHolding = false;
    float _remainingTime;

    internal AuxPlayer SelAux { get; private set; }
    internal GameObject GrabbedObj
    {
        get => SelAux.GrabbedObj;
        set => SelAux.GrabbedObj = value;
    }
    internal Interactable FocusedObj { get; private set; }

    private void Awake()
    {
        _inputActions = new InputSystem_Actions();
        _pNetwork = GetComponent<PlayerNetwork>();
    }

    private void OnEnable()
    {
        if (!_pNetwork.IsLocalPlayer) {
            enabled = false;
            return;
        }

        EnablePlayerInputs(_pNetwork.IsLocalPlayer);
        CheckCameraTransformRef();

        SpawnPlayer();
    }

    private void OnDisable()
    {
        DisablePlayerInputs(_pNetwork.IsLocalPlayer);
    }

    private void FixedUpdate()
    {
        Move();
        UpdateFocused();
        MoveGrabbedObjs();
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

    private void SpawnPlayer()
    {
        SetUpPlayer(_pNetwork.SpawnPlayer(_playerPrefab, new Vector3[] { _spawnPointA, _spawnPointB}));
    }

    public void SetUpPlayer(GameObject[] models)
    {
        if (_playerA != null || _playerB != null)
        {
            Destroy(_playerA);
            Destroy(_playerB);
        }

        _playerA = models[0];
        _pNetwork.AttachPlayer(_playerA);
        _playerA.tag = "Player";
        _auxA = _playerA.GetComponentInChildren<AuxPlayer>();

        if (models.Length > 1)
        {
            _playerB = models[1];
            _pNetwork.AttachPlayer(_playerB);
            _playerB.tag = "Player";
            _auxB = _playerB.GetComponentInChildren<AuxPlayer>();
        }

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

    #region INTERACTION EVENTS

    public void Interact(InputAction.CallbackContext context)
    {
        // FOCUSED OBJ CHECKER
        if (FocusedObj == null)
        {
            // Return on performed
            if (!context.performed) return;

            // Drop grabbed
            if (GrabbedObj != null)
            {
                GrabbedObj.GetComponent<Pickable>().RequestDropObj(SelAux);
            }

            return;
        }

        switch (FocusedObj.InteractionType)
        {

            case InteractionType.Simple:
                HandleSimple(context, SelAux);
                break;

            case InteractionType.Hold:
                HandleHold(context, SelAux);
                break;

            case InteractionType.Still:
                HandleStill(context, SelAux);
                break;
        }
    }


    void HandleSimple(InputAction.CallbackContext ctx, AuxPlayer player)
    {
        if (!ctx.performed) return;

        FocusedObj.Interact(player);
    }

    void HandleHold(InputAction.CallbackContext ctx, AuxPlayer player)
    {
        if (ctx.started)
        {
            _isHolding = true;

            _holdCoroutine = StartCoroutine(HoldInteractionCoroutine(player, player.FocusedObj.HoldTime));

            Debug.Log("Started Hold... HoldTime: " + player.FocusedObj.HoldTime);
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

    IEnumerator HoldInteractionCoroutine(AuxPlayer player, float holdTime)
    {
        _remainingTime = holdTime;

        while (_isHolding && _remainingTime > 0f)
        {
            _remainingTime -= Time.deltaTime;
            yield return null;
        }

        if (_isHolding)
        {
            // Done Holding (OnInteract pending to be developed...)
            FocusedObj.Interact(player);
        }

        _isHolding = false;
        _holdCoroutine = null;
    }


    void HandleStill(InputAction.CallbackContext ctx, AuxPlayer player)
    {
        if (!ctx.performed || player == null)
            return;

        if (_stillCoroutine != null)
            StopCoroutine(_stillCoroutine);

        _stillCoroutine = StartCoroutine(StillInteractionCoroutine(player, player.FocusedObj.HoldTime));

        Debug.Log("Started Still interaction...");
    }

    IEnumerator StillInteractionCoroutine(AuxPlayer player, float holdTime)
    {
        float remainingTime = holdTime;

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
            FocusedObj.Interact(player);
            Debug.Log("Still completed");
        }

        _stillCoroutine = null;
    }

    #endregion

    protected void UpdateFocused()
    {
        if (_rb == null || SelAux == null)
        {
            FocusedObj = null;
            return;
        }

        const float FOV_THRESHOLD = 0.5f;
        const float DIST_WEIGHT = 0.1f;

        float bestScore = float.MinValue;
        Interactable best = null;

        Vector3 origin = _rb.position;
        Vector3 forward = _rb.transform.forward;

        List<Interactable> candidates = SelAux.FocusCandidates;

        for (int i = 0; i < candidates.Count; i++)
        {
            Interactable interactable = candidates[i];
            if (!interactable) continue;
            if (!interactable.Active) continue;

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

        SelAux.FocusedObj = best;
        FocusedObj = best;
    }

    void MoveGrabbedObjs()
    {
        if (_auxA != null && _auxA.GrabbedObj != null)
        {
            _auxA.GrabbedObj.transform.position =
                _playerA.transform.position + new Vector3(0, 2f, 0);
        }

        if (_pNetwork.IsOfflineMode) return;

        if (_auxB != null && _auxB.GrabbedObj != null)
        {
            _auxB.GrabbedObj.transform.position =
                _playerB.transform.position + new Vector3(0, 2f, 0);
        }
    }

    private void OnDrawGizmos()
    {
        if (TryGetComponent<BoxCollider>(out var c))
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
