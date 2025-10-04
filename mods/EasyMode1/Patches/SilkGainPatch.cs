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
    // Werte aus Plugin.Config

        [HarmonyPrefix]
        private static void Prefix(ref int amount)
        {
            if (!Plugin.EnableSilkGain) return;
            if (amount <= 0) return;

            int old = amount;
            int boosted = EasyMode1.Plugin.RoundRandomly(amount * Plugin.SilkMultiplier);
            if (boosted < 1) boosted = 1;

            if (EasyMode1.Plugin.DebugLogs)
                EasyMode1.Plugin.Log?.LogInfo($"[SilkGainPatch] {old} -> {boosted}");

            amount = boosted;
        }
    }
}