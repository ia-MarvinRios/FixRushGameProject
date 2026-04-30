using FixRush;
using UnityEngine;

public class Manual : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private IInteractable.Type _interactionType = IInteractable.Type.Simple;
    [SerializeField] private float _holdTime = 2f;

    private PlayerController _pcCache;

    public IInteractable.Type InteractionType => _interactionType;
    public float HoldTime => _holdTime;

    public void Interact(PlayerController player)
    {
        _pcCache = player;
        player.ShowWorkParticle(true);
        InGameUI.Instance.Manual.SetActive(true);

    }

    public void HideParticles()
    {
        if (_pcCache == null) { return; }
        _pcCache.ShowWorkParticle(false);
        _pcCache = null;
    }
}

