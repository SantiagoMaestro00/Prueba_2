using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TareaValidator : MonoBehaviour
{
    public static TareaValidator Instance { get; private set; }

    [Header("Referencias")]
    public NPCManager npcManager;
    public GameObject panelTareaCompletada;
    public UnityEngine.UI.Text textoCompletado;

    [Header("Configuración")]
    public float tiempoAntesDeRegresar = 2.5f;
    public int monedasPorTarea = 100;
    public int xpPorTarea = 50;
    public AudioClip sonidoTareaCompletada;

    private Dictionary<string, int> componentesInstalados = new Dictionary<string, int>();
    private bool recompensasPendientes = false;
    private int monedasPendientes = 0;
    private int xpPendiente = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    void Start()
    {
        if (npcManager == null) npcManager = NPCManager.Instance;
        if (panelTareaCompletada != null) panelTareaCompletada.SetActive(false);
        VerificarRecompensasPendientes();
    }

    void OnEnable() => PlacementEvents.OnComponentPlaced += OnComponenteColocado;
    void OnDisable() => PlacementEvents.OnComponentPlaced -= OnComponenteColocado;

    private void OnComponenteColocado(string tipo, Arrastrable item, RectTransform slot)
    {
        RegistrarComponente(tipo);
    }

    public void RegistrarComponente(string tipo)
    {
        if (string.IsNullOrEmpty(tipo)) return;
        if (componentesInstalados.ContainsKey(tipo)) componentesInstalados[tipo]++;
        else componentesInstalados[tipo] = 1;
        ValidarTarea();
    }

    public void ReiniciarContador() => componentesInstalados.Clear();

    private void ValidarTarea()
    {
        if (npcManager == null) return;
        Pedido pedido = npcManager.GetPedidoActual();
        if (pedido == null) return;

        bool tareaCompleta = true;
        foreach (var req in pedido.componentesRequeridos)
        {
            int instalados = componentesInstalados.ContainsKey(req.tipoComponente) ? componentesInstalados[req.tipoComponente] : 0;
            if (instalados < req.cantidadRequerida) { tareaCompleta = false; break; }
        }

        if (tareaCompleta) CompletarTarea();
    }

    private void CompletarTarea()
    {
        if (sonidoTareaCompletada != null) AudioSource.PlayClipAtPoint(sonidoTareaCompletada, Camera.main.transform.position);

        recompensasPendientes = true;
        monedasPendientes = monedasPorTarea;
        xpPendiente = xpPorTarea;

        if (PlayerProgress.Instance != null) PlayerProgress.Instance.RegistrarTareaCompletada();

        if (panelTareaCompletada != null)
        {
            panelTareaCompletada.SetActive(true);
            if (textoCompletado != null) textoCompletado.text = "¡Tarea Completada!\nRegresando...";
        }
        StartCoroutine(RegresarASala());
    }

    private IEnumerator RegresarASala()
    {
        yield return new WaitForSeconds(tiempoAntesDeRegresar);
        if (SceneController.Instance != null) SceneController.Instance.SalirTaller();
        yield return new WaitForSeconds(0.5f);
        DarRecompensasPendientes();
    }

    private void DarRecompensasPendientes()
    {
        if (!recompensasPendientes) return;

        if (PlayerProgress.Instance != null)
        {
            PlayerProgress.Instance.AñadirMonedas(monedasPendientes);
            PlayerProgress.Instance.AñadirExperiencia(xpPendiente);
        }

        GameObject clienteObj = GameObject.Find("Cliente");
        if (clienteObj != null)
        {
            var movement = clienteObj.GetComponent<NPCMovement>();
            if (movement != null) movement.SalirDelLocal();
        }
        recompensasPendientes = false;
    }

    private void VerificarRecompensasPendientes()
    {
        if (recompensasPendientes) StartCoroutine(DarRecompensasConDelay());
    }

    private IEnumerator DarRecompensasConDelay()
    {
        yield return new WaitForSeconds(1f);
        DarRecompensasPendientes();
    }
}