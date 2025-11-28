using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

    private bool tallerCargado = false;
    private bool operacionEnCurso = false;
    public bool tareaComenzada = false;

    [Header("Referencias")]
    public GameObject cliente;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (cliente == null)
        {
            cliente = GameObject.Find("Cliente");
        }
    }

    public void CargarEscena(string nombreEscena)
    {
        Debug.Log($"[SceneController] Solicitud de cargar escena: {nombreEscena}");

        if (nombreEscena == "Taller")
        {
            EntrarTaller();
        }
        else if (nombreEscena == "Juego" || nombreEscena == "Main")
        {
            SalirTaller();
        }
        else
        {
            SceneManager.LoadScene(nombreEscena);
        }
    }

    public void EntrarTaller()
    {
        if (operacionEnCurso) return;
        StartCoroutine(MostrarTallerCoroutine());
    }

    public void SalirTaller()
    {
        if (operacionEnCurso) return;
        StartCoroutine(OcultarTallerCoroutine());
    }

    private IEnumerator MostrarTallerCoroutine()
    {
        operacionEnCurso = true;

        // Si la escena NO está cargada (o fue descargada), la cargamos de nuevo
        if (!tallerCargado)
        {
            var op = SceneManager.LoadSceneAsync("Taller", LoadSceneMode.Additive);
            while (!op.isDone) yield return null;
            tallerCargado = true;
        }

        ActivarRootObjects("Taller");
        DesactivarRootObjects("Juego");

        if (cliente != null)
        {
            var movement = cliente.GetComponent<NPCMovement>();
            if (movement != null)
                movement.OcultarVisualCliente();
        }

        operacionEnCurso = false;
    }

    private IEnumerator OcultarTallerCoroutine()
    {
        operacionEnCurso = true;

        // 1. Ocultamos visualmente el taller primero para que la transición sea rápida
        DesactivarRootObjects("Taller");
        ActivarRootObjects("Juego");

        if (cliente != null)
        {
            var movement = cliente.GetComponent<NPCMovement>();
            if (movement != null)
                movement.MostrarVisualCliente();
        }

        // 2. Si terminamos una tarea, procedemos a limpiar la escena
        if (tareaComenzada)
        {
            Debug.Log("[SceneController] Tarea completada. Limpiando escena Taller...");

            if (TareaValidator.Instance != null)
            {
                TareaValidator.Instance.ReiniciarContador();
            }

            // Esperamos los 2 segundos para el tema del icono
            yield return new WaitForSeconds(2f);

            // --- AQUÍ ESTÁ LA MAGIA DEL RESETEO ---
            // Descargamos la escena por completo. Esto borra todo lo que pusiste en el taller.
            var op = SceneManager.UnloadSceneAsync("Taller");
            while (!op.isDone) yield return null;

            // Marcamos como NO cargada, para que la próxima vez 'EntrarTaller' la cargue fresca
            tallerCargado = false;

            tareaComenzada = false;
            Debug.Log("[SceneController] Escena Taller descargada y reseteada.");
        }

        operacionEnCurso = false;
    }

    private void ActivarRootObjects(string sceneName)
    {
        // Verificamos si la escena sigue cargada antes de intentar activar cosas
        var scene = SceneManager.GetSceneByName(sceneName);
        if (scene.IsValid() && scene.isLoaded)
        {
            foreach (var go in scene.GetRootGameObjects())
            {
                go.SetActive(true);
            }
        }
    }

    private void DesactivarRootObjects(string sceneName)
    {
        var scene = SceneManager.GetSceneByName(sceneName);
        if (scene.IsValid() && scene.isLoaded)
        {
            foreach (var go in scene.GetRootGameObjects())
            {
                if (sceneName == "Juego" && go.name == "_Systems")
                    continue;
                go.SetActive(false);
            }
        }
    }

    public void ComenzarTarea()
    {
        Debug.Log("[SceneController] Tarea comenzada");
        tareaComenzada = true;

        if (TareaValidator.Instance != null)
        {
            TareaValidator.Instance.ReiniciarContador();
        }
    }
}