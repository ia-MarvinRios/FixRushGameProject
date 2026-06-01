using Photon.Pun;
using UnityEngine;

public class PickableItem : Pickable
{
    [Header("Item Data")]
    [SerializeField] private ItemData _itemData;
    [SerializeField] private Collider _triggerCollider; // ← asigna el SphereCollider trigger

    public ItemData ItemData => _itemData;

    public override void PickUp(PlayerController player)
    {
        base.PickUp(player);

        // Desactivar trigger para que no interfiera con el focus
        if (_triggerCollider != null)
            _triggerCollider.enabled = false;
    }

    public void Consume()
    {
        ItemSpawner.NotifyPickedUp(_itemData);

        if (PhotonNetwork.InRoom)
            PhotonNetwork.Destroy(gameObject);
        else
            Destroy(gameObject);
    }
}