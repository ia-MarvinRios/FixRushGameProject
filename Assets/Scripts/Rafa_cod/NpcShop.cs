using FixRush;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

/// <summary>
/// NPC de la tienda.
/// - Muestra panel UI solo cuando está en el counter
/// - Implementa IInteractable para recibir el item del jugador
/// </summary>
public class NpcShop : MonoBehaviourPun, AIExtension.IQueueAgent, IInteractable
{
    [Header("Componentes")]
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private BoxCollider _collider;

    [Header("Request UI")]
    [SerializeField] private GameObject _requestPanel;
    [SerializeField] private Image _requestIcon;

    // --- IQueueAgent ---
    public int QueueIndex { get; set; }
    public Vector3 Size => _collider != null ? _collider.size : Vector3.one;
    public NavMeshAgent Agent => _agent;
    public ItemData RequestedItem { get; private set; }

    // --- IInteractable ---
    public IInteractable.Type InteractionType => IInteractable.Type.Simple;
    public float HoldTime => 0f;
    public bool Shared => false;

    private bool _isAtCounter = false;

    // -----------------------------------------------------------------------
    #region UNITY

    private void Awake()
    {
        if (_agent == null) _agent = GetComponent<NavMeshAgent>();
        if (_collider == null) _collider = GetComponent<BoxCollider>();

        HideRequest();
    }

    #endregion

    // -----------------------------------------------------------------------
    #region SETUP

    public void AssignRequest(ItemData item)
    {
        RequestedItem = item;
        HideRequest(); // ✅ Oculto hasta llegar al counter
        Debug.Log($"[NpcShop] NPC solicitando: {item?.ItemName}");
    }

    /// <summary>
    /// NpcManager llama esto cuando el NPC llega al counter.
    /// </summary>
    public void SetAtCounter(bool value)
    {
        _isAtCounter = value;

        // ✅ Panel solo visible cuando está en el counter
        if (value && RequestedItem != null)
            ShowRequest(RequestedItem);
        else
            HideRequest();
    }

    #endregion

    // -----------------------------------------------------------------------
    #region IINTERACTABLE

    public void InteractionStarted(PlayerController player) { }

    /// <summary>
    /// El jugador interactúa con el NPC mientras lleva el item.
    /// </summary>
    public void Interact(PlayerController player)
    {
        Debug.Log($"[NpcShop] Interact llamado! isAtCounter: {_isAtCounter}, GrabbedObj: {player.GrabbedObj?.name}");
        if (!_isAtCounter) return;

        if (!_isAtCounter)
        {
            Debug.Log("[NpcShop] NPC no está en el counter todavía.");
            return;
        }

        if (player.GrabbedObj == null)
        {
            Debug.Log("[NpcShop] El jugador no lleva nada.");
            return;
        }

        if (!player.GrabbedObj.TryGetComponent(out PickableItem heldItem))
        {
            Debug.Log("[NpcShop] El objeto no es un PickableItem.");
            return;
        }

        bool accepted = TryReceiveItem(player, heldItem);

        if (accepted)
            NpcManager.Instance.MoveNpcToEndPoint(this);
    }

    public void CancelInteraction(PlayerController player) { }

    #endregion

    // -----------------------------------------------------------------------
    #region ITEM DELIVERY

    public bool TryReceiveItem(PlayerController player, PickableItem item)
    {
        if (item == null || item.ItemData == null) return false;
        if (RequestedItem == null) return false;

        if (item.ItemData != RequestedItem)
        {
            Debug.Log($"[NpcShop] Incorrecto. Pedía '{RequestedItem.ItemName}', recibió '{item.ItemData.ItemName}'.");
            return false;
        }

        // ✅ Soltar del jugador primero, luego consumir
        player.DropObject(player.GrabbedObj);
        item.Consume();

        OnServed();
        return true;
    }

    public void OnServed()
    {
        HideRequest();
        SetAtCounter(false);
        Debug.Log("[NpcShop] NPC atendido!");
    }

    #endregion

    // -----------------------------------------------------------------------
    #region UI

    private void ShowRequest(ItemData item)
    {
        if (_requestPanel != null) _requestPanel.SetActive(true);
        if (_requestIcon != null && item.Icon != null) _requestIcon.sprite = item.Icon;
    }

    private void HideRequest()
    {
        if (_requestPanel != null) _requestPanel.SetActive(false);
    }

    #endregion
}