using System;
using HarmonyLib;
using GlobalSettings;     // for Gameplay.* (Silksong assemblies)
using UnityEngine;

namespace EasyMode1.Patches
{
    /// <summary>
    /// Kompass auf der Karte anzeigen, sobald gekauft — Ausrüsten nicht nötig.
    /// Patcht: GameMap.PositionCompassAndCorpse()
    /// </summary>
    [HarmonyPatch(typeof(GameMap), nameof(GameMap.PositionCompassAndCorpse))]
    internal static class CompassAlwaysOnPatch
    {
        private static bool Prefix(
            GameMap __instance,
            ref bool ___displayingCompass,
            GameObject ___compassIcon,
            ShadeMarkerArrow ___shadeMarker,
            GameObject ___currentSceneObj)
        {
            try
            {
                if (!Plugin.EnableCompassAlwaysOn)
                    return true;
                // Szene-Referenz aktualisieren (wie im Fremdmod)
                __instance.UpdateCurrentScene();

                // Kompass-Icon steuern
                if (___currentSceneObj != null && ___compassIcon != null)
                {
                    var compassTool = Gameplay.CompassTool; // ToolItem
                    bool show = compassTool != null
                                && compassTool.IsUnlocked
                                && !__instance.IsLostInAbyssPreMap();

                    if (EasyMode1.Plugin.DebugLogs)
                        EasyMode1.Plugin.Log?.LogInfo($"[CompassPatch] unlocked={compassTool?.IsUnlocked == true}, show={show}");

                    ___compassIcon.SetActive(show);
                    ___displayingCompass = show;
                }

                // Shade/Corpse-Pfeil wie im Original aktualisieren
                if (___shadeMarker != null)
                    ___shadeMarker.SetPosition(__instance.GetCorpsePosition());

                // Original überspringen – wir haben already handled
                return false;
            }
            catch (Exception ex)
            {
                EasyMode1.Plugin.Log?.LogError($"[CompassPatch] exception: {ex}");
                // Fallback: original Methode laufen lassen
                return true;
            }
        }
    }
}
