using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using FixRush;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    public static PhotonManager Instance { get; private set; }

    private const string LOG_FORMAT = "<color=lightblue>[PhotonManager]</color>";

    internal Room CurrentRoom { get; private set; }

    internal Action OnPhotonConnected;
    internal Action OnRoomJoin;
    internal delegate void OnRemotePlayerLeft(PlayerData playerData);
    internal OnRemotePlayerLeft OnRemotePlayerLeave;
    internal delegate void OnPlayerListUpdated(List<PlayerData> playerList);
    internal OnPlayerListUpdated OnPlayerListChanged;
    internal delegate void OnRoomListUpdated(List<RoomData> rooms);
    internal OnRoomListUpdated OnRoomListChanged;

    private bool _startSinglePlayer = false;

    internal bool IsMasterClient => PhotonNetwork.IsMasterClient;
    internal string MyUserID => PhotonNetwork.LocalPlayer.UserId;
    internal string MyNickname => PhotonNetwork.NickName;
    internal bool OfflineMode { get => PhotonNetwork.OfflineMode; set => PhotonNetwork.OfflineMode = value; }


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
        ConnectToPhoton();
    }

    #region CONNECTION_AND_ROOM_MANAGEMENT

    private void ConnectToPhoton()
    {
        OfflineMode = false;
        PhotonNetwork.AutomaticallySyncScene = true;

        Debug.Log($"{LOG_FORMAT} Connecting to Photon...");
        PhotonNetwork.ConnectUsingSettings();
    }

    private void DisconnectFromPhoton()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
            Debug.Log($"{LOG_FORMAT} Disconnecting from Photon...");
        }
        else
        {
            Debug.LogWarning($"{LOG_FORMAT} Cannot disconnect: Not currently connected to Photon.");
        }
    }

    public void SetNickname(string nickname)
    {
        PhotonNetwork.NickName = nickname;
        Debug.Log($"{LOG_FORMAT} Nickname set to: {nickname}");
    }

    public string CreateRoom()
    {
        // Variables
        string roomName;
        RoomOptions options;

        if (OfflineMode)
        {
            roomName = "Offline Room";
            options = new RoomOptions
            {
                MaxPlayers = 1,
                IsVisible = false,
                IsOpen = false
            };
        }
        else
        {
            // Generate a unique room name using the player's nickname and a random number
            roomName = $"{PhotonNetwork.NickName}_{UnityEngine.Random.Range(1, 777)}'s room";

            // Create room options with a max player count of 4 and make it visible and open
            options = new RoomOptions
            {
                MaxPlayers = 4,
                IsVisible = true,
                IsOpen = true
            };
        }

        PhotonNetwork.CreateRoom(roomName, options);

        Debug.Log($"{LOG_FORMAT} Creating room: {roomName}");
        return roomName;
    }

    public void JoinRoom(string roomName)
    {
        if (PhotonNetwork.InRoom)
        {
            Debug.LogWarning($"{LOG_FORMAT} Already in a room. Leaving current room before joining new one.");
            LeaveRoom();
        }
        PhotonNetwork.JoinRoom(roomName);
        Debug.Log($"{LOG_FORMAT} Joining room: {roomName}");
    }

    public void LeaveRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            Debug.Log($"{LOG_FORMAT} Leaving room...");
        }
        else
        {
            Debug.LogWarning($"{LOG_FORMAT} Cannot leave room: Not currently in a room.");
        }
    }

    public void RefreshRoomList()
    {
        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }

    public void SetReady(bool isReady)
    {
        Hashtable props = new Hashtable { { "ready", isReady } };

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        Debug.Log($"{LOG_FORMAT} Set ready status to: {isReady}");
    }

    public bool CheckAllReady()
    {
        bool allReady = true;
        string notReadyPlayers = "";

        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (!player.CustomProperties.ContainsKey("ready") ||
                !(bool)player.CustomProperties["ready"])
            {
                notReadyPlayers += $"{player.NickName}, ";
                allReady = false;
            }
        }

        if (!allReady) Debug.Log($"{LOG_FORMAT} The following players are not ready: {notReadyPlayers}");

        return allReady;
    }

    public void StartSingleplayer()
    {
        DisconnectFromPhoton();
        _startSinglePlayer = true;
    }

    public void TryStartGame()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (CheckAllReady())
        {
            Debug.Log($"{LOG_FORMAT} All players are ready. Starting game...");

            CurrentRoom.IsOpen = false;
            CurrentRoom.IsVisible = false;

            PhotonNetwork.LoadLevel("Overworld");
        }
        else
        {
            Debug.Log($"{LOG_FORMAT} Cannot start game: Not all players are ready.");
        }
    }

    #endregion

    #region DATA_MANAGEMENT

    /// <summary>
    /// Converts a Photon Player object to a PlayerData object, extracting the relevant custom properties and handling missing properties with default values.
    /// </summary>
    /// <param name="player">The Photon Player object to convert.</param>
    /// <returns>A PlayerData object containing the player's data.</returns>
    private PlayerData ConverToPlayerData(Player player)
    {
        // Get the custom properties for the player, with default values if they don't exist
        bool isReady = player.CustomProperties.ContainsKey("ready") && (bool)player.CustomProperties["ready"];
        int bodyID = player.CustomProperties.ContainsKey("body") ? (int)player.CustomProperties["body"] : 0;
        int hatID = player.CustomProperties.ContainsKey("hat") ? (int)player.CustomProperties["hat"] : 0;
        string skinColorHex = player.CustomProperties.ContainsKey("skinColorHex") ? (string)player.CustomProperties["skinColorHex"] : "E6D1B1FF";

        PlayerData playerData = new PlayerData(player.UserId, player.NickName, isReady, bodyID, hatID, skinColorHex);
        
        return playerData;
    }

    private PlayerData GetRemotePlayerData(Player player) { return ConverToPlayerData(player); }

    /// <summary>
    /// Gets the current list of players in the room by iterating through PhotonNetwork.PlayerList and 
    /// converting each Player object to a PlayerData object using the ConverToPlayerData method.
    /// </summary>
    /// <returns>A list of PlayerData objects representing the current players in the room.</returns>
    internal List<PlayerData> GetCurrentPlayerList()
    {
        List<PlayerData> playerList = new List<PlayerData>();

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            playerList.Add(ConverToPlayerData(player));
        }

        return playerList;
    }

    internal PlayerData GetThisPlayerData()
    {
        Player player = PhotonNetwork.LocalPlayer;

        return ConverToPlayerData(player);
    }

    public string GetPhotonInfo()
    {
        return $"Region: {PhotonNetwork.CloudRegion}, Build: {PhotonNetwork.AppVersion}";
    }

    /// <summary>
    /// Sets an integer custom property for the local player. This can be used to store character customization choices like hat and body IDs, or other game-related data. 
    /// The property is added to a hashtable and then sent to Photon to update the player's custom properties. A debug log is printed to confirm the change.
    /// </summary>
    /// <param name="propertyName">The name of the custom property to set.</param>
    /// <param name="index">The integer value to assign to the custom property.</param>
    public void SetIntProperty(string propertyName, int index)
    {
        Hashtable props = new Hashtable { { propertyName, index } };

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        Debug.Log($"{LOG_FORMAT} Changed {propertyName} to id: {index}");
    }

    /// <summary>
    /// Sets a string custom property for the local player. This can be used to store character customization choices like skin color or other game-related data.
    /// The property is added to a hashtable and then sent to Photon to update the player's custom properties. A debug log is printed to confirm the change.
    /// </summary>
    /// <param name="propertyName">The name of the custom property to set.</param>
    /// <param name="value">The string value to assign to the custom property.</param>
    public void SetStringProperty(string propertyName, string value)
    {
        Hashtable props = new Hashtable { { propertyName, value } };

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        Debug.Log($"{LOG_FORMAT} Changed {propertyName} to: {value}");
    }

    #endregion

    #region PHOTON_CALLBACKS

    public override void OnConnectedToMaster()
    {
        if (!PhotonNetwork.OfflineMode)
        {
            PhotonNetwork.JoinLobby(TypedLobby.Default);
        }

        OnPhotonConnected?.Invoke();

        Debug.Log($"{LOG_FORMAT} Connected to Photon Master Server.");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        OfflineMode = true;
        Debug.LogWarning($"{LOG_FORMAT} Disconnected from Photon. Reason: {cause}");

        if (_startSinglePlayer)
        {
            _startSinglePlayer = false;
            CreateRoom();
        }
    }

    public override void OnJoinedRoom()
    {
        CurrentRoom = PhotonNetwork.CurrentRoom;
        SetReady(false);

        OnPlayerListChanged?.Invoke(GetCurrentPlayerList());

        OnRoomJoin?.Invoke();

        Debug.Log($"{LOG_FORMAT} Joined room: {CurrentRoom.Name} | Master: {PhotonNetwork.IsMasterClient}");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        OnPlayerListChanged?.Invoke(GetCurrentPlayerList());
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // If MasterClient leaves, leave too
        if (otherPlayer.IsMasterClient)
        {
            PhotonNetwork.LeaveRoom();
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }

        // Handle delegates
        OnRemotePlayerLeave?.Invoke(GetRemotePlayerData(otherPlayer));
        OnPlayerListChanged?.Invoke(GetCurrentPlayerList());
    }

    public override void OnLeftRoom()
    {
        Debug.Log($"{LOG_FORMAT} Left room.");

        // If we left because the MasterClient left, reconnect to Photon and return to the lobby
        if (!PhotonNetwork.IsConnected)
        {
            ConnectToPhoton();
        }

        // Return to main menu if not already there
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }

        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        List<RoomData> rooms = new List<RoomData>();

        foreach (var room in roomList)
        {
            if (!room.IsOpen || !room.IsVisible || room.RemovedFromList) continue;

            RoomData card = new RoomData();
            card.Name = room.Name;
            card.PlayerCount = room.PlayerCount;
            card.MaxPlayers = room.MaxPlayers;

            rooms.Add(card);
        }

        Debug.Log($"{LOG_FORMAT} Room list updated. Total rooms: {rooms.Count}");

        OnRoomListChanged?.Invoke(rooms);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey("ready"))
        {
            CheckAllReady();
            
        }
        OnPlayerListChanged?.Invoke(GetCurrentPlayerList());
    }

    #endregion
}
