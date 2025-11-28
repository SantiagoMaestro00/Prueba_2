using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class ProgresoTareaUI : MonoBehaviour
{
    [Header("Referencias")]
    public Text textoProgreso;
    public GameObject panelProgreso;

    private NPCManager npcManager;
    private TareaValidator validator;

    void Start()
    {
        npcManager = NPCManager.Instance;
        validator = TareaValidator.Instance;

        if (panelProgreso != null)
            panelProgreso.SetActive(false);
    }

    void OnEnable()
    {
        PlacementEvents.OnComponentPlaced += ActualizarProgreso;
    }

    void OnDisable()
    {
        PlacementEvents.OnComponentPlaced -= ActualizarProgreso;
    }

    // Mostrar cuando se entra al taller
    public void MostrarProgreso()
    {
        if (panelProgreso != null)
            panelProgreso.SetActive(true);

        ActualizarProgreso("", null, null);
    }

    // Ocultar cuando se sale del taller
    public void OcultarProgreso()
    {
        if (panelProgreso != null)
            panelProgreso.SetActive(false);
    }

    private void ActualizarProgreso(string tipo, Arrastrable item, RectTransform slot)
    {
        if (textoProgreso == null || npcManager == null || validator == null)
            return;

        Pedido pedido = npcManager.GetPedidoActual();
        if (pedido == null) return;

        var sb = new StringBuilder();
        sb.AppendLine("PROGRESO DE LA TAREA:");
        sb.AppendLine();

        foreach (var req in pedido.componentesRequeridos)
        {
            int instalado = 0; // Necesitarías exponer esto desde TareaValidator
            sb.AppendLine($"{req.tipoComponente}: {instalado}/{req.cantidadRequerida}");
        }

        textoProgreso.text = sb.ToString();
    }
}