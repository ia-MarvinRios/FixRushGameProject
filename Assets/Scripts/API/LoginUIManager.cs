using Fixrush.Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginUIManager : MonoBehaviour
{
    [Header("Panel Login")]
    [SerializeField] private GameObject panelLogin;
    [SerializeField] private TMP_InputField inputCorreoLogin;
    [SerializeField] private TMP_InputField inputPasswordLogin;
    [SerializeField] private Button btnLogin;
    [SerializeField] private TMP_Text textoErrorLogin;

    [Header("Panel Registro")]
    [SerializeField] private GameObject panelRegistro;
    [SerializeField] private TMP_InputField inputNombre;
    [SerializeField] private TMP_InputField inputNickname;
    [SerializeField] private TMP_InputField inputCorreoRegistro;
    [SerializeField] private TMP_InputField inputPasswordRegistro;
    [SerializeField] private TMP_InputField inputFechaNacimiento; // formato yyyy-MM-dd
    [SerializeField] private Button btnRegistrar;
    [SerializeField] private TMP_Text textoErrorRegistro;

    [Header("Escena a cargar tras login exitoso")]
    [SerializeField] private string nombreEscenaJuego = "MenuPrincipal";


    private void Start()
    {
        btnLogin.onClick.AddListener(OnClickLogin);
        btnRegistrar.onClick.AddListener(OnClickRegistrar);
        MostrarError(textoErrorLogin, "");
        MostrarError(textoErrorRegistro, "");
    }


    //  BOTÓN LOGIN

    private void OnClickLogin()
    {
        MostrarError(textoErrorLogin, "");

        string correo = inputCorreoLogin.text.Trim();
        string password = inputPasswordLogin.text;

        if (string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(password))
        {
            MostrarError(textoErrorLogin, "Completa todos los campos.");
            return;
        }

        btnLogin.interactable = false;

        AuthManager.Instance.Login(
            new LoginRequest { correo = correo, password = password },

            onSuccess: data =>
            {
                btnLogin.interactable = true;
                Debug.Log($"Login OK: {data.nickname} | Dinero: {data.dinero} | Nivel: {data.nivel}");

                // Cargar la escena principal del juego
                UnityEngine.SceneManagement.SceneManager.LoadScene(nombreEscenaJuego);
            },

            onError: msg =>
            {
                btnLogin.interactable = true;
                MostrarError(textoErrorLogin, msg);
            });
    }


    //  BOTÓN REGISTRAR

    private void OnClickRegistrar()
    {
        MostrarError(textoErrorRegistro, "");

        string nombre = inputNombre.text.Trim();
        string nickname = inputNickname.text.Trim();
        string correo = inputCorreoRegistro.text.Trim();
        string password = inputPasswordRegistro.text;
        string fecha = inputFechaNacimiento.text.Trim(); // "yyyy-MM-dd"

        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(nickname) ||
            string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(password) ||
            string.IsNullOrEmpty(fecha))
        {
            MostrarError(textoErrorRegistro, "Completa todos los campos.");
            return;
        }

        btnRegistrar.interactable = false;

        AuthManager.Instance.Registrar(
            new RegisterRequest
            {
                nombre = nombre,
                nickname = nickname,
                correo = correo,
                password = password,
                fechaNacimiento = fecha
            },

            onSuccess: data =>
            {
                btnRegistrar.interactable = true;
                Debug.Log($"Registro OK: {data.nickname} (ID {data.playerId})");

                // Volver al panel de login para que el jugador inicie sesión
                MostrarPanelLogin();
            },

            onError: msg =>
            {
                btnRegistrar.interactable = true;
                MostrarError(textoErrorRegistro, msg);
            });
    }

    //  HELPERS DE UI

    public void MostrarPanelLogin()
    {
        panelLogin.SetActive(true);
        panelRegistro.SetActive(false);
    }

    public void MostrarPanelRegistro()
    {
        panelLogin.SetActive(false);
        panelRegistro.SetActive(true);
    }

    private void MostrarError(TMP_Text campo, string mensaje)
    {
        if (campo == null) return;
        campo.text = mensaje;
        campo.gameObject.SetActive(!string.IsNullOrEmpty(mensaje));
    }
}