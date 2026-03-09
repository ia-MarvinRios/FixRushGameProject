using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class VehicleNetwork : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    internal bool IsMaster => PhotonNetwork.IsMasterClient;

    internal void CleanupNetworkState()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        PhotonNetwork.RemoveBufferedRPCs(
            photonView.ViewID,
            nameof(RPC_AttachVehicle)
        );
        PhotonNetwork.RemoveBufferedRPCs(
            photonView.ViewID,
            nameof(RPC_InitializeVehicle)
        );
    }

    void IPunInstantiateMagicCallback.OnPhotonInstantiate(PhotonMessageInfo info)
    {
        IssueType[] Issues;

        object[] data = photonView.InstantiationData;
        if (data == null || data.Length == 0)
            return;

        int[] issueIds = (int[])data[0];
        Issues = new IssueType[issueIds.Length];

        for (int i = 0; i < issueIds.Length; i++)
        {
            Issues[i] = (IssueType)issueIds[i];
        }

        Vehicle v = GetComponent<Vehicle>();
        v.Issues = Issues;
        v.IsFixed = (bool)data[1];
    }

    internal void AttachVehicle(GameObject child, GameObject parent)
    {
        PhotonView childPhotonView = child.GetComponent<PhotonView>();
        PhotonView parentPhotonView = parent.GetComponent<PhotonView>();

        photonView.RPC(
            nameof(RPC_AttachVehicle),
            RpcTarget.AllBuffered,
            childPhotonView.ViewID,
            parentPhotonView.ViewID
        );
    }

    internal void InitializeVehicle()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        photonView.RPC(
            nameof(RPC_InitializeVehicle),
            RpcTarget.AllBuffered
        );
    }

    internal void RequestResolveDirtIssue(Vehicle v)
    {
        photonView.RPC(
            nameof(RPC_RepairDirtIssue),
            RpcTarget.AllBuffered,
            v.GetComponent<PhotonView>().ViewID
        );
    }

    public void MoveCarToEndPoint(Vehicle v)
    {
        photonView.RPC(nameof(RPC_MoveCarToEndPoint),
            RpcTarget.MasterClient,
            v.GetComponent<PhotonView>().ViewID
        );
    }

    #region RPCs

    [PunRPC]
    void RPC_AttachVehicle(int childViewID, int parentViewID)
    {
        PhotonView child = PhotonView.Find(childViewID);
        PhotonView parent = PhotonView.Find(parentViewID);

        if (child == null || parent == null) return;

        child.transform.SetParent(parent.transform, true);
    }
    [PunRPC]
    void RPC_InitializeVehicle()
    {
        gameObject.GetComponent<Vehicle>().Initialize();
    }
    [PunRPC]
    void RPC_RepairDirtIssue(int vehicleViewID)
    {
        Vehicle v = PhotonView.Find(vehicleViewID).GetComponent<Vehicle>();

        DirtIssue i = v.CurrentIssue as DirtIssue;
        if (i == null) return;
        i.ResolveDirtIssue();
    }

    [PunRPC]
    void RPC_MoveCarToEndPoint(int vPhotonView)
    {
        if (!IsMaster) return;

        Vehicle v = PhotonView.Find(vPhotonView).GetComponent<Vehicle>();

        VManager.Instance.MoveToEndPoint(v);
    }

    #endregion
}
