using UnityEngine;

[CreateAssetMenu(fileName = "NuevoComponente", menuName = "Componente Educativo")]
public class ComponenteData : ScriptableObject
{
    [Header("Información Visual")]
    public Sprite icono; // <-- Aquí va la foto de la pieza

    [Header("El Objeto del Juego")]
    public GameObject prefabDeLaPieza;

    [Header("Información Básica")]
    public string nombrePieza; // Ej: Memoria RAM

    [Header("Dato Didáctico")]
    [TextArea(3, 10)]
    public string descripcionEducativa; // Ej: "Sirve para..."

    [Header("Dato Técnico (Opcional)")]
    public string especificaciones; // Ej: "8GB DDR4"
}