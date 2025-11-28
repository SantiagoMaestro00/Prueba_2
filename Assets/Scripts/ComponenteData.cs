using UnityEngine;

[CreateAssetMenu(fileName = "NuevoComponente", menuName = "Componente Educativo")]
public class ComponenteData : ScriptableObject
{
    [Header("Información Básica")]
    public string nombrePieza;

    [Header("Dato Didáctico")]
    [TextArea(3, 10)]
    public string descripcionEducativa;

    [Header("Dato Técnico")]
    public string especificaciones;
}