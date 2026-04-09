using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    internal bool IsMasterClient => PhotonNetwork.IsMasterClient;
    internal string MyUserID => PhotonNetwork.LocalPlayer.UserId;


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

    private void ConnectToPhoton()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        Debug.Log($"{LOG_FORMAT} Connecting to Photon...");
        PhotonNetwork.ConnectUsingSettings();
    }

    private PlayerData ConverToPlayerData(Player player)
    {
        // Get the custom properties for the player, with default values if they don't exist
        bool isReady = player.CustomProperties.ContainsKey("ready") && (bool)player.CustomProperties["ready"];
        int bodyID = player.CustomProperties.ContainsKey("body") ? (int)player.CustomProperties["body"] : 0;
        int hatID = player.CustomProperties.ContainsKey("hat") ? (int)player.CustomProperties["hat"] : 0;

        PlayerData playerData = new PlayerData(player.UserId, player.NickName, isReady, bodyID, hatID);
        
        return playerData;
    }

    private PlayerData GetRemotePlayerData(Player player) { return ConverToPlayerData(player); }

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

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby(TypedLobby.Default);

        OnPhotonConnected?.Invoke();

        Debug.Log($"{LOG_FORMAT} Connected to Photon Master Server.");
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

    public void SetNickname(string nickname)
    {
        PhotonNetwork.NickName = nickname;
        Debug.Log($"{LOG_FORMAT} Nickname set to: {nickname}");
    }

    public string CreateRoom()
    {
        // Generate a unique room name using the player's nickname and a random number
        string roomName = $"{PhotonNetwork.NickName}_{UnityEngine.Random.Range(1, 777)}'s room";

        // Create room options with a max player count of 4 and make it visible and open
        RoomOptions options = new RoomOptions
        {
            MaxPlayers = 4,
            IsVisible = true,
            IsOpen = true
        };

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

    public string GetPhotonInfo()
    {
        return $"Region: {PhotonNetwork.CloudRegion}, Build: {PhotonNetwork.AppVersion}";
    }

    public void SetIntProperty(string propertyName, int index)
    {
        Hashtable props = new Hashtable { { propertyName, index } };

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        Debug.Log($"{LOG_FORMAT} Changed {propertyName} to id: {index}");
    }
}
