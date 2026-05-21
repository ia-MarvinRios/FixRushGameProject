using FixRush;
using UnityEngine;
using UnityEngine.VFX;

public class OfflinePlayer : PlayerController
{
    [Header("Offline Player Settings")]
    [SerializeField] private GameObject _defaultHat;
    [SerializeField] private GameObject _defaultBody;

    private void Awake()
    {
        _inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _camera = Camera.main;

        EnableAllInputs();
        CreateAvatar();
    }

    private void OnDisable()
    {
        if (_inputActions == null) return;

        DisableAllInputs();
    }

    private void OnDestroy()
    {
        if (_inputActions == null) return;

        DisableAllInputs();
    }

    private void FixedUpdate()
    {
        ApplyGravity();
        Move();
        UpdateFocused();
    }

    private void OnTriggerEnter(Collider other)
    {
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

        IPickupable objP = obj.GetComponent<IPickupable>();

        objP.PickUp(this);
    }

    internal override void DropObject(GameObject obj)
    {
        IPickupable objP = obj.GetComponent<IPickupable>();

        objP.Drop(this);
    }

    #endregion

    #region VISUALS

    private void CreateAvatar()
    {
        GameObject body = Instantiate(
            _defaultBody,
            Model1.transform
        );

        GameObject hat = Instantiate(
            _defaultHat,
            Model1.transform
        );

        body.transform.localPosition = Vector3.zero;
        body.transform.localRotation = Quaternion.identity;
        hat.transform.localPosition = Vector3.zero;
        hat.transform.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// Particulas que se activaran cuando se este trabajando en una reparación, Se espera un bool, true = play, false = stop
    /// </summary>
    /// <param name="status"></param>
    public override void ShowWorkParticle(bool show)
    {
        VisualEffect vs = GetComponent<VisualEffect>();

        if (show) { vs.Play(); }
        else { vs.Stop(); }
    }

    #endregion
}
