using FixRushGame;
using UnityEngine;
using UnityEngine.AI;

public abstract class Vehicle : MonoBehaviour
{
    [Header("Vehicle Settings")]
    [Space(5)]
    [SerializeField] private Transform _modelObject;
    [SerializeField] private Collider _vehicleCollider;
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private IssueType[] _issues;

    public IssueType[] Issues { get { return _issues; } internal set { _issues = value; } }
    public Transform Model {  get { return _modelObject; } }
    public BoxCollider Collider { get { return (BoxCollider)_vehicleCollider; } }
    public Vector3 Size { get { return _vehicleCollider.bounds.size; } }
    public NavMeshAgent Agent { get { return _agent; } }
    public int QueueIndex { get; set; }

    private void OnDestroy()
    {
        Fix();
    }

    public abstract void Initialize();
    public void MoveTo(Vector3 target) { _agent.SetDestination(target); }
    public void Fix() { VManager.Instance.RemoveVehicle(this); }

    public Vector3 GetNearestObjFromArray(Vector3 refPosition, Vector3[] objectsPositions)
    {
        if (objectsPositions == null || objectsPositions.Length <= 0)
        {
            Debug.LogWarning($"Array is null or empty. [GetNearestObjFromArray] called from '{gameObject.name}'");
            return Vector3.zero;
        }

        Vector3 nearest = objectsPositions[0];
        float best = (nearest - refPosition).sqrMagnitude;

        for (int i = 1; i < objectsPositions.Length; i++)
        {
            float d = (objectsPositions[i] - refPosition).sqrMagnitude;

            if (d < best)
            {
                best = d;
                nearest = objectsPositions[i];
            }
        }

        return nearest;
    }
}
