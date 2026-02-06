using Photon.Pun;
using Photon.Realtime;
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
        PhotonNetwork.GameVersion = Application.version;

        Debug.Log("[PhotonLauncher] Connecting to Photon...");
        PhotonNetwork.ConnectUsingSettings();
    }

    public void JoinOrCreateRoom()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogWarning("[PhotonLauncher] Not connected yet.");
            return;
        }

        _isTryingToJoin = true;

        string roomName = $"{_roomNameBase}_{Random.Range(1000, 9999)}";

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = _maxPlayers,
            IsVisible = true,
            IsOpen = true
        };

        Debug.Log($"[PhotonLauncher] Joining or creating room \"{roomName}\"...");
        PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);
    }

    public void LeaveRoom()
    {
        if (!PhotonNetwork.InRoom) return;
        PhotonNetwork.LeaveRoom();
    }

    #endregion

    #region PUN CALLBACKS

    public override void OnConnectedToMaster()
    {
        Debug.Log("[PhotonLauncher] Connected to Master server.");
        if (_isTryingToJoin)
        {
            JoinOrCreateRoom();
            return;
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("[PhotonLauncher] Joined room successfully!");

        // Cargar la escena principal (solo el MasterClient)
        if (PhotonNetwork.IsMasterClient && !string.IsNullOrEmpty(_gameplaySceneName))
        {
            Debug.Log($"[PhotonLauncher] Loading gameplay scene \"{_gameplaySceneName}\"...");
            PhotonNetwork.LoadLevel(_gameplaySceneName);
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"[PhotonLauncher] Failed to join room: {message} (Code: {returnCode})");

        // Intentar crear una sala alternativa si la falla fue por existencia
        _isTryingToJoin = true;
        JoinOrCreateRoom();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"[PhotonLauncher] Disconnected from Photon: {cause}");
    }

    public override void OnLeftRoom()
    {
        Debug.Log("[PhotonLauncher] Left room.");

        // Opcional: volver a menú o desconectar
        // SceneManager.LoadScene("MainMenu");
    }

    #endregion
}
