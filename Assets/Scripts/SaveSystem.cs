using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

// DEFINICIÓN DE DATOS
[System.Serializable]
public class GameData
{
    public int monedas = 0;
    public int nivelJugador = 1;
    public int experiencia = 0;
    public int experienciaParaSiguienteNivel = 100;
    public int tareasCompletadas = 0;
    public int clientesAtendidos = 0;
    public int componentesInstalados = 0;
    public string ultimaVezJugado;
    public List<string> componentesDesbloqueados = new List<string>();

    public GameData()
    {
        monedas = 0;
        nivelJugador = 1;
        experiencia = 0;
        experienciaParaSiguienteNivel = 100;
        ActualizarTimestamp();
    }

    public void ActualizarTimestamp()
    {
        ultimaVezJugado = DateTime.Now.ToString();
    }

    public bool AñadirExperiencia(int cantidad)
    {
        experiencia += cantidad;
        if (experiencia >= experienciaParaSiguienteNivel)
        {
            experiencia -= experienciaParaSiguienteNivel;
            nivelJugador++;
            experienciaParaSiguienteNivel = Mathf.RoundToInt(experienciaParaSiguienteNivel * 1.5f);
            return true;
        }
        return false;
    }

    public void DesbloquearComponente(string componente)
    {
        if (!componentesDesbloqueados.Contains(componente))
            componentesDesbloqueados.Add(componente);
    }
}

// SISTEMA DE GUARDADO
public static class SaveSystem
{
    private const string SAVE_KEY = "GameDataSave";
    private static readonly string SAVE_PATH = Application.persistentDataPath + "/savegame.json";

    public enum SaveMethod { PlayerPrefs, JSON }
    public static SaveMethod MetodoDeGuardado = SaveMethod.PlayerPrefs;

    public static void GuardarDatos(GameData datos)
    {
        if (datos == null) return;
        datos.ActualizarTimestamp();
        string json = JsonUtility.ToJson(datos, true);

        try
        {
            if (MetodoDeGuardado == SaveMethod.PlayerPrefs)
            {
                PlayerPrefs.SetString(SAVE_KEY, json);
                PlayerPrefs.Save();
            }
            else
            {
                File.WriteAllText(SAVE_PATH, json);
            }
        }
        catch (Exception e) { Debug.LogError($"Error guardando: {e.Message}"); }
    }

    public static GameData CargarDatos()
    {
        try
        {
            string json = "";
            if (MetodoDeGuardado == SaveMethod.PlayerPrefs)
            {
                if (PlayerPrefs.HasKey(SAVE_KEY)) json = PlayerPrefs.GetString(SAVE_KEY);
                else return new GameData();
            }
            else
            {
                if (File.Exists(SAVE_PATH)) json = File.ReadAllText(SAVE_PATH);
                else return new GameData();
            }

            var datos = JsonUtility.FromJson<GameData>(json);
            return datos != null ? datos : new GameData();
        }
        catch { return new GameData(); }
    }

    public static void BorrarDatos()
    {
        if (MetodoDeGuardado == SaveMethod.PlayerPrefs)
        {
            if (PlayerPrefs.HasKey(SAVE_KEY)) PlayerPrefs.DeleteKey(SAVE_KEY);
        }
        else
        {
            if (File.Exists(SAVE_PATH)) File.Delete(SAVE_PATH);
        }
    }
}