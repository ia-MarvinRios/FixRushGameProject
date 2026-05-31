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

    [Header("Day Night Cycle")]
    [SerializeField] internal Gradient _skyColorGradient;
    [SerializeField] internal Light _directionalLight;

    [Header("UI Reference")]
    [SerializeField] InGameUI _ui;

    [Header("Room Physics Objects")]
    [SerializeField] private Rigidbody[] _roomPhysicObjects;

    private int _globalCash;

    public float LevelCoutdownStep;
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

    internal float GetCurrentCash() { return _globalCash; }

    internal void AddCashMaster(int cash)
    {
        if (!PhotonNetwork.IsMasterClient) { return; }

        _globalCash += cash;
        Debug.Log($"{LOG_FORMAT} Current Cash: {_globalCash}");

        // Update Everyone's Cash
        photonView.RPC(
            nameof(RPC_SyncCash),
            RpcTarget.All,
            _globalCash
        );
    }

    private IEnumerator LevelTimeCountdownCoroutine()
    {
        float currentTime = LevelData.TimeLimitSeconds;
        float xRotation;
        Vector3 rotation = Vector3.zero;

        while (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            LevelCoutdownStep = currentTime / LevelData.TimeLimitSeconds;

            _directionalLight.color = _skyColorGradient.Evaluate(LevelCoutdownStep);

            xRotation = (1f - LevelCoutdownStep) * 180f;
            rotation.x = xRotation;
            _directionalLight.transform.eulerAngles = rotation;

            InGameUI.Instance.TimeText.text = GetTime(LevelCoutdownStep);

            yield return null;
        }

        rotation = Vector3.zero;
        _directionalLight.color = _skyColorGradient.Evaluate(1f);
        _directionalLight.transform.eulerAngles = rotation;

        // Sync Game Over Event on Clients
        photonView.RPC(
            nameof(RPC_SyncGameOverEvent),
            RpcTarget.All
        );
    }

    private string GetTime(float step)
    {
        float normalizedTime = 1f - step;

        float totalHours = 6f + normalizedTime * 12f;

        int hours = Mathf.FloorToInt(totalHours);
        int minutes = Mathf.FloorToInt((totalHours - hours) * 60f);

        string period = hours >= 12 ? "PM" : "AM";

        int displayHour = hours % 12;
        if (displayHour == 0)
            displayHour = 12;

        return $"{displayHour}:{minutes:00} {period}";
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
    private void RPC_SyncCash(int cash)
    {
        _globalCash = cash;

        // UI
        _ui.UpdateCashUI(cash);
    }
    [PunRPC]
    private void RPC_SyncGameOverEvent()
    {
        OnLevelTimeOut?.Invoke();
    }

    #endregion

}
