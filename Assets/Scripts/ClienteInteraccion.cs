using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClienteInteraccion : MonoBehaviour
{
    [Header("UI del Cliente")]
    public GameObject iconoNotificacion;
    public Vector3 offsetIcono = new Vector3(0, 2.5f, 0);

    [Header("Panel de Pedido")]
    public GameObject panelPedido;
    public TMP_Text textoPedido;

    [Header("Botones Panel")]
    public Button botonEmpezarTarea;
    public Button botonCerrar;

    [Header("Referencias")]
    public NPCManager npcManager;
    public SceneController sceneController;

    private NPCMovement npcMovement;
    private Button botonIcono;
    private Camera mainCamera;
    private TypewriterEffect typewriter;

    // Variable local de control
    private bool tareaIniciadaLocal = false;

    void Awake()
    {
        npcMovement = GetComponent<NPCMovement>();
        mainCamera = Camera.main;

        if (npcManager == null) npcManager = NPCManager.Instance;
        if (sceneController == null) sceneController = SceneController.Instance;

        if (textoPedido != null)
            typewriter = textoPedido.GetComponent<TypewriterEffect>();
    }

    void OnEnable()
    {
        if (npcMovement != null)
            npcMovement.OnLlegadaAlMostrador += AlLlegarAlMostrador;

        VerificarEstadoIcono();
    }

    void OnDisable()
    {
        if (npcMovement != null)
            npcMovement.OnLlegadaAlMostrador -= AlLlegarAlMostrador;
    }

    void Start()
    {
        OcultarTodo();
        ConfigurarBotones();
    }

    // --- NUEVA FUNCIÓN PARA EL CICLO INFINITO ---
    public void ResetearInteraccion()
    {
        // Olvidamos que hicimos tarea con el cliente anterior
        tareaIniciadaLocal = false;
        OcultarTodo();
        Debug.Log("[ClienteInteraccion] Memoria reseteada para nuevo cliente.");
    }
    // -------------------------------------------

    private void ConfigurarBotones()
    {
        if (iconoNotificacion != null)
        {
            botonIcono = iconoNotificacion.GetComponent<Button>();
            if (botonIcono != null)
            {
                botonIcono.onClick.RemoveAllListeners();
                botonIcono.onClick.AddListener(OnClickIcono);
            }
        }

        if (botonEmpezarTarea != null)
        {
            botonEmpezarTarea.onClick.RemoveAllListeners();
            botonEmpezarTarea.onClick.AddListener(OnClickEmpezarTarea);
        }

        if (botonCerrar != null)
        {
            botonCerrar.onClick.RemoveAllListeners();
            botonCerrar.onClick.AddListener(OnClickCerrar);
        }
    }

    void Update()
    {
        if (iconoNotificacion != null && iconoNotificacion.activeSelf)
        {
            if (sceneController != null && sceneController.tareaComenzada)
            {
                iconoNotificacion.SetActive(false);
                return;
            }

            Vector3 worldPos = transform.position + offsetIcono;
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
            iconoNotificacion.transform.position = screenPos;
        }
    }

    private void AlLlegarAlMostrador()
    {
        MostrarIcono();
    }

    public void MostrarIcono()
    {
        if (tareaIniciadaLocal) return;

        if (sceneController != null && sceneController.tareaComenzada)
        {
            OcultarIcono();
            return;
        }

        if (iconoNotificacion != null) iconoNotificacion.SetActive(true);
    }

    private void VerificarEstadoIcono()
    {
        bool tareaGlobal = (sceneController != null && sceneController.tareaComenzada);

        if (tareaIniciadaLocal || tareaGlobal)
        {
            OcultarIcono();
        }
    }

    public void OcultarIcono()
    {
        if (iconoNotificacion != null) iconoNotificacion.SetActive(false);
    }

    public void OnClickIcono()
    {
        OcultarIcono();
        MostrarPanelPedido();
    }

    public void MostrarPanelPedido()
    {
        if (panelPedido != null) panelPedido.SetActive(true);

        if (npcManager != null)
        {
            Pedido pedidoActual = npcManager.GetPedidoActual();

            if (pedidoActual != null)
            {
                string mensaje = pedidoActual.MensajeAleatorio();

                if (typewriter != null)
                    typewriter.Escribir(mensaje);
                else if (textoPedido != null)
                    textoPedido.text = mensaje;
            }
        }
    }

    private void OnClickEmpezarTarea()
    {
        tareaIniciadaLocal = true;

        if (panelPedido != null) panelPedido.SetActive(false);
        OcultarIcono();

        if (sceneController != null)
        {
            sceneController.ComenzarTarea();
            sceneController.CargarEscena("Taller");
        }
    }

    private void OnClickCerrar()
    {
        if (panelPedido != null) panelPedido.SetActive(false);

        bool tareaGlobal = (sceneController != null && sceneController.tareaComenzada);
        if (!tareaIniciadaLocal && !tareaGlobal)
        {
            MostrarIcono();
        }
    }

    public void OcultarTodo()
    {
        OcultarIcono();
        if (panelPedido != null) panelPedido.SetActive(false);
    }
}