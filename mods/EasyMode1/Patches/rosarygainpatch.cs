// Patches/RosaryGainPatch.cs
using System;
using HarmonyLib;

namespace EasyMode1.Patches
{
    /// <summary>
    /// Multipliziert jeden Rosary-Gewinn mit 1.5 (nur bei positiven Zuwächsen).
    /// Hook: GameManager.IntAdd(string key, int amount)
    /// </summary>
    [HarmonyPatch(typeof(GameManager), "IntAdd", new Type[] { typeof(string), typeof(int) })]
    public static class RosaryGainPatch
    {
        private const float RosaryMultiplier = 1.5f; // x1.5 statt x2

        [HarmonyPrefix]
        private static void Prefix([HarmonyArgument(0)] string intName, ref int amount)
        {
            // Nur Rosaries (intern "geo") und nur Gewinne anfassen
            if (!string.Equals(intName, "geo", StringComparison.OrdinalIgnoreCase))
                return;
            if (amount <= 0)
                return;

            int old = amount;
            int boosted = EasyMode1.Plugin.RoundRandomly(amount * RosaryMultiplier);
            if (boosted < 1) boosted = 1;

            if (EasyMode1.Plugin.DebugLogs)
                EasyMode1.Plugin.Log?.LogInfo($"[RosaryGainPatch] geo {old} -> {boosted}");

            amount = boosted;
        }
    }
}