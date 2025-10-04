using HarmonyLib;

namespace EasyMode1.Patches
{
    /// <summary>
    /// Erhöht allen vom Helden ausgeteilten Schaden um 20 %.
    /// Patch-Ziel: HealthManager.TakeDamage(HitInstance hitInstance)
    /// </summary>
    [HarmonyPatch(typeof(HealthManager), "TakeDamage", new System.Type[] { typeof(HitInstance) })]
    internal static class OutgoingDamagePatch
    {
    private const float OutgoingMultiplier = 1.20f; // +20 %

        [HarmonyPrefix]
        private static void Prefix(ref HitInstance hitInstance)
        {
            if (!Plugin.EnableOutgoingDamage) return;
            // Nur Schaden, der VOM Helden kommt, verstärken
            if (!hitInstance.IsHeroDamage) return;

            // Multiplikativ verstärken – integriert sich sauber mit anderem Balancing
            float old = hitInstance.Multiplier;
            float mult = Plugin.OutgoingMultiplier > 0f ? Plugin.OutgoingMultiplier : 1f;
            hitInstance.Multiplier *= mult;

            if (EasyMode1.Plugin.DebugLogs)
                EasyMode1.Plugin.Log?.LogInfo($"[OutgoingDamagePatch] Mult {old} -> {hitInstance.Multiplier}");
        }
    }
}