using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections; // Necesario para la animación

public class CabinetSlotUI : MonoBehaviour
{
    [Header("Configuración")]
    public string aceptaTipo = "RAM";

    [Header("Minijuego")]
    public bool requiereMinijuego = false;
    public string minijuegoId = "RAM";

    [Header("Visuales")]
    public GameObject prefabInstalado;
    public GameObject efectoExito;

    [Header("Feedback Visual")]
    public Color colorInvisible = new Color(1f, 1f, 1f, 0f);
    public Color colorResaltado = new Color(0.5f, 1f, 0.5f, 0.5f); // Verde
    public Color colorAlerta = new Color(1f, 0.9f, 0.2f, 0.5f);    // Amarillo

    [Range(0.5f, 5f)]
    public float velocidadParpadeo = 2.0f; // Qué tan rápido parpadea

    private Image miImagen;
    private bool ocupado = false;
    private GameObject visualInstaladoActual;

    // Variable para controlar la animación
    private Coroutine animacionActual;

    // --- EVENTOS ESTÁTICOS ---
    public static event Action<string> OnAlguienAgarroPieza;
    public static event Action OnAlguienSoltoPieza;

    public static void NotificarAgarre(string tipo) => OnAlguienAgarroPieza?.Invoke(tipo);
    public static void NotificarSoltar() => OnAlguienSoltoPieza?.Invoke();

    void Awake()
    {
        miImagen = GetComponent<Image>();
        if (miImagen != null)
        {
            miImagen.raycastTarget = true;
            SetColor(colorInvisible);
        }
    }

    void OnEnable()
    {
        OnAlguienAgarroPieza += EvaluarPiezaArrastrada;
        OnAlguienSoltoPieza += ApagarLuz;
    }

    void OnDisable()
    {
        OnAlguienAgarroPieza -= EvaluarPiezaArrastrada;
        OnAlguienSoltoPieza -= ApagarLuz;
    }

    void EvaluarPiezaArrastrada(string tipoPiezaArrastrada)
    {
        if (ocupado) return;

        // 1. COMPATIBLE EXACTO (VERDE)
        if (EsCompatible(tipoPiezaArrastrada))
        {
            IniciarAnimacion(colorResaltado);
        }
        // 2. TIPO CORRECTO / MODELO INCORRECTO (AMARILLO)
        else if (EsMismaCategoria(tipoPiezaArrastrada))
        {
            IniciarAnimacion(colorAlerta);
        }
    }

    void ApagarLuz()
    {
        if (!ocupado)
        {
            DetenerAnimacion();
            SetColor(colorInvisible);
        }
    }

    // 👇 LÓGICA DE ANIMACIÓN 👇
    void IniciarAnimacion(Color colorObjetivo)
    {
        // Si ya hay una animación corriendo, la detenemos para empezar la nueva
        DetenerAnimacion();
        animacionActual = StartCoroutine(RutinaParpadeo(colorObjetivo));
    }

    void DetenerAnimacion()
    {
        if (animacionActual != null)
        {
            StopCoroutine(animacionActual);
            animacionActual = null;
        }
    }

    IEnumerator RutinaParpadeo(Color baseColor)
    {
        while (true) // Bucle infinito hasta que se detenga la corrutina
        {
            // Mathf.PingPong crea un valor que sube y baja (como una pelota rebotando)
            // Oscila entre 0.2 (mínimo visible) y el Alpha original del color
            float alphaVariable = Mathf.PingPong(Time.time * velocidadParpadeo, baseColor.a - 0.2f) + 0.2f;

            if (miImagen != null)
            {
                miImagen.color = new Color(baseColor.r, baseColor.g, baseColor.b, alphaVariable);
            }

            yield return null; // Esperar al siguiente frame
        }
    }
    // 👆 ------------------ 👆

    private bool EsCompatible(string tipoPieza)
    {
        string tPieza = (tipoPieza ?? "").Trim();
        string tSlot = (aceptaTipo ?? "").Trim();
        return string.Equals(tPieza, tSlot, StringComparison.OrdinalIgnoreCase);
    }

    private bool EsMismaCategoria(string tipoPieza)
    {
        string tPieza = (tipoPieza ?? "").ToUpper();
        string tSlot = (aceptaTipo ?? "").ToUpper();

        if (tPieza.Contains("RAM") && tSlot.Contains("RAM")) return true;
        if (tPieza.Contains("CPU") && tSlot.Contains("CPU")) return true;
        if ((tPieza.Contains("HDD") || tPieza.Contains("SSD")) &&
            (tSlot.Contains("HDD") || tSlot.Contains("SSD"))) return true;

        return false;
    }

    private void SetColor(Color c)
    {
        if (miImagen != null) miImagen.color = c;
    }

    // --- INSTALACIÓN ---
    public bool TryHandleDrop(Arrastrable arr)
    {
        if (ocupado) return false;
        if (!EsCompatible(arr.tipoComponente)) return false;

        ApagarLuz(); // Detiene la animación e invisibiliza

        if (requiereMinijuego && MiniGameManager.Instance != null)
            MiniGameManager.Instance.StartMiniGame(minijuegoId, arr, this);
        else
            AcceptPlacementFromMinigame(arr);

        return true;
    }

    public void AcceptPlacementFromMinigame(Arrastrable arr)
    {
        if (ocupado) return;
        ocupado = true;
        ApagarLuz();

        if (arr != null) Destroy(arr.gameObject);

        if (prefabInstalado != null)
        {
            visualInstaladoActual = Instantiate(prefabInstalado, transform);
            visualInstaladoActual.transform.localPosition = Vector3.zero;
            ItemAutoFit autoFit = visualInstaladoActual.GetComponent<ItemAutoFit>();
            if (autoFit != null) autoFit.FitIntoSlot(GetComponent<RectTransform>());
        }

        if (efectoExito != null)
        {
            GameObject particulas = Instantiate(efectoExito, transform.position, Quaternion.identity);
            Destroy(particulas, 2f);
        }
    }

    public void QuitarComponente()
    {
        if (visualInstaladoActual != null) Destroy(visualInstaladoActual);
        ocupado = false;
        ApagarLuz();
    }
}