using Photon.Pun;
using ExitGames.Client.Photon;
using UnityEngine.Events;

public class GameStartListener : MonoBehaviourPun
{
    public UnityEvent OnGameStart;

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    private void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == GameManagerNetwork.StartGameEventCode)
        {
            OnGameStart?.Invoke();
        }
    }
}
