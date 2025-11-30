using UnityEngine;
using System.Collections;

public class PopUpEffect : MonoBehaviour
{
    [Header("Configuración")]
    public float duracion = 0.4f;      // Qué tan rápido aparece
    public float tamañoFinal = 1f;     // Tamaño normal (generalmente 1)
    public AnimationCurve curvaRebote = new AnimationCurve(
        new Keyframe(0, 0),
        new Keyframe(0.7f, 1.1f), // Se pasa un poquito (Rebote)
        new Keyframe(1, 1)        // Vuelve a su tamaño normal
    );

    private void OnEnable()
    {
        // Cada vez que el objeto se activa (SetActive true), ejecutamos la animación
        transform.localScale = Vector3.zero; // Empezamos invisibles (tamaño 0)
        StartCoroutine(AnimarApertura());
    }

    IEnumerator AnimarApertura()
    {
        float tiempo = 0;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float porcentaje = tiempo / duracion;

            // Calculamos el tamaño usando la curva para dar el efecto rebote
            float escala = curvaRebote.Evaluate(porcentaje) * tamañoFinal;
            transform.localScale = new Vector3(escala, escala, 1);

            yield return null;
        }

        // Aseguramos que quede en el tamaño exacto al final
        transform.localScale = new Vector3(tamañoFinal, tamañoFinal, 1);
    }
}