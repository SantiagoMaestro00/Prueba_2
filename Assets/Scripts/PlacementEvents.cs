using System;
using UnityEngine;

public static class PlacementEvents
{
    public static event Action<string, Arrastrable, RectTransform> OnComponentPlaced;

    public static void ComponentPlaced(string tipoComponente, Arrastrable item, RectTransform slot)
    {
        // Avisar a TareaValidator
        OnComponentPlaced?.Invoke(tipoComponente, item, slot);

        // Registrar estadística
        if (PlayerProgress.Instance != null)
        {
            PlayerProgress.Instance.RegistrarComponenteInstalado();
        }
    }
}