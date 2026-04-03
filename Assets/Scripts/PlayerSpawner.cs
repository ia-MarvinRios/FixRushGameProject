using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the player spawning and the avatar loading for remote and local players.
/// This class sends a request to the MasterClient to register the player in the game,
/// then MasterClient sends a response to sync the avatar spawning on remote clients.
/// </summary>
public class PlayerSpawner : MonoBehaviourPun
{
    public static PlayerSpawner Instance {  get; private set; }

    [Header("Game Content Reference")]
    [SerializeField] internal GameContent GameContent;
    [Header("Player Spawning")]
    [SerializeField] private GameObject _playerPrefab;

    internal PlayerController Controller { get; private set; }

    private void Awake()
    {
        Instance = this;
        SpawnPlayer();
    }

    /// <summary>
    /// Converts the specified player data to a hashtable representation.
    /// </summary>
    /// <param name="data">The player data to convert. Cannot be null.</param>
    /// <returns>A hashtable containing the player's name and skin ID.</returns>
    private Hashtable ToHashtable(PlayerData data)
    {
        return new Hashtable {
            { "userID", data.Uid },
            { "playerName", data.PlayerName },
            { "bodyID", data.BodyID },
            { "hatID", data.HatID }
        };
    }

    /// <summary>
    /// Creates a new PlayerData instance from the specified hashtable.
    /// </summary>
    /// <remarks>The "isReady" property of the returned PlayerData is always set to <see langword="true"/>.
    /// The method expects the hashtable to contain valid and correctly typed values for the required keys.</remarks>
    /// <param name="hashtable">A hashtable containing player data. Must include entries for "playerName" (string) and "skinID" (int).</param>
    /// <returns>A PlayerData object populated with values from the hashtable.</returns>
    private PlayerData FromHashtable(Hashtable hashtable)
    {
        return new PlayerData(
            userID: (string)hashtable["userID"],
            playerName: (string)hashtable["playerName"],
            isReady: true,
            bodyID: (int)hashtable["bodyID"],
            hatID: (int)hashtable["hatID"]
        );
    }

    private void SpawnPlayer()
    {
        // Get prefab info
        PlayerController playerController = _playerPrefab.GetComponent<PlayerController>();

        // Do spawning
        playerController = PhotonNetwork.Instantiate(
            _playerPrefab.name,
            playerController.Player.SpawnPoint,
            Quaternion.identity
        ).GetComponent<PlayerController>();

        Controller = playerController;

        // Get the player controller photon view
        PhotonView playerPv = playerController.GetComponent<PhotonView>();

        // Link player UI with Network Handler
        InGameUI.Instance.LinkNetworkHandler(playerController.GetComponent<PlayerNetworkHandler>());

        // Get player data
        Hashtable spawnData = ToHashtable(PhotonManager.Instance.GetThisPlayerData());

        // Sync player spawn on server
        photonView.RPC(
            nameof(RPC_SyncPlayerSpawnOnServer),
            RpcTarget.MasterClient,
            playerPv.ViewID,
            spawnData,
            false
        );
    }

    internal int LoadAvatar(int playerViewID, Hashtable spawnData)
    {
        // Get skin object and spawn data
        PlayerData data = FromHashtable(spawnData);
        //GameObject skinPrefab = GameContent.Skins[data.SkinID].Prefab;

        // Spawn skin prefab
        PhotonView avatarView = PhotonNetwork.Instantiate(
            "REPLACE",
            Vector3.up,
            Quaternion.identity
        ).GetComponent<PhotonView>();

        return avatarView.ViewID;
    }

    internal void SetAvatarParent(int playerViewID, int avatarViewID)
    {
        // Get the references of the objects to use
        Transform remotePlayer = PhotonView.Find(playerViewID).transform;
        GameObject skinPrefab = PhotonView.Find(avatarViewID).gameObject;

        // OWNERSHIP
        // Get avatar viewID
        if (skinPrefab.TryGetComponent(out PhotonView avatarPv))
        {
            // Fix Pivot
            float pivotOffset = remotePlayer.GetComponent<Collider>().bounds.size.y / 2;
            skinPrefab.transform.position = new Vector3(remotePlayer.position.x, remotePlayer.position.y - pivotOffset, remotePlayer.position.z);

            // Set remote player parent of the avatar
            skinPrefab.transform.SetParent(remotePlayer, true);

            // Initialize player avatar
            skinPrefab.GetComponent<Avatar>().Initialize();

            // Set ownership to remote player
            avatarPv.TransferOwnership(playerViewID);
        }
    }

    #region RPCs

    [PunRPC]
    private void RPC_SyncPlayerSpawnOnServer(int playerViewID, Hashtable spawnData, bool left)
    {
        if (!PhotonNetwork.IsMasterClient) { return; }

        if (left) { GameManager.Instance.UnregisterPlayer(playerViewID); }
        else { GameManager.Instance.RegisterPlayer(playerViewID, spawnData); }
    }

    #endregion
}
