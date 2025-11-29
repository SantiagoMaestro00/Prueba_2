using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // Necesario para las Listas

public class InventarioManager : MonoBehaviour
{
    public static InventarioManager Instance;

    [Header("=== CONFIGURACIÓN PRINCIPAL ===")]
    public GameObject panelInventario; // Arrastra aquí el "Contenedor_UI" (El padre de todo)
    public Transform contenedorItems;  // El objeto "Grid" dentro de la caja roja
    public GameObject slotPrefab;      // El PREFAB del botón (ItemSlot)

    [Header("=== TUS DATOS ===")]
    public List<ComponenteData> inventarioInicial; // Arrastra aquí tus archivos (Datos_RAM, Datos_CPU...)

    [Header("=== ZONA DE DETALLES (Panel Beige) ===")]
    public GameObject panelDetalles;   // El panel beige (Panel_Detalles)
    // public Image iconoGrande;       // (YA NO LO USAMOS, LO QUITASTE DEL DISEÑO)
    public TMP_Text tituloText;        // El texto de arriba
    public TMP_Text descripcionText;   // El texto de abajo
    public Button botonUsar;           // El botón "USAR"

    // Variable para recordar qué pieza seleccionó el usuario
    private ComponenteData itemActualSeleccionado;

    private void Awake()
    {
        // Singleton simple
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }

        // Empezamos con todo cerrado
        CerrarInventario();
    }

    // --- ABRIR Y CERRAR ---

    public void AlternarInventario()
    {
        bool estaAbierto = panelInventario.activeSelf;
        if (estaAbierto)
            CerrarInventario();
        else
            AbrirInventario();
    }

    public void AbrirInventario()
    {
        // 1. Encendemos el PADRE (Contenedor_UI)
        panelInventario.SetActive(true);

        // 2. 👇 ¡ESTA LÍNEA ES LA CLAVE! 👇
        // Obligamos al panel de detalles a apagarse al inicio
        if (panelDetalles != null)
        {
            panelDetalles.SetActive(false);
        }

        // 3. Dibujamos los items
        RenderizarItems();
    }

    public void CerrarInventario()
    {
        panelInventario.SetActive(false);
    }

    // --- DIBUJAR LOS BOTONES ---

    void RenderizarItems()
    {
        // 1. Limpieza: Borrar botones viejos para no duplicarlos
        foreach (Transform child in contenedorItems)
        {
            Destroy(child.gameObject);
        }

        // 2. Creación: Crear un botón por cada dato en la lista
        foreach (var dato in inventarioInicial)
        {
            // Crear el objeto
            GameObject nuevoBoton = Instantiate(slotPrefab, contenedorItems);

            // Configurar su imagen y datos
            ItemSlot scriptDelSlot = nuevoBoton.GetComponent<ItemSlot>();
            if (scriptDelSlot != null)
            {
                scriptDelSlot.Configurar(dato);
            }
        }
    }

    // --- SELECCIÓN Y USO ---

    public void SeleccionarItem(ComponenteData datos)
    {
        itemActualSeleccionado = datos;

        // Mostrar el panel derecho (Esto activa el movimiento lateral)
        panelDetalles.SetActive(true);

        // Llenar la info en tu nuevo diseño
        tituloText.text = datos.nombrePieza;
        descripcionText.text = datos.descripcionEducativa;

        // Activar el botón de usar
        botonUsar.interactable = true;

        // Limpiamos clicks viejos y ponemos el nuevo
        botonUsar.onClick.RemoveAllListeners();
        botonUsar.onClick.AddListener(UsarItemActual);
    }

    public void UsarItemActual()
    {
        if (itemActualSeleccionado != null && itemActualSeleccionado.prefabDeLaPieza != null)
        {
            // 1. Cerramos el inventario
            CerrarInventario();

            // 2. Calculamos posición del mouse
            Vector3 posicionMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            posicionMouse.z = 0;

            // 3. ¡NACIMIENTO DE LA PIEZA!
            Instantiate(itemActualSeleccionado.prefabDeLaPieza, posicionMouse, Quaternion.identity);

            Debug.Log("Creando pieza: " + itemActualSeleccionado.nombrePieza);
        }
        else
        {
            Debug.LogError("ERROR: El componente " + itemActualSeleccionado?.nombrePieza + " no tiene asignado el PREFAB en el archivo de datos.");
        }
    }
}