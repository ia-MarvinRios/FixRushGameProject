using FixRushGame;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    InputSystem_Actions _inputActions;

    List<Card> _activeCards = new List<Card>();

    public List<Card> ActiveCards { get { return _activeCards; } }

    [Header("References")]
    [SerializeField] GameObject _pauseMenu;
    [SerializeField] GameObject _missionsPanel;
    [SerializeField] GameObject _cardPrefab;

    private void Awake()
    {
        Instance = this;
        _inputActions = new InputSystem_Actions();
    }
    private void Start()
    {
        EnableUIInputs();
    }
    private void OnDisable()
    {
        DisableUIInputs();
    }

    void EnableUIInputs()
    {
        _inputActions.UI.Enable();

        _inputActions.UI.F3.performed += ToggleConsole;
        _inputActions.UI.Enter.performed += HandleEnterInput;
        _inputActions.UI.Escape.performed += TogglePauseMenu;
    }
    void DisableUIInputs()
    {
        _inputActions.UI.F3.performed -= ToggleConsole;
        _inputActions.UI.Enter.performed -= HandleEnterInput;
        _inputActions.UI.Escape.performed -= TogglePauseMenu;
        _inputActions.UI.Disable();
    }

    void TogglePauseMenu(InputAction.CallbackContext ctx)
    {
        if (_pauseMenu.activeSelf)
        {
            GUIBrain.Instance.EndInteraction();
        }
        else
        {
            GUIBrain.Instance.StartNewInteraction(_pauseMenu);
        }
    }

    void ToggleConsole(InputAction.CallbackContext ctx) { RayConsole.RayConsoleBehaviour.Instance.Toggle(ctx); }
    void HandleEnterInput(InputAction.CallbackContext ctx)
    {
        RayConsole.RayConsoleBehaviour.Instance.EnterCommand(ctx);
    }

    // ----- PUBLIC METHODS -----
    public void GoToScene(string sceneName) { SceneManager.LoadScene(sceneName, LoadSceneMode.Single); }
    public void QuitGame() { Application.Quit(); }

    public void AddIssuesCard(FixRushGame.IssueType[] issues)
    {
        Card card = Instantiate(_cardPrefab, _missionsPanel.transform).GetComponent<Card>();

        card.SetUpCard(issues);
        _activeCards.Add(card);
    }
    public void RemoveIssuesCard(Card card)
    {
        if (_activeCards.Contains(card))
        {
            _activeCards.Remove(card);
            Destroy(card.gameObject);
        }
    }
}
