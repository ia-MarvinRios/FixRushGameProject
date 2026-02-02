using FixRushGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class VManager : MonoBehaviour
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
    [SerializeField] Transform _target;

    NavMeshAgent _leader;
    bool _readyToFix = false;
    private static IssueType[] _allIssues = (IssueType[])System.Enum.GetValues(typeof(IssueType));
    List<IssueType> selected = new List<IssueType>();

    public List<GameObject> ActiveVehicles { get; private set; }

    private void Awake()
    {
        Instance = this;
        ActiveVehicles = new List<GameObject>();
    }

    private void Start()
    {
        SpawnVehicles();
    }

    void SpawnVehicles()
    {
        StartCoroutine(SpawnVehiclesCoroutine());
    }

    IssueType[] AssignIssues(Vehicle v)
    {
        selected.Clear();

        int t = Random.Range(1, 3);

        for (int i = 0; i < t; i++)
        {
            IssueType issue = GetRandomIssue();
            if (!selected.Contains(issue))
                selected.Add(issue);
        }

        v.Issues = selected.ToArray();
        return v.Issues;
    }

    IssueType GetRandomIssue()
    {
        return _allIssues[Random.Range(0, _allIssues.Length)];
    }

    IEnumerator SpawnVehiclesCoroutine()
    {
        while (true)
        {
            if (ActiveVehicles.Count < _maxInstances)
            {
                GameObject obj = Instantiate(
                    _vehiclePrefabs[Random.Range(0, _vehiclePrefabs.Length)],
                    _spawnPoint,
                    Quaternion.Euler(0, -90, 0),
                    transform);

                Vehicle v = obj.GetComponent<Vehicle>();

                // Assign issues and show UI
                UIManager.Instance.AddIssuesCard(AssignIssues(v));

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

    float GetQueueDistance(Vector3 size, int index)
    {
        return (size.z + _waitPointOffset) * index;
    }
    public float GetPathLength(NavMeshPath path)
    {
        float length = 0f;

        for (int i = 1; i < path.corners.Length; i++)
        {
            length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }

        return length;
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

    public void RemoveVehicle(Vehicle v)
    {
        int index = ActiveVehicles.IndexOf(v.gameObject);

        if (index < 0)
            return;

        ActiveVehicles.RemoveAt(index);

        UpdateLeader();
        Destroy(v.gameObject); // destroy after getting index

        // Update UI
        UIManager.Instance.RemoveIssuesCard(UIManager.Instance.ActiveCards[index]);

        _readyToFix = false;

        if (_leader != null && _leader.isOnNavMesh)
        {
            RecalculateQueueFrom(index);
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
    public IEnumerator MoveToEndPoint(NavMeshAgent agent, Car car)
    {
        agent.isStopped = false;
        agent.SetDestination(_endPoint);
        Debug.Log("Moving to endpoint...");
        yield return new WaitUntil(() => HasReachedDestination(agent));
        car.Fix();
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
            v.Initialize();
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

    public bool HasReachedDestination(NavMeshAgent agent)
    {
        if (!agent || !agent.enabled || !agent.isOnNavMesh)
            return false;

        if (agent.pathPending || 
            agent.remainingDistance > agent.stoppingDistance + 0.05f || 
            agent.hasPath && agent.velocity.sqrMagnitude > 0.01f)
            return false;

        return true;
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
