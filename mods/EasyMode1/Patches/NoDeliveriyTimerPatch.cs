using System;
using HarmonyLib;

namespace EasyMode1.Patches
{
    // Ziel wie im Fremdmod: HeroController.TickDeliveryItems()
    [HarmonyPatch(typeof(HeroController), "TickDeliveryItems")]
    internal static class NoDeliveryTimerPatch
    {
        private static bool _loggedOnce;
        [HarmonyPrefix]
        private static bool Prefix()
        {
            try
            {
                if (EasyMode1.Plugin.DebugLogs && !_loggedOnce)
                {
                    EasyMode1.Plugin.Log?.LogInfo("[NoDeliveryTimerPatch] skipping TickDeliveryItems (no delivery timers)");
                    _loggedOnce = true;
                }

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
