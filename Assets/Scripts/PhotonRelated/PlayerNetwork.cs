using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerNetwork : MonoBehaviourPun
{
    private PlayerController _player;

    private void Awake()
    {
        if (!photonView.IsMine) return;

        _player = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        if (!photonView.IsMine) return;

        _player.EnablePlayerInputs(PhotonNetwork.OfflineMode);
        _player.CheckCameraTransformRef();
        SpawnPlayer();
    }
    private void OnDisable()
    {
        if (!photonView.IsMine) return;

        _player.DisablePlayerInputs(PhotonNetwork.OfflineMode);
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        _player.Move();
    }

    void SpawnPlayer()
    {
        if (PhotonNetwork.OfflineMode)
        {
            Debug.Log("[PlayerNetwork] Spawning local player models...");

            GameObject a = PhotonNetwork.Instantiate(_player.PlayerPrefab.name, Vector3.zero, Quaternion.identity, 0, null);
            a.transform.parent = _player.transform;
            a.tag = "Player";

            GameObject b = PhotonNetwork.Instantiate(_player.PlayerPrefab.name, Vector3.zero, Quaternion.identity, 0, null);
            b.transform.parent = _player.transform;
            b.tag = "Player";

            GameObject[] models = new GameObject[2] { a, b };

            _player.SetUpPlayer(models);
        }
        else
        {
            Debug.Log("[PlayerNetwork] Spawning player over network...");

            GameObject playerObj = PhotonNetwork.Instantiate(_player.PlayerPrefab.name, Vector3.zero, Quaternion.identity, 0, null);
            playerObj.transform.parent = _player.transform;
            playerObj.tag = "Player";

            GameObject[] models = new GameObject[1] { playerObj };

            _player.SetUpPlayer(models);
        }
        
    }

    #region NETWORK COMMANDS

    #endregion

    #region RPCs

    #endregion
}
