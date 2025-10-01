using System;
using HarmonyLib;
using TeamCherry.Localization;

namespace EasyMode1.Patches
{
    // Ziel: CurrencyObjectBase.MagnetToolIsEquipped()  (non-public -> Name als String)
    [HarmonyPatch(typeof(CurrencyObjectBase), "MagnetToolIsEquipped")]
    internal static class RosaryMagnetPatch
    {
        // Coin/Rosaries magnetisch erzwingen, aber nur wenn das Magnet-Tool freigeschaltet ist
        private static bool Prefix(ref bool __result, LocalisedString ___popupName, ToolItem ___magnetTool)
        {
            try
            {
                var key = ___popupName?.Key;

                if (string.Equals(key, "INV_NAME_COIN", StringComparison.OrdinalIgnoreCase)
                    && ___magnetTool != null && ___magnetTool.IsUnlocked)
                {
                    __result = true; // Magnet aktiv
                    if (EasyMode1.Plugin.DebugLogs)
                        EasyMode1.Plugin.Log?.LogInfo("[RosaryMagnetPatch] forced magnet for COIN (tool unlocked)");
                    return false;    // Original überspringen
                }

                return true; // Original entscheiden lassen
            }
            catch (Exception ex)
            {
                EasyMode1.Plugin.Log?.LogError($"[RosaryMagnetPatch] exception: {ex}");
                return true;
            }
        }
    }
}
