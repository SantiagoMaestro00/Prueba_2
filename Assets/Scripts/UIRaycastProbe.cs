using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIRaycastProbe : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            var raycasters = FindObjectsOfType<GraphicRaycaster>(true);
            Debug.Log($"[Probe] Raycasters activos en escena: {raycasters.Length} -> {string.Join(", ", raycasters.Select(rc => rc.gameObject.name))}");
        }

        if (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.P)) return;

        if (EventSystem.current == null)
        {
            Debug.LogWarning("[Probe] No hay EventSystem en la escena.");
            return;
        }

        var ped = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(ped, results);

        if (results.Count == 0)
        {
            Debug.Log("[Probe] No hay NADA bajo el cursor (UI).");
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("[Probe] Raycasts bajo el cursor (de arriba a abajo):");
        int i = 0;
        foreach (var r in results)
        {
            var go = r.gameObject;
            var img = go.GetComponent<Image>();
            var slot = go.GetComponent<CabinetSlotUI>();
            var cg = go.GetComponentInParent<CanvasGroup>(true);
            var canvas = go.GetComponentInParent<Canvas>(true);

            sb.Append($"{++i}. {GetPath(go)}");
            sb.Append($" | Canvas={(canvas != null ? canvas.name : "none")}");
            sb.Append($" | Image.RaycastTarget={(img != null && img.raycastTarget)}");
            if (slot != null) sb.Append(" | CabinetSlotUI=SI");
            if (cg != null) sb.Append($" | CanvasGroup(blocksRaycasts={cg.blocksRaycasts}, interactable={cg.interactable}, alpha={cg.alpha})");
            sb.AppendLine();
        }
        Debug.Log(sb.ToString());
    }

    private string GetPath(GameObject go)
    {
        var path = go.name;
        var t = go.transform.parent;
        while (t != null)
        {
            path = t.name + "/" + path;
            t = t.parent;
        }
        return path;
    }
}