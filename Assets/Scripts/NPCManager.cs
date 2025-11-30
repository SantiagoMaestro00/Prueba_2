using UnityEngine;
using UnityEngine.UI; // Necesario para manipular Textos y Paneles
using System.Collections;

[DisallowMultipleComponent]
public class NPCManager : MonoBehaviour
{
    [System.Serializable]
    public class ClienteData
    {
        public string nombre;
        public string mensaje;
        public Sprite spriteCaminar;
        public Sprite spriteReposo;
        public AnimationClip idleClip;
        public AnimationClip walkClip;
    }

    public static NPCManager Instance;

    [Header("Datos Generales")]
    public ClienteData[] clientes; // Skins variadas
    public Pedido[] pedidos;       // Misiones en orden

    [Header("Referencias Visibles")]
    public SpriteRenderer clienteSpriteRenderer;
    public NPCRequestPanel panel;

    [Header("Animator Override")]
    public AnimationClip baseIdleClip;
    public AnimationClip baseWalkClip;

    [Header("Fin del Juego")]
    public GameObject panelVictoria;
    public Text textoPuntajeFinal;

    private ClienteData clienteActual;
    private Pedido pedidoActual;

    // Variable para llevar el control de la misión actual
    private int indicePedidoActual = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        if (clienteSpriteRenderer == null) clienteSpriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
    }

    void Start()
    {
        if (Instance != this) return;

        if (panelVictoria != null) panelVictoria.SetActive(false);

        StartCoroutine(InitNextFrame());
    }

    IEnumerator InitNextFrame()
    {
        yield return null;
        MostrarSiguienteCliente();
    }

    public void MostrarSiguienteCliente()
    {
        if (clientes == null || clientes.Length == 0) return;
        if (pedidos == null || pedidos.Length == 0) return;

        // 🛑 1. VERIFICAR SI YA TERMINAMOS EL JUEGO
        if (indicePedidoActual >= pedidos.Length)
        {
            Debug.Log("🎉 [NPCManager] ¡No hay más pedidos! Juego Completado.");
            EjecutarVictoria();
            return;
        }

        // 🎨 2. ELEGIR SKIN ALEATORIA
        int clienteId = Random.Range(0, clientes.Length);
        clienteActual = clientes[clienteId];

        // 📜 3. ELEGIR PEDIDO EN ORDEN SECUENCIAL
        pedidoActual = pedidos[indicePedidoActual];

        Debug.Log($"[NPCManager] Generando Cliente. Misión #{indicePedidoActual + 1}: {pedidoActual.name}");

        // 4. CONFIGURAR ANIMACIONES
        var anim = clienteSpriteRenderer.GetComponent<Animator>();
        if (anim == null) anim = GetComponentInChildren<Animator>(true);

        bool overrideOk = (anim != null) && (baseIdleClip != null) && (baseWalkClip != null) &&
                          (clienteActual.idleClip != null) && (clienteActual.walkClip != null);

        if (overrideOk)
        {
            var ovr = new AnimatorOverrideController(anim.runtimeAnimatorController);
            ovr[baseIdleClip] = clienteActual.idleClip;
            ovr[baseWalkClip] = clienteActual.walkClip;
            anim.runtimeAnimatorController = ovr;
        }
        else
        {
            if (clienteActual.spriteCaminar != null)
                clienteSpriteRenderer.sprite = clienteActual.spriteCaminar;
        }
    }

    public void MostrarPedidoActual()
    {
        if (panel != null && pedidoActual != null)
            panel.ShowPanel(pedidoActual.MensajeAleatorio());
    }

    public void SolicitarSiguienteCliente(float tiempoEspera = 3f)
    {
        indicePedidoActual++;
        StartCoroutine(GenerarClienteConRetraso(tiempoEspera));
    }

    private IEnumerator GenerarClienteConRetraso(float delay)
    {
        yield return new WaitForSeconds(delay);
        MostrarSiguienteCliente();

        if (indicePedidoActual < pedidos.Length && clienteSpriteRenderer != null)
        {
            var movement = clienteSpriteRenderer.GetComponent<NPCMovement>();
            if (movement == null) movement = clienteSpriteRenderer.GetComponentInParent<NPCMovement>();
            if (movement != null) movement.ReiniciarCiclo();
        }
    }

    // --- LÓGICA DE VICTORIA ---
    void EjecutarVictoria()
    {
        if (panelVictoria != null)
        {
            panelVictoria.SetActive(true);

            // 👇 AQUÍ ESTÁ LA CORRECCIÓN 👇
            if (PlayerProgress.Instance != null && textoPuntajeFinal != null)
            {
                // Usamos GetMonedas() en lugar de monedasActuales
                textoPuntajeFinal.text = "Ganancias Totales: $" + PlayerProgress.Instance.GetMonedas();
            }
        }
    }

    public Pedido GetPedidoActual() => pedidoActual;
    public ClienteData GetClienteActual() => clienteActual;
}