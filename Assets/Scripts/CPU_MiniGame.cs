using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class CPU_MiniGame : MonoBehaviour, IMiniGame
{
    [Header("UI")]
    public Text infoText;

    [Header("Elementos interactivos")]
    public MiniGameDraggable cpuDraggable;
    public MiniGameDropZone cpuDropZone;

    [Header("Visuales")]
    public GameObject socketClosed;
    public GameObject socketOpen;
    public GameObject socketWithCPU;

    [Header("Palancas (Click)")]
    public Button leverClickAreaOpen;
    public Button leverClickAreaClose;

    [Header("Panel bloqueador (opcional)")]
    public GameObject blockerPanel;

    private Arrastrable currentItem;
    private CabinetSlotUI currentSlot;
    private Action<bool> completion;

    private bool socketOpened = false;

    void Start()
    {
        Debug.Log("=== [CPU_MiniGame] START ===");
        ConfigureCanvas();

        // Debug inicial del InfoText
        if (infoText != null)
        {
            Debug.Log($"[CPU_MiniGame] InfoText encontrado en Start: {infoText.gameObject.name}");
            DebugInfoText();
        }
        else
        {
            Debug.LogError("[CPU_MiniGame] InfoText es NULL en Start!");
        }
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
            Debug.Log($"[CPU_MiniGame] Canvas encontrado en: {canvas.gameObject.name}");
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            canvas.overrideSorting = true;
            canvas.gameObject.SetActive(true);

            Debug.Log($"[CPU_MiniGame] Canvas configurado:");
            Debug.Log($"  - GameObject: {canvas.gameObject.name}");
            Debug.Log($"  - RenderMode: {canvas.renderMode}");
            Debug.Log($"  - SortingOrder: {canvas.sortingOrder}");
            Debug.Log($"  - OverrideSorting: {canvas.overrideSorting}");
            Debug.Log($"  - Activo: {canvas.gameObject.activeSelf}");
        }
        else
        {
            Debug.LogError("[CPU_MiniGame] No se encontró Canvas!");
        }
    }

    public void Init(Arrastrable item, CabinetSlotUI slot, Action<bool> onComplete)
    {
        this.currentItem = item;
        this.currentSlot = slot;
        this.completion = onComplete;

        Debug.Log("=== [CPU_MiniGame] INIT LLAMADO ===");

        ConfigureCanvas();

        socketOpened = false;

        // Estado inicial
        if (socketClosed != null) socketClosed.SetActive(true);
        if (socketOpen != null) socketOpen.SetActive(false);
        if (socketWithCPU != null) socketWithCPU.SetActive(false);
        if (cpuDraggable != null) cpuDraggable.gameObject.SetActive(false);
        if (blockerPanel != null) blockerPanel.SetActive(true);

        // Configurar texto de instrucciones
        ConfigureInfoText("1. Haz click en la palanca naranja para abrir el socket");

        // Configurar botón de palanca para ABRIR
        if (leverClickAreaOpen != null)
        {
            Debug.Log("[CPU_MiniGame] Configurando botón de palanca (abrir)");
            leverClickAreaOpen.onClick.RemoveAllListeners();
            leverClickAreaOpen.onClick.AddListener(OnLeverOpenClicked);
            leverClickAreaOpen.interactable = true;

            Image leverImage = leverClickAreaOpen.GetComponent<Image>();
            if (leverImage != null)
            {
                leverImage.raycastTarget = true;
            }
        }
        else
        {
            Debug.LogError("[CPU_MiniGame] leverClickAreaOpen es NULL!");
        }

        // Configurar botón de palanca para CERRAR
        if (leverClickAreaClose != null)
        {
            Debug.Log("[CPU_MiniGame] Configurando botón de palanca (cerrar)");
            leverClickAreaClose.onClick.RemoveAllListeners();
            leverClickAreaClose.onClick.AddListener(OnLeverCloseClicked);
            leverClickAreaClose.interactable = false;

            Image leverImage = leverClickAreaClose.GetComponent<Image>();
            if (leverImage != null)
            {
                leverImage.raycastTarget = true;
            }
        }
        else
        {
            Debug.LogWarning("[CPU_MiniGame] leverClickAreaClose es NULL.");
        }

        // Configurar drop zone del CPU
        if (cpuDropZone != null)
        {
            cpuDropZone.onItemDropped.RemoveAllListeners();
            cpuDropZone.onItemDropped.AddListener(OnCPUPlaced);
        }
        else
        {
            Debug.LogError("[CPU_MiniGame] cpuDropZone es NULL!");
        }

        Transform root = transform.root;
        if (root != null)
        {
            root.SetAsLastSibling();
        }

        Debug.Log("=== [CPU_MiniGame] INIT COMPLETADO ===");
    }

    void ConfigureInfoText(string message)
    {
        Debug.Log($"=== [CPU_MiniGame] ConfigureInfoText llamado con mensaje: '{message}' ===");

        if (infoText != null)
        {
            // Forzar que esté activo
            infoText.gameObject.SetActive(true);

            // Configurar texto
            infoText.text = message;

            // Forzar color blanco sólido
            infoText.color = new Color(1f, 1f, 1f, 1f);

            // Tamaño de fuente grande
            infoText.fontSize = 36; // Más grande para asegurar que se vea

            // Forzar negrita
            infoText.fontStyle = FontStyle.Bold;

            // Asegurar que todos los padres estén activos
            Transform parent = infoText.transform.parent;
            int nivel = 0;
            while (parent != null)
            {
                Debug.Log($"[CPU_MiniGame] Activando padre nivel {nivel}: {parent.gameObject.name} (activo: {parent.gameObject.activeSelf})");
                parent.gameObject.SetActive(true);
                parent = parent.parent;
                nivel++;
            }

            // Añadir Outline si no tiene
            Outline outline = infoText.GetComponent<Outline>();
            if (outline == null)
            {
                outline = infoText.gameObject.AddComponent<Outline>();
                Debug.Log("[CPU_MiniGame] Outline añadido al InfoText");
            }
            outline.effectColor = new Color(0f, 0f, 0f, 1f); // Negro sólido
            outline.effectDistance = new Vector2(3f, -3f);
            outline.enabled = true;

            Debug.Log("[CPU_MiniGame] ===== DEBUG INFOTEXT =====");
            Debug.Log($"[CPU_MiniGame] - GameObject: {infoText.gameObject.name}");
            Debug.Log($"[CPU_MiniGame] - Texto asignado: '{infoText.text}'");
            Debug.Log($"[CPU_MiniGame] - Color: {infoText.color}");
            Debug.Log($"[CPU_MiniGame] - FontSize: {infoText.fontSize}");
            Debug.Log($"[CPU_MiniGame] - FontStyle: {infoText.fontStyle}");
            Debug.Log($"[CPU_MiniGame] - Activo (self): {infoText.gameObject.activeSelf}");
            Debug.Log($"[CPU_MiniGame] - Activo (jerarquía): {infoText.gameObject.activeInHierarchy}");
            Debug.Log($"[CPU_MiniGame] - Enabled: {infoText.enabled}");
            Debug.Log($"[CPU_MiniGame] - Alignment: {infoText.alignment}");
            Debug.Log($"[CPU_MiniGame] - ResizeTextForBestFit: {infoText.resizeTextForBestFit}");

            // Debug del Canvas del texto
            Canvas textCanvas = infoText.GetComponentInParent<Canvas>();
            if (textCanvas != null)
            {
                Debug.Log($"[CPU_MiniGame] - Canvas padre: {textCanvas.gameObject.name}");
                Debug.Log($"[CPU_MiniGame] - Canvas RenderMode: {textCanvas.renderMode}");
                Debug.Log($"[CPU_MiniGame] - Canvas SortingOrder: {textCanvas.sortingOrder}");
            }

            // Debug del RectTransform
            DebugInfoText();

            // Forzar rebuild
            Canvas.ForceUpdateCanvases();

            Debug.Log("[CPU_MiniGame] ===== FIN DEBUG INFOTEXT =====");
        }
        else
        {
            Debug.LogError("[CPU_MiniGame] ========================================");
            Debug.LogError("[CPU_MiniGame] InfoText es NULL!");
            Debug.LogError("[CPU_MiniGame] Asigna la referencia en el Inspector");
            Debug.LogError("[CPU_MiniGame] ========================================");
        }
    }

    void DebugInfoText()
    {
        if (infoText == null) return;

        RectTransform rt = infoText.GetComponent<RectTransform>();
        if (rt != null)
        {
            Debug.Log("[CPU_MiniGame] ----- RectTransform -----");
            Debug.Log($"  - anchoredPosition: {rt.anchoredPosition}");
            Debug.Log($"  - sizeDelta: {rt.sizeDelta}");
            Debug.Log($"  - anchorMin: {rt.anchorMin}");
            Debug.Log($"  - anchorMax: {rt.anchorMax}");
            Debug.Log($"  - pivot: {rt.pivot}");
            Debug.Log($"  - localScale: {rt.localScale}");
            Debug.Log($"  - localPosition: {rt.localPosition}");
            Debug.Log($"  - position (world): {rt.position}");

            // Verificar si está dentro de la pantalla
            Vector3[] worldCorners = new Vector3[4];
            rt.GetWorldCorners(worldCorners);
            Debug.Log($"  - World corners[0] (bottom-left): {worldCorners[0]}");
            Debug.Log($"  - World corners[2] (top-right): {worldCorners[2]}");

            float screenHeight = Screen.height;
            float screenWidth = Screen.width;
            Debug.Log($"  - Screen size: {screenWidth}x{screenHeight}");

            bool isVisible = worldCorners[2].y > 0 && worldCorners[0].y < screenHeight &&
                           worldCorners[2].x > 0 && worldCorners[0].x < screenWidth;
            Debug.Log($"  - ¿Visible en pantalla? {isVisible}");

            // Debug del padre
            Debug.Log($"  - Padre: {rt.parent?.gameObject.name ?? "NULL"}");
            if (rt.parent != null)
            {
                RectTransform parentRT = rt.parent.GetComponent<RectTransform>();
                if (parentRT != null)
                {
                    Debug.Log($"  - Padre sizeDelta: {parentRT.sizeDelta}");
                    Debug.Log($"  - Padre anchoredPosition: {parentRT.anchoredPosition}");
                }
            }
        }
    }

    // Paso 1: Abrir el socket
    public void OnLeverOpenClicked()
    {
        Debug.Log("[CPU_MiniGame] ¡¡¡OnLeverOpenClicked llamado!!!");

        if (socketOpened)
        {
            Debug.Log("[CPU_MiniGame] Socket ya está abierto, ignorando");
            return;
        }

        socketOpened = true;

        ConfigureInfoText("2. Arrastra el CPU al socket (alinea correctamente)");

        if (socketClosed != null)
        {
            Debug.Log("[CPU_MiniGame] Desactivando socketClosed");
            socketClosed.SetActive(false);
        }

        if (socketOpen != null)
        {
            Debug.Log("[CPU_MiniGame] Activando socketOpen");
            socketOpen.SetActive(true);
        }

        if (cpuDraggable != null)
        {
            Debug.Log("[CPU_MiniGame] Activando cpuDraggable");
            cpuDraggable.gameObject.SetActive(true);
        }

        if (leverClickAreaOpen != null)
            leverClickAreaOpen.interactable = false;
    }

    // Paso 2: Colocar el CPU
    void OnCPUPlaced(MiniGameDraggable draggable)
    {
        Debug.Log("[CPU_MiniGame] CPU colocado");

        if (!socketOpened)
        {
            ConfigureInfoText("¡Primero abre el socket haciendo click en la palanca!");
            return;
        }

        ConfigureInfoText("3. Haz click en la palanca naranja para cerrar el socket");

        if (cpuDraggable != null)
        {
            var draggableComponent = cpuDraggable.GetComponent<MiniGameDraggable>();
            if (draggableComponent != null)
            {
                draggableComponent.enabled = false;
                Debug.Log("[CPU_MiniGame] CPU fijado, no se puede arrastrar más");
            }

            var cpuImage = cpuDraggable.GetComponent<Image>();
            if (cpuImage != null)
            {
                cpuImage.color = new Color(1f, 1f, 1f, 0.9f);
            }
        }

        if (leverClickAreaClose != null)
        {
            leverClickAreaClose.interactable = true;
            Debug.Log("[CPU_MiniGame] Palanca de cierre activada");
        }
    }

    // Paso 3: Cerrar el socket
    public void OnLeverCloseClicked()
    {
        Debug.Log("[CPU_MiniGame] ¡¡¡OnLeverCloseClicked llamado!!!");

        if (!socketOpened)
        {
            ConfigureInfoText("¡Primero abre el socket!");
            return;
        }

        ConfigureInfoText("¡CPU instalado correctamente!");

        if (socketOpen != null)
        {
            Debug.Log("[CPU_MiniGame] Desactivando socketOpen");
            socketOpen.SetActive(false);
        }

        if (cpuDraggable != null)
        {
            Debug.Log("[CPU_MiniGame] Ocultando cpuDraggable");
            cpuDraggable.gameObject.SetActive(false);
        }

        if (socketWithCPU != null)
        {
            Debug.Log("[CPU_MiniGame] Activando socketWithCPU");
            socketWithCPU.SetActive(true);
        }

        if (leverClickAreaClose != null)
            leverClickAreaClose.interactable = false;

        StartCoroutine(CompleteMiniGameAfterDelay(1.5f));
    }

    IEnumerator CompleteMiniGameAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        Debug.Log("[CPU_MiniGame] Completando minijuego");
        completion?.Invoke(true);
    }

    public void Abort()
    {
        Debug.Log("[CPU_MiniGame] Minijuego abortado");
        completion?.Invoke(false);
    }

    void OnDestroy()
    {
        if (leverClickAreaOpen != null)
            leverClickAreaOpen.onClick.RemoveListener(OnLeverOpenClicked);

        if (leverClickAreaClose != null)
            leverClickAreaClose.onClick.RemoveListener(OnLeverCloseClicked);

        if (cpuDropZone != null)
            cpuDropZone.onItemDropped.RemoveListener(OnCPUPlaced);
    }
}