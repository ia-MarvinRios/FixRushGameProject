using UnityEngine;
using Photon.Pun;

public class Generator : Interactable
{
    [Header("Generator Settings")]
    [SerializeField] private GameObject _prefab;

    [Header("Destruction")]
    [SerializeField] private float _destructionDelay = 10f;

    public override void Interact(PlayerController player)
    {
        if (_prefab == null)
        {
            Debug.LogWarning($"[Generator({name})] Prefab not assigned for Generator.");
            return;
        }

        GameObject obj = PhotonNetwork.InstantiateRoomObject(_prefab.name, transform.position, Quaternion.identity);

        // Ask Master to queue object destruction
        photonView.RPC(
            nameof(RPC_QueueDestruction), 
            RpcTarget.MasterClient, 
            obj.GetComponent<PhotonView>().ViewID,
            _destructionDelay
        );

        player.PickUpObject(obj);
    }

    #region RPCs

    [PunRPC]
    private void RPC_QueueDestruction(int objectViewID, float delay)
    {
        GameObject obj = PhotonView.Find(objectViewID)?.gameObject;

        GarbajeCollector.Instance.QueueDestruction(obj, delay);
    }

    #endregion
}
