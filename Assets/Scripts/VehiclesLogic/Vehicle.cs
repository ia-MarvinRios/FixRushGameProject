using UnityEngine;
using UnityEngine.AI;

public abstract class Vehicle : MonoBehaviour
{
    [Header("Vehicle Settings")]
    [Space(5)]
    [SerializeField] private Collider _vehicleCollider;
    [SerializeField] private NavMeshAgent _agent;

    public Vector3 Size { get { return _vehicleCollider.bounds.size; } }
    public NavMeshAgent Agent { get { return _agent; } }
    public int QueueIndex { get; set; }

    private void OnDestroy()
    {
        VManager.Instance.RemoveVehicle(this);
    }

    public abstract void Initialize();
    public void MoveTo(Vector3 target) { _agent.SetDestination(target); }

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
