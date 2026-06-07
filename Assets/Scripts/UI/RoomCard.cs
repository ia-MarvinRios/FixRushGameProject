using TMPro;
using UnityEngine;
using FixRush;

public class RoomCard : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text _roomInfoText;

    private string _roomName;

    public void SetRoomInfo(RoomData roomData)
    {
        _roomName = roomData.Name;
        _roomInfoText.text = $"{roomData.Name} {roomData.PlayerCount}/{roomData.MaxPlayers}";
    }

    public void JoinRoom() { PhotonManager.Instance.JoinRoom(_roomName); }
}