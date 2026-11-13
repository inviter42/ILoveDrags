using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ILoveDrags;

[BepInPlugin(Guid, PluginName, Version)]
// ReSharper disable once InconsistentNaming
public class ILoveDrags: BaseUnityPlugin
{
    public const string PluginName = "ILoveDrags";
    public const string Guid = "inviter42.ilovedrags";
    public const string Version = "1.0.0";

    internal static MonoBehaviour DhhRuntimeInstance;

    internal static ConfigEntry<bool> ConfigEntryPluginEnabled { get; private set; }

    private static Harmony Harmony;


    private void Awake() {
        if (Application.productName != "PlayHome") {
            Logger.LogWarning("Ash plugin is main game only, loading has been interrupted.");
            return;
        }

        InitializePlugin();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy() {
        if (Application.productName != "PlayHome")
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode) {
        SetDhhInstance();
    }

    private void InitializePlugin() {
        Harmony = new Harmony($"{Guid}.harmony");

        ConfigEntryPluginEnabled = Config.Bind(
            "Settings",
            "Enable/Disable plugin",
            true
        );

        Harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    /// <summary>
    /// Because DHH relies on IllusionPlugin assembly I don't have, have to do it like this
    /// </summary>
    private void SetDhhInstance() {
        DhhRuntimeInstance = GameObject.Find("DHH_Base")
            ?.GetComponents<MonoBehaviour>()
            .FirstOrDefault(mb => mb != null && mb.GetType().Name == "HighHeelRuntime");

        Logger.LogDebug($"dhh instance {DhhRuntimeInstance}");
    }
}
