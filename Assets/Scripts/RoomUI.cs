using Photon.Realtime;
using UnityEngine;

public class RoomUI : MonoBehaviour
{
    internal RoomInfo RoomInfo { get; set; }

    public void JoinRoom()
    {
        if (RoomInfo == null) return;

        PhotonLauncher.Instance.JoinRoom(RoomInfo.Name);
    }
}
