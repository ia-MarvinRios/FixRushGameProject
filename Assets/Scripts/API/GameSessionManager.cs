using Fixrush.Network;
using System.Collections.Generic;
using UnityEngine;

public class GameSessionManager : MonoBehaviour
{
    [Header("UI de jugador")]
    [SerializeField] private TMPro.TMP_Text textoDinero;
    [SerializeField] private TMPro.TMP_Text textoNivel;
    [SerializeField] private TMPro.TMP_Text textoNickname;

    [Header("GameObjects de herramientas")]
    // Arrastra aquí los GameObjects de cada herramienta.
    // El id debe coincidir con el id del Item en la base de datos.
    [SerializeField] private List<ItemGameObject> herramientas;

    [Header("GameObjects de cosméticos")]
    [SerializeField] private List<ItemGameObject> cosmeticos;

    [Header("Cosméticos equipables (cuerpo y gorro)")]
    [SerializeField] private List<ItemGameObject> opcionesCuerpo;
    [SerializeField] private List<ItemGameObject> opcionesGorro;

    private void Start()
    {
        // Si no hay sesión activa redirige al login
        if (!AuthManager.Instance.Logueado)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Login");
            return;
        }

        CargarSesion();
    }


    //  Carga todos los datos de sesión en la escena
    
    private void CargarSesion()
    {
        var sesion = AuthManager.Instance.Sesion;

        // ── UI básica ─────────────────────────────────────────────
        if (textoDinero != null) textoDinero.text = $"${sesion.dinero:N0}";
        if (textoNivel != null) textoNivel.text = $"Nivel {sesion.nivel}";
        if (textoNickname != null) textoNickname.text = sesion.nickname;

        // ── Habilitar herramientas según tiene = true/false ───────
        foreach (var item in herramientas)
            item.gameObject.SetActive(AuthManager.Instance.TieneItem(item.itemId));

        // ── Habilitar cosméticos desbloqueados ────────────────────
        foreach (var item in cosmeticos)
            item.gameObject.SetActive(AuthManager.Instance.TieneItem(item.itemId));

        // ── Aplicar cosméticos equipados ──────────────────────────
        AplicarEquip(sesion.cosmeticoCuerpoId, sesion.cosmeticoGorroId);
    }


    //  Aplica visualmente el cuerpo y gorro equipados

    private void AplicarEquip(int? cuerpoId, int? gorroId)
    {
        // Desactiva todos y activa solo el equipado
        foreach (var op in opcionesCuerpo)
            op.gameObject.SetActive(cuerpoId.HasValue && op.itemId == cuerpoId.Value);

        foreach (var op in opcionesGorro)
            op.gameObject.SetActive(gorroId.HasValue && op.itemId == gorroId.Value);
    }


    //  Ejemplo: el jugador compra un ítem en la tienda

    public void ComprarItem(int itemId, decimal costo)
    {
        decimal dineroActual = AuthManager.Instance.Sesion.dinero;

        if (dineroActual < costo)
        {
            Debug.Log("Dinero insuficiente.");
            return;
        }

        decimal nuevoDinero = dineroActual - costo;

        // 1. Desbloquear el ítem
        PlayerManager.Instance.ActualizarItem(
            itemId, true,
            onSuccess: () =>
            {
                // 2. Descontar el dinero
                PlayerManager.Instance.ActualizarDinero(
                    nuevoDinero,
                    onSuccess: dinero =>
                    {
                        if (textoDinero != null)
                            textoDinero.text = $"${dinero:N0}";

                        // 3. Habilitar el GameObject del ítem en escena
                        var itemGO = herramientas.Find(i => i.itemId == itemId)
                                  ?? cosmeticos.Find(i => i.itemId == itemId);
                        itemGO?.gameObject.SetActive(true);

                        Debug.Log($"Item {itemId} comprado. Dinero restante: {dinero}");
                    },
                    onError: msg => Debug.LogError($"Error guardando dinero: {msg}"));
            },
            onError: msg => Debug.LogError($"Error desbloqueando item: {msg}"));
    }

    //  Ejemplo: el jugador equipa un cosmético

    public void EquiparCosmetico(int? cuerpoId, int? gorroId)
    {
        PlayerManager.Instance.Equipar(
            cuerpoId, gorroId,
            onSuccess: () =>
            {
                var sesion = AuthManager.Instance.Sesion;
                AplicarEquip(sesion.cosmeticoCuerpoId, sesion.cosmeticoGorroId);
                Debug.Log($"Equipado: cuerpo={sesion.cosmeticoCuerpoId} gorro={sesion.cosmeticoGorroId}");
            },
            onError: msg => Debug.LogError($"Error equipando: {msg}"));
    }

    //  Ejemplo: subir de nivel al terminar una carrera

    public void SubirNivel(int expGanada)
    {
        var sesion = AuthManager.Instance.Sesion;
        int nuevaExp = sesion.experiencia + expGanada;
        int nuevoNivel = sesion.nivel;

        // Regla simple: cada 1000 xp sube un nivel
        if (nuevaExp >= 1000)
        {
            nuevoNivel++;
            nuevaExp -= 1000;
        }

        PlayerManager.Instance.ActualizarNivel(
            nuevoNivel, nuevaExp,
            onSuccess: () =>
            {
                if (textoNivel != null)
                    textoNivel.text = $"Nivel {nuevoNivel}";
                Debug.Log($"Nivel actualizado: {nuevoNivel} | XP: {nuevaExp}");
            },
            onError: msg => Debug.LogError($"Error guardando nivel: {msg}"));
    }
}

//  ItemGameObject
//  Clase auxiliar para conectar un id de item con su GameObject
//  directamente desde el Inspector de Unity.

[System.Serializable]
public class ItemGameObject
{
    public int itemId;
    public GameObject gameObject;
}