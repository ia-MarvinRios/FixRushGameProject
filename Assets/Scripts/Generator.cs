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

        photonView.RPC(
            nameof(RPC_RequestSpawnAndPickUp), 
            RpcTarget.MasterClient,
            player.GetComponent<PhotonView>().ViewID
        );
    }

    #region RPCs

    [PunRPC]
    private void RPC_RequestSpawnAndPickUp(int playerViewID)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Instantiate the object on the Master Client
            GameObject obj = PhotonNetwork.Instantiate(_prefab.name, transform.position, Quaternion.identity);
            int objectViewID = obj.GetComponent<PhotonView>().ViewID;

            // Queue object destruction
            GarbajeCollector.Instance.QueueDestruction(obj, _destructionDelay);

            // Send RPC
            photonView.RPC(
                nameof(RPC_GetObject),
                RpcTarget.All,
                playerViewID,
                objectViewID
            );
        }
    }

    [PunRPC]
    private void RPC_GetObject(int playerViewID, int objectViewID)
    {
        PhotonView playerView = PhotonView.Find(playerViewID);
        if (!playerView.IsMine) { return; }

        PlayerController player = playerView.GetComponent<PlayerController>();
        GameObject obj = PhotonView.Find(objectViewID).gameObject;

        if (player != null && obj != null)
        {
            player.PickUpObject(obj);
        }
    }

    #endregion
}
