using UnityEngine;
using UnityEngine.EventSystems; // NECESARIO PARA DETECTAR LA UI
using System.Collections.Generic; // NECESARIO PARA LAS LISTAS

public class Arrastrable : MonoBehaviour
{
    // --- 1. COMPATIBILIDAD ---
    [Header("Identificación")]
    public string tipoComponente;

    // --- 2. VARIABLES DE MOVIMIENTO ---
    private Vector3 offset;
    private float zCoord;
    private bool estaSiendoArrastrado = false;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void OnMouseDown()
    {
        zCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
        offset = gameObject.transform.position - GetMouseWorldPos();
        estaSiendoArrastrado = true;
    }

    void OnMouseDrag()
    {
        transform.position = GetMouseWorldPos() + offset;
    }

    // --- 3. LÓGICA DE SOLTAR (HÍBRIDA) ---
    void OnMouseUp()
    {
        estaSiendoArrastrado = false;
        bool encontreSlot = false;

        Debug.Log("--- SOLTANDO PIEZA ---");

        // ========================================================================
        // INTENTO A: BUSCAR EN LA INTERFAZ (UI) <-- ¡ESTO ES LO QUE FALTABA!
        // ========================================================================
        if (EventSystem.current != null)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> resultadosUI = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, resultadosUI);

            foreach (RaycastResult resultado in resultadosUI)
            {
                Debug.Log("UI Detectada: " + resultado.gameObject.name);

                // Buscamos si el objeto UI tiene el tag "Slot"
                if (resultado.gameObject.CompareTag("Slot"))
                {
                    Debug.Log("¡ENCONTRÉ EL SLOT EN LA UI!");
                    ProcesarSlot(resultado.gameObject);
                    encontreSlot = true;
                    break;
                }
            }
        }

        // ========================================================================
        // INTENTO B: BUSCAR EN LA FÍSICA (Si no encontramos nada en la UI)
        // ========================================================================
        if (!encontreSlot)
        {
            Vector2 mousePos2D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D[] cosasFisicas = Physics2D.OverlapPointAll(mousePos2D);

            foreach (Collider2D col in cosasFisicas)
            {
                if (col.gameObject == gameObject) continue; // Ignorarnos a nosotros mismos

                if (col.CompareTag("Slot"))
                {
                    Debug.Log("¡ENCONTRÉ EL SLOT EN FÍSICA!");
                    ProcesarSlot(col.gameObject);
                    encontreSlot = true;
                    break;
                }
            }
        }

        // Si fallaron los dos intentos, regresamos
        if (!encontreSlot)
        {
            Debug.Log("No encontré nada. Regresando.");
            VolverAlInicio();
        }
    }

    // Función auxiliar para no repetir código
    void ProcesarSlot(GameObject slotObject)
    {
        RectTransform slotTransform = slotObject.GetComponent<RectTransform>();
        if (slotTransform != null)
        {
            PlacementEvents.ComponentPlaced(this.tipoComponente, this, slotTransform);
            Debug.Log("Evento enviado al MiniGameManager.");
        }
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = zCoord;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    // --- 4. FUNCIONES DE COMPATIBILIDAD ---
    public void VolverAlInicio() { transform.position = startPos; }
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