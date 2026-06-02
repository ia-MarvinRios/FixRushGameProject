using Newtonsoft.Json;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Fixrush.Network
{
    [Serializable] public class UpdateDineroRequest { public decimal dinero; }
    [Serializable] public class UpdateItemRequest { public bool tiene; }
    [Serializable] public class UpdateNivelRequest { public int nivel; public int experiencia; }
    [Serializable]
    public class UpdateEquipRequest
    {
        public int? cosmeticoCuerpoId;
        public int? cosmeticoGorroId;
    }

    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance { get; private set; }

        [Header("URL base del API (sin / al final)")]
        [SerializeField]
        private string baseUrl = "https://fixrushgameapi20260526123518-ahbtexhvh8fbchdw.canadacentral-01.azurewebsites.net";

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ============================================================
        //  ACTUALIZAR DINERO
        //  Uso:
        //    PlayerManager.Instance.ActualizarDinero(
        //        2500.00m,
        //        onSuccess: dinero => Debug.Log($"Dinero guardado: {dinero}"),
        //        onError:   msg    => Debug.LogError(msg)
        //    );
        // ============================================================
        public void ActualizarDinero(decimal nuevoDinero,
            Action<decimal> onSuccess, Action<string> onError)
        {
            if (!AuthManager.Instance.Logueado) { onError?.Invoke("No hay sesion activa."); return; }

            int playerId = AuthManager.Instance.Sesion.playerId;
            var body = JsonConvert.SerializeObject(new UpdateDineroRequest { dinero = nuevoDinero });

            StartCoroutine(Put(
                $"{baseUrl}/api/players/{playerId}/dinero", body,
                json =>
                {
                    var resp = JsonConvert.DeserializeObject<ApiResponse<object>>(json);
                    if (resp.success)
                    {
                        AuthManager.Instance.ActualizarDineroLocal(nuevoDinero);
                        onSuccess?.Invoke(nuevoDinero);
                    }
                    else onError?.Invoke(resp.message);
                },
                onError));
        }

        // ============================================================
        //  ACTUALIZAR ÍTEM (desbloquear o bloquear)
        //  Uso:
        //    // El jugador compra el Turbo Kit (id = 4)
        //    PlayerManager.Instance.ActualizarItem(
        //        itemId:    4,
        //        tiene:     true,
        //        onSuccess: ()  => turboObj.SetActive(true),
        //        onError:   msg => Debug.LogError(msg)
        //    );
        // ============================================================
        public void ActualizarItem(int itemId, bool tiene,
            Action onSuccess, Action<string> onError)
        {
            if (!AuthManager.Instance.Logueado) { onError?.Invoke("No hay sesion activa."); return; }

            int playerId = AuthManager.Instance.Sesion.playerId;
            var body = JsonConvert.SerializeObject(new UpdateItemRequest { tiene = tiene });

            StartCoroutine(Put(
                $"{baseUrl}/api/players/{playerId}/items/{itemId}", body,
                json =>
                {
                    var resp = JsonConvert.DeserializeObject<ApiResponse<object>>(json);
                    if (resp.success)
                    {
                        AuthManager.Instance.ActualizarItemLocal(itemId, tiene);
                        onSuccess?.Invoke();
                    }
                    else onError?.Invoke(resp.message);
                },
                onError));
        }

        // ============================================================
        //  ACTUALIZAR NIVEL
        //  Uso:
        //    PlayerManager.Instance.ActualizarNivel(
        //        nivel:       5,
        //        experiencia: 1200,
        //        onSuccess:   () => Debug.Log("Nivel guardado"),
        //        onError:     msg => Debug.LogError(msg)
        //    );
        // ============================================================
        public void ActualizarNivel(int nivel, int experiencia,
            Action onSuccess, Action<string> onError)
        {
            if (!AuthManager.Instance.Logueado) { onError?.Invoke("No hay sesion activa."); return; }

            int playerId = AuthManager.Instance.Sesion.playerId;
            var body = JsonConvert.SerializeObject(
                new UpdateNivelRequest { nivel = nivel, experiencia = experiencia });

            StartCoroutine(Put(
                $"{baseUrl}/api/players/{playerId}/nivel", body,
                json =>
                {
                    var resp = JsonConvert.DeserializeObject<ApiResponse<object>>(json);
                    if (resp.success)
                    {
                        AuthManager.Instance.Sesion.nivel = nivel;
                        AuthManager.Instance.Sesion.experiencia = experiencia;
                        onSuccess?.Invoke();
                    }
                    else onError?.Invoke(resp.message);
                },
                onError));
        }

        // ============================================================
        //  EQUIPAR COSMÉTICO (cuerpo, gorro o ambos)
        //  Pasa null en los que no quieras cambiar.
        //  Uso:
        //    // Equipar solo el cuerpo (id = 3)
        //    PlayerManager.Instance.Equipar(
        //        cuerpoId:  3,
        //        gorroId:   null,
        //        onSuccess: () => AplicarCosmeticoEnEscena(),
        //        onError:   msg => Debug.LogError(msg)
        //    );
        //
        //    // Equipar ambos
        //    PlayerManager.Instance.Equipar(3, 7, OnEquipado, OnError);
        // ============================================================
        public void Equipar(int? cuerpoId, int? gorroId,
            Action onSuccess, Action<string> onError)
        {
            if (!AuthManager.Instance.Logueado) { onError?.Invoke("No hay sesion activa."); return; }
            if (!cuerpoId.HasValue && !gorroId.HasValue)
            {
                onError?.Invoke("Debes enviar al menos un cosmetico para equipar.");
                return;
            }

            int playerId = AuthManager.Instance.Sesion.playerId;
            var body = JsonConvert.SerializeObject(new UpdateEquipRequest
            {
                cosmeticoCuerpoId = cuerpoId,
                cosmeticoGorroId = gorroId
            });

            StartCoroutine(Put(
                $"{baseUrl}/api/players/{playerId}/equip", body,
                json =>
                {
                    var resp = JsonConvert.DeserializeObject<ApiResponse<object>>(json);
                    if (resp.success)
                    {
                        AuthManager.Instance.ActualizarEquipLocal(cuerpoId, gorroId);
                        onSuccess?.Invoke();
                    }
                    else onError?.Invoke(resp.message);
                },
                onError));
        }

        // ============================================================
        //  HTTP PUT (interno)
        // ============================================================
        private IEnumerator Put(string url, string body,
            Action<string> onSuccess, Action<string> onError)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(body);

            using var req = new UnityWebRequest(url, "PUT");
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