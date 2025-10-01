// Patches/SilkGainPatch.cs
using System;
using HarmonyLib;

namespace EasyMode1.Patches
{
    [HarmonyPatch(typeof(HeroController), "AddSilk", new Type[] {
        typeof(int), typeof(bool), typeof(SilkSpool.SilkAddSource), typeof(bool)
    })]
    public static class SilkGainPatch
    {
        private const float SilkMultiplier = 2f; // +100%
        private const bool DebugLogs = true;       // zum schnellen Ein-/Ausschalten

        [HarmonyPrefix]
        private static void Prefix(ref int amount)
        {
            if (amount <= 0) return;

            int old = amount;
            int boosted = EasyMode1.Plugin.RoundRandomly(amount * SilkMultiplier);
            if (boosted < 1) boosted = 1;

            if (DebugLogs)
                EasyMode1.Plugin.Log?.LogInfo($"[SilkGainPatch] {old} -> {boosted}");

            amount = boosted;
        }
    }
}