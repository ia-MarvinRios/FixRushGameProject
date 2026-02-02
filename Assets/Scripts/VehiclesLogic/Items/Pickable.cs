using FixRushGame;
using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class Pickable : MonoBehaviour
{
    public AuxPlayer Owner { get; private set; }
    private SphereCollider _c;

    public bool IsPickable
    {
        get => _c.enabled;
        set => _c.enabled = value;
    }

    private void Awake()
    {
        _c = GetComponent<SphereCollider>();
    }

    private void Start()
    {
        Interactable.OnInteract += PickUp;
    }
    private void OnDestroy()
    {
        Interactable.OnInteract -= PickUp;
    }

    public void PickUp(Interactable obj, AuxPlayer aux)
    {
        if (obj != GetComponent<Interactable>())
            return;

        // Ownership
        if (Owner != null && Owner != aux)
            return;

        // Aux already holding something, drop it first
        if (aux.GrabbedObj != null)
        {
            aux.GrabbedObj.GetComponent<Pickable>().Drop();
        }

        Owner = aux;
        aux.GrabbedObj = gameObject;

        if (TryGetComponent(out Rigidbody rb))
            rb.isKinematic = true;

        IsPickable = false;
        obj.HideTooltip();

        aux.FocusCandidates.Remove(obj);

        transform.rotation = Quaternion.identity;
    }

    public void Drop()
    {
        if (Owner == null)
            return;

        Owner.GrabbedObj = null;
        Owner = null;

        if (TryGetComponent(out Rigidbody rb))
            rb.isKinematic = false;

        IsPickable = true;
    }

    public void ReleaseOwnership()
    {
        if (Owner != null)
        {
            Owner.GrabbedObj = null;
            Owner = null;
        }
    }
}
