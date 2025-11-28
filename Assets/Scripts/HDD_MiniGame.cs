using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class HDD_MiniGame : MonoBehaviour, IMiniGame
{
    [Header("UI Texto")]
    public Text infoText;

    [Header("Elementos draggables")]
    public MiniGameDraggable hddDraggable;
    public MiniGameDraggable sataCableDraggable;

    [Header("Drop Zones")]
    public MiniGameDropZone hddDropZone;
    public MiniGameDropZone sataDropZone;

    [Header("Visuales - Estados bahía (vertical)")]
    public GameObject emptyBay;          // Sprite 1: Bahía vacía
    public GameObject bayWithHDD;        // Sprite 2: Bahía con HDD
    public GameObject bayWithHDDCable;   // Sprite 4: Bahía con HDD + cable SATA

    [Header("Panel bloqueador (opcional)")]
    public GameObject blockerPanel;

    // Variables privadas
    private Arrastrable currentItem;
    private CabinetSlotUI currentSlot;
    private Action<bool> completion;

    private bool hddInstalled = false;
    //private bool cableConnected = false;

    void Start()
    {
        Debug.Log("=== [HDD_MiniGame] START ===");
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

            Debug.Log($"[HDD_MiniGame] Canvas configurado correctamente");
        }
        else
        {
            Debug.LogError("[HDD_MiniGame] No se encontró Canvas!");
        }
    }

    public void Init(Arrastrable item, CabinetSlotUI slot, Action<bool> onComplete)
    {
        this.currentItem = item;
        this.currentSlot = slot;
        this.completion = onComplete;

        Debug.Log("=== [HDD_MiniGame] INIT LLAMADO ===");

        ConfigureCanvas();

        hddInstalled = false;
        //cableConnected = false;

        // Estado inicial: solo bahía vacía visible
        if (emptyBay != null) emptyBay.SetActive(true);
        if (bayWithHDD != null) bayWithHDD.SetActive(false);
        if (bayWithHDDCable != null) bayWithHDDCable.SetActive(false);

        // HDD draggable visible, cable oculto
        if (hddDraggable != null) hddDraggable.gameObject.SetActive(true);
        if (sataCableDraggable != null) sataCableDraggable.gameObject.SetActive(false);

        if (blockerPanel != null) blockerPanel.SetActive(true);

        UpdateInfoText("Paso 1: Arrastra el disco duro a la bahía");

        // Configurar drop zones
        if (hddDropZone != null)
        {
            hddDropZone.onItemDropped.RemoveAllListeners();
            hddDropZone.onItemDropped.AddListener(OnHDDPlaced);
            Debug.Log("[HDD_MiniGame] HDD DropZone configurada");
        }

        if (sataDropZone != null)
        {
            sataDropZone.onItemDropped.RemoveAllListeners();
            sataDropZone.onItemDropped.AddListener(OnCablePlaced);
            Debug.Log("[HDD_MiniGame] SATA DropZone configurada");
        }

        Debug.Log("=== [HDD_MiniGame] INIT COMPLETADO ===");
    }

    void UpdateInfoText(string message)
    {
        Debug.Log($"[HDD_MiniGame] Texto: {message}");

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

    // PASO 1: Usuario arrastra HDD a la bahía
    void OnHDDPlaced(MiniGameDraggable draggable)
    {
        Debug.Log("[HDD_MiniGame] ¡HDD colocado en bahía!");

        hddInstalled = true;

        UpdateInfoText("Paso 2: Arrastra el cable SATA al disco duro");

        // Cambiar visual: bahía vacía → bahía con HDD
        if (emptyBay != null) emptyBay.SetActive(false);
        if (bayWithHDD != null) bayWithHDD.SetActive(true);

        // Ocultar HDD draggable
        if (hddDraggable != null) hddDraggable.gameObject.SetActive(false);

        // Mostrar cable SATA para conectar
        if (sataCableDraggable != null) sataCableDraggable.gameObject.SetActive(true);

        Debug.Log("[HDD_MiniGame] HDD instalado, cable SATA ahora visible");
    }

    // PASO 2: Usuario arrastra cable SATA al HDD
    void OnCablePlaced(MiniGameDraggable draggable)
    {
        Debug.Log("[HDD_MiniGame] ¡Cable SATA conectado!");

        if (!hddInstalled)
        {
            UpdateInfoText("¡Primero instala el disco duro en la bahía!");
            return;
        }

        //cableConnected = true;

        UpdateInfoText("¡Disco duro instalado y conectado correctamente! ✓");

        // Cambiar visual: HDD solo → HDD + cable
        if (bayWithHDD != null) bayWithHDD.SetActive(false);
        if (bayWithHDDCable != null) bayWithHDDCable.SetActive(true);

        // Ocultar cable draggable
        if (sataCableDraggable != null) sataCableDraggable.gameObject.SetActive(false);

        // Completar minijuego después de un delay
        StartCoroutine(CompleteMiniGameAfterDelay(1.5f));
    }

    IEnumerator CompleteMiniGameAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        Debug.Log("[HDD_MiniGame] Completando minijuego - ¡ÉXITO!");
        completion?.Invoke(true);
    }

    public void Abort()
    {
        Debug.Log("[HDD_MiniGame] Minijuego abortado");
        completion?.Invoke(false);
    }

    void OnDestroy()
    {
        // Limpiar listeners
        if (hddDropZone != null)
            hddDropZone.onItemDropped.RemoveListener(OnHDDPlaced);

        if (sataDropZone != null)
            sataDropZone.onItemDropped.RemoveListener(OnCablePlaced);
    }
}