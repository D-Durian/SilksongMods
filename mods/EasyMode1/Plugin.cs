using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace EasyMode1
{
    [BepInPlugin("com.duri.easymode1", "Easy Mode 1", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static bool DebugLogs = true; // globaler Debug-Log-Schalter
        private void Awake()
        {
            Log = Logger;

            var h = new Harmony("com.duri.easymode1");
            h.PatchAll(); // findet später unsere Patches automatisch

            Log.LogInfo("EasyMode1 loaded.");
        }
        internal static int RoundRandomly(float v)
        {
            int floor = (int)Math.Floor(v);
            float frac = v - floor;
            return (frac > 0f && _rng.NextDouble() < frac) ? floor + 1 : floor;
        }
        static readonly System.Random _rng = new System.Random();
    }
}
