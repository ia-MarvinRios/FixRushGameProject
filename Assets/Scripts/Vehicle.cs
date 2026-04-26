using FixRush;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;

public class Vehicle : MonoBehaviour
{
    [Header("Vehicle Settings")]
    [SerializeField] internal int QueueIndex = -1;

    [Header("References")]
    [SerializeField] internal NavMeshAgent Agent;
    [SerializeField] private BoxCollider _collider;
    [SerializeField] private DecalProjector _dirtDecal;
    [SerializeField] private Material _dirtMaterial;
    [SerializeField] private IIssue.Type[] _issues;

    internal Vector3 Size => _collider.size;
    internal IIssue.Type[] Issues { get => _issues; set => _issues = value; }
    internal IIssue CurrentIssue { get; set; }
    internal float DirtAlpha { get => _dirtMaterial.GetFloat("_Alpha"); set => _dirtMaterial.SetFloat("_Alpha", value); }

    private void Awake()
    {
        // Disable some components for clients
        if (!PhotonManager.Instance.IsMasterClient)
        {
            Agent.enabled = false;
            enabled = false;
            return;
        }
    }
    private void Start()
    {
        // Enable dirt decal if issue was added
        for (int i = 0; i < _issues.Length; i++)
        {
            if (_issues[i] == IIssue.Type.Dirty) { _dirtDecal.gameObject.SetActive(true); break; }
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
