// Patches/DashCooldownSetterPatch.cs
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace EasyMode1.Patches
{
    /// <summary>
    /// Verkürzt den Dash-Cooldown global, indem der frisch gesetzte Timer skaliert wird.
    /// Beispiel: 0.6f = 40% kürzerer Cooldown.
    /// </summary>
    [HarmonyPatch(typeof(HeroController), nameof(HeroController.SetDashCooldownTimer))]
    internal static class DashCooldownSetterPatch
    {
    // Werte aus Plugin.Config

        private static readonly FieldInfo FiDashCD     = AccessTools.Field(typeof(HeroController), "dashCooldownTimer");
        private static readonly FieldInfo FiBackDashCD = AccessTools.Field(typeof(HeroController), "backDashCooldownTimer"); // kann null sein

        [HarmonyPostfix]
        private static void Postfix(object __instance)
        {
            if (!Plugin.EnableDashCooldownAdjust) return;
            if (Plugin.DashCooldownMultiplier >= 1f) return;

            // Haupt-Dash
            if (FiDashCD != null)
            {
                float cur  = (float)FiDashCD.GetValue(__instance);
                float next = Mathf.Max(0f, cur * Plugin.DashCooldownMultiplier);
                if (next != cur)
                {
                    FiDashCD.SetValue(__instance, next);
                    if (EasyMode1.Plugin.DebugLogs)
                        EasyMode1.Plugin.Log?.LogInfo($"[DashCD] dashCooldownTimer {cur:0.###} -> {next:0.###}");
                }
            }

            // Back-Dash (falls vorhanden/benutzt)
            if (FiBackDashCD != null)
            {
                float curB = (float)FiBackDashCD.GetValue(__instance);
                if (curB > 0f)
                {
                    float nextB = Mathf.Max(0f, curB * Plugin.DashCooldownMultiplier);
                    if (nextB != curB)
                    {
                        FiBackDashCD.SetValue(__instance, nextB);
                        if (EasyMode1.Plugin.DebugLogs)
                            EasyMode1.Plugin.Log?.LogInfo($"[DashCD] backDashCooldownTimer {curB:0.###} -> {nextB:0.###}");
                    }
                }
            }
        }
    }
}
