using FixRush;
using Photon.Pun;
using UnityEngine;

public class vNetworkHandler : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    public float PatienceSliderStep;

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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Enviar datos
            stream.SendNext(PatienceSliderStep);
        }
        else
        {
            // Recibir datos
            PatienceSliderStep = (float)stream.ReceiveNext();
        }
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

    internal void SyncJackUnjackCar(PlayerController player, bool jack)
    {
        photonView.RPC(
            nameof(RPC_JackUnjackCar),
            RpcTarget.Others,
            photonView.ViewID,
            player.GetComponent<PhotonView>().ViewID,
            jack
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

    internal void SyncWashDirtIssue()
    {
        photonView.RPC(
            nameof(RPC_WashDirtIssue),
            RpcTarget.All,
            photonView.ViewID
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

    internal void DestroyChildren(GameObject parent, string childrenName)
    {
        if (parent.TryGetComponent(out PhotonView objPhotonView))
        {
            photonView.RPC(
                nameof(RPC_DestroyChildren),
                RpcTarget.All,
                objPhotonView.ViewID,
                childrenName
            );
        }
    }

    internal void SetSafeDestruction(bool safeDestruction)
    {
        photonView.RPC(
            nameof(RPC_SafeDestruction),
            RpcTarget.All,
            photonView.ViewID,
            safeDestruction
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
    private void RPC_JackUnjackCar(int carViewID, int playerViewID, bool jack)
    {
        Car car = PhotonView.Find(carViewID).GetComponent<Car>();
        PlayerController player = PhotonView.Find(playerViewID).GetComponent<PlayerController>();

        if (jack)
        {
            car.JackCar(player);
            return;
        }

        car.UnjackCar(player);
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
    private void RPC_WashDirtIssue(int vehicleViewID)
    {
        Vehicle vehicle = PhotonView.Find(vehicleViewID).GetComponent<Vehicle>();

        DirtIssue dirtIssue = (DirtIssue)vehicle.CurrentIssue;

        if (dirtIssue != null)
        {
            dirtIssue.Wash();
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
    private void RPC_DestroyChildren(int parentViewID, string childrenName)
    {
        PhotonView pv = PhotonView.Find(parentViewID);

        if (pv == null) { return; }

        GameObject obj = pv.gameObject;

        // No child specified affect root object
        if (string.IsNullOrEmpty(childrenName))
        {
            Debug.Log("[vNetworkHandler] No children specified. Skipping RPC...");
            return;
        }

        // Affect all direct children
        if (childrenName == "*all")
        {
            foreach (Transform child in obj.transform)
            {
                Destroy(child.gameObject);
            }

            return;
        }

        // Affect specific child
        foreach (Transform child in obj.transform)
        {
            if (child.name == childrenName)
            {
                Destroy(child.gameObject);
                return;
            }
        }
    }
    [PunRPC]
    private void RPC_SafeDestruction(int vehicleViewID, bool safe)
    {
        Vehicle vehicle = PhotonView.Find(vehicleViewID).GetComponent<Vehicle>();

        TireIssue issue = (TireIssue)vehicle.CurrentIssue;

        issue.SafeDestruction = safe;
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
