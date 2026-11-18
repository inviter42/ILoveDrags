using System.Collections;
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
public class ILoveDragsPlugin: BaseUnityPlugin
{
    public const string PluginName = "ILoveDrags";
    public const string Guid = "inviter42.ilovedrags";
    public const string Version = "1.0.2";

    internal static ConfigEntry<bool> ConfigEntryPluginEnabled { get; private set; }

    private static MonoBehaviour DhhRuntimeInstance;
    private static MonoBehaviour PhiblRuntimeInstance;
    private static Harmony Harmony;

    private static FieldInfo DhhMenuOn;
    private static MethodInfo DhhMethodInfo;

    private static FieldInfo PhiblMainWindowField;
    private static FieldInfo PhiblRectField;
    private static FieldInfo PhiblScaleField;
    private static FieldInfo PhiblCustomScaleField;


    public static bool IsOverDhhUi() {
        return DhhRuntimeInstance
               && ReflectionUtils.GetFieldValue<bool>(DhhRuntimeInstance, DhhMenuOn)
               && ReflectionUtils.GetMethodInvocationResult(DhhRuntimeInstance, DhhMethodInfo, 0f) is true;
    }
    public static bool IsOverPhiblUi() {
        if (PhiblRuntimeInstance == null)
            return false;

        if (!ReflectionUtils.GetFieldValue<bool>(PhiblRuntimeInstance, PhiblMainWindowField))
            return false;

        if (PhiblRectField == null || PhiblScaleField == null || PhiblCustomScaleField == null)
            return false;

        var rect = (Rect)PhiblRectField.GetValue(null);
        var scale = (Vector3)PhiblScaleField.GetValue(null);
        var custom = (float)PhiblCustomScaleField.GetValue(null);

        Vector2 screen = Input.mousePosition;
        var gui = new Vector2(
            screen.x / (scale.x * custom),
            (Screen.height - screen.y) / (scale.y * custom)
        );

        return rect.Contains(gui);
    }


    private void Awake() {
        if (Application.productName != "PlayHome") {
            Logger.LogWarning("ILoveDrags plugin is main game only, loading has been interrupted.");
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
        SetReflectionInfo();
        StartCoroutine(SetInstancesRoutine());
    }

    private IEnumerator SetInstancesRoutine() {
        yield return null;
        if (!PhiblRuntimeInstance) SetPhiblInstance();
        if (!DhhRuntimeInstance) SetDhhInstance();
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

    private void SetDhhInstance() {
        DhhRuntimeInstance = GameObject.Find("DHH_Base")
            ?.GetComponents<MonoBehaviour>()
            .FirstOrDefault(mb => mb && mb.GetType().Name == "HighHeelRuntime");

        Logger.LogDebug($"DHH instance {DhhRuntimeInstance}");
    }

    private void SetPhiblInstance() {
        PhiblRuntimeInstance = GameObject.Find("PHIBL")
            ?.GetComponents<MonoBehaviour>()
            .FirstOrDefault(mb => mb && mb.GetType().Name == "PHIBL");

        Logger.LogDebug($"PHIBL instance {PhiblRuntimeInstance}");
    }

    private void SetReflectionInfo() {
        var phibl = ReflectionUtils.FindType("PHIBL", "PHIBL");
        if (phibl == null) {
            Logger.LogError($"Unable to find PHIBL type in PHIBL assembly");
        } else {
            PhiblMainWindowField = phibl.GetField("mainWindow", BindingFlags.Instance | BindingFlags.NonPublic);
            if (PhiblMainWindowField == null) Logger.LogError($"Unable to find field 'mainWindow' in PHIBL assembly");
        }

        var utils = ReflectionUtils.FindType("PHIBL", "UIUtils");
        if (utils == null) {
            Logger.LogError($"Unable to find UIUtils type in PHIBL assembly");
        } else {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            PhiblRectField = utils.GetField("windowRect", flags);
            if (PhiblRectField == null) Logger.LogError($"Unable to find 'windowRect' in PHIBL assembly");

            PhiblScaleField = utils.GetField("scale", flags);
            if (PhiblScaleField == null) Logger.LogError($"Unable to find 'scale' in PHIBL assembly");

            PhiblCustomScaleField = utils.GetField("customscale", flags);
            if (PhiblCustomScaleField == null) Logger.LogError($"Unable to find 'customscale' in PHIBL assembly");
        }

        var highHeelRuntime = ReflectionUtils.FindType("ProjectHighHeel", "HighHeelRuntime");
        if (highHeelRuntime == null) {
            Logger.LogError($"Unable to find HighHeelRuntime type in ProjectHighHeel assembly");
        } else {
            DhhMenuOn = highHeelRuntime.GetField("MenuOn", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
            if (DhhMenuOn == null) Logger.LogError($"Unable to find 'MenuOn' in ProjectHighHeel assembly");

            DhhMethodInfo = highHeelRuntime.GetMethod("withinUI", BindingFlags.Instance | BindingFlags.NonPublic);
            if (DhhMethodInfo == null) Logger.LogError($"Unable to find 'withinUI' in ProjectHighHeel assembly");
        }
    }
}
