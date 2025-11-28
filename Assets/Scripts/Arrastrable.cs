using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(Image))]
public class Arrastrable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Tipo del componente (RAM, CPU, Almacenamiento)")]
    public string tipoComponente;

    public enum HomeRestorePolicy { PreserveAll, ForceCenteredSize }

    [Header("Restauración al inventario")]
    public HomeRestorePolicy homeRestorePolicy = HomeRestorePolicy.ForceCenteredSize;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Image image;

    private Vector3 startPosWorld;
    private Transform startParent;
    private bool colocado = false;

    // Snapshot del estado original (inventario)
    private RectTransform homeParent;
    private int homeSiblingIndex;
    private Vector2 homeAnchorMin, homeAnchorMax;
    private Vector2 homePivot;
    private Vector2 homeAnchoredPos;
    private Vector2 homeSizeDelta;
    private Vector2 homeOffsetMin, homeOffsetMax;
    private Vector3 homeLocalScale;
    private float homeRotationZ;
    private bool homePreserveAspect;
    private bool homeRaycastTarget;
    private Vector2 homeRectSizePixels;
    private bool homeCaptured = false;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }

    void OnEnable()
    {
        if (!homeCaptured)
            StartCoroutine(CaptureHomeAtEndOfFrame());
    }

    void Start()
    {
        if (!homeCaptured)
            StartCoroutine(CaptureHomeAtEndOfFrame());
    }

    private IEnumerator CaptureHomeAtEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        CaptureHomeIfNeeded();
    }

    private void CaptureHomeIfNeeded()
    {
        if (homeCaptured) return;

        homeParent = transform.parent as RectTransform;
        if (homeParent != null)
        {
            homeSiblingIndex = transform.GetSiblingIndex();

            homeAnchorMin = rectTransform.anchorMin;
            homeAnchorMax = rectTransform.anchorMax;
            homePivot = rectTransform.pivot;

            homeAnchoredPos = rectTransform.anchoredPosition;
            homeSizeDelta = rectTransform.sizeDelta;
            homeOffsetMin = rectTransform.offsetMin;
            homeOffsetMax = rectTransform.offsetMax;

            homeLocalScale = rectTransform.localScale;
            homeRotationZ = rectTransform.localEulerAngles.z;

            homePreserveAspect = image != null && image.preserveAspect;
            homeRaycastTarget = image != null && image.raycastTarget;

            homeRectSizePixels = rectTransform.rect.size;

            homeCaptured = true;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        CaptureHomeIfNeeded();

        colocado = false;
        startPosWorld = rectTransform.position;
        startParent = rectTransform.parent;

        canvasGroup.blocksRaycasts = false;
        rectTransform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        StartCoroutine(EndDragAfterFrame());
    }

    private IEnumerator EndDragAfterFrame()
    {
        yield return new WaitForEndOfFrame();

        if (!colocado)
            TryPlaceViaRaycast();

        if (!colocado)
        {
            canvasGroup.blocksRaycasts = true;
            rectTransform.position = startPosWorld;
            rectTransform.SetParent(startParent, true);
        }
    }

    private void TryPlaceViaRaycast()
    {
        if (EventSystem.current == null) return;

        var ped = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(ped, results);

        foreach (var r in results)
        {
            var slot = r.gameObject.GetComponent<CabinetSlotUI>();
            if (slot != null && slot.TryHandleDrop(this))
                return;
        }
    }

    /// <summary>
    /// CORREGIDO: Asegura que PlacementEvents se dispare SIEMPRE.
    /// </summary>
    public void AttachToSlot(RectTransform slot, float rotationZ, Vector2 padding, bool fitByHeight, bool fill)
    {
        Debug.Log($"[Arrastrable] ========================================");
        Debug.Log($"[Arrastrable] AttachToSlot llamado para: '{tipoComponente}'");
        Debug.Log($"[Arrastrable] ========================================");

        // 1) RAM específica
        var ramFit = GetComponent<RamAutoFit>();
        if (ramFit != null)
        {
            Debug.Log("[Arrastrable] Usando RamAutoFit");
            ramFit.FitIntoSlot(slot);
            colocado = true;

            // CRÍTICO: Notificar colocación
            Debug.Log($"[Arrastrable] ✅ Disparando PlacementEvents.ComponentPlaced para: '{tipoComponente}'");
            PlacementEvents.ComponentPlaced(tipoComponente, this, slot);
            return;
        }

        // 2) Auto-fit genérico (CPU/HDD u otros)
        var fitter = GetComponent<IAutoFit>();
        if (fitter != null)
        {
            Debug.Log("[Arrastrable] Usando IAutoFit genérico");
            fitter.FitIntoSlot(slot);
            colocado = true;

            // CRÍTICO: Notificar colocación
            Debug.Log($"[Arrastrable] ✅ Disparando PlacementEvents.ComponentPlaced para: '{tipoComponente}'");
            PlacementEvents.ComponentPlaced(tipoComponente, this, slot);
            return;
        }

        // 3) Genérico de respaldo
        Debug.Log("[Arrastrable] Usando ajuste genérico de respaldo");

        transform.SetParent(slot, false);

        var slotRect = slot.rect;
        float areaW = Mathf.Max(0, slotRect.width - padding.x * 2f);
        float areaH = Mathf.Max(0, slotRect.height - padding.y * 2f);

        float sprW = image && image.sprite ? image.sprite.rect.width : rectTransform.rect.width;
        float sprH = image && image.sprite ? image.sprite.rect.height : rectTransform.rect.height;

        bool rotated90 = Mathf.Abs(NormalizeAngle(rotationZ)) == 90f;

        rectTransform.localRotation = Quaternion.Euler(0, 0, rotationZ);
        rectTransform.localScale = Vector3.one;

        float contentW = rotated90 ? sprH : sprW;
        float contentH = rotated90 ? sprW : sprH;

        float finalW, finalH;
        if (fill)
        {
            finalW = areaW; finalH = areaH;
        }
        else if (fitByHeight)
        {
            finalH = areaH;
            finalW = (contentW / Mathf.Max(contentH, 1f)) * finalH;
            if (finalW > areaW)
            {
                finalW = areaW;
                finalH = (contentH / Mathf.Max(contentW, 1f)) * finalW;
            }
        }
        else
        {
            float scale = Mathf.Min(
                contentW > 0 ? areaW / contentW : 1f,
                contentH > 0 ? areaH / contentH : 1f
            );
            finalW = contentW * scale;
            finalH = contentH * scale;
        }

        Vector2 finalLocalSize = rotated90
            ? new Vector2(finalH, finalW)
            : new Vector2(finalW, finalH);

        rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = finalLocalSize;
        rectTransform.anchoredPosition = Vector2.zero;

        if (image != null)
        {
            image.preserveAspect = false;
            image.raycastTarget = false;
        }

        colocado = true;

        // CRÍTICO: Notificar colocación (fallback genérico)
        Debug.Log($"[Arrastrable] ✅ Disparando PlacementEvents.ComponentPlaced para: '{tipoComponente}'");
        PlacementEvents.ComponentPlaced(tipoComponente, this, slot);
    }

    public void MarcarColocadoEnSlot()
    {
        colocado = true;
        this.enabled = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void RestoreToHome()
    {
        if (!homeCaptured || homeParent == null)
        {
            this.enabled = true;
            colocado = false;
            canvasGroup.blocksRaycasts = true;
            if (image) image.raycastTarget = true;
            return;
        }

        transform.SetParent(homeParent, false);
        transform.SetSiblingIndex(homeSiblingIndex);

        if (homeRestorePolicy == HomeRestorePolicy.ForceCenteredSize)
        {
            rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = homeRectSizePixels;
            rectTransform.anchoredPosition = homeAnchoredPos;
        }
        else
        {
            rectTransform.anchorMin = homeAnchorMin;
            rectTransform.anchorMax = homeAnchorMax;
            rectTransform.pivot = homePivot;

            bool homeIsStretch = (homeAnchorMin != homeAnchorMax);
            if (homeIsStretch)
            {
                rectTransform.offsetMin = homeOffsetMin;
                rectTransform.offsetMax = homeOffsetMax;
            }
            else
            {
                rectTransform.sizeDelta = homeSizeDelta;
                rectTransform.anchoredPosition = homeAnchoredPos;
            }
        }

        rectTransform.localRotation = Quaternion.Euler(0, 0, homeRotationZ);
        rectTransform.localScale = homeLocalScale;

        if (image != null)
        {
            image.preserveAspect = homePreserveAspect;
            image.raycastTarget = homeRaycastTarget;
        }

        this.enabled = true;
        colocado = false;
        canvasGroup.blocksRaycasts = true;

        if (homeParent != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(homeParent);
    }

    public void VolverAlInicio()
    {
        colocado = false;
        rectTransform.position = startPosWorld;
        rectTransform.SetParent(startParent, true);
    }

    private float NormalizeAngle(float z)
    {
        z %= 360f;
        if (z > 180f) z -= 360f;
        if (z < -180f) z += 360f;
        return z;
    }
}