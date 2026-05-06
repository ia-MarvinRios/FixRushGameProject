using UnityEngine;

public class InWorldCanvas : MonoBehaviour
{
    public static InWorldCanvas Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject _objectSelectorPrefab;
    [SerializeField] private Camera _mainCamera;

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
}
