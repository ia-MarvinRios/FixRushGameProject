using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Interactable), typeof(PickableNetwork))]
public class Pickable : MonoBehaviour
{
    PickableNetwork _pickableNetwork;
    Coroutine _moveOnPicked;

    public AuxPlayer Owner { get; internal set; }
    public bool IsPickedUp => Owner != null;

    private void Awake()
    {
        _pickableNetwork = GetComponent<PickableNetwork>();
    }

    public void RequestPickUpObj(AuxPlayer entity)
    {
        _pickableNetwork.RequestPickUp(entity);
    }
    public void RequestDropObj(AuxPlayer entity)
    {
        _pickableNetwork.RequestDrop(entity);
    }

    internal void PickUp(AuxPlayer aux)
    {
        if (Owner != null && Owner != aux)
            return;

        // Aux already holding something, drop it first
        if (aux.GrabbedObj != null)
        {
            aux.GrabbedObj.GetComponent<Pickable>().RequestDropObj(aux);
        }

        Owner = aux;
        aux.GrabbedObj = gameObject;

        if (TryGetComponent(out Rigidbody rb))
            rb.isKinematic = true;

        aux.FocusCandidates.Remove(GetComponent<Interactable>());

        transform.rotation = Quaternion.identity;

        StartCoroutine(MoveOnPicked());
    }

    internal void Drop()
    {
        ReleaseOwnership();

        if (TryGetComponent(out Rigidbody rb))
            rb.isKinematic = false;

    }

    public void ReleaseOwnership()
    {
        if (Owner != null)
        {
            Owner.GrabbedObj = null;
            Owner = null;

            if (_moveOnPicked != null)
            {
                StopCoroutine(_moveOnPicked);
                _moveOnPicked = null;
            }
        }
    }

    IEnumerator MoveOnPicked()
    {
        while (Owner != null)
        {
            transform.position = Owner.transform.position + new Vector3(0, 2f, 0);
            yield return null;
        }

        _moveOnPicked = null;
    }
}
