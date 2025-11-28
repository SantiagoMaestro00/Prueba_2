using UnityEngine;
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

    [Header("Datos")]
    public ClienteData[] clientes;
    public Pedido[] pedidos;

    [Header("Referencias visibles")]
    public SpriteRenderer clienteSpriteRenderer;
    public NPCRequestPanel panel; // Conexión al script del panel

    [Header("Animator Override")]
    public AnimationClip baseIdleClip;
    public AnimationClip baseWalkClip;

    private ClienteData clienteActual;
    private Pedido pedidoActual;

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
        StartCoroutine(InitNextFrame());
    }

    IEnumerator InitNextFrame()
    {
        yield return null;
        MostrarClienteAleatorio();
    }

    public void MostrarClienteAleatorio()
    {
        if (clientes == null || clientes.Length == 0) return;
        if (pedidos == null || pedidos.Length == 0) return;

        // SELECCIÓN SIMPLE (SIN NIVELES)
        int clienteId = Random.Range(0, clientes.Length);
        int pedidoId = Random.Range(0, pedidos.Length);

        clienteActual = clientes[clienteId];
        pedidoActual = pedidos[pedidoId];

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
        Debug.Log($"[NPCManager] Cliente: {clienteActual.nombre} | Pedido: {pedidoActual.name}");
    }

    public void MostrarPedidoActual()
    {
        if (panel != null && pedidoActual != null)
            panel.ShowPanel(pedidoActual.MensajeAleatorio());
    }

    public void SolicitarSiguienteCliente(float tiempoEspera = 3f)
    {
        StartCoroutine(GenerarClienteConRetraso(tiempoEspera));
    }

    private IEnumerator GenerarClienteConRetraso(float delay)
    {
        yield return new WaitForSeconds(delay);
        MostrarClienteAleatorio();

        if (clienteSpriteRenderer != null)
        {
            var movement = clienteSpriteRenderer.GetComponent<NPCMovement>();
            if (movement == null) movement = clienteSpriteRenderer.GetComponentInParent<NPCMovement>();
            if (movement != null) movement.ReiniciarCiclo();
        }
    }

    public Pedido GetPedidoActual() => pedidoActual;
    public ClienteData GetClienteActual() => clienteActual;
}