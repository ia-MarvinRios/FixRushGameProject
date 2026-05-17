using FixRush;
using UnityEngine;

/// <summary>
/// A class representing a pickable object in the game.
/// Inherits from MonoBehaviour and implements IInteractable and IPickupable interfaces to allow interaction and pickup functionality.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Pickable : MonoBehaviour, IInteractable, IPickupable
{
    [Header("Interaction Settings")]
    [SerializeField] private IInteractable.Type _interactionType = IInteractable.Type.Simple;
    [Tooltip("If true, the interaction can be shared among multiple players. If false, only one player can interact with it at a time.")]
    [SerializeField] private bool _shared = false;
    [SerializeField] private float _holdTime = 2f;
    [Header("Pickable Settings")]
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private Collider _collider;

    public IInteractable.Type InteractionType => _interactionType;
    public bool Shared => _shared;
    public float HoldTime => _holdTime;

    /// <summary>
    /// Interaction method that allows the specified player to pick up this object. This method is called when the player interacts
    /// with the object.
    /// </summary>
    /// <param name="player">The player who is interacting with the object.</param>
    public virtual void Interact(PlayerController player)
    {
        player.PickUpObject(gameObject);
    }

    /// <summary>
    /// Cancel interaction method that can be overridden by derived classes to provide specific behavior when a player cancels an interaction with this object.
    /// </summary>
    /// <param name="player">The player who is canceling the interaction.</param>
    public virtual void CancelInteraction(PlayerController player)
    {
        return;
    }

    public virtual void InteractionStarted(PlayerController player)
    {
        return;
    }

    /// <summary>
    /// Allows the specified player to pick up this object, attaching it to the player's grab point and updating
    /// relevant references.
    /// </summary>
    /// <remarks>If the player is already holding another object, that object will be dropped before this one
    /// is picked up. The object is parented to the player's grab root, and its collider is disabled to prevent further
    /// interactions while held. Only the master client sets the object's Rigidbody to kinematic to avoid network
    /// conflicts.</remarks>
    /// <param name="player">The player who will pick up the object. Cannot be null and must have a valid grab point.</param>
    public virtual void PickUp(PlayerController player)
    {
        if (player.GrabbedObj != null)
        {
            player.DropObject(player.GrabbedObj);
        }

        // --- Disable some components ---
        // Collider
        _collider.enabled = false;

        // Rigidbody (only on master client to avoid conflicts)
        if (PhotonManager.Instance.IsMasterClient && _rigidbody != null)
        {
            _rigidbody.isKinematic = true;
        }

        // --- Parent to player ---
        transform.SetParent(player.ObjRoot);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // --- Set reference in player ---
        player.GrabbedObj = gameObject;
    }

    /// <summary>
    /// Drops the object, re-enabling components and unparenting it from the player.
    /// </summary>
    /// <param name="player">The player who is dropping the object.</param>
    public virtual void Drop(PlayerController player)
    {
        // --- Re-enable components ---
        // Collider
        _collider.enabled = true;

        // Rigidbody (only on master client to avoid conflicts)
        if (PhotonManager.Instance.IsMasterClient && _rigidbody != null)
        {
            _rigidbody.isKinematic = false;
            _rigidbody.linearVelocity = Vector3.zero;
        }

        // --- Unparent from player ---
        transform.SetParent(null);

        // --- Clear reference in player ---
        player.GrabbedObj = null;
    }

    internal void DisablePicking()
    {
        // --- Disable some components ---
        // Collider
        _collider.enabled = false;

        // Rigidbody (only on master client to avoid conflicts)
        if (PhotonManager.Instance.IsMasterClient && _rigidbody != null)
        {
            _rigidbody.isKinematic = true;
        }
    }

    internal void EnablePicking()
    {
        // --- Re-enable components ---
        // Collider
        _collider.enabled = true;

        // Rigidbody (only on master client to avoid conflicts)
        if (PhotonManager.Instance.IsMasterClient && _rigidbody != null)
        {
            _rigidbody.isKinematic = false;
            _rigidbody.linearVelocity = Vector3.zero;
        }
    }

    private void OnValidate()
    {
        // Ensure that the Rigidbody and Collider references are assigned in the inspector,
        // or try to get them from the GameObject if they are not assigned.
        if (_rigidbody == null)
        {
            TryGetComponent(out _rigidbody);
        }
        if (_collider == null)
        {
            TryGetComponent(out _collider);
        }
    }
}
