using UnityEngine;
using System.Collections.Generic;

// --- ESTA ES LA CLASE QUE FALTABA ---
[System.Serializable]
public class RequisitoComponente
{
    [Tooltip("Tipo de componente requerido (debe coincidir con el componente arrastrable)")]
    public string tipoComponente;

    [Tooltip("Cantidad requerida de este componente")]
    public int cantidadRequerida = 1;
}
// ------------------------------------

[CreateAssetMenu(fileName = "NuevoPedido", menuName = "Pedido")]
public class Pedido : MonoBehaviour
{
    [Header("Mensajes del pedido")]
    [TextArea(2, 4)]
    public string[] mensajes;

    [Header("Componentes requeridos")]
    public List<RequisitoComponente> componentesRequeridos = new List<RequisitoComponente>();

    public string MensajeAleatorio()
    {
        if (mensajes != null && mensajes.Length > 0)
        {
            int idx = Random.Range(0, mensajes.Length);
            return mensajes[idx];
        }
        return "Sin mensaje";
    }

    public bool RequiereTipo(string tipo)
    {
        foreach (var req in componentesRequeridos)
        {
            if (string.Equals(req.tipoComponente, tipo, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    public int GetCantidadRequerida(string tipo)
    {
        foreach (var req in componentesRequeridos)
        {
            if (string.Equals(req.tipoComponente, tipo, System.StringComparison.OrdinalIgnoreCase))
                return req.cantidadRequerida;
        }
        return 0;
    }
}