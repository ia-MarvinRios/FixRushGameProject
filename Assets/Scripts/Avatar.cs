using UnityEngine;

public class Avatar : MonoBehaviour
{
    private const string LOG_FORMAT = "<color=#27F5B7>[Avatar]</color>";

    [Header("References")]
    [SerializeField] private PlayerSettings _settings;
    [SerializeField] private Animator _animator;
    [SerializeField] private PlayerController _controller;

    internal void Initialize()
    {
        Debug.Log($"{LOG_FORMAT} ({name}) Initializing avatar...");

        _controller = GetComponentInParent<PlayerController>();
        _controller.Avatar = this;

        Debug.Log($"{LOG_FORMAT} Done!");
    }

    internal void ProcessAnimations()
    {
        return;
    }
}
