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

    [Header("Level Data Reference")]
    [SerializeField] internal LevelData LevelData;
    [Header("Player Spawner Reference")]
    [SerializeField] private PlayerSpawner _spawner;
    [Header("Room Physics Objects")]
    [SerializeField] private Rigidbody[] _roomPhysicObjects;


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

        foreach (Rigidbody rb in _roomPhysicObjects)
        {
            rb.isKinematic = false;
        }
    }

    private void Start()
    {
        // Audio
        AudioManager.Instance.PlayAllMusic(true);
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

        _onlinePlayers.Add(playerView);
    }
    internal void UnregisterPlayer(int playerViewID)
    {
        PhotonView playerView = PhotonView.Find(playerViewID);
        _onlinePlayers.Remove(playerView);
    }


    private void OnDrawGizmos()
    {
        if (LevelData != null)
        {
            Gizmos.color = Color.yellow;
            foreach (Vector3 spawnpoint in LevelData.Spawnpoints)
            {
                Gizmos.DrawWireCube(spawnpoint, new Vector3(0.39f, 1.8f, 0.39f));
            }
        }
    }

}
