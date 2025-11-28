using UnityEngine;
using UnityEngine.UI;

public class RamAutoFit : MonoBehaviour
{
    public enum FitMode { FitHeight, FitInside, Fill, FillStretch, Auto }

    [Header("Ajuste dentro de la ranura")]
    public Vector2 padding = new Vector2(2, 2);
    public float rotationZ = 90f;          // 90 o -90 para vertical
    public FitMode fitMode = FitMode.Fill; // Por defecto: lo que te funcionaba

    [Range(0.5f, 1.0f)]
    public float autoFillThreshold = 0.9f;

    public void FitIntoSlot(RectTransform slot)
    {
        var rt = GetComponent<RectTransform>();
        var img = GetComponent<Image>();

        // Reparent en coordenadas locales UI
        transform.SetParent(slot, false);

        // Rotación centrada
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.localRotation = Quaternion.Euler(0, 0, rotationZ);
        rt.localScale = Vector3.one;

        // Área del slot y clamp de padding
        var slotRect = slot.rect;
        float slotW = Mathf.Max(0, slotRect.width);
        float slotH = Mathf.Max(0, slotRect.height);

        float padX = Mathf.Min(padding.x, Mathf.Max(0f, slotW * 0.49f)); // no más de ~la mitad
        float padY = Mathf.Min(padding.y, Mathf.Max(0f, slotH * 0.49f));

        float areaW = Mathf.Max(0, slotW - padX * 2f);
        float areaH = Mathf.Max(0, slotH - padY * 2f);

        // FillStretch: anchors a todo el rect
        if (fitMode == FitMode.FillStretch)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(padX, padY);
            rt.offsetMax = new Vector2(-padX, -padY);
            rt.anchoredPosition = Vector2.zero;

            if (img) { img.preserveAspect = false; img.raycastTarget = false; }
            return;
        }

        // Datos del sprite
        float imgW = (img && img.sprite) ? img.sprite.rect.width : Mathf.Max(rt.rect.width, 1f);
        float imgH = (img && img.sprite) ? img.sprite.rect.height : Mathf.Max(rt.rect.height, 1f);

        float nz = NormalizeAngle(rotationZ);
        bool rotated90 = Mathf.Abs(nz) == 90f;

        // Auto: decidir FillStretch vs FitHeight
        if (fitMode == FitMode.Auto)
        {
            float contentW = rotated90 ? imgH : imgW;
            float contentH = rotated90 ? imgW : imgH;

            float fitHeightW = (contentW / Mathf.Max(contentH, 1f)) * areaH;
            if (fitHeightW > areaW * autoFillThreshold)
            {
                // Cambia a FillStretch con el mismo clamp de padding
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = new Vector2(padX, padY);
                rt.offsetMax = new Vector2(-padX, -padY);
                rt.anchoredPosition = Vector2.zero;

                if (img) { img.preserveAspect = false; img.raycastTarget = false; }
                return;
            }
            // si no, continúa como FitHeight
        }

        // Fill clásico: ocupar toda el área útil (manteniendo ejes correctos)
        float finalScreenW, finalScreenH;
        switch (fitMode)
        {
            case FitMode.Fill:
                finalScreenW = areaW; finalScreenH = areaH; break;
            case FitMode.FitInside:
                {
                    float contentW2 = rotated90 ? imgH : imgW;
                    float contentH2 = rotated90 ? imgW : imgH;
                    float scale = Mathf.Min(
                        contentW2 > 0 ? areaW / contentW2 : 1f,
                        contentH2 > 0 ? areaH / contentH2 : 1f
                    );
                    finalScreenW = contentW2 * scale;
                    finalScreenH = contentH2 * scale;
                }
                break;
            case FitMode.FitHeight:
            default:
                {
                    float contentW2 = rotated90 ? imgH : imgW;
                    float contentH2 = rotated90 ? imgW : imgH;
                    finalScreenH = areaH;
                    finalScreenW = (contentW2 / Mathf.Max(contentH2, 1f)) * finalScreenH;
                    if (finalScreenW > areaW)
                    {
                        finalScreenW = areaW;
                        finalScreenH = (contentH2 / Mathf.Max(contentW2, 1f)) * finalScreenW;
                    }
                }
                break;
        }

        // Intercambio de ejes locales si ±90°
        Vector2 finalLocalSize = rotated90
            ? new Vector2(finalScreenH, finalScreenW)
            : new Vector2(finalScreenW, finalScreenH);

        // Centrado con sizeDelta
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = finalLocalSize;

        if (img) { img.preserveAspect = false; img.raycastTarget = false; }
    }

    private float NormalizeAngle(float z)
    {
        z %= 360f;
        if (z > 180f) z -= 360f;
        if (z < -180f) z += 360f;
        return z;
    }
}