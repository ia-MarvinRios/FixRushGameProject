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
    [SerializeField] Transform[] _targets;

    [Header("Level Data Reference")]
    [SerializeField] private LevelData _levelData;

    NavMeshAgent _leader;
    Coroutine _spawnCoroutine;
    WaitForSeconds _spawnIntervalWaitTime;
    bool _readyToFix = false;

    int _prefabIndex = -1;
    Vehicle _temp = null;

    internal List<GameObject> ActiveVehicles { get; private set; }

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

        ActiveVehicles = new List<GameObject>();
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
            if (ActiveVehicles.Count < _maxInstances)
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
                    new object[] { IssuesToIntArray(AssignIssues(_temp)) }
                );

                if (obj == null) { continue; }

                // Get the Vehicle component from the instantiated object
                Vehicle v = obj.GetComponent<Vehicle>();

                // Add first
                ActiveVehicles.Add(obj);
                v.QueueIndex = ActiveVehicles.Count - 1;

                // Update Leader
                UpdateLeader();

                // Recalc only from the new
                if (_leader != null && _leader.isOnNavMesh)
                {
                    AIExtension.RecalculateQueueFrom(
                        v.QueueIndex, 
                        _waitPointOffset, 
                        _WaitPoint1,
                        _leader,
                        ObjectsToAgentsList(ActiveVehicles));
                }

                yield return _spawnIntervalWaitTime;
            }

            if (ActiveVehicles.Count > 0)
                MoveToTargetPos(ActiveVehicles[0].GetComponent<NavMeshAgent>());

            yield return new WaitForSeconds(1.5f);
        }
    }

    #region SPAWN_LOGIC

    public void UpdateLeader()
    {
        _leader = null;

        if (ActiveVehicles.Count == 0)
            return;

        NavMeshAgent agent =
            ActiveVehicles[^1].GetComponent<NavMeshAgent>();

        if (agent != null && agent.enabled && agent.isOnNavMesh)
            _leader = agent;
    }

    void MoveToTargetPos(NavMeshAgent agent)
    {
        if (AIExtension.HasReachedDestination(agent) && !_readyToFix)
        {
            agent.SetDestination(_targets[0].position);
            StartCoroutine(SetUpForFixing(agent));
        }
    }

    public Coroutine MoveToEndPoint(Vehicle v)
    {
        NavMeshAgent agent = v.Agent;
        return StartCoroutine(MoveToEndPointCoroutine(agent, v));
    }

    IEnumerator SetUpForFixing(NavMeshAgent agent)
    {
        if (agent == null)
        {
            Debug.LogWarning("Trying to set up vehicle but it's reference is null.");
            yield break;
        }

        yield return new WaitUntil(() => AIExtension.HasReachedDestination(agent));

        agent.isStopped = true;
        agent.transform.position = _targets[0].position;
        agent.transform.rotation = _targets[0].rotation;
        _readyToFix = true;


        if (agent.TryGetComponent(out Vehicle v))
            v.InitializeVehicle();

    }

    public IEnumerator MoveToEndPointCoroutine(NavMeshAgent agent, Vehicle v)
    {
        agent.isStopped = false;
        agent.SetDestination(_endPoint);
        Debug.Log("Moving to endpoint...");
        yield return new WaitUntil(() => AIExtension.HasReachedDestination(agent));
        v.Fix();
    }

    public void RemoveVehicle(Vehicle v)
    {
        int index = ActiveVehicles.IndexOf(v.gameObject);

        if (index < 0)
            return;

        ActiveVehicles.RemoveAt(index);

        UpdateLeader();

        // Destroy after getting index
        PhotonNetwork.Destroy(v.gameObject);

        _readyToFix = false;

        if (_leader != null && _leader.isOnNavMesh)
        {
            AIExtension.RecalculateQueueFrom(
                index,
                _waitPointOffset,
                _WaitPoint1,
                _leader,
                ObjectsToAgentsList(ActiveVehicles)
            );
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

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(_spawnPoint, Vector3.one);
        Gizmos.DrawWireCube(_WaitPoint1, Vector3.one);
        Gizmos.DrawWireCube(_endPoint, Vector3.one);

        Gizmos.color = Color.gold;
        foreach (var target in _targets)
        {
            Gizmos.DrawSphere(target.position, 0.5f);
        }
    }
}
