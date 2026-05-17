using FixRush;
using System;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Trigger : MonoBehaviour, IInteractable
{
    [Header("Prefab References")]
    [SerializeField] SphereCollider _sphereCollider;
    [SerializeField] private IInteractable.Type _interactionType = IInteractable.Type.Simple;
    [Tooltip("If true, the interaction can be shared among multiple players. If false, only one player can interact with it at a time.")]
    [SerializeField] private bool  _shared   = false;
    [SerializeField] private float _holdTime = 2f;

    private Action<PlayerController, Trigger> _onInteractionStarted;
    private Action<PlayerController, Trigger> _onInteracted;
    private Action<PlayerController, Trigger> _onInteractionCanceled;
    private Action<Trigger> _onDestroy;
    public IInteractable.Type InteractionType => _interactionType;
    public float HoldTime => _holdTime;
    public bool Shared => _shared;

    private void OnDestroy()
    {
        _onDestroy?.Invoke(this);
    }

    /// <summary>
    /// Sets the trigger's properties. This is used to initialize the trigger after instantiating it, since we can't set these properties in the prefab.
    /// </summary>
    /// <param name="radius">The radius of the trigger's sphere collider.</param>
    /// <param name="holdTime">The time required to hold the interaction.</param>
    /// <param name="onInteracted">The action to perform when the trigger is interacted with.</param>
    /// <param name="interactionType">The type of interaction.</param>
    /// <returns>The initialized trigger.</returns>
    public Trigger Set(
        float radius, float holdTime, bool shared, 
        Action<PlayerController, Trigger> onInteracted, 
        IInteractable.Type interactionType = IInteractable.Type.Simple)
    {
        _shared                = shared;
        _holdTime              = holdTime;
        _onInteracted          = onInteracted;
        _interactionType       = interactionType;
        _sphereCollider.radius = radius;

        return this;
    }
    public Trigger Set(
        float radius, float holdTime, bool shared, 
        Action<PlayerController, Trigger> onInteracted, 
        Action<PlayerController, Trigger> onInteractionCanceled, 
        IInteractable.Type interactionType = IInteractable.Type.Simple)
    {
        _shared                = shared;
        _holdTime              = holdTime;
        _onInteracted          = onInteracted;
        _interactionType       = interactionType;
        _sphereCollider.radius = radius;
        _onInteractionCanceled = onInteractionCanceled;

        return this;
    }
    public Trigger Set(
        float radius, float holdTime, bool shared, 
        Action<PlayerController, Trigger> onInteractionStarted, 
        Action<PlayerController, Trigger> onInteracted, 
        Action<PlayerController, Trigger> onInteractionCanceled, 
        IInteractable.Type interactionType = IInteractable.Type.Simple)
    {
        _shared                = shared;
        _holdTime              = holdTime;
        _onInteracted          = onInteracted;
        _interactionType       = interactionType;
        _sphereCollider.radius = radius;
        _onInteractionStarted  = onInteractionStarted;
        _onInteractionCanceled = onInteractionCanceled;

        return this;
    }
    public void OnDestroyTrigger(Action<Trigger> onDestroyTrigger)
    {
        _onDestroy = onDestroyTrigger;
    }

    public void Interact(PlayerController player) { _onInteracted?.Invoke(player, this); }

    public void CancelInteraction(PlayerController player) { _onInteractionCanceled?.Invoke(player, this); }

    public void InteractionStarted(PlayerController player) { _onInteractionStarted?.Invoke(player, this); }
}
