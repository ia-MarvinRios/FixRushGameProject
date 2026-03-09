using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] GameObject _mainMenu;
    [SerializeField] TMP_Text _roomsMessage;
    [SerializeField] Transform _roomsContainer;
    [SerializeField] GameObject _roomsUiPrefab;
    public TMP_Text InfoText;

    Coroutine _updateRoomListCoroutine;

    Dictionary<RoomInfo, GameObject> _rooms = new Dictionary<RoomInfo, GameObject>();

    private void Start()
    {
        GUIBrain.Instance.StartNewInteraction(_mainMenu);
    }

    private void LateUpdate()
    {
        InfoText.text = PhotonLauncher.Instance.GetUserInfo();
    }

    public void GoToScene(string name) { SceneManager.LoadScene(name, LoadSceneMode.Single); }
    public void CreateRoom() { PhotonLauncher.Instance.CreateRoom(); }
    public void OpenMultiplayerMenu(GameObject menu)
    {
        GUIBrain.Instance.OpenNewMenu(menu);
        _updateRoomListCoroutine = StartCoroutine(UpdateRoomListCoroutine());
    }
    public void CloseMultiplayerMenu()
    {
        GUIBrain.Instance.CloseCurrentMenu();
        StopCoroutine(_updateRoomListCoroutine);
    }


    public void GetRoomList()
    {
        var rooms = PhotonLauncher.Instance.cachedRooms;

        if (rooms == null || rooms.Count == 0)
        {
            _roomsMessage.text = "No rooms available.";
            return;
        }

        foreach (RoomInfo room in rooms.Values)
        {
            if (_rooms.ContainsKey(room)) continue;

            GameObject obj = Instantiate(_roomsUiPrefab);
            obj.transform.SetParent(_roomsContainer);
            obj.transform.localScale = Vector3.one;

            obj.GetComponent<RoomUI>().RoomInfo = room;

            TMP_Text objText = obj.GetComponentInChildren<TMP_Text>();
            objText.text = $"{room.Name} ({room.PlayerCount}/{room.MaxPlayers})";

            _rooms.Add(room, obj);
            //sb.AppendLine($"{room.Name} ({room.PlayerCount}/{room.MaxPlayers})");
        }

        _roomsMessage.text = $"Found {_rooms.Count} rooms.";
    }

    IEnumerator UpdateRoomListCoroutine()
    {
        while (true)
        {
            GetRoomList();
            yield return new WaitForSeconds(1.5f);
        }
    }
}
