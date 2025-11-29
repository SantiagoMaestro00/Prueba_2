using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class MiniGameManager : MonoBehaviour
{
    public static MiniGameManager Instance { get; private set; }

    [System.Serializable]
    public class Entry
    {
        public string id; // Ej: "RAM"
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

    // 👇👇👇 AQUÍ ESTABA EL ERROR: FALTABA ESCUCHAR EL EVENTO 👇👇👇

    void OnEnable()
    {
        // Nos suscribimos al evento
        PlacementEvents.OnComponentPlaced += HandleComponentPlaced;
        Debug.Log("[MiniGameManager] Escuchando eventos...");
    }

    void OnDisable()
    {
        // Nos desuscribimos para evitar errores de memoria
        PlacementEvents.OnComponentPlaced -= HandleComponentPlaced;
    }

    // Esta función sirve de puente entre el Evento y el StartMiniGame
    private void HandleComponentPlaced(string id, Arrastrable item, RectTransform slotTransform)
    {
        Debug.Log($"[MiniGameManager] Evento recibido para: {id}");

        // El evento nos da un RectTransform, pero StartMiniGame pide un CabinetSlotUI.
        // Buscamos el script en el objeto del slot.
        CabinetSlotUI slotScript = slotTransform.GetComponent<CabinetSlotUI>();

        if (slotScript != null)
        {
            StartMiniGame(id, item, slotScript);
        }
        else
        {
            Debug.LogError($"[MiniGameManager] El objeto '{slotTransform.name}' tiene Tag 'Slot' pero le falta el script 'CabinetSlotUI'.");
            // Opcional: Si solo usas física y no tienes CabinetSlotUI, aquí tendrías que cambiar la lógica.
            // Pero como tu StartMiniGame usa 'slot.AcceptPlacementFromMinigame', es obligatorio tener el script.
        }
    }

    // 👆👆👆 ------------------------------------------------------- 👆👆👆

    public void StartMiniGame(string id, Arrastrable item, CabinetSlotUI slot)
    {
        Debug.Log($"[MiniGameManager] StartMiniGame llamado con id='{id}'");

        if (currentInstance != null)
        {
            Debug.LogWarning("[MiniGameManager] Ya hay un minijuego activo, destruyéndolo primero");
            Destroy(currentInstance);
            currentInstance = null;
            currentMini = null;
            Time.timeScale = 1f;
        }

        var entry = registry.Find(e => e.id == id);

        // Si no encontramos el minijuego en la lista, terminamos la colocación directo
        if (entry == null || entry.miniGamePrefab == null)
        {
            Debug.LogWarning($"[MiniGameManager] No hay prefab registrado para id '{id}', colocando directo sin minijuego.");
            slot.AcceptPlacementFromMinigame(item);
            return;
        }

        Debug.Log($"[MiniGameManager] Instanciando prefab '{entry.miniGamePrefab.name}'");

        Transform parent = overlayParent ? overlayParent : null;
        currentInstance = Instantiate(entry.miniGamePrefab, parent);

        Debug.Log($"[MiniGameManager] Prefab instanciado: {currentInstance.name}");

        // Configuración automática del Canvas del Minijuego
        Canvas canvas = currentInstance.GetComponent<Canvas>();
        if (canvas == null) canvas = currentInstance.GetComponentInChildren<Canvas>(true);

        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            canvas.overrideSorting = true;
        }

        if (canvas != null && canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
        {
            canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        currentInstance.SetActive(true);
        currentInstance.transform.SetAsLastSibling();

        currentMini = currentInstance.GetComponent<IMiniGame>();
        if (currentMini == null) currentMini = currentInstance.GetComponentInChildren<IMiniGame>(true);

        if (currentMini == null)
        {
            Debug.LogError("[MiniGameManager] El prefab no implementa IMiniGame.");
            FinishMiniGame(false);
            return;
        }

        Debug.Log("[MiniGameManager] Pausando juego e iniciando Minijuego");
        Time.timeScale = 0f;

        currentMini.Init(item, slot, (success) =>
        {
            Debug.Log($"[MiniGameManager] Minijuego completado. Éxito: {success}");
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
        if (currentInstance != null)
            Destroy(currentInstance);

        currentInstance = null;
        currentMini = null;
    }
}