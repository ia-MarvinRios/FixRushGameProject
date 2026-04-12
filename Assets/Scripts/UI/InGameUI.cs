using UnityEngine;

public class InGameUI : MonoBehaviour
{
    public static InGameUI Instance {  get; private set; }

    [Header("Game Content Reference")]
    [SerializeField] private GameContent _gameContent;

    [Header("UI Elements")]
    [SerializeField] private GameObject _pauseMenuPanel;

    internal PlayerNetworkHandler NetworkHandler { get; private set; }

    private void Awake()
    {
        Instance = this;
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

    internal void LinkNetworkHandler(PlayerNetworkHandler networkHandler) { NetworkHandler = networkHandler; }

}
