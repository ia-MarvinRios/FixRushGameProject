using FixRush;
using UnityEngine;

public class Manual : MonoBehaviour, IInteractable
{

    public IInteractable.Type InteractionType => IInteractable.Type.Simple;

    public float HoldTime => 0;

    public void Interact(PlayerController player)
    {
        
        InGameUI.Instance.Manual.SetActive(true);

    }
}

