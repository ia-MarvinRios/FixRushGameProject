using FixRush;
using UnityEngine;

public class Manual : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private IInteractable.Type _interactionType = IInteractable.Type.Simple;
    [SerializeField] private float _holdTime = 2f;

    public IInteractable.Type InteractionType => _interactionType;
    public float HoldTime => _holdTime;

    public void Interact(PlayerController player)
    {
        
        InGameUI.Instance.Manual.SetActive(true);

    }
}

