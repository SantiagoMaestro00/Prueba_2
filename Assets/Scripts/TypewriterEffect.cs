using UnityEngine;
using TMPro;
using System.Collections;

public class TypewriterEffect : MonoBehaviour
{
    [Header("Configuración")]
    public float typeSpeed = 0.05f;

    private TMP_Text textComponent;
    private string fullText;
    private Coroutine currentCoroutine;

    // Variable interna privada
    private bool isTypingFlag = false;

    void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    // --- CORRECCIÓN 1: IsTyping ahora es una función ---
    // Esto arregla el error CS1955
    public bool IsTyping()
    {
        return isTypingFlag;
    }

    // --- CORRECCIÓN 2: SkipTypewriter acepta un argumento ---
    // Esto arregla el error CS1501. El parámetro 'textoOpcional' se ignora pero hace feliz al compilador.
    public void SkipTypewriter(string textoOpcional = "")
    {
        if (isTypingFlag)
        {
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);

            textComponent.text = fullText; // Mostramos el texto completo
            isTypingFlag = false;
        }
    }

    // --- FUNCIONES GENERALES ---

    public void Escribir(string textoNuevo)
    {
        ShowText(textoNuevo);
    }

    public void ShowText(string text)
    {
        fullText = text;

        if (textComponent == null)
            textComponent = GetComponent<TMP_Text>();

        textComponent.text = "";

        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(TypeTextCoroutine());
    }

    IEnumerator TypeTextCoroutine()
    {
        isTypingFlag = true;

        foreach (char c in fullText)
        {
            textComponent.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTypingFlag = false;
    }
}