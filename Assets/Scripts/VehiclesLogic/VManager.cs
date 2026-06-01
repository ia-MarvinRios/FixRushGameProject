using FixRush;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.Rendering.DebugUI;


/// <summary>
/// This class is responsible for managing the vehicles in the game, including spawning, queue management and movement
/// to targets.The leader is actually the last vehicle in the queue, since the vehicles move from the spawn point to 
/// the wait point, and then to the end point.
/// </summary>
public class VManager : MonoBehaviourPun
{
    public static VManager Instance { get; private set; }

    [Header("Vehicle Spawner")]
    [Space(10)]
    [SerializeField, Range(1, 10)] int _maxInstances = 5;
    [SerializeField] GameObject[] _vehiclePrefabs;
    [SerializeField] Vector3 _spawnPoint;
    [SerializeField] Vector3 _WaitPoint1;
    [SerializeField] Vector3 _endPoint;
    [SerializeField] float _waitPointOffset = 1f;
    [SerializeField, Range(0.1f, 5f)] float _spawnInterval = 0.5f;
    [SerializeField] ReparationPlatform[] _targets;
    [Space(10)]
    [Header("Vehicle Lists")]
    [SerializeField] List<GameObject> _queueMain;
    [SerializeField] List<GameObject> _queueReparation;
    [SerializeField] List<GameObject> _queueDestruction;

    [Header("Level Data Reference")]
    [SerializeField] private LevelData _levelData;

    NavMeshAgent _leader;
    Coroutine _spawnCoroutine;
    WaitForSeconds _spawnIntervalWaitTime;
    Vehicle _temp = null;

    int _prefabIndex = -1;
    int _previousIssueCount = 0;

    // ---------Internal Access------------
    internal List<GameObject> QueueVehicles { get => _queueMain; }
    internal List<GameObject> DestructionQueueVehicles { get => _queueDestruction; }
    internal List<GameObject> InReparationVehicles { get => _queueReparation; }

    private static IIssue.Type[] _selectableIssues = (IIssue.Type[])System.Enum.GetValues(typeof(IIssue.Type));
    private HashSet<IIssue.Type> _selected = new HashSet<IIssue.Type>();

