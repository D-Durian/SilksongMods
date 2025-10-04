// Patches/RosaryGainPatch.cs
using System;
using HarmonyLib;

namespace EasyMode1.Patches
{
    /// <summary>
    /// Multipliziert jeden Rosary-Gewinn mit 1.5 (nur bei positiven Zuwächsen).
    /// Hook: PlayerData.AddGeo(int amount)
    /// </summary>
    [HarmonyPatch(typeof(PlayerData), "AddGeo", new Type[] { typeof(int) })]
    public static class RosaryGainPatch
    {

        [HarmonyPrefix]
        private static void Prefix(ref int amount)
        {
            if (!Plugin.EnableRosaryGain) return;
            // Nur Gewinne anfassen
            if (amount <= 0)
                return;

            int old = amount;
            int boosted = EasyMode1.Plugin.RoundRandomly(amount * Plugin.RosaryMultiplier);
            if (boosted < 1) boosted = 1;

            if (EasyMode1.Plugin.DebugLogs)
                EasyMode1.Plugin.Log?.LogInfo($"[RosaryGainPatch] {old} -> {boosted}");

            amount = boosted;
        }
    }
}