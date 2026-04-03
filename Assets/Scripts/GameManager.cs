using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Only the MasterClient should have an active instance of this class, as it is responsible for spawning players and managing game state.
/// </summary>
public class GameManager : MonoBehaviourPun
{
    public static GameManager Instance { get; private set; }
    private const string LOG_FORMAT = "<color=#pink>[Game Manager]</color>";

    [Header("Player Spawner Reference")]
    [SerializeField] private PlayerSpawner _spawner;


    private List<PhotonView> _onlinePlayers = new List<PhotonView>();

    private void Awake()
    {
        // Disable this component for non-MasterClients
        if (!PhotonNetwork.IsMasterClient)
        {
            enabled = false;
            return;
        }

        Instance = this;
    }



    internal void RegisterPlayer(int playerViewID, Hashtable spawnData)
    {
        // Find the player controller view in the scene
        PhotonView playerView = PhotonView.Find(playerViewID);

        if (playerView == null)
        {
            Debug.Log($"{LOG_FORMAT} Couldn't find player controller with photonViewID: {playerViewID}");
            return;
        }

        // Load and get the avatar view id
        int avatarViewID = _spawner.LoadAvatar(playerViewID, spawnData);

        photonView.RPC(
            nameof(RPC_SyncSpawnOnClients),
            RpcTarget.All,
            playerViewID,
            avatarViewID
        );

        _onlinePlayers.Add(playerView);
    }
    internal void UnregisterPlayer(int playerViewID)
    {
        PhotonView playerView = PhotonView.Find(playerViewID);
        _onlinePlayers.Remove(playerView);
    }
    
    #region RPCs
    
    [PunRPC]
    private void RPC_SyncSpawnOnClients(int playerViewID, int avatarViewID)
    {
        PlayerSpawner.Instance.SetAvatarParent(playerViewID, avatarViewID);
    }

    #endregion
}
