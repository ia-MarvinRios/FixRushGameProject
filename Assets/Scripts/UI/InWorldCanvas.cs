using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InWorldCanvas : MonoBehaviour
{
    public static InWorldCanvas Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject _objectSelectorPrefab;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Animator _scenarioAnimator;
    [SerializeField] private Transform _gate;

    [Header("UI Prefabs")]
    [SerializeField] private Slider _patienceSliderPrefab;

    private Dictionary<GameObject, Transform> _dynamicElements = new Dictionary<GameObject, Transform>();
    private List<GameObject> _toRemove                         = new List<GameObject>();

    internal GameObject Selector { get; private set; }

    private void Awake()
    {
        Instance = this;

        Selector = Instantiate(_objectSelectorPrefab, transform);
        Selector.SetActive(false);
    }

    private void LateUpdate()
    {
        if (Selector.activeSelf)
        {
            Selector.transform.LookAt(_mainCamera.transform);
        }

        UpdateDynamicElements();
    }

    private void UpdateDynamicElements()
    {
        foreach (KeyValuePair<GameObject, Transform> element in _dynamicElements)
        {
            if (element.Key == null || element.Value == null)
            {
                _toRemove.Add(element.Key);
                continue;
            }

            element.Key.transform.position = element.Value.position;
            element.Key.transform.LookAt(_mainCamera.transform);
        }

        foreach (GameObject key in _toRemove)
        {
            Destroy(key);
            _dynamicElements.Remove(key);
        }
    }

    public void ShowSelector(bool show)
    {
        Selector.SetActive(show);
    }
    public void SetSelector(Vector3 position)
    {
        Selector.SetActive(true);

        Selector.transform.position = position;
    }

    public Slider CreatePatienceSlider(Transform target)
    {
        Slider slider = Instantiate(
            _patienceSliderPrefab, 
            transform
        );

        _dynamicElements.Add(
            slider.gameObject, 
            target
        );

        return slider;
    }
    public void RemovePatienceSlider(Slider slider) { _dynamicElements.Remove(slider.gameObject); }

    public void RequestUnlockGate()
    {
        AudioManager.Instance.PlayOnTarget("GateOpening", _gate);
        _scenarioAnimator.SetTrigger("OpenGate");
    }
}
