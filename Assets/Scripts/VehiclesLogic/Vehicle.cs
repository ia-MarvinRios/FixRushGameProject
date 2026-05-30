using FixRush;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.VFX;

[RequireComponent(typeof(vNetworkHandler))]
public abstract class Vehicle : MonoBehaviour, AIExtension.IQueueAgent
{
    [Header("Vehicle Settings")]
    [SerializeField] private int _queueIndex = -1;
    [SerializeField] internal Gradient PatienceGradient;

    [Header("Vechicle References")]
    [SerializeField] internal vNetworkHandler NetworkHandler;
    [SerializeField] internal NavMeshAgent Agent;
    [SerializeField] internal Animator mAnimator;
    [SerializeField] private BoxCollider _collider;
    [SerializeField] private DecalProjector _dirtDecal;
    [SerializeField] private Material _dirtMaterial;
    [SerializeField] private VisualEffect _sudsParticles;
    [SerializeField] private Transform _sliderRoot;
    [SerializeField] internal GameObject TriggerPrefab;
    [SerializeField] private IIssue.Type[] _issueTypes;

    private bool _isInitialized = false;
    internal Slider PatienceSlider;
    internal Image SliderFillImage;

    public int QueueIndex
    {
        get => _queueIndex; 
        set => _queueIndex = value; 
    }
    public Vector3 Size => _collider.size;
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
    private void OnEnable()
    {
        // Set references to impatience slider UI
        PatienceSlider = InWorldCanvas.Instance.CreatePatienceSlider(_sliderRoot);
        SliderFillImage = PatienceSlider.fillRect.GetComponent<Image>();
    }
    private void Start()
    {
        SetupDirt();

        if (!PhotonManager.Instance.IsMasterClient) { return; }
        TimeOutCountdown();
    }

    public virtual void InitializeVehicle()
    {
        if (PhotonManager.Instance.IsMasterClient && !_isInitialized)
        {
            _isInitialized = true;
            StopAllCoroutines();
            NetworkHandler.SyncInitialization();
        }
    }

    public void Fix()
    {
        if (!PhotonManager.Instance.IsMasterClient) { return; }

        // Set fixed and destroy.
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

    internal void ShowSuds(float duration) { _sudsParticles.Play(); }
    internal void HideSuds() { _sudsParticles.Stop(); }

    internal void TimeOutCountdown()
    {
        int   totalSeconds       = QueueIndex * 60 + IssueTypes.Length * 60;
        float difficultyModifier = GameManager.Instance.LevelData.LevelDifficulty * 2f * 0.01f;

        float countdownSecs = totalSeconds - totalSeconds * difficultyModifier;

        StartCoroutine(CountdownCoroutine(countdownSecs));
    }

    private IEnumerator CountdownCoroutine(float startSeconds)
    {
        float seconds = startSeconds;

        while (seconds > 0)
        {
            // Pause countdown on moving
            if (!AIExtension.HasReachedDestination(Agent))
            {
                yield return null;
                continue;
            }

            seconds -= Time.deltaTime;
            PatienceSlider.value = seconds / startSeconds;
            SliderFillImage.color = PatienceGradient.Evaluate(seconds / startSeconds);

            yield return null;
        }

        NetworkHandler.TimeOut();
    }

    internal void TimeOut()
    {
        AudioManager.Instance.PlaySoundByName("Error");

        if (!PhotonManager.Instance.IsMasterClient) { return; }
        VManager.Instance.MoveToEndPoint(this);
    }
}
