using System;
using System.Reflection;
using HarmonyLib;
using GlobalSettings;
// ToolItem & Gameplay.* stecken in Assembly-CSharp

namespace EasyMode1.Patches
{
    // Ziel-Typ wie im Fremdmod
    [HarmonyPatch(typeof(ToolItemManager.ToolStatus))]
    internal static class UnequippedWallClingPatch
    {
        // privates Feld "tool" der ToolStatus-Instanz
        private static readonly FieldInfo toolField =
            AccessTools.Field(typeof(ToolItemManager.ToolStatus), "tool");

        private static void Apply(object __instance, ref bool __result)
        {
            if (!Plugin.EnableWallClingUnlocked) return;
            // wenn schon true, nichts ändern
            if (__result) return;

            // Feld sauber lesen
            var tool = toolField?.GetValue(__instance) as ToolItem;
            if (tool == null) return;

            // Genau wie im Fremdmod: Ascendant's Grip (WallClingTool) & freigeschaltet
            if (tool == Gameplay.WallClingTool && tool.IsUnlocked)
            {
                __result = true; // gilt als "ausgerüstet"
                if (EasyMode1.Plugin.DebugLogs)
                    EasyMode1.Plugin.Log?.LogInfo("[UnequippedWallCling] forced equipped (unlocked).");
            }
        }

        [HarmonyPostfix, HarmonyPatch("get_IsEquipped")]
        private static void Postfix_IsEquipped(object __instance, ref bool __result)
        {
            try { Apply(__instance, ref __result); }
            catch (Exception ex) { EasyMode1.Plugin.Log?.LogError($"[UnequippedWallCling] IsEquipped ex: {ex}"); }
        }

        [HarmonyPostfix, HarmonyPatch("get_IsEquippedCutscene")]
        private static void Postfix_IsEquippedCutscene(object __instance, ref bool __result)
        {
            try { Apply(__instance, ref __result); }
            catch (Exception ex) { EasyMode1.Plugin.Log?.LogError($"[UnequippedWallCling] IsEquippedCutscene ex: {ex}"); }
        }
    }
}
