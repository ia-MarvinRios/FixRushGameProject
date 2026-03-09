using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class VManager : MonoBehaviour
{
    private VManagerNetwork _vManagerNetwork;
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
    [SerializeField] Transform _target;

    NavMeshAgent _leader;
    bool _readyToFix = false;
    Coroutine _spawnCoroutine;

    static IssueType[] _allIssues = (IssueType[])System.Enum.GetValues(typeof(IssueType));
    HashSet<IssueType> _selected = new HashSet<IssueType>();


    internal List<GameObject> ActiveVehicles { get; private set; }
    internal GameObject[] VehiclePrefabs { get => _vehiclePrefabs; }
    internal Vector3 SpawnPoint { get => _spawnPoint; }
    internal Quaternion SpawnRotation { get => Quaternion.Euler(0, -90, 0); }

    private void Awake()
    {
        _vManagerNetwork = GetComponent<VManagerNetwork>();
        
        Instance = this;
        ActiveVehicles = new List<GameObject>();
    }

    public void SpawnVehicles()
    {
        if (_spawnCoroutine != null) return;

        if (!_vManagerNetwork.CanSpawn()) return;

        _spawnCoroutine = StartCoroutine(SpawnVehiclesCoroutine());
    }
    
    IEnumerator SpawnVehiclesCoroutine()
    {
        while (true)
        {
            if (ActiveVehicles.Count < _maxInstances)
            {
                Debug.Log("[VManager] Spawning vehicle...");
                GameObject obj = _vManagerNetwork.SpawnVehicle(
                    _vehiclePrefabs, 
                    _spawnPoint, 
                    SpawnRotation, 
                    AssignIssues(),
                    false
                );

                _vManagerNetwork.AttachVehicle(obj);

                if (obj == null)
                    continue;

                Vehicle v = obj.GetComponent<Vehicle>();

                // Assign issues and show UI
                //UIManager.Instance.AddIssuesCard(v.Issues);

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

                yield return new WaitForSeconds(_spawnInterval);
            }

            if (ActiveVehicles.Count > 0)
                MoveToTargetPos(ActiveVehicles[0].GetComponent<NavMeshAgent>());

            yield return new WaitForSeconds(0.1f);
        }
    }

    public void RemoveVehicle(Vehicle v)
    {
        int index = ActiveVehicles.IndexOf(v.gameObject);

        if (index < 0)
            return;

        ActiveVehicles.RemoveAt(index);

        UpdateLeader();
        _vManagerNetwork.DestroyVehicle(v.gameObject); // destroy after getting index

        // Update UI
        //UIManager.Instance.RemoveIssuesCard(UIManager.Instance.ActiveCards[index]);

        _readyToFix = false;

        if (_leader != null && _leader.isOnNavMesh)
        {
            RecalculateQueueFrom(index);
        }
    }

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

    void MoveToTargetPos(NavMeshAgent agent)
    {
        if (HasReachedDestination(agent) && !_readyToFix)
        {
            agent.SetDestination(_target.position);
            StartCoroutine(SetUpForFixing(agent));
        }
    }

    public Coroutine MoveToEndPoint(Vehicle v)
    {
        NavMeshAgent agent = v.Agent;
        return StartCoroutine(MoveToEndPointCoroutine(agent, v));
    }

    public IEnumerator MoveToEndPointCoroutine(NavMeshAgent agent, Vehicle v)
    {
        agent.isStopped = false;
        agent.SetDestination(_endPoint);
        Debug.Log("Moving to endpoint...");
        yield return new WaitUntil(() => HasReachedDestination(agent));
        v.Fix();
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
        agent.transform.position = _target.position;
        agent.transform.rotation = _target.rotation;
        _readyToFix = true;

        if (agent.TryGetComponent(out Vehicle v))
            v.VehicleNetwork.InitializeVehicle();

    }

    float GetQueueDistance(Vector3 size, int index) { return (size.z + _waitPointOffset) * index; }
    float GetPathLength(NavMeshPath path)
    {
        float length = 0f;

        for (int i = 1; i < path.corners.Length; i++)
        {
            length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }

        return length;
    }
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

    IssueType[] AssignIssues()
    {
        _selected.Clear();

        int t = Random.Range(1, 3);

        while (_selected.Count < t)
        {
            IssueType issue = GetRandomIssue();
            _selected.Add(issue);
        }

        return new List<IssueType>(_selected).ToArray();
    }
    IssueType GetRandomIssue()
    {
        return _allIssues[Random.Range(0, _allIssues.Length)];
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(_spawnPoint, Vector3.one);
        Gizmos.DrawWireCube(_WaitPoint1, Vector3.one);
        Gizmos.DrawWireCube(_endPoint, Vector3.one);
        Gizmos.color = Color.gold;
        Gizmos.DrawSphere(_target.position, 1f);
    }

}
