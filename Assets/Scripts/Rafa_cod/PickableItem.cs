using Photon.Pun;
using UnityEngine;

public class PickableItem : Pickable
{
    [Header("Item Data")]
    [SerializeField] private ItemData _itemData;

    [Header("Interaction")]
    [SerializeField] private Collider _triggerCollider;

    public ItemData ItemData => _itemData;

    // -----------------------------------------------------------------------
    #region PICKABLE

    public override void PickUp(PlayerController player)
    {
        base.PickUp(player);

        if (_triggerCollider != null)
            _triggerCollider.enabled = false;

        PhotonView playerView = player.GetComponent<PhotonView>();

        if (playerView != null)
            photonView.RPC("RPC_OnPickedUp", RpcTarget.OthersBuffered, playerView.ViewID);
    }

    public override void Drop(PlayerController player)
    {
        base.Drop(player);

        if (_triggerCollider != null)
            _triggerCollider.enabled = true;

        photonView.RPC("RPC_OnDropped", RpcTarget.OthersBuffered);
    }

    #endregion

    // -----------------------------------------------------------------------
    #region NETWORK VISUAL SYNC

    [PunRPC]
    private void RPC_OnPickedUp(int playerViewID)
    {
        PhotonView playerView = PhotonView.Find(playerViewID);
        if (playerView == null) return;

        PlayerController player = playerView.GetComponent<PlayerController>();
        if (player == null) return;

        if (_triggerCollider != null)
            _triggerCollider.enabled = false;

        transform.SetParent(player.ObjRoot);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (TryGetComponent(out Rigidbody rb))
            rb.isKinematic = true;
    }

    [PunRPC]
    private void RPC_OnDropped()
    {
        transform.SetParent(null);

        if (_triggerCollider != null)
            _triggerCollider.enabled = true;

        if (TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
        }
    }

    #endregion

    // -----------------------------------------------------------------------
    #region CONSUME

    public void Consume()
    {
        ItemSpawner.NotifyPickedUp(_itemData);

        if (PhotonNetwork.InRoom)
            PhotonNetwork.Destroy(gameObject);
        else
            Destroy(gameObject);
    }

    #endregion
}