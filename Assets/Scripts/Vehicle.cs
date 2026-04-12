using FixRush;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class Vehicle : MonoBehaviour
{
    [Header("Vehicle Settings")]
    [SerializeField] internal int QueueIndex = -1;

    [Header("References")]
    [SerializeField] internal NavMeshAgent Agent;
    [SerializeField] private BoxCollider _collider;
    [SerializeField] private IIssue.Type[] _issues;

    internal Vector3 Size => _collider.size;
    internal IIssue.Type[] Issues { get => _issues; set => _issues = value; }
    internal IIssue CurrentIssue { get; set; }

    private void Awake()
    {
        if (!PhotonManager.Instance.IsMasterClient)
        {
            Agent.enabled = false;
            enabled = false;
            return;
        }
    }

    internal void InitializeVehicle()
    {
        return;
    }

    internal void MoveTo(Vector3 destination)
    {
        Agent.SetDestination(destination);
    }

    public void Fix()
    {
        VManager.Instance.RemoveVehicle(this);
        //IsFixed = true;
    }
}
