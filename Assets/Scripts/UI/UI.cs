using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


/* ---- PARA EL QUE LEA ESTO: ----
 * Esta clase se encarga de manejar la UI principal del juego, maneja el cambio de estado activo de los
 * objetos del menú. Otras clases como "PlayerList" se encargan de actualizar los elementos específicos dentro de cada panel.
 * Esta clase trabaja en conjunto con "PhotonManager" para actualizar la UI en función del estado de la conexión y la sala.
 * --------------------------------------------------------------------------------------------------------------------------
*/

public class UI : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private PlayerSettings _playerSettings;
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
    [Header("Singleplayer References")]
    [SerializeField] private GameObject _singleplayerPanel;

    private bool _isReady = false;
    private List<GameObject> _playerCards = new List<GameObject>();

    private void Start()
    {
        PhotonManager.Instance.OnPhotonConnected += SetBuildInfoText;
        PhotonManager.Instance.OnRoomJoin += OpenLobbyScreen;

        // Set nickname field to the current Photon nickname if it exists
        if (!string.IsNullOrEmpty(PhotonManager.Instance.MyNickname))
        {
            _nicknameField.text = PhotonManager.Instance.MyNickname;
        }

        // Audio
        AudioManager.Instance.PlayAllMusic(true);
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
        if (PhotonManager.Instance.OfflineMode)
        {
            _singleplayerPanel.SetActive(true);
            _mainMenuPanel.SetActive(false);
        }
        else
        {
            _lobbyPanel.SetActive(true);
            _roomSelectionPanel.SetActive(false);
            _mainMenuPanel.SetActive(false);
        } 
    }

    public void QuitGame()
    {
        Debug.Log("[UI] Exiting game...");
        Application.Quit();
    }
    public bool CheckNickname()
    {
        // Check if the nickname field is empty or valid
        if (string.IsNullOrEmpty(_nicknameField.text) ||
            _nicknameField.text.Length > 10)
        {
            Debug.LogWarning("[UI] Nickname cannot be empty!");
            _uiAnimator.SetTrigger("InvalidNickname");

            return false;
        }
        else
        {
            // Set the player's nickname in Photon
            PhotonManager.Instance.SetNickname(_nicknameField.text);

            return true;
        }
    }

    public void EnterSinglePlayer()
    {
        if (!CheckNickname()) { return; }

        PhotonManager.Instance.StartSingleplayer();
    }
    public void ExitSinglePlayer()
    {
        PhotonManager.Instance.OfflineMode = false;
        _singleplayerPanel.SetActive(false);
        _mainMenuPanel.SetActive(true);
    }

    public void EnterMultiplayer()
    {
        if (!CheckNickname()) { return; }

        // Show the room selection panel and hide the main menu
        _roomSelectionPanel.SetActive(true);
        _mainMenuPanel.SetActive(false);
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

        if (PhotonManager.Instance.OfflineMode)
        {
            _singleplayerPanel.SetActive(false);
            _mainMenuPanel.SetActive(true);
        }
        else
        {
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
