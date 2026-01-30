using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject _pauseMenu;

    InputSystem_Actions _inputActions;

    private void Awake()
    {
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
}
