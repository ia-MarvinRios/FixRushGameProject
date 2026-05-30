using Photon.Pun;
using System;
using System.Collections;
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

    [Header("UI Reference")]
    [SerializeField] InGameUI _ui;

    [Header("Room Physics Objects")]
    [SerializeField] private Rigidbody[] _roomPhysicObjects;

    private int _globalCash;

    public static event Action OnLevelTimeOut;

    private void Awake()
    {
        Instance = this;

        if (!PhotonNetwork.IsMasterClient) { return; }

        foreach (Rigidbody rb in _roomPhysicObjects)
        {
            rb.isKinematic = false;
        }
    }

    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient) { return; }

        LevelTimeCountdownStart();
    }

    private void LevelTimeCountdownStart()
    {
        StartCoroutine(LevelTimeCountdownCoroutine());
    }

    internal void AddCashMaster(int cash)
    {
        if (!PhotonNetwork.IsMasterClient) { return; }

        _globalCash += cash;
        Debug.Log($"{LOG_FORMAT} Current Cash: {_globalCash}");

        // Update Everyone's UI
        photonView.RPC(
            nameof(RPC_SyncCashUI),
            RpcTarget.All,
            _globalCash
        );
    }

    private IEnumerator LevelTimeCountdownCoroutine()
    {
        float currentTime = LevelData.TimeLimitSeconds;

        while (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            yield return null;
        }

        OnLevelTimeOut?.Invoke();
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

    #region RPCs

    [PunRPC]
    private void RPC_SyncCashUI(int cash)
    {
        _ui.UpdateCashUI(cash);
    }

    #endregion

}
