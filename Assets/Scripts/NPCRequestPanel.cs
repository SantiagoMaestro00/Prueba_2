using UnityEngine;
using TMPro;

public class NPCRequestPanel : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject panel;
    public TMP_Text textBox;

    [Header("Efecto Typewriter")]
    public TypewriterEffect typewriter;
    public bool useTypewriter = true;

    private string mensajeActual = "";
    private string ultimoMensaje = "";

    public void ShowPanel(string mensaje)
    {
        if (SceneController.Instance != null && SceneController.Instance.tareaComenzada)
        {
            panel.SetActive(false);
        }
        else
        {
            panel.SetActive(true);
            mensajeActual = mensaje;
            ultimoMensaje = mensaje;

            if (useTypewriter && typewriter != null)
                typewriter.ShowText(mensaje);
            else
                textBox.text = mensaje;
        }
    }

    public void HidePanel()
    {
        if (typewriter != null && typewriter.IsTyping())
            typewriter.SkipTypewriter(mensajeActual);
        panel.SetActive(false);
    }

    public void OnClickComenzar()
    {
        if (typewriter != null && typewriter.IsTyping())
        {
            typewriter.SkipTypewriter(mensajeActual);
            return;
        }

        if (SceneController.Instance != null)
        {
            SceneController.Instance.ComenzarTarea();
            SceneController.Instance.EntrarTaller();
        }
        HidePanel();
    }

    public void OnPanelClick()
    {
        if (typewriter != null && typewriter.IsTyping())
            typewriter.SkipTypewriter(mensajeActual);
    }

    public void ResetPanel()
    {
        if (SceneController.Instance != null && !SceneController.Instance.tareaComenzada)
            ShowPanel(ultimoMensaje);
        else
            HidePanel();
    }
}