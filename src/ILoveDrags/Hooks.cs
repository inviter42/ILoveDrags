using System;
using System.Reflection;
using HarmonyLib;

namespace ILoveDrags;

[HarmonyPatch]
internal class Hooks
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(IllusionCamera), "LateUpdate")]
    // ReSharper disable once InconsistentNaming
    internal static bool LateUpdatePrefix(IllusionCamera __instance) {
        if (!ILoveDrags.ConfigEntryPluginEnabled.Value)
            return true;

        return !IsDhhVisible();
    }

    private static bool IsDhhVisible() {
        return ILoveDrags.DhhRuntimeInstance
               && GetFieldValue<bool>(ILoveDrags.DhhRuntimeInstance, "MenuOn")
               && GetMethodInvocationResult(ILoveDrags.DhhRuntimeInstance, "withinUI", new [] { typeof(float) }, 0f) is true;
    }

    private static FieldInfo DhhFieldInfo;
    private static MethodInfo DhhMethodInfo;

    private static T GetFieldValue<T>(object obj, string name) {
        if (obj == null)
            return default;

        DhhFieldInfo ??= obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);

        if (DhhFieldInfo == null)
            return default;

        return (T)DhhFieldInfo.GetValue(obj);
    }

    private static object GetMethodInvocationResult(object obj, string name, Type[] parameterTypes, params object[] args) {
        if (obj == null)
            return null;

        DhhMethodInfo ??= obj.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic, null, parameterTypes, null);

        return DhhMethodInfo?.Invoke(obj, args);
    }
}
