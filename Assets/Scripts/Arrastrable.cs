using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class Arrastrable : MonoBehaviour
{
    // --- 1. CONFIGURACIÓN ---
    [Header("Identificación")]
    public string tipoComponente;

    // --- 2. VARIABLES INTERNAS ---
    private Vector3 offset;
    private float zCoord;
    private bool estaSiendoArrastrado = false;
    private Vector3 startPos;

    private Vector3 escalaOriginal;
    private SpriteRenderer mySprite;
    private int ordenCapaOriginal;

    void Start()
    {
        startPos = transform.position;
        escalaOriginal = transform.localScale;
        mySprite = GetComponent<SpriteRenderer>();
        if (mySprite) ordenCapaOriginal = mySprite.sortingOrder;
    }

    // --- CLIC DERECHO: CANCELAR ---
    void OnMouseOver()
    {
        if (!estaSiendoArrastrado && Input.GetMouseButtonDown(1))
        {
            // Avisamos que soltamos la pieza (para apagar luces verdes por si acaso)
            CabinetSlotUI.NotificarSoltar();

            Debug.Log("Cancelando pieza.");
            RestoreToHome();
        }
    }

    void OnMouseDown()
    {
        if (Input.GetMouseButton(0))
        {
            zCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
            offset = gameObject.transform.position - GetMouseWorldPos();
            estaSiendoArrastrado = true;

            // Visual
            transform.localScale = escalaOriginal * 1.2f;
            if (mySprite) mySprite.sortingOrder = 100;

            // 👇 NUEVO: AVISAR A LAS RANURAS "¡ENCIÉNDANSE!" 👇
            CabinetSlotUI.NotificarAgarre(this.tipoComponente);
        }
    }

    void OnMouseDrag()
    {
        if (estaSiendoArrastrado)
        {
            transform.position = GetMouseWorldPos() + offset;
        }
    }

    void OnMouseUp()
    {
        if (!estaSiendoArrastrado) return;
        estaSiendoArrastrado = false;

        // Restaurar visual
        transform.localScale = escalaOriginal;
        if (mySprite) mySprite.sortingOrder = ordenCapaOriginal;

        // 👇 NUEVO: AVISAR A LAS RANURAS "¡APÁGUENSE!" 👇
        CabinetSlotUI.NotificarSoltar();

        if (Input.GetMouseButtonUp(0) == false) return;

        bool encontreSlot = false;

        // A. UI RAYCAST
        if (EventSystem.current != null)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
            List<RaycastResult> resultadosUI = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, resultadosUI);

            foreach (RaycastResult resultado in resultadosUI)
            {
                if (resultado.gameObject.CompareTag("Slot"))
                {
                    ProcesarSlot(resultado.gameObject);
                    encontreSlot = true;
                    break;
                }
            }
        }

        // B. FÍSICA RAYCAST
        if (!encontreSlot)
        {
            Vector2 mousePos2D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D[] cosasFisicas = Physics2D.OverlapPointAll(mousePos2D);

            foreach (Collider2D col in cosasFisicas)
            {
                if (col.gameObject == gameObject) continue;
                if (col.CompareTag("Slot"))
                {
                    ProcesarSlot(col.gameObject);
                    encontreSlot = true;
                    break;
                }
            }
        }

        if (!encontreSlot)
        {
            VolverAlInicio();
        }
    }

    void ProcesarSlot(GameObject slotObject)
    {
        CabinetSlotUI slotScript = slotObject.GetComponent<CabinetSlotUI>();

        if (slotScript != null)
        {
            string miTipo = (this.tipoComponente ?? "").Trim();
            string tipoAceptado = (slotScript.aceptaTipo ?? "").Trim();

            if (miTipo != tipoAceptado)
            {
                Debug.LogWarning($"[Arrastrable] Incompatible: {miTipo} vs {tipoAceptado}");
                if (NotificationManager.Instance != null)
                {
                    string mensajeError = GenerarMensajeEducativo(miTipo, tipoAceptado);
                    NotificationManager.Instance.MostrarError(mensajeError);
                }
                StartCoroutine(AnimacionRechazo());
                return;
            }
        }

        RectTransform slotTransform = slotObject.GetComponent<RectTransform>();
        if (slotTransform != null)
        {
            PlacementEvents.ComponentPlaced(this.tipoComponente, this, slotTransform);
        }
    }

    IEnumerator AnimacionRechazo()
    {
        Color colorOriginal = Color.white;
        if (mySprite != null) { colorOriginal = mySprite.color; mySprite.color = Color.red; }

        Vector3 posicionBase = transform.position;
        float duracion = 0.4f;
        float tiempo = 0;

        while (tiempo < duracion)
        {
            transform.position = posicionBase + (Vector3)(UnityEngine.Random.insideUnitCircle * 0.15f);
            tiempo += Time.deltaTime;
            yield return null;
        }

        if (mySprite != null) mySprite.color = colorOriginal;
        VolverAlInicio();
    }

    string GenerarMensajeEducativo(string mio, string slot)
    {
        if (mio.Contains("DDR3") && slot.Contains("DDR4")) return "¡ERROR! Esta RAM es DDR3 y no encaja en DDR4.";
        if (mio.Contains("1150") && slot.Contains("1200")) return "¡INCOMPATIBLE! Socket incorrecto.";
        if (mio.Contains("IDE") && slot.Contains("SATA")) return "¡OBSOLETO! El disco IDE no entra aquí.";
        return $"Error: {mio} no va en {slot}.";
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = zCoord;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    public void VolverAlInicio()
    {
        transform.position = startPos;
        transform.localScale = escalaOriginal;
    }

    public void RestoreToHome() { Destroy(gameObject); }
    public void MarcarColocadoEnSlot()
    {
        this.enabled = false;
        if (GetComponent<Collider2D>()) GetComponent<Collider2D>().enabled = false;
    }
    public void AttachToSlot(RectTransform slot, float r, Vector2 p, bool h, bool f)
    {
        if (slot != null) transform.position = slot.position;
        transform.rotation = Quaternion.Euler(0, 0, r);
        MarcarColocadoEnSlot();
    }
}