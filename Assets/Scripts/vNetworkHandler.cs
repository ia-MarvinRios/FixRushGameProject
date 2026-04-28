using Photon.Pun;
using UnityEngine;
using FixRush;

public class vNetworkHandler : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        // get th int issueIDs from the instantiation data int array
        IIssue.Type[] Issues;

        object[] data = photonView.InstantiationData;
        if (data == null || data.Length == 0)
            return;

        int[] issueIds = (int[])data[0];
        Issues = new IIssue.Type[issueIds.Length];

        for (int i = 0; i < issueIds.Length; i++)
        {
            Issues[i] = (IIssue.Type)issueIds[i];
        }

        Vehicle v = GetComponent<Vehicle>();
        v.IssueTypes = Issues;
        //v.IsFixed = (bool)data[1];
    }

    public void MoveCarToEndPoint(Vehicle v)
    {
        photonView.RPC(nameof(RPC_MoveCarToEndPoint),
            RpcTarget.MasterClient,
            v.GetComponent<PhotonView>().ViewID
        );
    }
    internal void SyncDirt()
    {
        photonView.RPC(
            nameof(RPC_SyncDirt),
            RpcTarget.All,
            photonView.ViewID
        );
    }

    internal void SyncDirtAlpha(float value)
    {
        photonView.RPC(
            nameof(RPC_SyncDirtAlpha),
            RpcTarget.All,
            photonView.ViewID,
            value
        );
    }

    #region RPCs

    [PunRPC]
    void RPC_MoveCarToEndPoint(int vehicleViewID)
    {
        if (!PhotonNetwork.IsMasterClient) { return; }

        // Find vehicle
        Vehicle v = PhotonView.Find(vehicleViewID).GetComponent<Vehicle>();

        // Move it to the end point
        VManager.Instance.MoveToEndPoint(v);
    }
    [PunRPC]
    void RPC_SyncDirt(int vehicleViewID)
    {
        Vehicle vehicle = PhotonView.Find(vehicleViewID).GetComponent<Vehicle>();

        vehicle.SetupDirt();
    }
    [PunRPC]
    void RPC_SyncDirtAlpha(int vehicleViewID, float value)
    {
        Vehicle vehicle = PhotonView.Find(vehicleViewID).GetComponent<Vehicle>();

        vehicle.DirtAlpha = value;
    }

    #endregion
}
