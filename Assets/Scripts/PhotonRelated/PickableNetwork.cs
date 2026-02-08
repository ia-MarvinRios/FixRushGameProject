using Photon.Pun;
using UnityEngine;

public class PickableNetwork : MonoBehaviourPun
{
    public void RequestPickUp(AuxPlayer player)
    {
        photonView.RPC(nameof(RPC_PickUp), RpcTarget.All, player.PhotonView.ViewID);
    }
    public void RequestDrop(AuxPlayer player)
    {
        photonView.RPC(nameof(RPC_Drop), RpcTarget.All, player.PhotonView.ViewID);
    }

    [PunRPC]
    void RPC_PickUp(int playerViewID)
    {
        var player = PhotonView.Find(playerViewID).GetComponent<AuxPlayer>();
        GetComponent<Pickable>().PickUp(player);
    }
    [PunRPC]
    void RPC_Drop(int playerViewID)
    {
        var player = PhotonView.Find(playerViewID).GetComponent<AuxPlayer>();
        GetComponent<Pickable>().Drop();
    }
}
