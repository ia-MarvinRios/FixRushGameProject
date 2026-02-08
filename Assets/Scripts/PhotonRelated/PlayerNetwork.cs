using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerNetwork : MonoBehaviourPun
{
    public bool IsLocalPlayer => photonView.IsMine || PhotonNetwork.OfflineMode;
    public bool IsOfflineMode => PhotonNetwork.OfflineMode;

    internal void CleanupNetworkState()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        PhotonNetwork.RemoveBufferedRPCs(
            photonView.ViewID,
            "RPC_AttachPlayer"
        );
    }

    internal GameObject[] SpawnPlayer(GameObject prefab, Vector3[] positions)
    {
        GameObject[] models;

        if (PhotonNetwork.OfflineMode)
        {
            Debug.Log("[PlayerNetwork] Spawning local player models...");

            GameObject a = 
                PhotonNetwork.Instantiate(
                    prefab.name,
                    positions[0], 
                    Quaternion.identity, 
                    0, 
                    null);

            GameObject b = 
                PhotonNetwork.Instantiate(
                    prefab.name,
                    positions[1], 
                    Quaternion.identity, 
                    0, 
                    null);

            models = new GameObject[2] { a, b };
        }
        else
        {
            Debug.Log("[PlayerNetwork] Spawning player over network...");

            GameObject playerObj = 
                PhotonNetwork.Instantiate(
                    prefab.name,
                    positions[0], 
                    Quaternion.identity, 
                    0, 
                    null);

            models = new GameObject[1] { playerObj };
        }
        
        return models;

    }

    internal void AttachPlayer(GameObject child)
    {
        PhotonView childPhotonView = child.GetComponent<PhotonView>();
        PhotonView parentPhotonView = GetComponent<PhotonView>();

        photonView.RPC(
            "RPC_AttachPlayer",
            RpcTarget.AllBuffered,
            childPhotonView.ViewID,
            parentPhotonView.ViewID
        );
    }

    #region RPCs

    [PunRPC]
    void RPC_AttachPlayer(int childViewID, int parentViewID)
    {
        PhotonView child = PhotonView.Find(childViewID);
        PhotonView parent = PhotonView.Find(parentViewID);

        if (child == null || parent == null) return;

        child.transform.SetParent(parent.transform, true);
    }

    #endregion
}
