using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class CarNetwork : MonoBehaviourPun
{
    internal void CleanupNetworkState()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        PhotonNetwork.RemoveBufferedRPCs(photonView.ViewID);
    }

    internal void RequestPlaceGato(AuxPlayer player, int gatoRootIndex, Car car)
    {
        photonView.RPC(
            nameof(RPC_PlaceGato),
            RpcTarget.AllBuffered,
            player.PhotonView.ViewID,
            gatoRootIndex,
            car.GetComponent<PhotonView>().ViewID
        );
    }

    internal void RequestRemoveGato(int gatoRootIndex, Car car, AuxPlayer entity)
    {
        photonView.RPC(
            nameof(RPC_RemoveGato),
            RpcTarget.AllBuffered,
            gatoRootIndex,
            car.GetComponent<PhotonView>().ViewID,
            entity.GetComponent<PhotonView>().ViewID
        );
    }

    internal void RequestSetUpTires(Car car)
    {
        photonView.RPC(
            nameof(RPC_SetUpTires),
            RpcTarget.AllBuffered,
            car.GetComponent<PhotonView>().ViewID,
            car.WheelRoots.Length
        );
    }

    internal GameObject SpawnObject(string objName, Vector3 position, Quaternion rotation)
    {
        return PhotonNetwork.Instantiate(objName, position, rotation);
    }
    internal void DestroyObject(GameObject obj) { PhotonNetwork.Destroy(obj); }

    internal void RequestRemoveOldWheel(Car car, int wheelID)
    {
        photonView.RPC(
            nameof(RPC_RemoveOldWheel),
            RpcTarget.AllBuffered,
            wheelID,
            car.GetComponent<PhotonView>().ViewID
        );
    }

    internal void RequestSetUpEmptyWheelRoot(int gatoID, Vector3 position, Car car)
    {
        photonView.RPC(
            nameof(RPC_SetUpEmptyWheelRoot),
            RpcTarget.AllBuffered,
            car.WheelRoots.Length,
            gatoID,
            position,
            car.GetComponent<PhotonView>().ViewID
        );
    }

    #region RPCs

    [PunRPC]
    void RPC_PlaceGato(int playerViewID, int gatoRootIndex, int carViewID)
    {
        AuxPlayer entity = PhotonView.Find(playerViewID).GetComponent<AuxPlayer>();
        Car car = PhotonView.Find(carViewID).GetComponent<Car>();

        car.PlaceGato(entity, gatoRootIndex);
    }
    [PunRPC]
    void RPC_RemoveGato(int gatoRootIndex, int carViewID, int entityViewID)
    {
        Car car = PhotonView.Find(carViewID).GetComponent<Car>();
        AuxPlayer entity = PhotonView.Find(entityViewID).GetComponent<AuxPlayer>();

        car.RemoveGato(gatoRootIndex, entity);
    }
    [PunRPC]
    void RPC_SetUpTires(int carViewID, int idCount)
    {
        Car car = PhotonView.Find(carViewID).GetComponent<Car>();
        TireIssue i = car.CurrentIssue as TireIssue;

        if (i == null) return;

        List<int> wIDList = new List<int>();
        for (int j = 0; j < idCount; j++)
        {
            wIDList.Add(j + 1);
        }

        i.SetUpTires(wIDList);
    }
    [PunRPC]
    void RPC_RemoveOldWheel(int wheelID, int carViewID)
    {
        Car car = PhotonView.Find(carViewID).GetComponent<Car>();

        TireIssue i = car.CurrentIssue as TireIssue;
        if (i == null) return;

        i.RemoveWheel(wheelID);
    }
    [PunRPC]
    void RPC_SetUpEmptyWheelRoot(int idCount, int gatoID, Vector3 position, int carViewID)
    {
        Car car = PhotonView.Find(carViewID).GetComponent<Car>();
        TireIssue i = car.CurrentIssue as TireIssue;

        if (i == null) return;

        List<int> rIDList = new List<int>();
        for (int j = 0; j < idCount; j++)
        {
            rIDList.Add(j + 1);
        }

        i.SetUpEmptyWheelRoot(rIDList, gatoID, position);
    }

    #endregion

}
