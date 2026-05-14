using FixRush;
using Photon.Pun;
using UnityEngine;

/// <summary>
/// A class representing an interactable object in the game. This class implements the IInteractable interface and provides basic functionality for interaction, 
/// such as holding time and shared interactions. It can be extended by other classes to provide specific interaction behavior.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Interactable : MonoBehaviourPun, IInteractable
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
    public virtual void Interact(PlayerController player) { }

    /// <summary>
    /// Cancel interaction method that can be overridden by derived classes to provide specific behavior when a player cancels an interaction with this object.
    /// </summary>
    /// <param name="player">The player who is canceling the interaction.</param>
    public virtual void CancelInteraction(PlayerController player) { }

    public virtual void InteractionStarted(PlayerController player) { }

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
