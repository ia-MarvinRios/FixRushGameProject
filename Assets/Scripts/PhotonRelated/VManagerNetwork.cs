using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(VManager))]
public class VManagerNetwork : MonoBehaviourPun
{
    public bool CanSpawn()
    {
        return PhotonNetwork.IsMasterClient;
    }

    internal GameObject SpawnVehicle(GameObject[] prefabs, Vector3 spawnPoint, Quaternion rotation, IssueType[] issues, bool isFixed)
    {
        if (!PhotonNetwork.IsMasterClient) return null;

        int[] issueIds = new int[issues.Length];

        for (int i = 0; i < issues.Length; i++)
        {
            issueIds[i] = (int)issues[i];
        }

        GameObject obj = PhotonNetwork.InstantiateRoomObject(
            prefabs[Random.Range(0, prefabs.Length)].name,
            spawnPoint,
            rotation,
            0,
            new object[] { issueIds, isFixed});
        
        return obj;
    }

    internal void DestroyVehicle(GameObject vehicle)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonView view = vehicle.GetComponent<PhotonView>();

        if (view != null)
        {
            vehicle.GetComponent<Vehicle>().VehicleNetwork.CleanupNetworkState();
            PhotonNetwork.Destroy(view);
        }
    }

    internal void AttachVehicle(GameObject child)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        child.GetComponent<Vehicle>().VehicleNetwork.AttachVehicle(child, gameObject);
    }
}
