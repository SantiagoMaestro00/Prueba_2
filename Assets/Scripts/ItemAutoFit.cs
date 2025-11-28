using UnityEngine;
using UnityEngine.UI;

public class ItemAutoFit : MonoBehaviour, IAutoFit
{
    public enum FitMode { FitInside, FillStretch, Fill, FitHeight, FitWidth, AutoInsideOrStretch }

    [Header("Ajuste dentro de la ranura")]
    public Vector2 padding = new Vector2(6, 6);

    [Tooltip("Rotación en Z al ponerlo en el slot (0 para CPU/HDD). Usa 90 o -90 si necesitas vertical.")]
    public float rotationZ = 0f;

    [Tooltip("FillStretch rellena todo el rect del slot; FitInside mantiene proporción.")]
    public FitMode fitMode = FitMode.FitInside;

    [Tooltip("Modo Auto: si FitInside requiere > umbral del ancho disponible, cambia a FillStretch.")]
    [Range(0.5f, 1.0f)]
    public float autoStretchThreshold = 0.9f;

    public void FitIntoSlot(RectTransform slot)
    {
        var rt = GetComponent<RectTransform>();
        var img = GetComponent<Image>();

        // Reparent local al slot
        transform.SetParent(slot, false);

        // Rotación y base
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.localRotation = Quaternion.Euler(0, 0, rotationZ);
        rt.localScale = Vector3.one;

        // Área útil del slot (en ejes de pantalla)
        var slotRect = slot.rect;
        float areaW = Mathf.Max(0, slotRect.width - padding.x * 2f);
        float areaH = Mathf.Max(0, slotRect.height - padding.y * 2f);

        // Auto decide entre Inside y Stretch
        FitMode mode = fitMode;
        if (fitMode == FitMode.AutoInsideOrStretch)
        {
            GetSpriteSize(img, rt, out float sprW, out float sprH);
            bool rot90 = Mathf.Abs(NormalizeAngle(rotationZ)) == 90f;
            float contentW = rot90 ? sprH : sprW;
            float contentH = rot90 ? sprW : sprH;

            float scaleInside = Mathf.Min(
                contentW > 0 ? areaW / contentW : 1f,
                contentH > 0 ? areaH / contentH : 1f
            );
            float fitInsideW = contentW * scaleInside;

            mode = (fitInsideW > areaW * autoStretchThreshold) ? FitMode.FillStretch : FitMode.FitInside;
        }

        if (mode == FitMode.FillStretch)
        {
            // Ocupa TODO el rect (con padding) vía anchors: robusto ante cambios de resolución
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = padding;
            rt.offsetMax = -padding;
            rt.anchoredPosition = Vector2.zero;

            if (img) { img.preserveAspect = false; img.raycastTarget = false; }
            return;
        }

        // Para los demás modos usamos sizeDelta, corrigiendo ejes si hay rotación ±90°
        GetSpriteSize(img, rt, out float imgW, out float imgH);
        bool rotated90 = Mathf.Abs(NormalizeAngle(rotationZ)) == 90f;

        float contentW2 = rotated90 ? imgH : imgW;
        float contentH2 = rotated90 ? imgW : imgH;

        float finalScreenW = areaW;
        float finalScreenH = areaH;

        switch (mode)
        {
            case FitMode.Fill:
                finalScreenW = areaW; finalScreenH = areaH;
                break;

            case FitMode.FitWidth:
                finalScreenW = areaW;
                finalScreenH = (contentH2 / Mathf.Max(contentW2, 1f)) * finalScreenW;
                if (finalScreenH > areaH)
                {
                    finalScreenH = areaH;
                    finalScreenW = (contentW2 / Mathf.Max(contentH2, 1f)) * finalScreenH;
                }
                break;

            case FitMode.FitHeight:
                finalScreenH = areaH;
                finalScreenW = (contentW2 / Mathf.Max(contentH2, 1f)) * finalScreenH;
                if (finalScreenW > areaW)
                {
                    finalScreenW = areaW;
                    finalScreenH = (contentH2 / Mathf.Max(contentW2, 1f)) * finalScreenW;
                }
                break;

            case FitMode.FitInside:
            default:
                {
                    float scale = Mathf.Min(
                        contentW2 > 0 ? areaW / contentW2 : 1f,
                        contentH2 > 0 ? areaH / contentH2 : 1f
                    );
                    finalScreenW = contentW2 * scale;
                    finalScreenH = contentH2 * scale;
                }
                break;
        }

        // Intercambio de ejes locales si rotación es ±90°
        Vector2 finalLocalSize = rotated90
            ? new Vector2(finalScreenH, finalScreenW)
            : new Vector2(finalScreenW, finalScreenH);

        // Centrado con sizeDelta
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = finalLocalSize;

        if (img) { img.preserveAspect = false; img.raycastTarget = false; }
    }

    private void GetSpriteSize(Image img, RectTransform rt, out float w, out float h)
    {
        if (img && img.sprite)
        {
            w = img.sprite.rect.width;
            h = img.sprite.rect.height;
        }
        else
        {
            // Fallback al tamaño actual del rect
            w = Mathf.Max(rt.rect.width, 1f);
            h = Mathf.Max(rt.rect.height, 1f);
        }
    }

    private float NormalizeAngle(float z)
    {
        z %= 360f;
        if (z > 180f) z -= 360f;
        if (z < -180f) z += 360f;
        return z;
    }
}