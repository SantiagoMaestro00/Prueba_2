using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class CabinetSlotUI : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [Header("Tipo aceptado (RAM, CPU, Almacenamiento)")]
    public string aceptaTipo = "RAM";

    [Header("Minijuego")]
    public bool requiereMinijuego = false;
    public string minijuegoId = "CPU";

    [Header("Estado")]
    public bool ocupado = false;
    public Arrastrable itemColocado;

    [Header("Ajuste del ÍTEM dentro del slot")]
    public Vector2 padding = new Vector2(4, 4);
    public float rotationZForAccepted = 0f;
    public bool fitByHeight = true;
    public bool fillExact = false;

    [Header("Quitar componente")]
    public bool permiteQuitar = true;
    public PointerEventData.InputButton botonQuitar = PointerEventData.InputButton.Right;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;
        var arr = eventData.pointerDrag.GetComponent<Arrastrable>();
        if (arr == null) return;
        TryHandleDrop(arr);
    }

    public bool TryHandleDrop(Arrastrable arr)
    {
        Debug.Log($"[CabinetSlotUI] TryHandleDrop llamado. Tipo={arr.tipoComponente}, AceptaTipo={aceptaTipo}, RequiereMinijuego={requiereMinijuego}");

        if (ocupado)
        {
            Debug.Log("[CabinetSlotUI] Slot ya ocupado");
            arr.VolverAlInicio();
            return false;
        }

        var tipo = (arr.tipoComponente ?? "").Trim();
        if (!string.Equals(tipo, (aceptaTipo ?? "").Trim(), StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log($"[CabinetSlotUI] Tipo no coincide: '{tipo}' != '{aceptaTipo}'");
            arr.VolverAlInicio();
            return false;
        }

        if (requiereMinijuego && MiniGameManager.Instance != null)
        {
            Debug.Log($"[CabinetSlotUI] Iniciando minijuego con id='{minijuegoId}'");
            MiniGameManager.Instance.StartMiniGame(minijuegoId, arr, this);
        }
        else
        {
            Debug.Log("[CabinetSlotUI] Colocando directo sin minijuego");
            arr.AttachToSlot((RectTransform)transform, rotationZForAccepted, padding, fitByHeight, fillExact);
            ocupado = true;
            itemColocado = arr;
            arr.MarcarColocadoEnSlot();
        }
        return true;
    }

    public void AcceptPlacementFromMinigame(Arrastrable arr)
    {
        Debug.Log("[CabinetSlotUI] AcceptPlacementFromMinigame llamado");

        if (ocupado) return;
        arr.AttachToSlot((RectTransform)transform, rotationZForAccepted, padding, fitByHeight, fillExact);
        ocupado = true;
        itemColocado = arr;
        arr.MarcarColocadoEnSlot();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!permiteQuitar || itemColocado == null) return;
        if (eventData.button != botonQuitar) return;
        QuitarComponente();
    }

    public void QuitarComponente()
    {
        if (itemColocado == null) return;
        itemColocado.RestoreToHome();
        itemColocado = null;
        ocupado = false;
    }
}