using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Maneja el menú de pausa del juego y oculta la interfaz (HUD) al pausar.
/// </summary>
public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance { get; private set; }

    [Header("Referencias UI")]
    [Tooltip("Panel del menú de pausa (el que se muestra/oculta)")]
    public GameObject pauseMenuPanel;

    // --- NUEVA VARIABLE ---
    [Tooltip("Arrastra aquí el objeto padre que tiene las Monedas, Nivel y XP")]
    public GameObject hudPanel;
    // ---------------------

    [Header("Configuración")]
    [Tooltip("Tecla para abrir/cerrar el menú de pausa")]
    public KeyCode pauseKey = KeyCode.Escape;

    private bool isPaused = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("[PauseMenuManager] ¡pauseMenuPanel no está asignado!");
        }

        // Nos aseguramos de que el HUD empiece visible
        if (hudPanel != null)
        {
            hudPanel.SetActive(true);
        }

        ResumeGame();
    }

    void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        Debug.Log("[PauseMenuManager] ⏸️ Juego pausado");

        isPaused = true;

        // 1. Mostramos el menú de pausa
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        // 2. OCULTAMOS las monedas y nivel (HUD)
        if (hudPanel != null)
        {
            hudPanel.SetActive(false);
        }

        Time.timeScale = 0f;
        AudioListener.pause = true;
    }

    public void ResumeGame()
    {
        Debug.Log("[PauseMenuManager] ▶️ Juego reanudado");

        isPaused = false;

        // 1. Ocultamos el menú de pausa
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // 2. MOSTRAMOS de nuevo las monedas y nivel
        if (hudPanel != null)
        {
            hudPanel.SetActive(true);
        }

        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    public void SaveGame()
    {
        Debug.Log("[PauseMenuManager] 💾 Guardando partida...");

        if (PlayerProgress.Instance != null)
        {
            PlayerProgress.Instance.GuardarProgreso();
            Debug.Log("[PauseMenuManager] ✅ Partida guardada exitosamente");
        }
        else
        {
            Debug.LogError("[PauseMenuManager] ❌ No se pudo guardar: PlayerProgress.Instance es null");
        }
    }

    public void OpenSettings()
    {
        Debug.Log("[PauseMenuManager] ⚙️ Abriendo ajustes...");
        Debug.LogWarning("[PauseMenuManager] Menú de ajustes no implementado aún");
    }

    public void QuitGame()
    {
        Debug.Log("[PauseMenuManager] 🚪 Cerrando el juego...");

        if (PlayerProgress.Instance != null)
        {
            PlayerProgress.Instance.GuardarProgreso();
        }

        Time.timeScale = 1f;

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // MÉTODOS PÚBLICOS PARA BOTONES
    public void OnClickResumeButton() => ResumeGame();
    public void OnClickSaveButton() => SaveGame();
    public void OnClickSettingsButton() => OpenSettings();
    public void OnClickQuitButton() => QuitGame();

    public bool IsPaused() => isPaused;
}