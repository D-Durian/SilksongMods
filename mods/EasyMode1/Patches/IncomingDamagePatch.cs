// Patches/IncomingDamagePatch.cs
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace EasyMode1.Patches
{
    /// <summary>
    /// Reduziert eingehenden Schaden um 30 % deterministisch.
    /// Greift direkt in PlayerData.TakeHealth(int amount) ein (finale int-Schadensgröße),
    /// vermeidet damit Rundungen auf Float-Basis und abstruse Werte.
    /// </summary>
    [HarmonyPatch(typeof(PlayerData))]
    public static class IncomingDamagePatch
    {
        // -30 %: integerbasiert via (amount * 70) / 100
        private const int DamagePercent = 70; // 70 % des ursprünglichen Schadens

        // Patche alle Überladungen von TakeHealth, deren erstes Argument ein int ist
        static IEnumerable<MethodBase> TargetMethods()
        {
            return typeof(PlayerData)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.Name == "TakeHealth")
                .Where(m =>
                {
                    var p = m.GetParameters();
                    return p.Length >= 1 && p[0].ParameterType == typeof(int);
                });
        }

        [HarmonyPrefix]
        private static void Prefix([HarmonyArgument(0)] ref int amount)
        {
            if (!Plugin.EnableIncomingDamage) return;
            if (amount <= 0) return;

            // Sanity-Check: sehr große Werte ignorieren (andere Mods/Überläufe)
            if (amount > 100000)
            {
                if (EasyMode1.Plugin.DebugLogs)
                    EasyMode1.Plugin.Log?.LogWarning($"[IncomingDamagePatch] abnormal amount {amount}, skip scaling");
                return;
            }

            int old = amount;
            // Deterministische Skalierung ohne Floating-Point-Rundung (Floor durch Integer-Division)
            int percent = Math.Max(0, Math.Min(100, Plugin.DamageTakenPercent));
            int reduced = (old * percent) / 100;
            if (reduced < 1) reduced = 1; // nie auf 0 fallen lassen (anpassbar)

            if (EasyMode1.Plugin.DebugLogs)
                EasyMode1.Plugin.Log?.LogInfo($"[IncomingDamagePatch] {old} -> {reduced}");

            amount = reduced;
        }
    }
}
