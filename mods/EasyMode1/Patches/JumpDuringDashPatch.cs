using HarmonyLib;
using System.Reflection;

namespace EasyMode1.Patches
{
    /// <summary>
    /// Erlaubt Sprünge während des Dashens, indem beim CanJump-Check die dashing-Flag temporär deaktiviert wird.
    /// So bleibt die Originallogik (onGround/ledgeBuffer usw.) vollständig erhalten.
    /// </summary>
    [HarmonyPatch(typeof(HeroController), nameof(HeroController.CanJump))]
    internal static class JumpDuringDashPatch
    {
        private static readonly FieldInfo FiCState  = AccessTools.Field(typeof(HeroController), "cState");
        private static readonly FieldInfo FiDashing = (FiCState != null && FiCState.FieldType != null)
            ? AccessTools.Field(FiCState.FieldType, "dashing")
            : null;

        [HarmonyPrefix]
        private static void Prefix(HeroController __instance, out bool __state)
        {
            __state = false;
            if (!Plugin.EnableJumpDuringDash) return;

            if (FiCState == null || FiDashing == null) return;
            var cstate = FiCState.GetValue(__instance);
            if (cstate == null) return;

            bool isDashing = (bool)FiDashing.GetValue(cstate);
            if (isDashing)
            {
                // Temporär dashing deaktivieren, damit CanJump nicht direkt false zurückgibt
                FiDashing.SetValue(cstate, false);
                __state = true; // wir haben geändert; in Postfix zurücksetzen
                if (Plugin.DebugLogs)
                    Plugin.Log?.LogInfo("[JumpDuringDash] Temporarily cleared cState.dashing during CanJump().");
            }
        }

        [HarmonyPostfix]
        private static void Postfix(HeroController __instance, bool __state)
        {
            if (!Plugin.EnableJumpDuringDash) return;
            if (!__state) return; // nichts geändert

            if (FiCState == null || FiDashing == null) return;
            var cstate = FiCState.GetValue(__instance);
            if (cstate == null) return;

            // dashing-Flag wiederherstellen
            FiDashing.SetValue(cstate, true);
        }
    }
}
