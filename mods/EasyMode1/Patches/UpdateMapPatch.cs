// Patches/UpdateMapPatch.cs
using System;
using HarmonyLib;

namespace EasyMode1.Patches
{
    /// <summary>
    /// Aktualisiert die Karte sofort beim Öffnen der Schnellkarte,
    /// wenn der Spieler die Feder (Quill) besitzt.
    /// Ziel: GameMap.TryOpenQuickMap() [Postfix]
    /// </summary>
    [HarmonyPatch(typeof(GameMap), "TryOpenQuickMap")]
    internal static class UpdateMapPatch
    {
        [HarmonyPostfix]
        private static void Postfix()
        {
            try
            {
                var pd = PlayerData.instance;
                var gm = GameManager.instance;

                if (pd != null && gm != null && pd.hasQuill)
                {
                    gm.UpdateGameMap();

                    if (EasyMode1.Plugin.DebugLogs)
                        EasyMode1.Plugin.Log?.LogInfo("[UpdateMapPatch] Forced map update on quick map open (hasQuill).");
                }
                else
                {
                    if (EasyMode1.Plugin.DebugLogs)
                        EasyMode1.Plugin.Log?.LogInfo($"[UpdateMapPatch] Skipped update (pd:{pd!=null}, gm:{gm!=null}, hasQuill:{(pd?.hasQuill ?? false)}).");
                }
            }
            catch (Exception ex)
            {
                EasyMode1.Plugin.Log?.LogError($"[UpdateMapPatch] exception: {ex}");
            }
        }
    }
}
