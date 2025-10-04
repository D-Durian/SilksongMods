using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace EasyMode1.Patches
{
	/// <summary>
	/// Reduziert Heilungskosten, indem PlayerData.TakeSilk(int amount, SilkSpool.SilkTakeSource source) u.ä. Overloads gepatcht werden.
	/// Ziel: Nur bei Healing-Quelle greifen; Kostenfaktor per Config (default 0.5 = halbe Kosten).
	/// </summary>
	[HarmonyPatch(typeof(PlayerData))]
	internal static class HealCostPatch
	{
		// Patche alle TakeSilk-Overloads, deren erstes Argument int ist
		static IEnumerable<MethodBase> TargetMethods()
		{
			return typeof(PlayerData)
				.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				.Where(m => m.Name == "TakeSilk")
				.Where(m =>
				{
					var p = m.GetParameters();
					return p.Length >= 1 && p[0].ParameterType == typeof(int);
				});
		}

		[HarmonyPrefix]
		private static bool Prefix(PlayerData __instance, [HarmonyArgument(0)] ref int amount)
		{
			try
			{
				if (!Plugin.EnableHealCostAdjust) return true;
				if (amount <= 0) return true;

				// Heuristik wie im Referenzmod: nur bei großen Abzügen (typisch Heilung) eingreifen
				bool likelyHealingCost = amount > 4;

				if (!likelyHealingCost)
					return true; // andere Silk-Abzüge in Ruhe lassen

				// Wenn volle HP, dann keine Silk abziehen (Original überspringen)
				if (__instance != null && __instance.health >= __instance.maxHealth)
				{
					if (Plugin.DebugLogs)
						Plugin.Log?.LogInfo("[HealCostPatch] full HP -> skip silk deduction");
					return false; // Original nicht ausführen
				}

				int old = amount;

				// 1) variable Kosten bei <3 fehlenden Masken nachbilden
				if (__instance != null)
				{
					int missing = Math.Max(0, __instance.maxHealth - __instance.health);
					if (missing > 0 && missing < 3)
					{
						float ratio = missing / 3f;
						amount = Plugin.RoundRandomly(amount * ratio);
					}
				}

				// 2) zusätzliche Halbierung (oder beliebiger Faktor aus Config)
				float factor = Math.Max(0f, Plugin.HealCostFactor); // 0.5 = halbieren
				amount = Plugin.RoundRandomly(amount * factor);
				if (amount < 0) amount = 0;

				if (Plugin.DebugLogs)
					Plugin.Log?.LogInfo($"[HealCostPatch] {old} -> {amount} (factor {factor})");

				return true; // Original weiter ausführen (mit reduziertem amount)
			}
			catch (Exception ex)
			{
				Plugin.Log?.LogError($"[HealCostPatch] exception: {ex}");
				return true; // Safety
			}
		}
	}
}

