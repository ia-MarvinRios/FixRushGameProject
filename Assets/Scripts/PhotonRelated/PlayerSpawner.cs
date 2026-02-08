using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : MonoBehaviourPun
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform[] _spawnPoints;

    private void Start()
    {
        if (!PhotonNetwork.InRoom)
            return;

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            { "Ready", true }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        Debug.Log($"[Ready] Player {PhotonNetwork.LocalPlayer.ActorNumber} ready");
    }


    public void OnEnable()
    {
        StartCoroutine(SpawnWhenReady());
    }

    IEnumerator SpawnWhenReady()
    {
        yield return null;
        yield return null;

        int index = (PhotonNetwork.LocalPlayer.ActorNumber - 1) % _spawnPoints.Length;
        Transform spawn = _spawnPoints[index];

        PhotonNetwork.Instantiate(
            _playerPrefab.name,
            spawn.position,
            spawn.rotation
        );
    }
}
