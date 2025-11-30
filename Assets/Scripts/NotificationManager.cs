using UnityEngine;
using UnityEngine.UI; // Necesario para el componente Text
using System.Collections;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;

    [Header("Arrastra aquí los objetos de la UI")]
    public GameObject panelAlerta;
    public Text textoMensaje; // Si usas TextMeshPro, cambia 'Text' por 'TMP_Text'

    void Awake()
    {
        // Patrón Singleton: Asegura que solo haya un Manager y sea accesible desde todos lados
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void MostrarError(string mensaje)
    {
        // 1. Ponemos el texto nuevo
        if (textoMensaje != null)
            textoMensaje.text = mensaje;

        // 2. Encendemos el panel
        if (panelAlerta != null)
            panelAlerta.SetActive(true);

        // 3. Reiniciamos el temporizador para ocultarlo
        StopAllCoroutines();
        StartCoroutine(OcultarDespuesDeTiempo(3f));
    }

    IEnumerator OcultarDespuesDeTiempo(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        if (panelAlerta != null)
            panelAlerta.SetActive(false);
    }
}