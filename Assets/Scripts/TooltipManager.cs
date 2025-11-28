using UnityEngine;
using TMPro;
using System.Collections; // Necesario para las corrutinas (animación)

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    [Header("Referencias UI")]
    public GameObject tooltipPanel;
    public TMP_Text tituloText;
    public TMP_Text cuerpoText;
    public TMP_Text extraText;
    // NUEVA REFERENCIA: El controlador de transparencia
    public CanvasGroup canvasGroupPanel;

    [Header("Configuración")]
    public Vector3 offset = new Vector3(200, -50, 0);
    [Tooltip("Duración del fade-in en segundos")]
    public float fadeDuration = 0.5f; // Medio segundo para aparecer

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        // SINGLETON LOCAL
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        // Aseguramos que al inicio esté oculto y transparente
        OcultarTooltipInstantaneo();
    }

    private void Update()
    {
        if (tooltipPanel != null && tooltipPanel.activeSelf)
        {
            tooltipPanel.transform.position = Input.mousePosition + offset;
        }
    }

    public void MostrarTooltip(ComponenteData datos)
    {
        if (datos == null) return;

        // 1. Llenamos los textos
        if (tituloText != null) tituloText.text = datos.nombrePieza;
        if (cuerpoText != null) cuerpoText.text = datos.descripcionEducativa;
        if (extraText != null) extraText.text = datos.especificaciones;

        // 2. Activamos el objeto (pero será invisible porque el alpha es 0)
        if (tooltipPanel != null) tooltipPanel.SetActive(true);

        // 3. Iniciamos la animación de Fade In
        // Si ya había una animación corriendo, la detenemos para empezar la nueva
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(AnimarFadeIn());
    }

    public void OcultarTooltip()
    {
        // Para ocultar, lo hacemos instantáneo para que se sienta ágil al quitar el mouse
        OcultarTooltipInstantaneo();
    }

    private void OcultarTooltipInstantaneo()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        if (canvasGroupPanel != null) canvasGroupPanel.alpha = 0f; // Lo hacemos transparente

        if (tooltipPanel != null) tooltipPanel.SetActive(false); // Lo desactivamos
    }

    // --- LA CORRUTINA DE ANIMACIÓN ---
    private IEnumerator AnimarFadeIn()
    {
        float contador = 0f;
        if (canvasGroupPanel == null) yield break; // Seguridad

        canvasGroupPanel.alpha = 0f; // Empezamos totalmente transparente

        // Mientras no pase el tiempo definido...
        while (contador < fadeDuration)
        {
            contador += Time.deltaTime;
            // Calculamos el alpha basado en el tiempo que ha pasado (de 0 a 1)
            canvasGroupPanel.alpha = Mathf.Lerp(0f, 1f, contador / fadeDuration);
            // Esperamos al siguiente frame
            yield return null;
        }

        // Aseguramos que al final sea 100% visible
        canvasGroupPanel.alpha = 1f;
        fadeCoroutine = null;
    }
}