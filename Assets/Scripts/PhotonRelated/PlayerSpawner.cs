using Photon.Pun;
using UnityEngine;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform[] _spawnPoints;

    private void Start()
    {
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        int index = (PhotonNetwork.LocalPlayer.ActorNumber - 1) % _spawnPoints.Length;
        Transform spawn = _spawnPoints[index];

        PhotonNetwork.Instantiate(
            _playerPrefab.name,
            spawn.position,
            spawn.rotation
        );
    }
}
