using UnityEngine;
using FixRush;

public class Jack : MonoBehaviour, IInteractable, IPickupable
{
    [Header("Interaction Settings")]
    [SerializeField] private IInteractable.Type _interactionType = IInteractable.Type.Simple;
    [SerializeField] private float _holdTime = 2f;
    
    public IInteractable.Type InteractionType => _interactionType;
    public float HoldTime => _holdTime;

    public void Interact(PlayerController player)
    {
        player.PickUpObject(gameObject);
    }

    public void PickUp(PlayerController player)
    {
        if (player.GrabbedObj != null)
        {
            player.DropObject(gameObject);
        }

        // Disable some components
        GetComponent<BoxCollider>().enabled = false;
        if (PhotonManager.Instance.IsMasterClient) GetComponent<Rigidbody>().isKinematic = true;
        transform.SetParent(player.ObjRoot);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        player.GrabbedObj = gameObject;
    }

    public void Drop(PlayerController player)
    {
        // Re-enable components
        GetComponent<BoxCollider>().enabled = true;
        if (PhotonManager.Instance.IsMasterClient) GetComponent<Rigidbody>().isKinematic = false;
        transform.SetParent(null);

        player.GrabbedObj = null;
    }
}
