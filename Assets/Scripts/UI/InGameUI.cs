using FixRush;
using TMPro;
using UnityEngine;

public class InGameUI : MonoBehaviour
{
    public static InGameUI Instance {  get; private set; }

    [Header("Game Content Reference")]
    [SerializeField] private GameContent _gameContent;

    [Header("UI Elements")]
    [SerializeField] private GameObject _pauseMenuPanel;
    [SerializeField] private TMP_Text _taskPanelText;
    [SerializeField] internal GameObject Manual;

    [Header("Animations")]
    [SerializeField] private Animator _animator;

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

    internal void LinkNetworkHandler(PlayerNetworkHandler networkHandler) { NetworkHandler = networkHandler; }

}
