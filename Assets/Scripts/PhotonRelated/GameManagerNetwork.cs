using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameManagerNetwork : MonoBehaviourPunCallbacks
{
    public static GameManagerNetwork Instance { get; private set; }

    public static readonly byte StartGameEventCode = 1;

    private bool _gameStarted = false;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (AllPlayersReady())
            StartGame();
    }

    bool AllPlayersReady()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            // If the property "Ready" does not exist
            if (!player.CustomProperties.ContainsKey("Ready"))
                return false;

            // If it exists but is false
            if (!(bool)player.CustomProperties["Ready"])
                return false;
        }

        return true;
    }

    void StartGame()
    {
        if (_gameStarted)
            return;

        _gameStarted = true;

        RaiseEventOptions options = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.All
        };

        PhotonNetwork.RaiseEvent(
            StartGameEventCode,
            null,
            options,
            SendOptions.SendReliable
        );
    }

}
