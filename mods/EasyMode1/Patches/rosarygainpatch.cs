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
        private static void Prefix(string key, ref int amount)
        {
            // Nur Rosaries (intern "geo") und nur Gewinne anfassen
            if (!string.Equals(key, "geo", StringComparison.OrdinalIgnoreCase))
                return;
            if (amount <= 0)
                return;

            int old = amount;
            int boosted = (int)Math.Round(amount * RosaryMultiplier);
            if (boosted < 1) boosted = 1;

            // Debug (optional): zum Testen aktivieren
            // EasyMode1.Plugin.Log?.LogInfo($"[RosaryGainPatch] {old} -> {boosted}");

            amount = boosted;
        }
    }
}