    private void Awake()
    {
        Instance = this;

        if (!PhotonNetwork.IsMasterClient)
        {
            enabled = false;
            return;
        }

        _spawnIntervalWaitTime = new WaitForSeconds(_spawnInterval);
    }

    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient) { return; }

        _spawnCoroutine = StartCoroutine(SpawnVehicles());
    }

    private IEnumerator SpawnVehicles()
    {
        while (true)
        {
            // --- Spawning flow ---
            if (QueueVehicles.Count < _maxInstances)
            {
                Debug.Log("[VManager] Spawning vehicle...");

                // Get a random prefab from the list
                _prefabIndex = Random.Range(0, _vehiclePrefabs.Length);

                // Get the Vehicle component from the prefab to assign issues before instantiating
                _temp = _vehiclePrefabs[_prefabIndex].GetComponent<Vehicle>();

                // Instantiate the vehicle
                GameObject obj = PhotonNetwork.InstantiateRoomObject(
                    _vehiclePrefabs[_prefabIndex].name,
                    _spawnPoint,
                    Quaternion.identity,
                    0,
                    new object[] { IssuesToIntArray(AssignIssues(_temp)), _previousIssueCount }
                );

                if (obj == null) { continue; }

                // Get the Vehicle component from the instantiated object
                Vehicle v = obj.GetComponent<Vehicle>();

                // Add first
                QueueVehicles.Add(obj);
                v.QueueIndex = QueueVehicles.Count - 1;

                // Update previous
                _previousIssueCount += v.IssueTypes.Length;

                // Update Leader
                UpdateLeader();

                // Recalc queue
                UpdateQueueVehicles(0);

                yield return _spawnIntervalWaitTime;
            }

            if (QueueVehicles.Count > 0 && GetFirstAvailablePlatform() != null)
            {
                MoveToTargetPos(QueueVehicles[0].GetComponent<Vehicle>().Agent);
            }

            yield return new WaitForSeconds(1.5f);
        }
    }

    #region SPAWN_LOGIC

    public void UpdateLeader()
    {
        _leader = null;

        if (QueueVehicles.Count == 0)
            return;

        NavMeshAgent agent =
            QueueVehicles[^1].GetComponent<NavMeshAgent>();

        if (agent != null && agent.enabled && agent.isOnNavMesh)
            _leader = agent;
    }

    void MoveToTargetPos(NavMeshAgent agent)
    {
        if (AIExtension.HasReachedDestination(agent))
        {
            StartCoroutine(SetUpForFixing(agent));
        }
    }

    IEnumerator SetUpForFixing(NavMeshAgent agent)
    {
        if (agent == null)
        {
            Debug.LogWarning("Trying to set up vehicle but it's reference is null.");
            yield break;
        }
        if (!agent.TryGetComponent(out Vehicle v)) { yield break; }

        // Previous adjustments
        ReparationPlatform platform = GetFirstAvailablePlatform();
        platform.Taken = true;
        _previousIssueCount -= v.IssueTypes.Length;

        // Move to reparation list
        QueueVehicles.Remove(agent.gameObject);
        InReparationVehicles.Add(agent.gameObject);

        // Move vehicle agent
        Debug.Log("[VManager] Moving to target...");
        agent.isStopped = false;
        agent.SetDestination(platform.Target.position);

        yield return null;
        yield return new WaitUntil(() => AIExtension.HasReachedDestination(agent));

        // Snap car to position
        agent.isStopped = true;
        agent.transform.position = platform.Target.position;
        agent.transform.rotation = platform.Target.rotation;

        // Init vehicle
        v.Platform = platform;
        v.InitializeVehicle();
    }

    public Coroutine MoveToEndPoint(Vehicle v) { return StartCoroutine(MoveToEndPointCoroutine(v.Agent, v)); }

    public IEnumerator MoveToEndPointCoroutine(NavMeshAgent agent, Vehicle v)
    {
        _previousIssueCount -= v.IssueTypes.Length;

        // Move to destruction list
        QueueVehicles.Remove(agent.gameObject);
        InReparationVehicles.Remove(agent.gameObject);
        DestructionQueueVehicles.Add(agent.gameObject);

        // Para evitar que se peleen por llegar al final
        agent.stoppingDistance = 1.5f;

        // Move
        Debug.Log("[VManager] Moving to endpoint...");
        agent.isStopped = false;
        agent.SetDestination(_endPoint);

        yield return new WaitUntil(() => AIExtension.HasReachedDestination(agent));

        // Remove and destroy vehicle
        DestructionQueueVehicles.Remove(agent.gameObject);
        PhotonNetwork.Destroy(v.gameObject);
    }

    public void UpdateQueueVehicles(int index)
    {
        Debug.Log($"[VManager] UpdatingQueue from index: {index} | leader: {_leader.name}...");

        // Recalc only from the new
        if (_leader != null && _leader.isOnNavMesh)
        {
            AIExtension.RecalculateQueueFrom(
                index,
                _waitPointOffset,
                _WaitPoint1,
                _leader,
                ObjectsToAgentsList(QueueVehicles));
        }
    }

    public void RemoveVehicle(Vehicle v)
    {
        int index = QueueVehicles.IndexOf(v.gameObject);
        _previousIssueCount -= v.IssueTypes.Length;

        if (index >= 0)
        {
            QueueVehicles.RemoveAt(index);
        }
        else
        {
            index = 0;
        }

        UpdateLeader();

        // Destroy after getting index
        PhotonNetwork.Destroy(v.gameObject);

        UpdateQueueVehicles(index);
    }

    public void EnableAllPlatforms()
    {
        foreach (ReparationPlatform p in _targets)
        {
            p.Unlocked = true;
        }
    }

    IIssue.Type[] AssignIssues(Vehicle v)
    {
        _selected.Clear();

        // Calculate the number of issues based on the current level difficulty
        // FORMULA: (MAX_ISSUES * CurrentDifficulty) / MAX_DIFFICULTY
        float issuesCount = v.IssueTypes.Length * (float)_levelData.LevelDifficulty / LevelData.MAX_DIFFICULTY;
        issuesCount = (int)System.Math.Round(issuesCount, System.MidpointRounding.AwayFromZero);

        while (_selected.Count < issuesCount)
        {
            IIssue.Type issue = GetRandomIssue(v);
            _selected.Add(issue);
        }

        return new List<IIssue.Type>(_selected).ToArray();
    }
    IIssue.Type GetRandomIssue(Vehicle v)
    {
        _selectableIssues = v.IssueTypes;

        return _selectableIssues[Random.Range(0, _selectableIssues.Length)];
    }
    int[] IssuesToIntArray(IIssue.Type[] issues)
    {
        int[] intArray = new int[issues.Length];
        for (int i = 0; i < issues.Length; i++)
        {
            intArray[i] = (int)issues[i];
        }
        return intArray;
    }
    List<NavMeshAgent> ObjectsToAgentsList(List<GameObject> objects)
    {
        List<NavMeshAgent> agents = new List<NavMeshAgent>();
        foreach (GameObject obj in objects)
        {
            agents.Add(obj.GetComponent<NavMeshAgent>());
        }

        return agents;
    }
    ReparationPlatform GetFirstAvailablePlatform()
    {
        foreach (ReparationPlatform p in _targets)
        {
            if (!p.Unlocked) { continue; }
            if (p.Taken) continue;
            return p;
        }

        return null;
    }

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(_spawnPoint, Vector3.one);
        Gizmos.DrawWireCube(_WaitPoint1, Vector3.one);
        Gizmos.DrawWireCube(_endPoint, Vector3.one);

        Gizmos.color = Color.green;
        foreach (var target in _targets)
        {
            if (target.Taken) { Gizmos.color = Color.gold; }
            if (!target.Unlocked) { Gizmos.color = Color.darkRed; }

            Gizmos.DrawSphere(target.Target.position, 0.5f);
        }
    }
}
