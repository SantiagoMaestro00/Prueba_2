using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class RAM_MiniGame : MonoBehaviour, IMiniGame
{
    [Header("UI Texto")]
    public Text infoText;

    [Header("Elementos interactivos")]
    public MiniGameDraggable ramDraggable;
    public MiniGameDropZone ramDropZone;

    [Header("Visuales - Estados del slot")]
    public GameObject slotClosed;      // Sprite: Slot cerrado (pestañas cerradas)
    public GameObject slotOpen;        // Sprite: Slot abierto (pestañas abiertas)
    public GameObject slotWithRAM;     // Sprite: Slot con RAM instalada

    [Header("Botones de pestañas - Abrir (en slotClosed)")]
    public Button clipButtonOpenLeft;
    public Button clipButtonOpenRight;

    [Header("Botones de pestañas - Cerrar (en slotOpen)")]
    public Button clipButtonCloseLeft;
    public Button clipButtonCloseRight;

    [Header("Panel bloqueador (opcional)")]
    public GameObject blockerPanel;

    // Variables privadas
    private Arrastrable currentItem;
    private CabinetSlotUI currentSlot;
    private Action<bool> completion;

    private bool slotOpened = false;
    private bool ramPlaced = false;

    void Start()
    {
        Debug.Log("=== [RAM_MiniGame] START ===");
        ConfigureCanvas();
    }

    void ConfigureCanvas()
    {
        Canvas canvas = GetComponent<Canvas>();

        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
            canvas = GetComponentInChildren<Canvas>(true);

        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            canvas.overrideSorting = true;
            canvas.gameObject.SetActive(true);

            Debug.Log($"[RAM_MiniGame] Canvas configurado correctamente");
        }
        else
        {
            Debug.LogError("[RAM_MiniGame] No se encontró Canvas!");
        }
    }

    public void Init(Arrastrable item, CabinetSlotUI slot, Action<bool> onComplete)
    {
        this.currentItem = item;
        this.currentSlot = slot;
        this.completion = onComplete;

        Debug.Log("=== [RAM_MiniGame] INIT LLAMADO ===");

        ConfigureCanvas();

        slotOpened = false;
        ramPlaced = false;

        // Estado inicial: solo slot cerrado visible
        if (slotClosed != null) slotClosed.SetActive(true);
        if (slotOpen != null) slotOpen.SetActive(false);
        if (slotWithRAM != null) slotWithRAM.SetActive(false);
        if (ramDraggable != null) ramDraggable.gameObject.SetActive(false);
        if (blockerPanel != null) blockerPanel.SetActive(true);

        // Texto inicial
        UpdateInfoText("Paso 1: Haz click en las pestañas para abrir el slot");

        // Configurar botones de ABRIR (en slotClosed)
        if (clipButtonOpenLeft != null)
        {
            clipButtonOpenLeft.onClick.RemoveAllListeners();
            clipButtonOpenLeft.onClick.AddListener(OnClipsOpenClicked);
            clipButtonOpenLeft.interactable = true;
            Debug.Log("[RAM_MiniGame] Botón abrir izquierdo configurado");
        }

        if (clipButtonOpenRight != null)
        {
            clipButtonOpenRight.onClick.RemoveAllListeners();
            clipButtonOpenRight.onClick.AddListener(OnClipsOpenClicked);
            clipButtonOpenRight.interactable = true;
            Debug.Log("[RAM_MiniGame] Botón abrir derecho configurado");
        }

        // Configurar botones de CERRAR (en slotOpen)
        if (clipButtonCloseLeft != null)
        {
            clipButtonCloseLeft.onClick.RemoveAllListeners();
            clipButtonCloseLeft.onClick.AddListener(OnClipsCloseClicked);
            clipButtonCloseLeft.interactable = false; // Inactivo hasta colocar RAM
        }

        if (clipButtonCloseRight != null)
        {
            clipButtonCloseRight.onClick.RemoveAllListeners();
            clipButtonCloseRight.onClick.AddListener(OnClipsCloseClicked);
            clipButtonCloseRight.interactable = false; // Inactivo hasta colocar RAM
        }

        // Configurar drop zone
        if (ramDropZone != null)
        {
            ramDropZone.onItemDropped.RemoveAllListeners();
            ramDropZone.onItemDropped.AddListener(OnRAMPlaced);
        }

        Debug.Log("=== [RAM_MiniGame] INIT COMPLETADO ===");
    }

    void UpdateInfoText(string message)
    {
        Debug.Log($"[RAM_MiniGame] Texto: {message}");

        if (infoText != null)
        {
            infoText.gameObject.SetActive(true);
            infoText.text = message;
            infoText.color = Color.white;
            infoText.fontSize = 36;
            infoText.fontStyle = FontStyle.Bold;

            // Asegurar que esté activo
            Transform parent = infoText.transform.parent;
            while (parent != null)
            {
                parent.gameObject.SetActive(true);
                parent = parent.parent;
            }

            // Añadir Outline si no tiene
            Outline outline = infoText.GetComponent<Outline>();
            if (outline == null)
            {
                outline = infoText.gameObject.AddComponent<Outline>();
            }
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(3f, -3f);
            outline.enabled = true;

            Canvas.ForceUpdateCanvases();
        }
    }

    // PASO 1: Usuario hace click en pestañas para abrir
    public void OnClipsOpenClicked()
    {
        Debug.Log("[RAM_MiniGame] ¡Click en abrir pestañas!");

        if (slotOpened)
        {
            Debug.Log("[RAM_MiniGame] Slot ya está abierto");
            return;
        }

        slotOpened = true;

        UpdateInfoText("Paso 2: Arrastra el módulo RAM al slot");

        // Cambiar visual: cerrado → abierto
        if (slotClosed != null) slotClosed.SetActive(false);
        if (slotOpen != null) slotOpen.SetActive(true);

        // Mostrar RAM para arrastrar
        if (ramDraggable != null) ramDraggable.gameObject.SetActive(true);

        // Desactivar botones de abrir
        if (clipButtonOpenLeft != null) clipButtonOpenLeft.interactable = false;
        if (clipButtonOpenRight != null) clipButtonOpenRight.interactable = false;

        Debug.Log("[RAM_MiniGame] Slot abierto, RAM visible");
    }

    // PASO 2: Usuario arrastra y suelta RAM
    void OnRAMPlaced(MiniGameDraggable draggable)
    {
        Debug.Log("[RAM_MiniGame] RAM colocada en el slot");

        if (!slotOpened)
        {
            UpdateInfoText("¡Primero abre el slot haciendo click en las pestañas!");
            return;
        }

        ramPlaced = true;

        UpdateInfoText("Paso 3: Haz click en las pestañas para cerrar y asegurar la RAM");

        // Deshabilitar drag (RAM ya no se puede mover)
        if (ramDraggable != null)
        {
            var draggableComponent = ramDraggable.GetComponent<MiniGameDraggable>();
            if (draggableComponent != null)
            {
                draggableComponent.enabled = false;
            }

            // Hacer RAM semi-transparente para indicar que está fija
            var ramImage = ramDraggable.GetComponent<Image>();
            if (ramImage != null)
            {
                ramImage.color = new Color(1f, 1f, 1f, 0.9f);
            }
        }

        // Activar botones de cerrar
        if (clipButtonCloseLeft != null)
        {
            clipButtonCloseLeft.interactable = true;
            Debug.Log("[RAM_MiniGame] Botón cerrar izquierdo activado");
        }

        if (clipButtonCloseRight != null)
        {
            clipButtonCloseRight.interactable = true;
            Debug.Log("[RAM_MiniGame] Botón cerrar derecho activado");
        }
    }

    // PASO 3: Usuario hace click en pestañas para cerrar
    public void OnClipsCloseClicked()
    {
        Debug.Log("[RAM_MiniGame] ¡Click en cerrar pestañas!");

        if (!slotOpened || !ramPlaced)
        {
            UpdateInfoText("¡Primero coloca la RAM en el slot!");
            return;
        }

        UpdateInfoText("¡RAM instalada correctamente! ✓");

        // Cambiar visual: abierto con RAM → cerrado con RAM
        if (slotOpen != null) slotOpen.SetActive(false);
        if (ramDraggable != null) ramDraggable.gameObject.SetActive(false);
        if (slotWithRAM != null) slotWithRAM.SetActive(true);

        // Desactivar botones de cerrar
        if (clipButtonCloseLeft != null) clipButtonCloseLeft.interactable = false;
        if (clipButtonCloseRight != null) clipButtonCloseRight.interactable = false;

        // Completar minijuego después de un delay
        StartCoroutine(CompleteMiniGameAfterDelay(1.5f));
    }

    IEnumerator CompleteMiniGameAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        Debug.Log("[RAM_MiniGame] Completando minijuego - ¡ÉXITO!");
        completion?.Invoke(true);
    }

    public void Abort()
    {
        Debug.Log("[RAM_MiniGame] Minijuego abortado");
        completion?.Invoke(false);
    }

    void OnDestroy()
    {
        // Limpiar listeners
        if (clipButtonOpenLeft != null)
            clipButtonOpenLeft.onClick.RemoveListener(OnClipsOpenClicked);

        if (clipButtonOpenRight != null)
            clipButtonOpenRight.onClick.RemoveListener(OnClipsOpenClicked);

        if (clipButtonCloseLeft != null)
            clipButtonCloseLeft.onClick.RemoveListener(OnClipsCloseClicked);

        if (clipButtonCloseRight != null)
            clipButtonCloseRight.onClick.RemoveListener(OnClipsCloseClicked);

        if (ramDropZone != null)
            ramDropZone.onItemDropped.RemoveListener(OnRAMPlaced);
    }
}