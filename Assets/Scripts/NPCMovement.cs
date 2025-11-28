using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(SpriteRenderer))]
public class NPCMovement : MonoBehaviour
{
    [Header("Configuracion de Movimiento")]
    public float velocidad = 2f;

    [Header("Posiciones")]
    public Vector3 posicionInicio = new Vector3(-10f, -2f, 0f);
    public Vector3 posicionCentral = new Vector3(-3f, -2f, 0f);
    public Vector3 posicionSalida = new Vector3(-10f, -2f, 0f);

    [Header("Mensajes")]
    public GameObject panelDialogo;
    public UnityEngine.UI.Text textoDialogo;
    public float tiempoMensajeDespedida = 2f;

    [Header("Comportamiento")]
    public bool entrarAutomaticamente = true;

    public event Action OnLlegadaAlMostrador;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool estaMoviendose = false;
    private bool saliendoDelLocal = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        if (panelDialogo != null) panelDialogo.SetActive(false);

        // Solo entra automáticamente la primera vez que arranca el juego
        if (entrarAutomaticamente) StartCoroutine(EntrarConDelay(0.5f));
    }

    // --- FUNCIÓN PARA REINICIAR EL CICLO ---
    public void ReiniciarCiclo()
    {
        Debug.Log("[NPCMovement] 🔄 Reiniciando ciclo para el siguiente cliente.");

        // 1. Resetear variables
        estaMoviendose = false;
        saliendoDelLocal = false;

        // 2. Resetear Interacción (Importante para que vuelva a salir el icono)
        var interaccion = GetComponent<ClienteInteraccion>();
        if (interaccion != null) interaccion.ResetearInteraccion();

        // 3. Mover al inicio y hacer visible
        transform.position = posicionInicio;
        MostrarVisualCliente();

        // 4. Entrar de nuevo
        StartCoroutine(EntrarConDelay(0.1f));
    }
    // ---------------------------------------

    private IEnumerator EntrarConDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        EntrarAlLocal();
    }

    public void EntrarAlLocal()
    {
        transform.position = posicionInicio;

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = false;
            spriteRenderer.enabled = true;
        }
        if (animator != null) animator.SetBool("isWalking", true);

        StartCoroutine(MoverHacia(posicionCentral, () =>
        {
            if (animator != null) animator.SetBool("isWalking", false);
            estaMoviendose = false;
            OnLlegadaAlMostrador?.Invoke();
        }));
    }

    public void SalirDelLocal(string mensajeDespedida = "¡Gracias!")
    {
        if (saliendoDelLocal) return;
        saliendoDelLocal = true;
        StartCoroutine(SecuenciaDespedida(mensajeDespedida));
    }

    private IEnumerator SecuenciaDespedida(string mensaje)
    {
        MostrarMensaje(mensaje);
        yield return new WaitForSeconds(tiempoMensajeDespedida);
        OcultarMensaje();

        if (spriteRenderer != null) spriteRenderer.flipX = true;
        if (animator != null) animator.SetBool("isWalking", true);

        yield return StartCoroutine(MoverHacia(posicionSalida, () =>
        {
            Debug.Log("[NPCMovement] Cliente salió. Ocultando y solicitando siguiente.");

            // NO DESTRUIMOS EL OBJETO. Lo ocultamos.
            OcultarVisualCliente();

            // Avisamos al Manager para que traiga al siguiente en 3 segundos
            if (NPCManager.Instance != null)
            {
                NPCManager.Instance.SolicitarSiguienteCliente(3f);
            }
        }));
    }

    private IEnumerator MoverHacia(Vector3 objetivo, Action alLlegar = null)
    {
        estaMoviendose = true;
        while (Vector3.Distance(transform.position, objetivo) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, objetivo, velocidad * Time.deltaTime);
            yield return null;
        }
        transform.position = objetivo;
        estaMoviendose = false;
        alLlegar?.Invoke();
    }

    public void MostrarMensaje(string mensaje)
    {
        if (panelDialogo != null)
        {
            panelDialogo.SetActive(true);
            if (textoDialogo != null) textoDialogo.text = mensaje;
        }
    }

    public void OcultarMensaje()
    {
        if (panelDialogo != null) panelDialogo.SetActive(false);
    }

    public void OcultarVisualCliente()
    {
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        OcultarMensaje();
    }

    public void MostrarVisualCliente()
    {
        if (spriteRenderer != null) spriteRenderer.enabled = true;
    }
}