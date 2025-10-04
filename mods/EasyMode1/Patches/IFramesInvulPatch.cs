using System;
using System.Reflection;
using HarmonyLib;

namespace EasyMode1.Patches
{
    /// <summary>
    /// Erzwingt mind. 0.7s i-frames, indem vor Start der Invulnerable-Coroutine
    /// invulnerableDuration = max(invulnerableDuration, 0.7f) gesetzt wird.
    /// </summary>
    [HarmonyPatch(typeof(HeroController), "Invulnerable")]
    internal static class IFrames_ClampInvulnerable
    {
    // Wert aus Plugin.Config

        // private float invulnerableDuration;
        private static readonly FieldInfo FiInvulnDuration =
            AccessTools.Field(typeof(HeroController), "invulnerableDuration");

        // optional: freezes (wir lassen die Freeze-Zeit unberührt)
        // private static readonly FieldInfo FiInvulnFreeze =
        //     AccessTools.Field(typeof(HeroController), "invulnerableFreezeDuration");

        [HarmonyPrefix]
        private static void Prefix(object __instance)
        {
            if (!Plugin.EnableIFramesClamp) return;
            if (FiInvulnDuration == null) return;

            try
            {
                float cur = (float)FiInvulnDuration.GetValue(__instance);
                float min = Math.Max(0f, Plugin.MinIFramesSeconds);
                float next = cur < min ? min : cur;
                if (next != cur)
                {
                    FiInvulnDuration.SetValue(__instance, next);
                    if (EasyMode1.Plugin.DebugLogs)
                        EasyMode1.Plugin.Log?.LogInfo($"[IFrames] invulnerableDuration: {cur:0.###} -> {next:0.###}");
                }
            }
            catch (Exception ex)
            {
                EasyMode1.Plugin.Log?.LogError($"[IFrames] clamp failed: {ex}");
            }
        }
    }
}
