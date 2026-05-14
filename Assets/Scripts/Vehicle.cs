using FixRush;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;

[RequireComponent(typeof(vNetworkHandler))]
public abstract class Vehicle : MonoBehaviour
{
    [Header("Vehicle Settings")]
    [SerializeField] internal int QueueIndex = -1;

    [Header("Vechicle References")]
    [SerializeField] internal vNetworkHandler NetworkHandler;
    [SerializeField] internal NavMeshAgent Agent;
    [SerializeField] private BoxCollider _collider;
    [SerializeField] private DecalProjector _dirtDecal;
    [SerializeField] private Material _dirtMaterial;
    [SerializeField] private VisualEffect _sudsParticles;
    [SerializeField] internal GameObject TriggerPrefab;
    [SerializeField] private IIssue.Type[] _issueTypes;

    private bool _isInitialized = false;

    internal Vector3 Size => _collider.size;
    internal IIssue.Type[] IssueTypes
    { 
        get => _issueTypes; 
        set => _issueTypes = value;
    }
    internal IIssue CurrentIssue { get; set; }
    internal float DirtAlpha 
    { 
        get => _dirtMaterial.GetFloat("_Alpha"); 
        set => _dirtMaterial.SetFloat("_Alpha", value); 
    }
    internal bool IsFixed = false;

    private void Awake()
    {
        // Disable some components for clients
        if (!PhotonManager.Instance.IsMasterClient)
        {
            Agent.enabled = false;
            return;
        }
    }
    private void Start()
    {
        SetupDirt();
    }

    public virtual void InitializeVehicle()
    {
        if (PhotonManager.Instance.IsMasterClient && !_isInitialized)
        {
            _isInitialized = true;
            NetworkHandler.SyncInitialization();
        }
    }

    public void Fix()
    {
        if (!PhotonManager.Instance.IsMasterClient) { return; }

        VManager.Instance.RemoveVehicle(this);
        IsFixed = true;

        // Audio
        AudioManager.Instance.PlaySoundByName("CarDone");
    }

    internal void MoveTo(Vector3 destination)
    {
        if (!PhotonManager.Instance.IsMasterClient) { return; }

        Agent.SetDestination(destination);
    }

    internal void SetupDirt()
    {
        for (int i = 0; i < _issueTypes.Length; i++)
        {
            // Enable dirt decal if issue was added and create it's material instance
            if (_issueTypes[i] == IIssue.Type.Dirty)
            {
                // Create
                Material newMat = new Material(_dirtMaterial);

                // Assign
                _dirtMaterial = newMat;
                _dirtDecal.material = _dirtMaterial;

                // Show
                _dirtDecal.gameObject.SetActive(true);

                break;
            }
        }
    }

    internal void ShowSuds(float duration)
    {
        _sudsParticles.Play();
    }
    internal void HideSuds()
    {
        _sudsParticles.Stop();
    }
}
