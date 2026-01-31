using FixRushGame;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[RequireComponent(typeof(Interactable))]
public class Pickable : MonoBehaviour
{
    public bool IsPickable
    {
        get
        {
            return GetComponent<SphereCollider>().enabled;
        }
        set
        {
            GetComponent<SphereCollider>().enabled = value;
        }
    }

    private void Start()
    {
        Interactable.OnInteract += PickUp;
    }
    private void OnDestroy()
    {
        Interactable.OnInteract -= PickUp;
    }

    public void PickUp(Interactable obj, Player entity)
    {
        if (obj == GetComponent<Interactable>())
        {
            if (TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
            IsPickable = false;
            obj.HideTooltip();
            
            transform.rotation = Quaternion.identity;

            entity.FocusCandidates.Remove(obj);

            if (entity.GrabbedObj != null) { entity.GrabbedObj.GetComponent<Pickable>().Drop(entity); }

            entity.GrabbedObj = gameObject;
        }
    }

    public void Drop(Player entity)
    {
        entity.GrabbedObj = null;

        if (TryGetComponent(out Rigidbody rb)) rb.isKinematic = false;

        IsPickable = true;
    }
}
