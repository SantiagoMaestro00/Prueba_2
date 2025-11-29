using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public Image imagenDelIcono;
    private ComponenteData misDatos;

    public void Configurar(ComponenteData datos)
    {
        misDatos = datos;
        imagenDelIcono.sprite = datos.icono;
        imagenDelIcono.preserveAspect = true;
    }

    public void AlHacerClic()
    {
        // CORREGIDO: Ahora solo enviamos 'misDatos'.
        // Borramos el segundo argumento que causaba el error.
        InventarioManager.Instance.SeleccionarItem(misDatos);
    }
}