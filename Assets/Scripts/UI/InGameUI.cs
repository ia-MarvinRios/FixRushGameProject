using FixRush;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
    public static InGameUI Instance {  get; private set; }

    [Header("Game Content Reference")]
    [SerializeField] private GameContent _gameContent;

    [Header("UI Elements")]
    [SerializeField] private GameObject _pauseMenuPanel;
    [SerializeField] private TMP_Text _taskPanelText;
    [SerializeField] private TMP_Text _hintDescription;
    [SerializeField] private GameObject _taskProgressPanel;
    [SerializeField] private Slider _taskProgressSlider;
    [SerializeField] internal GameObject Manual;

    [Header("Animations")]
    [SerializeField] private Animator _animator;

    // Tasks
    private float _currentProgress = 0f;
    private float _modifier = 1f;
    private Coroutine _taskProgressCoroutine;

    internal PlayerNetworkHandler NetworkHandler { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Audio
        AudioManager.Instance.PlayAllMusic(true);
    }

    public void MainMenu()
    {
        PhotonManager.Instance.LeaveRoom();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void TogglePauseMenu()
    {
        _pauseMenuPanel.SetActive(!_pauseMenuPanel.activeSelf);

    }

    public void ShowTaskPanel(bool show, IIssue.Type issueType = IIssue.Type.Dirty)
    {
        _animator.SetBool("ShowTaskPanel", show);

        string description = issueType switch {
            IIssue.Type.Dirty => "Car is dirty",
            IIssue.Type.Tires => "Replace Tires",
            _ => "unknown"
        };

        _taskPanelText.text = show ? description : string.Empty;

        if (!show)
        {
            // Audio
            AudioManager.Instance.PlaySoundByName("TaskCompleted");
        }

    }

    public void ShowHint(string hint, float duration = 2f)
    {
        _hintDescription.text = hint;

        StartCoroutine(ShowHintCoroutine(duration));
    }
    private IEnumerator ShowHintCoroutine(float delay)
    {
        _animator.SetBool("ShowHint", true);

        yield return new WaitForSeconds(delay);

        _animator.SetBool("ShowHint", false);
    }

    public void StartTaskProgress(float duration)
    {
        _currentProgress = 0f;
        _taskProgressSlider.value = _currentProgress;

        _taskProgressCoroutine = StartCoroutine(TaskProgressCoroutine(duration));
    }
    public void StopTaskProgress(bool succesful)
    {
        if (_taskProgressCoroutine != null)
        {
            StopCoroutine(_taskProgressCoroutine);
            _taskProgressCoroutine = null;
        }
        _currentProgress = 0f;
        _taskProgressSlider.value = _currentProgress;
        _taskProgressPanel.SetActive(false);

        if (succesful)
        {
            AudioManager.Instance.PlaySoundByName("TaskCompleted");
        }
    }
    private IEnumerator TaskProgressCoroutine(float duration)
    {
        _taskProgressPanel.SetActive(true);

        while (_currentProgress < 1f)
        {
            _currentProgress += Time.deltaTime / duration * _modifier;
            _taskProgressSlider.value = _currentProgress;
            yield return null;
        }

        StopTaskProgress(true);
    }

    internal void LinkNetworkHandler(PlayerNetworkHandler networkHandler) { NetworkHandler = networkHandler; }

}
