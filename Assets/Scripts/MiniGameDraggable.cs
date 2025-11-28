using UnityEngine;
using UnityEngine.EventSystems;

public class MiniGameDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 originalPosition;
    private Transform originalParent;
    private int originalSiblingIndex;

    [Header("Configuración")]
    public bool returnToStartIfNotDropped = true;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform.anchoredPosition;
        originalParent = rectTransform.parent;
        originalSiblingIndex = rectTransform.GetSiblingIndex();

        rectTransform.SetParent(canvas.transform);
        rectTransform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        bool dropped = false;
        foreach (var result in results)
        {
            var dropZone = result.gameObject.GetComponent<MiniGameDropZone>();
            if (dropZone != null && dropZone.AcceptDrop(this))
            {
                dropped = true;
                break;
            }
        }

        if (!dropped && returnToStartIfNotDropped)
        {
            rectTransform.SetParent(originalParent);
            rectTransform.SetSiblingIndex(originalSiblingIndex);
            rectTransform.anchoredPosition = originalPosition;
        }
    }

    public void SnapToPosition(RectTransform target)
    {
        rectTransform.SetParent(target);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localScale = Vector3.one;
    }
}