// Patches/IncomingDamagePatch.cs
using System;
using HarmonyLib;

namespace EasyMode1.Patches
{
    /// <summary>
    /// Reduziert eingehenden Schaden um 30 %.
    /// Greift auf alle HeroController.TakeDamage-Überladungen,
    /// deren erstes Argument der Schadenswert (int) ist.
    /// </summary>
    [HarmonyPatch(typeof(HeroController))]
    public static class IncomingDamagePatch
    {
        private const float DamageTakenMultiplier = 0.70f; // -30 %

        // Patcht jede Methode namens "TakeDamage" (Überladungen inkl.),
        // bei der das 1. Argument (Index 0) ein int-Schaden ist.
        [HarmonyPatch("TakeDamage")]
        [HarmonyPrefix]
        private static void AnyTakeDamagePrefix([HarmonyArgument(0)] ref int damage)
        {
            if (damage <= 0) return;

            int old = damage;
            int reduced = (int)Math.Round(damage * DamageTakenMultiplier);
            if (reduced < 1) reduced = 1; // nie auf 0 fallen lassen

            // Debug (optional): auskommentieren, wenn‘s nervt
            // EasyMode1.Plugin.Log?.LogInfo($"[IncomingDamagePatch] {old} -> {reduced}");

            damage = reduced;
        }
    }
}
