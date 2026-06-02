using FixRush;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class vNetworkHandler : MonoBehaviourPun, IPunInstantiateMagicCallback, IPunObservable
{
    [Header("Stream Sync")]
    [SerializeField] private Vehicle _vehicle;

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

        _vehicle = GetComponent<Vehicle>();
        _vehicle.IssueTypes = Issues;
        _vehicle.PreviousIssueCount = (int)data[1];
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Enviar datos
            stream.SendNext(_vehicle.PatienceSlider.value);
        }
        else
        {
            // Recibir datos
            float value = (float)stream.ReceiveNext();

            if (_vehicle.SliderFillImage == null || _vehicle.PatienceSlider == null) return;

            if (Mathf.Abs(value - _vehicle.PatienceSlider.value) > 0.001f)
            {
                _vehicle.PatienceSlider.value = value;
            }

            _vehicle.SliderFillImage.color = _vehicle.PatienceGradient.Evaluate(value);
        }
    }

    public void SyncCarsProgressCounters(int repaired, int lost)
    {
        photonView.RPC(
            nameof(RPC_UpdateCarProgress),
            RpcTarget.All,
            repaired,
            lost
        );
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

    internal void TimeOut()
    {
        photonView.RPC(
            nameof(RPC_VehicleTimeOut),
            RpcTarget.All,
            photonView.ViewID
        );
    }

    internal void SyncJackUnjackCar(PlayerController player, Jack jack, bool jacked)
    {
        photonView.RPC(
            nameof(RPC_JackUnjackCar),
            RpcTarget.All,
            photonView.ViewID,
            player.GetComponent<PhotonView>().ViewID,
            jack.GetComponent<PhotonView>().ViewID,
            jacked
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

    internal void DestroyNetworkObjMaster(GameObject obj)
    {
        photonView.RPC(
            nameof(RPC_DestroyNetworkObject),
            RpcTarget.MasterClient,
            obj.GetComponent<PhotonView>().ViewID
        );
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

    internal void RequestPickUpObject(PlayerController player, GameObject obj)
    {
        photonView.RPC(
            nameof(RPC_GetObject),
            RpcTarget.All,
            player.GetComponent<PhotonView>().ViewID,
            obj.GetComponent<PhotonView>().ViewID
        );
    }

    #region RPCs

    [PunRPC]
    void RPC_UpdateCarProgress(int repaired, int lost)
    {
        GameManager.Instance.RepairedCars += repaired;
        GameManager.Instance.LostCars += lost;
    }
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
    private void RPC_VehicleTimeOut(int vehicleViewID)
    {
        Vehicle vehicle = PhotonView.Find(vehicleViewID).GetComponent<Vehicle>();

        vehicle.TimeOut();
    }
    [PunRPC]
    private void RPC_JackUnjackCar(int carViewID, int playerViewID, int jackViewID, bool jacked)
    {
        Car car = PhotonView.Find(carViewID).GetComponent<Car>();
        PlayerController player = PhotonView.Find(playerViewID).GetComponent<PlayerController>();
        Jack jack = PhotonView.Find(jackViewID).GetComponent<Jack>();

        if (jack != null && jacked)
        {
            car.JackCar(player);
            car.mJack = jack;
            car.IsJacked = true;
            return;
        }

        car.UnjackCar(player);
        car.mJack = null;
        car.IsJacked = false;
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
    private void RPC_DestroyNetworkObject(int objViewID)
    {
        GameObject obj = PhotonView.Find(objViewID).gameObject;
        PhotonNetwork.Destroy(obj);
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
