using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace EasyMode1.Patches
{
    [HarmonyPatch(typeof(HeroController), "Update")]
    internal static class ParryWindowTracker
    {
        // Stell hier ein, wie „großzügig“ das Fenster nach dem normalen Parry sein soll.
        // 0.12f ≈ ~7 Frames @60fps. Für mehr, z. B. 0.18f.
    // Wert aus Plugin.Config

        internal static float GraceUntil;

        private static readonly FieldInfo FiCState = AccessTools.Field(typeof(HeroController), "cState");
        private static FieldInfo FiParrying;

        [HarmonyPostfix]
        private static void Postfix(object __instance)
        {
            var cst = FiCState?.GetValue(__instance);
            if (cst == null) return;

            FiParrying ??= AccessTools.Field(cst.GetType(), "parrying");
            if (FiParrying == null) return;

            if ((bool)FiParrying.GetValue(cst))
                GraceUntil = Time.time + Plugin.ExtraParryGraceSeconds;  // Gnadenzeit läuft ab, wenn Parry endet
        }
    }
}
