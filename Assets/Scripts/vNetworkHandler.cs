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

    internal void SyncInitialization()
    {
        photonView.RPC(
            nameof(RPC_SyncInitialization), 
            RpcTarget.Others,
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

    internal void SyncWashCounterDirtIssue(int value)
    {
        photonView.RPC(
            nameof(RPC_SyncWashCounterDirtIssue),
            RpcTarget.All,
            photonView.ViewID,
            value
        );
    }

    internal void RequestResolveDirtIssue(Vehicle v)
    {
        photonView.RPC(
            nameof(RPC_RepairDirtIssue),
            RpcTarget.All,
            v.GetComponent<PhotonView>().ViewID
        );
    }

    internal void SyncSuds(bool show, float duration = 0f)
    {
        photonView.RPC(
            nameof(RPC_SyncSuds),
            RpcTarget.All,
            photonView.ViewID,
            show,
            duration
        );
    }

    internal void SetActiveObject(GameObject obj, bool active)
    {
        if (obj.TryGetComponent(out PhotonView objPhotonView))
        {
            photonView.RPC(
                nameof(RPC_SetObjectActive),
                RpcTarget.All,
                objPhotonView.ViewID,
                active,
                ""
            );
        }
    }

    internal void SetActiveChildren(GameObject parent, bool active)
    {
        if (parent.TryGetComponent(out PhotonView objPhotonView))
        {
            photonView.RPC(
                nameof(RPC_SetObjectActive),
                RpcTarget.All,
                objPhotonView.ViewID,
                active,
                "*all"
            );
        }
    }

    internal void SetActiveChildren(GameObject parent, string childrenName, bool active)
    {
        if (parent.TryGetComponent(out PhotonView objPhotonView))
        {
            photonView.RPC(
                nameof(RPC_SetObjectActive),
                RpcTarget.All,
                objPhotonView.ViewID,
                active,
                childrenName
            );
        }
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
    void RPC_SyncInitialization(int vehicleViewID)
    {
        Vehicle vehicle = PhotonView.Find(vehicleViewID).GetComponent<Vehicle>();

        vehicle.InitializeVehicle();
    }
    [PunRPC]
    void RPC_SyncDirtAlpha(int vehicleViewID, float value)
    {
        Vehicle vehicle = PhotonView.Find(vehicleViewID).GetComponent<Vehicle>();

        vehicle.DirtAlpha = value;
    }
    [PunRPC]
    void RPC_SyncSuds(int vehicleViewID, bool show, float duration)
    {
        Vehicle vehicle = PhotonView.Find(vehicleViewID).GetComponent<Vehicle>();
        if (show)
        {
            vehicle.ShowSuds(duration);
        }
        else
        {
            vehicle.HideSuds();
        }
    }
    [PunRPC]
    private void RPC_SyncWashCounterDirtIssue(int vehicleViewID, int value)
    {
        Vehicle vehicle = PhotonView.Find(vehicleViewID).GetComponent<Vehicle>();

        DirtIssue dirtIssue = (DirtIssue)vehicle.CurrentIssue;

        if (dirtIssue != null)
        {
            dirtIssue.WashCounter = value;
        }
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
    void RPC_SetObjectActive(int objectViewID, bool active, string childrenName)
    {
        PhotonView pv = PhotonView.Find(objectViewID);

        if (pv == null) { return; }

        GameObject obj = pv.gameObject;

        // No child specified affect root object
        if (string.IsNullOrEmpty(childrenName))
        {
            obj.SetActive(active);
            return;
        }

        // Affect all direct children
        if (childrenName == "*all")
        {
            foreach (Transform child in obj.transform)
            {
                child.gameObject.SetActive(active);
            }

            return;
        }

        // Affect specific child
        foreach (Transform child in obj.transform)
        {
            if (child.name == childrenName)
            {
                child.gameObject.SetActive(active);
                return;
            }
        }
    }

    #endregion
}
