using HarmonyLib;
using System.Reflection;
using GlobalEnums;
using UnityEngine;

namespace EasyMode1.Patches
{
    [HarmonyPatch(typeof(HeroController), "CheckParry")]
    internal static class ParryWindowGrace
    {
        private static readonly FieldInfo FiCState = AccessTools.Field(typeof(HeroController), "cState");
        private static readonly FieldInfo FiParryAttack = FiCState != null ? AccessTools.Field(FiCState.FieldType, "parryAttack") : null;
        private static readonly FieldInfo FiParrying   = FiCState != null ? AccessTools.Field(FiCState.FieldType, "parrying")   : null;
        private static readonly FieldInfo FiSilkFSM    = AccessTools.Field(typeof(HeroController), "silkSpecialFSM");

        [HarmonyPrefix]
        private static bool Prefix(HeroController __instance, DamageHero damageHero)
        {
            // nur ENEMY kann geparried werden (wie im Original)
            if (damageHero == null || damageHero.hazardType != HazardType.ENEMY)
                return true;

            // wenn Parry sowieso aktiv ist, Original laufen lassen
            var cst = FiCState?.GetValue(__instance);
            if (cst != null && FiParrying != null && (bool)FiParrying.GetValue(cst))
                return true;

            // innerhalb der Gnadenzeit?
            if (Time.time > ParryWindowTracker.GraceUntil)
                return true;

            // ⇒ Parry manuell auslösen – exakt wie der Originalzweig in CheckParry
            var enemy = damageHero.gameObject;
            if (enemy.transform.position.x > __instance.transform.position.x) __instance.FaceRight();
            else __instance.FaceLeft();

            var fsm = FiSilkFSM?.GetValue(__instance);
            var sendEvent = fsm?.GetType().GetMethod("SendEvent", new[] { typeof(string) });
            sendEvent?.Invoke(fsm, new object[] { "PARRIED" });

            if (cst != null && FiParryAttack != null)
                FiParryAttack.SetValue(cst, true);

            if (EasyMode1.Plugin.DebugLogs)
                EasyMode1.Plugin.Log?.LogInfo($"[ParryGrace] granted parry in +{Plugin.ExtraParryGraceSeconds:0.###}s window");

            // Original-CheckParry überspringen – Parry wurde bereits ausgelöst
            return false;
        }
    }
}
