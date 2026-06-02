using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace Fixrush.Network
{

    //  DTOs


    [Serializable]
    public class LoginRequest
    {
        public string correo;
        public string password;
    }

    [Serializable]
    public class RegisterRequest
    {
        public string nombre;
        public string nickname;
        public string correo;
        public string password;
        public string fechaNacimiento; // "yyyy-MM-dd"
    }

    [Serializable]
    public class ItemDto
    {
        public int id;
        public string nombre;
        public string tipo;   // "herramienta" o "cosmetico"
        public string grupo;
        public bool tiene;
    }

    [Serializable]
    public class LoginResponse
    {
        public int playerId;
        public string nickname;
        public string nombre;
        public decimal dinero;
        public int nivel;
        public int experiencia;
        public int? cosmeticoCuerpoId;  // null si no tiene nada equipado
        public int? cosmeticoGorroId;
        public List<ItemDto> items;
    }

    [Serializable]
    public class RegisterResponse
    {
        public int playerId;
        public string nickname;
        public string mensaje;
    }

    [Serializable]
    public class ApiResponse<T>
    {
        public bool success;
        public string message;
        public T data;
    }


    //  AuthManager
   

    public class AuthManager : MonoBehaviour
    {
        // ── Singleton ────────────────────────────────────────────
        public static AuthManager Instance { get; private set; }

        // ── Config ───────────────────────────────────────────────
        [Header("URL base del API (sin / al final)")]
        [SerializeField]
        private string baseUrl = "https://fixrushgameapi20260526123518-ahbtexhvh8fbchdw.canadacentral-01.azurewebsites.net";

        // ── Sesión activa ─────────────────────────────────────────
        // Una vez logueado, accede a estos datos desde cualquier script:
        //   AuthManager.Instance.Sesion.dinero
        //   AuthManager.Instance.Sesion.nivel
        //   AuthManager.Instance.Sesion.items
        public LoginResponse Sesion { get; private set; }
        public bool Logueado => Sesion != null;


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject); // persiste entre escenas
        }

        // ============================================================
        //  LOGIN
        //  Uso:
        //    AuthManager.Instance.Login(
        //        new LoginRequest { correo = "...", password = "..." },
        //        onSuccess: data => { /* cargar escena del juego */ },
        //        onError:   msg  => { /* mostrar mensaje de error */ }
        //    );
        // ============================================================
        public void Login(LoginRequest req,
            Action<LoginResponse> onSuccess,
            Action<string> onError)
        {
            StartCoroutine(Post(
                $"{baseUrl}/api/auth/login",
                JsonConvert.SerializeObject(req),
                json =>
                {
                    var resp = JsonConvert.DeserializeObject<ApiResponse<LoginResponse>>(json);
                    if (resp.success)
                    {
                        Sesion = resp.data;
                        onSuccess?.Invoke(resp.data);
                    }
                    else
                    {
                        onError?.Invoke(resp.message);
                    }
                },
                onError));
        }

        // ============================================================
        //  REGISTER
        //  Uso:
        //    AuthManager.Instance.Registrar(
        //        new RegisterRequest { nombre = "...", ... },
        //        onSuccess: data => { /* ir a pantalla de login */ },
        //        onError:   msg  => { /* mostrar error */ }
        //    );
        // ============================================================
        public void Registrar(RegisterRequest req,
            Action<RegisterResponse> onSuccess,
            Action<string> onError)
        {
            StartCoroutine(Post(
                $"{baseUrl}/api/auth/register",
                JsonConvert.SerializeObject(req),
                json =>
                {
                    var resp = JsonConvert.DeserializeObject<ApiResponse<RegisterResponse>>(json);
                    if (resp.success) onSuccess?.Invoke(resp.data);
                    else onError?.Invoke(resp.message);
                },
                onError));
        }

        // ============================================================
        //  HELPERS DE SESIÓN
        //  Úsalos desde cualquier script del juego para consultar
        //  el estado del jugador sin hacer otra llamada al API.
        // ============================================================

        // ¿El jugador tiene este ítem desbloqueado?
        //   bool tieneNitro = AuthManager.Instance.TieneItem(6);
        //   nitroObj.SetActive(tieneNitro);
        public bool TieneItem(int itemId)
        {
            if (!Logueado) return false;
            return Sesion.items.Exists(i => i.id == itemId && i.tiene);
        }

        // Obtener todos los ítems de un tipo ("herramienta" o "cosmetico")
        //   var cosmeticos = AuthManager.Instance.ItemsPorTipo("cosmetico");
        public List<ItemDto> ItemsPorTipo(string tipo)
        {
            if (!Logueado) return new List<ItemDto>();
            return Sesion.items.FindAll(i => i.tipo == tipo);
        }

        // Obtener solo los ítems que el jugador tiene desbloqueados
        //   var misHerramientas = AuthManager.Instance.ItemsDesbloqueados("herramienta");
        public List<ItemDto> ItemsDesbloqueados(string tipo)
        {
            if (!Logueado) return new List<ItemDto>();
            return Sesion.items.FindAll(i => i.tipo == tipo && i.tiene);
        }

        // Actualiza el dinero local sin llamar al API
        // (PlayerManager ya lo llama automáticamente al guardar)
        public void ActualizarDineroLocal(decimal nuevoDinero)
        {
            if (Logueado) Sesion.dinero = nuevoDinero;
        }

        // Actualiza el equip local después de equipar un cosmético
        public void ActualizarEquipLocal(int? cuerpoId, int? gorroId)
        {
            if (!Logueado) return;
            if (cuerpoId.HasValue) Sesion.cosmeticoCuerpoId = cuerpoId;
            if (gorroId.HasValue) Sesion.cosmeticoGorroId = gorroId;
        }

        // Actualiza el estado local de un ítem después de desbloquearlo
        public void ActualizarItemLocal(int itemId, bool tiene)
        {
            if (!Logueado) return;
            var item = Sesion.items.Find(i => i.id == itemId);
            if (item != null) item.tiene = tiene;
        }

        public void CerrarSesion() => Sesion = null;

        // ============================================================
        //  HTTP POST (interno)
        // ============================================================
        private IEnumerator Post(string url, string body,
            Action<string> onSuccess, Action<string> onError)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(body);

            using var req = new UnityWebRequest(url, "POST");
            req.uploadHandler = new UploadHandlerRaw(bytes);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.timeout = 15;

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(req.downloadHandler.text);
            }
            else
            {
                try
                {
                    var err = JsonConvert.DeserializeObject<ApiResponse<object>>(req.downloadHandler.text);
                    onError?.Invoke(err?.message ?? req.error);
                }
                catch { onError?.Invoke(req.error); }
            }
        }
    }
}
