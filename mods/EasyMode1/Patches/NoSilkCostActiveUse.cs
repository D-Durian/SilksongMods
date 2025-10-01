using HarmonyLib;

namespace EasyMode1.Patches
{
    /// <summary>
    /// Setzt Silk-Kosten für aktive Nutzung (Swift Step, Clawline) auf 0.
    /// Ziel: HeroController.TakeSilk(int amount, SilkSpool.SilkTakeSource source)
    /// </summary>
    [HarmonyPatch(typeof(HeroController), "TakeSilk", new System.Type[] { typeof(int), typeof(SilkSpool.SilkTakeSource) })]
    //[HarmonyPriority(Priority.Last)] // optional: falls andere Mods auch am amount drehen
    internal static class NoSilkCost_ActiveUse
    {
        [HarmonyPrefix]
        private static void Prefix(ref int amount, SilkSpool.SilkTakeSource source)
        {
            // Nur „aktive Nutzung“ kostenlos machen – Heilung/Drain/Curses bleiben unberührt
            if (source == SilkSpool.SilkTakeSource.ActiveUse)
            {
                if (EasyMode1.Plugin.DebugLogs)
                    EasyMode1.Plugin.Log?.LogInfo($"[NoSilkCost] ActiveUse: {amount} -> 0");
                amount = 0;
            }
        }
    }
}
