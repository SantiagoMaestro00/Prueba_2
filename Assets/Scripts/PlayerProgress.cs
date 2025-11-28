using UnityEngine;
using System;

public class PlayerProgress : MonoBehaviour
{
    public static PlayerProgress Instance { get; private set; }

    [Header("Datos del jugador")]
    public GameData datos;

    [Header("Auto-guardado")]
    public float intervaloAutoGuardado = 60f;
    private float tiempoDesdeUltimoGuardado = 0f;

    public event Action<int> OnMonedasCambiadas;
    public event Action<int, int> OnNivelCambiado;
    public event Action<int> OnExperienciaGanada;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CargarProgreso();
    }

    void Update()
    {
        if (intervaloAutoGuardado > 0)
        {
            tiempoDesdeUltimoGuardado += Time.deltaTime;
            if (tiempoDesdeUltimoGuardado >= intervaloAutoGuardado)
            {
                GuardarProgreso();
                tiempoDesdeUltimoGuardado = 0f;
            }
        }
    }

    void OnApplicationQuit() => GuardarProgreso();
    void OnApplicationPause(bool pauseStatus) { if (pauseStatus) GuardarProgreso(); }

    public void CargarProgreso()
    {
        datos = SaveSystem.CargarDatos();
        if (datos == null) datos = new GameData();
    }

    public void GuardarProgreso()
    {
        if (datos != null) SaveSystem.GuardarDatos(datos);
    }

    public void AñadirMonedas(int cantidad)
    {
        if (cantidad <= 0) return;
        datos.monedas += cantidad;
        OnMonedasCambiadas?.Invoke(datos.monedas);
        GuardarProgreso();
    }

    public void AñadirExperiencia(int cantidad)
    {
        if (cantidad <= 0) return;
        int nivelAnterior = datos.nivelJugador;
        bool subioNivel = datos.AñadirExperiencia(cantidad);
        OnExperienciaGanada?.Invoke(cantidad);

        if (subioNivel) OnNivelCambiado?.Invoke(nivelAnterior, datos.nivelJugador);
        GuardarProgreso();
    }

    public void RegistrarTareaCompletada()
    {
        datos.tareasCompletadas++;
        datos.clientesAtendidos++;
        GuardarProgreso();
    }

    // ESTA ES LA QUE FALTABA Y DABA ERROR
    public void RegistrarComponenteInstalado()
    {
        datos.componentesInstalados++;
        GuardarProgreso();
    }

    public void ResetearProgreso()
    {
        SaveSystem.BorrarDatos();
        datos = new GameData();
        GuardarProgreso();
        OnMonedasCambiadas?.Invoke(datos.monedas);
        OnNivelCambiado?.Invoke(0, datos.nivelJugador);
        OnExperienciaGanada?.Invoke(0);
    }

    public int GetMonedas() => datos.monedas;
    public int GetNivel() => datos.nivelJugador;
    public int GetExperiencia() => datos.experiencia;
    public int GetXPParaSiguienteNivel() => datos.experienciaParaSiguienteNivel;
    public float GetProgresoXP() => (float)datos.experiencia / datos.experienciaParaSiguienteNivel;
}