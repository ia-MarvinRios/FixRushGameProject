using FixRush;
using UnityEngine;
using UnityEngine.InputSystem;

public class OnlinePlayer : PlayerController
{
    [Header("Online Player Settings")]
    [SerializeField] private PlayerNetworkHandler _networkHandler;

    private void Awake()
    {
        if (!_networkHandler.PhotonViewIsMine) { return; }
        _inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        if (!_networkHandler.PhotonViewIsMine)
        {
            return;
        }

        _camera = Camera.main;

        EnableAllInputs();

        // Events
        GameManager.OnLevelTimeOut += DisableAllInputs;
    }

    private void OnDisable()
    {
        if (!_networkHandler.PhotonViewIsMine) { return; }
        if (_inputActions == null) return;

        DisableAllInputs();

        // Events
        GameManager.OnLevelTimeOut -= DisableAllInputs;
    }

    private void OnDestroy()
    {
        if (!_networkHandler.PhotonViewIsMine) { return; }
        if (_inputActions == null) return;

        DisableAllInputs();
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
            _focusCandidates.Add(
                ((MonoBehaviour)interactable).gameObject
            );
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!_networkHandler.PhotonViewIsMine) { return; }

        // Items logic
        if (other.TryGetComponent(out IInteractable interactable))
        {
            _focusCandidates.Remove(
                ((MonoBehaviour)interactable).gameObject
            );
        }
    }

    #region INTERACTIONS

    internal override void PickUpObject(GameObject obj)
    {
        _focusCandidates.Remove(obj);

        _networkHandler.PickUpRequest(obj);
    }

    internal override void DropObject(GameObject obj)
    {
        _networkHandler.DropRequest(obj);
    }

    #endregion

    #region VISUALS

    /// <summary>
    /// Particulas que se activaran cuando se este trabajando en una reparación, Se espera un bool, true = play, false = stop
    /// </summary>
    /// <param name="status"></param>
    public override void ShowWorkParticle(bool show)
    {
        _networkHandler.SyncWorkParticles(show);
    }

    #endregion
}
