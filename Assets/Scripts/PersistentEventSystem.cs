using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Hace que el EventSystem persista entre escenas.
/// </summary>
[RequireComponent(typeof(EventSystem))]
public class PersistentEventSystem : MonoBehaviour
{
    private static PersistentEventSystem instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.Log("[PersistentEventSystem] Ya existe un EventSystem, destruyendo este");
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("[PersistentEventSystem] ✅ EventSystem persistente creado");
    }

    void Update()
    {
        // Buscar y desactivar otros EventSystems
        var allEventSystems = FindObjectsOfType<EventSystem>();

        if (allEventSystems.Length > 1)
        {
            foreach (var es in allEventSystems)
            {
                if (es.gameObject != this.gameObject)
                {
                    Debug.LogWarning($"[PersistentEventSystem] Encontrado EventSystem duplicado: {es.gameObject.name}, destruyéndolo");
                    Destroy(es.gameObject);
                }
            }
        }
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}