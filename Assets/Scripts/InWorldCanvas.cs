using Unity.VisualScripting;
using UnityEngine;

public class InWorldCanvas : MonoBehaviour
{
    public static InWorldCanvas Instance { get; private set; }

    [Header("Settings")]
    [Tooltip("And 'In world' UI item prefab to spawn and delete to warm up the canvas.")]
    [SerializeField] GameObject _warmUp;
    [SerializeField] GameObject[] _tooltipPrefabs;

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        if (_warmUp == null)
        {
            Debug.LogWarning("WarmUp prefab not assigned.");
            return;
        }
        GameObject warmup = Instantiate(_warmUp, transform);
        warmup.name = "Warmup tooltip";
        warmup.SetActive(false);
    }

    /// <summary>
    /// Instantiates a copy of the specified GameObject as a child of this object's transform.
    /// </summary>
    /// <param name="item">The GameObject to instantiate. Cannot be null.</param>
    /// <returns>A new GameObject instance that is a copy of the specified item, parented to this object's transform.</returns>
    public GameObject InstatiateItem(GameObject item) { return Instantiate(item, transform); }
    public GameObject InstatiateItem(int itemIndex) { if (_tooltipPrefabs.Length <= 0) return null; return Instantiate(_tooltipPrefabs[itemIndex], transform); }
}
