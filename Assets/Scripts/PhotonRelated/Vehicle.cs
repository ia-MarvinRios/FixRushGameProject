using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(VehicleNetwork))]
public abstract class Vehicle : MonoBehaviour
{
    [Header("Vehicle Settings")]
    [Space(5)]
    [SerializeField] private Transform _modelObject;
    [SerializeField] private Collider _vehicleCollider;
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private IssueType[] _issues;

    VehicleNetwork _vehicleNetwork;
    IIssue _currentIssue = null;

    public bool IsFixed {  get; set; }
    public IssueType[] Issues { get { return _issues; } internal set { _issues = value; } }
    public Transform Model { get { return _modelObject; } }
    public BoxCollider Collider { get { return (BoxCollider)_vehicleCollider; } }
    public Vector3 Size { get { return _vehicleCollider.bounds.size; } }
    public NavMeshAgent Agent { get { return _agent; } }
    public int QueueIndex { get; set; }
    internal VehicleNetwork VehicleNetwork { get { return _vehicleNetwork; } }
    public IIssue CurrentIssue { get { return _currentIssue; } internal set { _currentIssue = value; } }

    private void OnEnable()
    {
        _vehicleNetwork = GetComponent<VehicleNetwork>();

        if (!VehicleNetwork.IsMaster)
            Agent.enabled = false;
    }

    private void OnDestroy()
    {
        Fix();
    }

    public abstract void Initialize();
    public void MoveTo(Vector3 target) { _agent.SetDestination(target); }
    public void Fix() {
        VManager.Instance.RemoveVehicle(this);
        IsFixed = true;
    }
}
