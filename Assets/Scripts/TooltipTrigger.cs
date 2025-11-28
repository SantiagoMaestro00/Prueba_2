using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections; // Necesario para el cronómetro (Corrutinas)

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Datos")]
    public ComponenteData datosDeLaPieza;

    [Header("Configuración")]
    [Tooltip("Segundos que el mouse debe esperar encima para mostrar el tooltip")]
    public float tiempoDeEspera = 3f; // 3 segundos por defecto

    private Coroutine cronometroActual;

    // Cuando el mouse ENTRA
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Si ya había un cronómetro corriendo (por si acaso), lo detenemos
        DetenerCronometro();

        // Iniciamos un nuevo cronómetro para esperar los 3 segundos
        cronometroActual = StartCoroutine(EsperarYMostrar());
    }

    // Cuando el mouse SALE
    public void OnPointerExit(PointerEventData eventData)
    {
        // IMPORTANTE: Si el mouse sale antes de tiempo, cancelamos el cronómetro
        DetenerCronometro();

        // Y ocultamos el tooltip si es que llegó a mostrarse
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.OcultarTooltip();
        }
    }

    private void DetenerCronometro()
    {
        if (cronometroActual != null)
        {
            StopCoroutine(cronometroActual);
            cronometroActual = null;
        }
    }

    // Esta es la función con temporizador
    private IEnumerator EsperarYMostrar()
    {
        // Esperamos los segundos configurados
        yield return new WaitForSeconds(tiempoDeEspera);

        // Si llegamos aquí, significa que el mouse no salió durante la espera.
        // ¡Mostramos el mensaje!
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.MostrarTooltip(datosDeLaPieza);
        }

        // Reseteamos la variable del cronómetro
        cronometroActual = null;
    }
}