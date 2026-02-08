using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhotonLauncher : MonoBehaviourPunCallbacks
{
    [Header("Photon Settings")]
    [Tooltip("Nombre base de la sala (se le pueden añadir sufijos)")]
    [SerializeField] private string _roomNameBase = "Room";

    [Tooltip("Max Players por sala")]
    [SerializeField] private byte _maxPlayers = 4;

    [Tooltip("Nombre de la escena principal a cargar al entrar a la sala")]
    [SerializeField] private string _gameplaySceneName = "Game";

    public static PhotonLauncher Instance { get; private set; }

    private bool _isTryingToJoin = false;

    internal Dictionary<string, RoomInfo> cachedRooms = new Dictionary<string, RoomInfo>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Connect();
    }

    #region PUBLIC API

    public void Connect()
    {
        if (PhotonNetwork.IsConnected) return;

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = "1";

        // Forzar región US
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "us";

        Debug.Log("[PhotonLauncher] Connecting to Photon...");
        PhotonNetwork.ConnectUsingSettings();
    }

    public void JoinRoom(string roomName)
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogWarning("[PhotonLauncher] Not connected yet.");
            return;
        }

        _isTryingToJoin = true;

        Debug.Log($"[PhotonLauncher] Joining room \"{roomName}\"...");
        PhotonNetwork.JoinRoom(roomName);
    }

    public void CreateRoom()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogWarning("[PhotonLauncher] Not connected yet.");
            return;
        }

        _isTryingToJoin = true;

        //string roomName = $"{_roomNameBase}_{Random.Range(1000, 9999)}";
        string roomName = $"{_roomNameBase}_testing";

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = _maxPlayers,
            IsVisible = true,
            IsOpen = true
        };

        Debug.Log($"[PhotonLauncher] Creating room \"{roomName}\"...");
        PhotonNetwork.CreateRoom(roomName, options, TypedLobby.Default);
    }

    public void LeaveRoom()
    {
        if (!PhotonNetwork.InRoom) return;
        PhotonNetwork.LeaveRoom();
    }

    public string GetUserInfo()
    {
        string info = $"User ID: {PhotonNetwork.LocalPlayer.UserId}\n" +
                      $"Nickname: {PhotonNetwork.LocalPlayer.NickName}\n" +
                      $"Is Master Client: {PhotonNetwork.IsMasterClient}\n" +
                      $"Current Room: {(PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.Name : "None")}\n" +
                      $"Region: {PhotonNetwork.CloudRegion}\n";
        return info;
    }

    #endregion

    #region PUN CALLBACKS

    public override void OnConnectedToMaster()
    {
        Debug.Log("[PhotonLauncher] Connected to Master server.");
        PhotonNetwork.JoinLobby();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList)
            {
                cachedRooms.Remove(room.Name);
            }
            else
            {
                cachedRooms[room.Name] = room;
            }
        }

        Debug.Log("Current Rooms: " + cachedRooms.Count);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("[PhotonLauncher] Joined room successfully!");

        // Cargar la escena principal (solo el MasterClient)
        if (PhotonNetwork.IsMasterClient && !string.IsNullOrEmpty(_gameplaySceneName))
        {
            StartCoroutine(GameReady());
        }
    }

    IEnumerator GameReady()
    {
        Debug.Log($"[PhotonLauncher] Loading gameplay scene \"{_gameplaySceneName}\"...");
        PhotonNetwork.LoadLevel(_gameplaySceneName);

        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == _gameplaySceneName);

        Debug.Log("[PhotonLauncher] Gameplay scene loaded.");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"[PhotonLauncher] Failed to join room: {message} (Code: {returnCode})");

        // Intentar crear una sala alternativa si la falla fue por existencia
        _isTryingToJoin = true;
        CreateRoom();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"[PhotonLauncher] Failed to create room: {message} (Code: {returnCode})");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"[PhotonLauncher] Disconnected from Photon: {cause}");
    }

    public override void OnLeftRoom()
    {
        Debug.Log("[PhotonLauncher] Left room.");

        if (!PhotonNetwork.IsMasterClient) return;

        // Opcional: volver a menú o desconectar
        // SceneManager.LoadScene("MainMenu");
    }

    #endregion
}
