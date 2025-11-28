using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerProgressUI : MonoBehaviour
{
    [Header("Referencias de Monedas")]
    public Text monedasText;

    [Header("Referencias de Nivel y XP")]
    public Text nivelText;
    public Image xpFillBar;
    public Text xpText;

    [Header("Formato de texto")]
    public string formatoMonedas = "Monedas: {0}";
    public string formatoNivel = "Nivel {0}";
    public string formatoXP = "{0} / {1} XP";

    [Header("Animación")]
    public bool animarCambios = true;
    public float duracionAnimacion = 0.5f;

    [Header("Efectos visuales opcionales")]
    public bool efectoParpadeo = true;

    private int monedasMostradas = 0;

    void Start()
    {
        if (PlayerProgress.Instance != null)
        {
            // Nos suscribimos a los eventos para saber cuándo actualizar
            PlayerProgress.Instance.OnMonedasCambiadas += ActualizarMonedas;
            PlayerProgress.Instance.OnExperienciaGanada += ActualizarXP;
            PlayerProgress.Instance.OnNivelCambiado += ActualizarNivel;

            // Actualizamos todo al iniciar
            ActualizarTodo();
        }
    }

    void OnDestroy()
    {
        if (PlayerProgress.Instance != null)
        {
            PlayerProgress.Instance.OnMonedasCambiadas -= ActualizarMonedas;
            PlayerProgress.Instance.OnExperienciaGanada -= ActualizarXP;
            PlayerProgress.Instance.OnNivelCambiado -= ActualizarNivel;
        }
    }

    public void ActualizarTodo()
    {
        if (PlayerProgress.Instance == null) return;

        // Usamos los Getters seguros del PlayerProgress
        monedasMostradas = PlayerProgress.Instance.GetMonedas();

        if (monedasText != null)
            monedasText.text = string.Format(formatoMonedas, monedasMostradas);

        ActualizarNivel(0, PlayerProgress.Instance.GetNivel());
        ActualizarXP(0); // Forzamos actualización de la barra
    }

    private void ActualizarMonedas(int nuevasMonedas)
    {
        if (monedasText == null) return;

        if (animarCambios && Application.isPlaying && nuevasMonedas != monedasMostradas)
        {
            StartCoroutine(AnimarMonedas(monedasMostradas, nuevasMonedas));
        }
        else
        {
            monedasMostradas = nuevasMonedas;
            monedasText.text = string.Format(formatoMonedas, nuevasMonedas);
        }

        if (efectoParpadeo && nuevasMonedas > monedasMostradas)
        {
            StartCoroutine(EfectoParpadeo(monedasText));
        }
    }

    private void ActualizarNivel(int nivelAnterior, int nivelNuevo)
    {
        if (nivelText == null) return;

        nivelText.text = string.Format(formatoNivel, nivelNuevo);

        if (nivelNuevo > nivelAnterior && nivelAnterior > 0)
        {
            if (efectoParpadeo) StartCoroutine(EfectoParpadeo(nivelText));
        }
    }

    // --- AQUÍ ESTABA EL ERROR ---
    // Hemos corregido esta función para usar los métodos correctos
    private void ActualizarXP(int xpGanada)
    {
        if (PlayerProgress.Instance == null) return;

        // Usamos las funciones GET del PlayerProgress en lugar de acceder a 'datos' directamente
        int xpActual = PlayerProgress.Instance.GetExperiencia();
        int xpRequerida = PlayerProgress.Instance.GetXPParaSiguienteNivel();
        float progreso = PlayerProgress.Instance.GetProgresoXP();

        // Actualizamos la barra visual
        if (xpFillBar != null)
        {
            xpFillBar.fillAmount = progreso;
        }

        // Actualizamos el texto de números (Ej: 50 / 100 XP)
        if (xpText != null)
        {
            xpText.text = string.Format(formatoXP, xpActual, xpRequerida);
        }
    }

    private IEnumerator AnimarMonedas(int desde, int hasta)
    {
        float tiempoTranscurrido = 0f;
        while (tiempoTranscurrido < duracionAnimacion)
        {
            tiempoTranscurrido += Time.deltaTime;
            float t = tiempoTranscurrido / duracionAnimacion;
            int valorActual = (int)Mathf.Lerp(desde, hasta, t);
            monedasText.text = string.Format(formatoMonedas, valorActual);
            yield return null;
        }
        monedasMostradas = hasta;
        monedasText.text = string.Format(formatoMonedas, hasta);
    }

    private IEnumerator EfectoParpadeo(Text texto)
    {
        Color colorOriginal = texto.color;
        Color colorResaltado = Color.yellow;

        for (int i = 0; i < 3; i++)
        {
            texto.color = colorResaltado;
            yield return new WaitForSeconds(0.1f);
            texto.color = colorOriginal;
            yield return new WaitForSeconds(0.1f);
        }
        texto.color = colorOriginal;
    }
}