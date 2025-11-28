using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class MiniGameManager : MonoBehaviour
{
    public static MiniGameManager Instance { get; private set; }

    [System.Serializable]
    public class Entry
    {
        public string id;
        public GameObject miniGamePrefab;
    }

    [Header("Registro de minijuegos")]
    public List<Entry> registry = new List<Entry>();

    [Header("Parent para instanciar minijuegos (Canvas Overlay)")]
    public Transform overlayParent;

    private GameObject currentInstance;
    private IMiniGame currentMini;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Debug.Log("[MiniGameManager] Inicializado");
    }

    public void StartMiniGame(string id, Arrastrable item, CabinetSlotUI slot)
    {
        Debug.Log($"[MiniGameManager] StartMiniGame llamado con id='{id}'");

        // Si ya hay un minijuego activo, destruirlo primero
        if (currentInstance != null)
        {
            Debug.LogWarning("[MiniGameManager] Ya hay un minijuego activo, destruyéndolo primero");
            Destroy(currentInstance);
            currentInstance = null;
            currentMini = null;
            Time.timeScale = 1f; // Restaurar tiempo por si acaso
        }

        var entry = registry.Find(e => e.id == id);
        if (entry == null || entry.miniGamePrefab == null)
        {
            Debug.LogWarning($"[MiniGameManager] No hay prefab para id '{id}', colocando directo.");
            slot.AcceptPlacementFromMinigame(item);
            return;
        }

        Debug.Log($"[MiniGameManager] Instanciando prefab '{entry.miniGamePrefab.name}'");

        // Instanciar en el overlayParent si existe, si no en el root de la escena
        Transform parent = overlayParent ? overlayParent : null;
        currentInstance = Instantiate(entry.miniGamePrefab, parent);

        Debug.Log($"[MiniGameManager] Prefab instanciado: {currentInstance.name}");

        // Buscar el Canvas en el root o en los hijos
        Canvas canvas = currentInstance.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = currentInstance.GetComponentInChildren<Canvas>(true);
        }

        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            canvas.overrideSorting = true;
            Debug.Log($"[MiniGameManager] Canvas configurado: {canvas.gameObject.name}, Overlay, sortOrder=999");
        }
        else
        {
            Debug.LogWarning("[MiniGameManager] El prefab no tiene componente Canvas");
        }

        // Asegurarse de que el Graphic Raycaster esté presente
        if (canvas != null && canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
        {
            canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            Debug.Log("[MiniGameManager] Añadido GraphicRaycaster al Canvas");
        }

        // Asegurarse de que esté activo y al frente
        currentInstance.SetActive(true);
        currentInstance.transform.SetAsLastSibling();

        // Buscar el componente IMiniGame en el root o en los hijos
        currentMini = currentInstance.GetComponent<IMiniGame>();
        if (currentMini == null)
        {
            currentMini = currentInstance.GetComponentInChildren<IMiniGame>(true);
        }

        if (currentMini == null)
        {
            Debug.LogError("[MiniGameManager] El prefab no implementa IMiniGame.");
            FinishMiniGame(false);
            return;
        }

        Debug.Log("[MiniGameManager] Pausando juego (Time.timeScale = 0)");
        Time.timeScale = 0f;

        Debug.Log("[MiniGameManager] Llamando a Init del minijuego");
        currentMini.Init(item, slot, (success) =>
        {
            Debug.Log($"[MiniGameManager] Minijuego completado con success={success}");
            Time.timeScale = 1f;

            if (success)
            {
                slot.AcceptPlacementFromMinigame(item);
            }
            else
            {
                item.RestoreToHome();
            }

            FinishMiniGame(success);
        });
    }

    private void FinishMiniGame(bool success)
    {
        Debug.Log($"[MiniGameManager] FinishMiniGame llamado con success={success}");

        if (currentInstance != null)
            Destroy(currentInstance);

        currentInstance = null;
        currentMini = null;
    }
}