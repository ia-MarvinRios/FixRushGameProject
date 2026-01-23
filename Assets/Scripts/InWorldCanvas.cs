using Unity.VisualScripting;
using UnityEngine;

public class InWorldCanvas : MonoBehaviour
{
    public static InWorldCanvas Instance { get; private set; }

    [Tooltip("And 'In world' UI item prefab to spawn and delete to warm up the canvas.")]
    [SerializeField] GameObject _warmUp;

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
        var warmup = Instantiate(_warmUp, transform);
        warmup.SetActive(false);
    }

    public GameObject InstatiateItem(GameObject item) { return Instantiate(item, transform); }
}
