using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IInteractable
{
    public void OnInteracted(AuxPlayer entity);
}

public class Interactable : MonoBehaviour
{
    [Header("Interaction Settings")]
    public bool Active = true;
    [Tooltip("Simple: Instant interaction.\n Hold: Requires holdtime to be completed.\n Still: Will be performed only if the player is in range and canceled if not.")]
    public InteractionType InteractionType;
    [Tooltip("Waiting time to complete a hold interaction. Ignored if the action is set to simple type.")]
    [Range(0, 20)] public float HoldTime = 0;
    [Space(20)]
    [SerializeField] UnityEvent<AuxPlayer> _onInteract;

    Pickable _p;

    public Pickable Pickable => _p;

    private void Awake()
    {
        if (TryGetComponent(out _p)) return;
    }

    public void SetInteractable(InteractionType interactionType, float holdTime)
    {
        InteractionType = interactionType;
        HoldTime = holdTime;
    }
    public void SetInteractable(InteractionType interactionType, float holdTime, float triggerRadius = 1f)
    {
        InteractionType = interactionType;
        HoldTime = holdTime;

        SphereCollider c = gameObject.AddComponent<SphereCollider>();
        c.isTrigger = true;
        c.radius = triggerRadius;
    }

    public void Interact(AuxPlayer entity)
    {
        if (!Active) return;

        if (_p != null)
            _p.RequestPickUpObj(entity);

        foreach (var i in GetComponentsInParent<IInteractable>())
            i.OnInteracted(entity);
        
        _onInteract?.Invoke(entity);
    }
}
