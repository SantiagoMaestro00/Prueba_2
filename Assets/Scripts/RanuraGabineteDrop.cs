using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Este script permite que la ranura acepte el componente correcto arrastrado
public class RanuraGabineteDrop : MonoBehaviour, IDropHandler
{
    [Header("Tipo de componente que acepta esta ranura")]
    public string tipoComponente;
    [Header("Imagen visual de la ranura")]
    public Image imagenRanura;
    [Header("¿Está ocupada la ranura?")]
    public bool ocupado = false;

    public void OnDrop(PointerEventData eventData)
    {
        if (!ocupado)
        {
            Arrastrable arrastrable = eventData.pointerDrag.GetComponent<Arrastrable>();
            if (arrastrable != null && arrastrable.tipoComponente == tipoComponente)
            {
                // Cambia la imagen de la ranura por la del componente arrastrado
                imagenRanura.sprite = arrastrable.GetComponent<Image>().sprite;
                ocupado = true;
                Destroy(arrastrable.gameObject); // Opcional: elimina el objeto arrastrado
            }
            // Si el tipo no coincide, no hace nada
        }
    }
}