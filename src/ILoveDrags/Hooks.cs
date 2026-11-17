using HarmonyLib;

namespace ILoveDrags;

[HarmonyPatch]
internal class Hooks
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(IllusionCamera), "LateUpdate")]
    // ReSharper disable once InconsistentNaming
    internal static bool LateUpdatePrefix(IllusionCamera __instance) {
        if (!ILoveDragsPlugin.ConfigEntryPluginEnabled.Value)
            return true;

        return !ILoveDragsPlugin.IsOverDhhUi() && !ILoveDragsPlugin.IsOverPhiblUi();
    }
}
