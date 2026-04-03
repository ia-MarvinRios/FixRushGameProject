using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    [Header("MainMenu References")]
    [SerializeField] private GameObject _mainMenuPanel;
    [SerializeField] private Animator _uiAnimator;
    [SerializeField] private TMP_InputField _nicknameField;
    [SerializeField] private TMP_Text _buildInfo;
    [Header("RoomSelector References")]
    [SerializeField] private GameObject _roomSelectionPanel;
    [Header("Lobby References")]
    [SerializeField] private GameObject _lobbyPanel;
    [SerializeField] private GameObject _startGameButton;
    [SerializeField] private TMP_Text _lobbyTitle;
    [SerializeField] private Transform _playerListPanel;
    [SerializeField] private GameObject _playerRoomCardPrefab;

    private bool _isReady = false;
    private List<GameObject> _playerCards = new List<GameObject>();

    private void Start()
    {
        PhotonManager.Instance.OnPhotonConnected += SetBuildInfoText;
        PhotonManager.Instance.OnRoomJoin += OpenLobbyScreen;
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        PhotonManager.Instance.OnPhotonConnected -= SetBuildInfoText;
        PhotonManager.Instance.OnRoomJoin -= OpenLobbyScreen;
    }

    private void SetBuildInfoText()
    {
        // Get build info from PhotonManager and display it in the UI
        _buildInfo.text = PhotonManager.Instance.GetPhotonInfo();
    }
    private void OpenLobbyScreen()
    {
        _lobbyPanel.SetActive(true);
        _roomSelectionPanel.SetActive(false);
        _mainMenuPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("[UI] Exiting game...");
        Application.Quit();
    }
    public void PlayGame()
    {
        // Check if the nickname field is empty or valid
        if (string.IsNullOrEmpty(_nicknameField.text) ||
            _nicknameField.text.Length > 10)
        {
            Debug.LogWarning("[UI] Nickname cannot be empty!");
            _uiAnimator.SetTrigger("InvalidNickname");
            return;
        }
        else
        {
            // Set the player's nickname in Photon
            PhotonManager.Instance.SetNickname(_nicknameField.text);

            // Show the room selection panel and hide the main menu
            _roomSelectionPanel.SetActive(true);
            _mainMenuPanel.SetActive(false);
        }
    }

    public void CreateRoom()
    {
        _lobbyTitle.text = 
            $"<color=#FF9F00>{PhotonManager.Instance.CreateRoom().Replace("room", "", System.StringComparison.OrdinalIgnoreCase)}</color> room {1}/{4}";

        // Showing the lobby panel is handled by the OnRoomJoin event

        // Disable the start game button for non-master clients
        _startGameButton.SetActive(true);
    }

    public void LeaveRoom()
    {
        PhotonManager.Instance.LeaveRoom();

        // Clear the lobby title and disable the start game button
        _lobbyTitle.text = string.Empty;
        _startGameButton.SetActive(false);

        // Show the room selection panel and hide the lobby panel
        _roomSelectionPanel.SetActive(true);
        _lobbyPanel.SetActive(false);

        // Clear player cards from the lobby
        foreach (var card in _playerCards)
        {
            Destroy(card);
            _playerCards.Remove(card);
        }
    }

    public void ToggleReadyState()
    {
        _isReady = !_isReady;

        PhotonManager.Instance.SetReady(_isReady);
        //_readyIcon.enabled = _isReady;
    }

    public void StartGame()
    {
        PhotonManager.Instance.TryStartGame();
    }
}
