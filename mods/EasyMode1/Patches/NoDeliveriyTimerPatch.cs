using System;
using HarmonyLib;

namespace EasyMode1.Patches
{
    // Ziel wie im Fremdmod: HeroController.TickDeliveryItems()
    [HarmonyPatch(typeof(HeroController), "TickDeliveryItems")]
    internal static class NoDeliveryTimerPatch
    {
        [HarmonyPrefix]
        private static bool Prefix()
        {
            try
            {
                if (EasyMode1.Plugin.DebugLogs)
                    EasyMode1.Plugin.Log?.LogInfo("[NoDeliveryTimerPatch] skipping TickDeliveryItems (no delivery timers)");

                // false = Original NICHT ausführen -> Timer werden effektiv deaktiviert
                return false;
            }
            catch (Exception ex)
            {
                EasyMode1.Plugin.Log?.LogError($"[NoDeliveryTimerPatch] exception: {ex}");
                return true; // Safety: falls etwas schiefgeht, Original laufen lassen
            }
        }
    }
}
