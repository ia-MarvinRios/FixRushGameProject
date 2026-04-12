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

    internal List<GameObject> ActiveVehicles { get; private set; }

    // Issues (!!!Make sure not to set more max issues than the available issue types)
    private const int MAX_ISSUES = 2;
    private const int MIN_ISSUES = 1;
    private static IIssue.Type[] _allIssues = (IIssue.Type[])System.Enum.GetValues(typeof(IIssue.Type));
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

                GameObject obj = PhotonNetwork.InstantiateRoomObject(
                    _vehiclePrefabs[Random.Range(0, _vehiclePrefabs.Length)].name,
                    _spawnPoint,
                    Quaternion.identity,
                    0,
                    new object[] { IssuesToIntArray(AssignIssues()) }
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
                    RecalculateQueueFrom(v.QueueIndex);
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
        if (HasReachedDestination(agent) && !_readyToFix)
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

        yield return new WaitUntil(() => HasReachedDestination(agent));

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
        yield return new WaitUntil(() => HasReachedDestination(agent));
        //v.Fix();
    }


    /// <summary>
    /// Recalculates the positions of vehicles in the queue starting from a specific index.
    /// </summary>
    /// <param name="startIndex">The index from which to start recalculating the queue.</param>
    public void RecalculateQueueFrom(int startIndex)
    {
        NavMeshPath path = new NavMeshPath();

        if (!_leader.CalculatePath(_WaitPoint1, path))
            return;

        float pathLength = GetPathLength(path);

        for (int i = startIndex; i < ActiveVehicles.Count; i++)
        {
            Vehicle v = ActiveVehicles[i].GetComponent<Vehicle>();
            v.QueueIndex = i;

            float distance = GetQueueDistance(v.Size, i);
            distance = Mathf.Min(distance, pathLength - v.Size.z);

            Vector3 point =
                GetPointFromPathEnd(path, distance);

            v.MoveTo(point);
        }
    }
    /// <summary>
    /// Gets the distance along the path for a vehicle based on its size and position in the queue.
    /// </summary>
    /// <param name="size">The size of the vehicle.</param>
    /// <param name="index">The index of the vehicle in the queue.</param>
    /// <returns>The distance along the path for the vehicle.</returns>
    float GetQueueDistance(Vector3 size, int index) { return (size.z + 0.5f + _waitPointOffset) * index; }
    /// <summary>
    /// Gets the total length of a NavMeshPath by summing the distances between its corners.
    /// </summary>
    /// <param name="path">The NavMeshPath to calculate the length of.</param>
    /// <returns>The total length of the path.</returns>
    float GetPathLength(NavMeshPath path)
    {
        float length = 0f;

        for (int i = 1; i < path.corners.Length; i++)
        {
            length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }

        return length;
    }
    /// <summary>
    /// Gets a point along a NavMeshPath that is a certain distance from the end of the path.
    /// </summary>
    /// <param name="path">The NavMeshPath to calculate the point on.</param>
    /// <param name="distanceFromEnd">The distance from the end of the path.</param>
    /// <returns>The point along the path at the specified distance from the end.</returns>
    Vector3 GetPointFromPathEnd(NavMeshPath path, float distanceFromEnd)
    {
        float remaining = distanceFromEnd;

        for (int i = path.corners.Length - 1; i > 0; i--)
        {
            float segmentLength =
                Vector3.Distance(path.corners[i], path.corners[i - 1]);

            if (remaining <= segmentLength)
            {
                Vector3 dir =
                    (path.corners[i - 1] - path.corners[i]).normalized;

                return path.corners[i] + dir * remaining;
            }

            remaining -= segmentLength;
        }

        return path.corners[0];
    }

    bool HasReachedDestination(NavMeshAgent agent)
    {
        if (!agent || !agent.enabled || !agent.isOnNavMesh)
            return false;

        if (agent.pathPending ||
            agent.remainingDistance > agent.stoppingDistance + 0.05f ||
            agent.hasPath && agent.velocity.sqrMagnitude > 0.01f)
            return false;

        return true;
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

        // Update UI
        //UIManager.Instance.RemoveIssuesCard(UIManager.Instance.ActiveCards[index]);

        _readyToFix = false;

        if (_leader != null && _leader.isOnNavMesh)
        {
            RecalculateQueueFrom(index);
        }
    }

    IIssue.Type[] AssignIssues()
    {
        _selected.Clear();

        // Calculate the number of issues based on the current level difficulty
        // FORMULA: (MAX_ISSUES * CurrentDifficulty) / MAX_DIFFICULTY
        float issuesCount = MAX_ISSUES * (float)_levelData.LevelDifficulty / LevelData.MAX_DIFFICULTY;
        issuesCount = (int)System.Math.Round(issuesCount, System.MidpointRounding.AwayFromZero);

        while (_selected.Count < issuesCount)
        {
            IIssue.Type issue = GetRandomIssue();
            _selected.Add(issue);
        }

        return new List<IIssue.Type>(_selected).ToArray();
    }
    IIssue.Type GetRandomIssue()
    {
        return _allIssues[Random.Range(0, _allIssues.Length)];
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
