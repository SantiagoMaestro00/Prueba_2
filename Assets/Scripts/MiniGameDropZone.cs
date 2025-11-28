using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class MiniGameDropZone : MonoBehaviour
{
    [Header("Configuración")]
    public List<string> acceptedTags = new List<string>();
    public bool snapToCenter = true;
    public bool onlyAcceptOne = true;

    [Header("Eventos")]
    public UnityEvent<MiniGameDraggable> onItemDropped;

    private MiniGameDraggable currentItem;

    public bool AcceptDrop(MiniGameDraggable draggable)
    {
        if (onlyAcceptOne && currentItem != null) return false;

        bool accepts = acceptedTags.Count == 0 || acceptedTags.Contains(draggable.tag);
        if (!accepts) return false;

        currentItem = draggable;
        if (snapToCenter)
        {
            draggable.SnapToPosition(GetComponent<RectTransform>());
        }

        onItemDropped?.Invoke(draggable);
        return true;
    }

    public void Clear()
    {
        currentItem = null;
    }
}