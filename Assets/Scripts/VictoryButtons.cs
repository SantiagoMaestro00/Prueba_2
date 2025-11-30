using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para cargar escenas

public class VictoryButtons : MonoBehaviour
{
    // Conecta este botón a "Reiniciar"
    public void ReiniciarJuego()
    {
        // Reinicia la escena actual
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        // Si usas un sistema de progreso estático (monedas), quizás quieras resetearlo:
        if (PlayerProgress.Instance != null)
        {
            // Opcional: Si quieres que empiece desde cero monedas
            // PlayerProgress.Instance.ResetearProgreso(); 
        }
    }

    // Conecta este botón a "Salir"
    public void SalirAlMenu()
    {
        // Opción A: Si tienes una escena de menú principal
        // SceneManager.LoadScene("MenuPrincipal"); 

        // Opción B: Cerrar el juego (solo funciona en el .exe final)
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}