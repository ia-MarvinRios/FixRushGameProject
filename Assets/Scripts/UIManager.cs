using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
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
    }
    void DisableUIInputs()
    {
        _inputActions.UI.F3.performed -= ToggleConsole;
        _inputActions.UI.Enter.performed -= HandleEnterInput;
        _inputActions.UI.Disable();
    }

    void ToggleConsole(InputAction.CallbackContext ctx) { RayConsole.RayConsoleBehaviour.Instance.Toggle(ctx); }
    void HandleEnterInput(InputAction.CallbackContext ctx)
    {
        RayConsole.RayConsoleBehaviour.Instance.EnterCommand(ctx);
    }
}
