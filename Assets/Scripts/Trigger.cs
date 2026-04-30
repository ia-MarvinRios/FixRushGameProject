using FixRush;
using System;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Trigger : MonoBehaviour, IInteractable
{
    [Header("Prefab References")]
    [SerializeField] SphereCollider _sphereCollider;
    [SerializeField] private IInteractable.Type _interactionType = IInteractable.Type.Simple;
    [SerializeField] private float _holdTime = 2f;

    private Action _onInteracted;

    public IInteractable.Type InteractionType => _interactionType;
    public float HoldTime => _holdTime;

    public Trigger Set(float radius, float holdTime, Action onInteracted, IInteractable.Type interactionType = IInteractable.Type.Simple)
    {
        _holdTime = holdTime;
        _interactionType = interactionType;
        _sphereCollider.radius = radius;
        _onInteracted = onInteracted;

        return this;
    }

    public void Interact(PlayerController player)
    {
        _onInteracted?.Invoke();
    }
}
