using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using FixRush;

/// <summary>
/// Handles the player spawning and the avatar loading for remote and local players.
/// This class sends a request to the MasterClient to register the player in the game,
/// then MasterClient sends a response to sync the avatar spawning on remote clients.
/// </summary>
public class PlayerSpawner : MonoBehaviourPun
{
    public static PlayerSpawner Instance { get; private set; }

    [Header("Player Spawning")]
    [SerializeField] private PlayerSettings _playerSettings;
    [SerializeField] private GameObject _playerPrefab;

    [Header("Level Data Reference")]
    [SerializeField] internal LevelData LevelData;

    internal PlayerController Controller { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
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
            { "actorNumber", data.ActorNumber },
            { "playerName", data.PlayerName },
            { "bodyID", data.BodyID },
            { "hatID", data.HatID },
            { "skinColorHex", data.SkinColorHex }
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
            actorNumber: (int)hashtable["actorNumber"],
            playerName: (string)hashtable["playerName"],
            isReady: true,
            bodyID: (int)hashtable["bodyID"],
            hatID: (int)hashtable["hatID"],
            skinColorHex: (string)hashtable["skinColorHex"]
        );
    }

    private void SpawnPlayer()
    {
        // Do spawning
        PlayerController playerController = PhotonNetwork.Instantiate(
            _playerPrefab.name,
            LevelData.Spawnpoints[PhotonNetwork.LocalPlayer.ActorNumber - 1],
            Quaternion.identity,
            0,
            new object[] { ColorUtility.ToHtmlStringRGBA(_playerSettings.SkinColor) }
        ).GetComponent<PlayerController>();

        Controller = playerController;

        // Get the player controller photon view
        PhotonView playerPv = playerController.GetComponent<PhotonView>();

        // Link player UI with Network Handler
        InGameUI.Instance.LinkNetworkHandler(playerController.GetComponent<PlayerNetworkHandler>());

        // Get player data
        Hashtable spawnData = ToHashtable(PhotonManager.Instance.GetThisPlayerData());
    }
}
