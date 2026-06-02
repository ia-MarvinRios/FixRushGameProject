using FixRush;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class NpcShop : MonoBehaviourPun, AIExtension.IQueueAgent, IInteractable
{
    [Header("Componentes")]
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private BoxCollider _collider;

    [Header("Request UI")]
    [SerializeField] private GameObject _requestPanel;
    [SerializeField] private Image _requestIcon;

    [Header("Reward")]
    [SerializeField] private int _deliveryFee = 50;

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
    #region REQUEST SETUP

    public void AssignRequest(ItemData item, int itemIndex)
    {
        photonView.RPC("RPC_AssignRequest", RpcTarget.AllBuffered, itemIndex);
    }

    [PunRPC]
    private void RPC_AssignRequest(int itemIndex)
    {
        RequestedItem = NpcManager.Instance.GetItemByIndex(itemIndex);

        HideRequest();

        Debug.Log($"[NpcShop] NPC solicitando: {RequestedItem?.ItemName}");
    }

    public void SetAtCounter(bool value)
    {
        photonView.RPC("RPC_SetAtCounter", RpcTarget.AllBuffered, value);
    }

    [PunRPC]
    private void RPC_SetAtCounter(bool value)
    {
        _isAtCounter = value;

        if (_isAtCounter && RequestedItem != null)
            ShowRequest(RequestedItem);
        else
            HideRequest();
    }

    #endregion

    // -----------------------------------------------------------------------
    #region IINTERACTABLE

    public void InteractionStarted(PlayerController player) { }

    public void Interact(PlayerController player)
    {
        if (!_isAtCounter)
        {
            Debug.Log("[NpcShop] NPC no esta en el counter todavia.");
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

        PhotonView playerView = player.GetComponent<PhotonView>();
        PhotonView itemView = heldItem.GetComponent<PhotonView>();

        if (playerView == null || itemView == null)
        {
            Debug.LogWarning("[NpcShop] Falta PhotonView en player o item.");
            return;
        }

        photonView.RPC(
            "RPC_TryReceiveItem",
            RpcTarget.MasterClient,
            playerView.ViewID,
            itemView.ViewID
        );
    }

    public void CancelInteraction(PlayerController player) { }

    #endregion

    // -----------------------------------------------------------------------
    #region ITEM DELIVERY

    [PunRPC]
    private void RPC_TryReceiveItem(int playerViewID, int itemViewID)
    {
        PhotonView playerView = PhotonView.Find(playerViewID);
        PhotonView itemView = PhotonView.Find(itemViewID);

        if (playerView == null || itemView == null)
            return;

        PlayerController player = playerView.GetComponent<PlayerController>();
        PickableItem item = itemView.GetComponent<PickableItem>();

        if (player == null || item == null)
            return;

        bool accepted = TryReceiveItem(player, item);

        if (!accepted)
            return;

        if (PhotonNetwork.IsMasterClient)
            GameManager.Instance.AddCashMaster(_deliveryFee);

        NpcManager.Instance.MoveNpcToEndPoint(this);
    }

    public bool TryReceiveItem(PlayerController player, PickableItem item)
    {
        if (item == null || item.ItemData == null) return false;
        if (RequestedItem == null) return false;

        if (item.ItemData != RequestedItem)
        {
            Debug.Log($"[NpcShop] Incorrecto. Pedia '{RequestedItem.ItemName}', recibio '{item.ItemData.ItemName}'.");
            return false;
        }

        player.DropObject(player.GrabbedObj);
        item.Consume();
        OnServed();

        return true;
    }

    public void OnServed()
    {
        photonView.RPC("RPC_OnServed", RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void RPC_OnServed()
    {
        _isAtCounter = false;
        HideRequest();

        Debug.Log("[NpcShop] NPC atendido!");
    }

    #endregion

    // -----------------------------------------------------------------------
    #region UI

    private void ShowRequest(ItemData item)
    {
        if (_requestPanel != null)
            _requestPanel.SetActive(true);

        if (_requestIcon != null && item != null)
            _requestIcon.sprite = item.Icon;
    }

    private void HideRequest()
    {
        if (_requestPanel != null)
            _requestPanel.SetActive(false);
    }

    #endregion
}