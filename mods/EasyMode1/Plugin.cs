using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace EasyMode1
{
    [BepInPlugin("com.duri.easymode1", "Easy Mode 1", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
    // Global Debug toggle
    internal static bool DebugLogs = true;

    // Feature toggles & values (öffentlich statisch, damit Patches ohne Config-Objekt darauf zugreifen)
    public static bool EnableIncomingDamage;
    public static int  DamageTakenPercent;

    public static bool EnableOutgoingDamage;
    public static float OutgoingMultiplier;

    public static bool EnableSilkGain;
    public static float SilkMultiplier;

    public static bool EnableRosaryGain;
    public static float RosaryMultiplier;

    public static bool EnableNoDeliveryTimer;

    public static bool EnableCompassAlwaysOn;
    public static bool EnableUpdateMapOnQuickOpen;

    public static bool EnableWallClingUnlocked;

    public static bool EnableDashCooldownAdjust;
    public static float DashCooldownMultiplier;

    // Allow jump to register even while currently dashing
    public static bool EnableJumpDuringDash;

    public static bool EnableIFramesClamp;
    public static float MinIFramesSeconds;

    public static bool EnableParryGrace;
    public static float ExtraParryGraceSeconds;

    // Healing cost adjustments (BindCost override removed)
    public static bool EnableHealCostAdjust;
    public static float HealCostFactor; // 0.5 = half of original cost

    // Equip any tool in any slot
    public static bool EnableEquipAnywhere;

    // Backing ConfigEntries (falls Live-Änderung gewünscht)
    private ConfigEntry<bool> _cfgDebugLogs;
    private ConfigEntry<bool> _cfgEnableIncoming;
    private ConfigEntry<int>  _cfgDamageTakenPercent;

    private ConfigEntry<bool> _cfgEnableOutgoing;
    private ConfigEntry<float> _cfgOutgoingMultiplier;

    private ConfigEntry<bool> _cfgEnableSilk;
    private ConfigEntry<float> _cfgSilkMultiplier;

    private ConfigEntry<bool> _cfgEnableRosary;
    private ConfigEntry<float> _cfgRosaryMultiplier;

    private ConfigEntry<bool> _cfgEnableNoDeliveryTimer;

    private ConfigEntry<bool> _cfgEnableCompassAlwaysOn;
    private ConfigEntry<bool> _cfgEnableUpdateMapOnQuickOpen;

    private ConfigEntry<bool> _cfgEnableWallClingUnlocked;

    private ConfigEntry<bool> _cfgEnableDashCooldownAdjust;
    private ConfigEntry<float> _cfgDashCooldownMultiplier;

    private ConfigEntry<bool> _cfgEnableJumpDuringDash;

    private ConfigEntry<bool> _cfgEnableIFramesClamp;
    private ConfigEntry<float> _cfgMinIFramesSeconds;

    private ConfigEntry<bool> _cfgEnableParryGrace;
    private ConfigEntry<float> _cfgExtraParryGraceSeconds;

    private ConfigEntry<bool> _cfgEnableHealCostAdjust;
    private ConfigEntry<float> _cfgHealCostFactor;
    private ConfigEntry<bool> _cfgEnableEquipAnywhere;
        private void Awake()
        {
            Log = Logger;

            // Load configuration
            LoadConfig();

            // Patch all
            var h = new Harmony("com.duri.easymode1");
            h.PatchAll();

            Log.LogInfo("EasyMode1 loaded.");
        }
        private void LoadConfig()
        {
            // Debug
            _cfgDebugLogs = Config.Bind("Debug", "EnableDebugLogs", true, "Enable verbose debug logs for EasyMode1.");
            DebugLogs = _cfgDebugLogs.Value;
            _cfgDebugLogs.SettingChanged += (_, __) => DebugLogs = _cfgDebugLogs.Value;

            // Incoming damage
            _cfgEnableIncoming = Config.Bind("IncomingDamage", "Enable", true, "Enable scaling of incoming damage.");
            _cfgDamageTakenPercent = Config.Bind("IncomingDamage", "DamageTakenPercent", 70, "Percent of original damage taken (0..100). 70 = -30%.");
            EnableIncomingDamage = _cfgEnableIncoming.Value;
            DamageTakenPercent = Math.Max(0, Math.Min(100, _cfgDamageTakenPercent.Value));
            _cfgEnableIncoming.SettingChanged += (_, __) => EnableIncomingDamage = _cfgEnableIncoming.Value;
            _cfgDamageTakenPercent.SettingChanged += (_, __) => DamageTakenPercent = Math.Max(0, Math.Min(100, _cfgDamageTakenPercent.Value));

            // Outgoing damage
            _cfgEnableOutgoing = Config.Bind("OutgoingDamage", "Enable", true, "Enable scaling of outgoing hero damage.");
            _cfgOutgoingMultiplier = Config.Bind("OutgoingDamage", "Multiplier", 1.2f, "Damage multiplier for hero outgoing damage.");
            EnableOutgoingDamage = _cfgEnableOutgoing.Value;
            OutgoingMultiplier = Math.Max(0f, _cfgOutgoingMultiplier.Value);
            _cfgEnableOutgoing.SettingChanged += (_, __) => EnableOutgoingDamage = _cfgEnableOutgoing.Value;
            _cfgOutgoingMultiplier.SettingChanged += (_, __) => OutgoingMultiplier = Math.Max(0f, _cfgOutgoingMultiplier.Value);

            // Silk
            _cfgEnableSilk = Config.Bind("Silk", "Enable", true, "Enable scaling of silk gains.");
            _cfgSilkMultiplier = Config.Bind("Silk", "Multiplier", 2.0f, "Silk gain multiplier.");
            EnableSilkGain = _cfgEnableSilk.Value;
            SilkMultiplier = Math.Max(0f, _cfgSilkMultiplier.Value);
            _cfgEnableSilk.SettingChanged += (_, __) => EnableSilkGain = _cfgEnableSilk.Value;
            _cfgSilkMultiplier.SettingChanged += (_, __) => SilkMultiplier = Math.Max(0f, _cfgSilkMultiplier.Value);

            // Rosary (geo)
            _cfgEnableRosary = Config.Bind("Rosary", "Enable", true, "Enable scaling of rosary/geo gains.");
            _cfgRosaryMultiplier = Config.Bind("Rosary", "Multiplier", 2f, "Rosary/geo gain multiplier.");
            EnableRosaryGain = _cfgEnableRosary.Value;
            RosaryMultiplier = Math.Max(0f, _cfgRosaryMultiplier.Value);
            _cfgEnableRosary.SettingChanged += (_, __) => EnableRosaryGain = _cfgEnableRosary.Value;
            _cfgRosaryMultiplier.SettingChanged += (_, __) => RosaryMultiplier = Math.Max(0f, _cfgRosaryMultiplier.Value);

            // Misc / Tools
            _cfgEnableNoDeliveryTimer = Config.Bind("Misc", "EnableNoDeliveryTimer", true, "Disable delivery timers.");
            EnableNoDeliveryTimer = _cfgEnableNoDeliveryTimer.Value;
            _cfgEnableNoDeliveryTimer.SettingChanged += (_, __) => EnableNoDeliveryTimer = _cfgEnableNoDeliveryTimer.Value;

            _cfgEnableCompassAlwaysOn = Config.Bind("Map", "EnableCompassAlwaysOn", true, "Show compass on map when unlocked, no equip needed.");
            _cfgEnableUpdateMapOnQuickOpen = Config.Bind("Map", "EnableUpdateOnQuickOpen", true, "Force map update on quick map open when quill owned.");
            EnableCompassAlwaysOn = _cfgEnableCompassAlwaysOn.Value;
            EnableUpdateMapOnQuickOpen = _cfgEnableUpdateMapOnQuickOpen.Value;
            _cfgEnableCompassAlwaysOn.SettingChanged += (_, __) => EnableCompassAlwaysOn = _cfgEnableCompassAlwaysOn.Value;
            _cfgEnableUpdateMapOnQuickOpen.SettingChanged += (_, __) => EnableUpdateMapOnQuickOpen = _cfgEnableUpdateMapOnQuickOpen.Value;

            _cfgEnableWallClingUnlocked = Config.Bind("Movement", "EnableWallClingUnlocked", true, "Treat wall cling as equipped when unlocked.");
            EnableWallClingUnlocked = _cfgEnableWallClingUnlocked.Value;
            _cfgEnableWallClingUnlocked.SettingChanged += (_, __) => EnableWallClingUnlocked = _cfgEnableWallClingUnlocked.Value;

            _cfgEnableDashCooldownAdjust = Config.Bind("Movement", "EnableDashCooldownAdjust", true, "Enable scaling of dash cooldown timers.");
            _cfgDashCooldownMultiplier = Config.Bind("Movement", "DashCooldownMultiplier", 0.1f, "Dash cooldown multiplier (0..1, lower = shorter).");
            EnableDashCooldownAdjust = _cfgEnableDashCooldownAdjust.Value;
            DashCooldownMultiplier = Math.Max(0f, _cfgDashCooldownMultiplier.Value);
            _cfgEnableDashCooldownAdjust.SettingChanged += (_, __) => EnableDashCooldownAdjust = _cfgEnableDashCooldownAdjust.Value;
            _cfgDashCooldownMultiplier.SettingChanged += (_, __) => DashCooldownMultiplier = Math.Max(0f, _cfgDashCooldownMultiplier.Value);

            _cfgEnableJumpDuringDash = Config.Bind("Movement", "EnableJumpDuringDash", true, "Allow jump input to register during a dash (removes short jump lock after dash start).");
            EnableJumpDuringDash = _cfgEnableJumpDuringDash.Value;
            _cfgEnableJumpDuringDash.SettingChanged += (_, __) => EnableJumpDuringDash = _cfgEnableJumpDuringDash.Value;

            _cfgEnableIFramesClamp = Config.Bind("Combat", "EnableIFramesClamp", true, "Clamp minimum invulnerability frames after hit.");
            _cfgMinIFramesSeconds = Config.Bind("Combat", "MinIFramesSeconds", 0.7f, "Minimum invulnerability duration in seconds.");
            EnableIFramesClamp = _cfgEnableIFramesClamp.Value;
            MinIFramesSeconds = Math.Max(0f, _cfgMinIFramesSeconds.Value);
            _cfgEnableIFramesClamp.SettingChanged += (_, __) => EnableIFramesClamp = _cfgEnableIFramesClamp.Value;
            _cfgMinIFramesSeconds.SettingChanged += (_, __) => MinIFramesSeconds = Math.Max(0f, _cfgMinIFramesSeconds.Value);

            _cfgEnableParryGrace = Config.Bind("Combat", "EnableParryGrace", true, "Enable extra parry grace window after a parry.");
            _cfgExtraParryGraceSeconds = Config.Bind("Combat", "ExtraParryGraceSeconds", 0.12f, "Additional parry grace seconds.");
            EnableParryGrace = _cfgEnableParryGrace.Value;
            ExtraParryGraceSeconds = Math.Max(0f, _cfgExtraParryGraceSeconds.Value);
            _cfgEnableParryGrace.SettingChanged += (_, __) => EnableParryGrace = _cfgEnableParryGrace.Value;
            _cfgExtraParryGraceSeconds.SettingChanged += (_, __) => ExtraParryGraceSeconds = Math.Max(0f, _cfgExtraParryGraceSeconds.Value);

            // Healing
            _cfgEnableHealCostAdjust = Config.Bind("Healing", "Enable", true, "Enable reduced silk cost for healing.");
            _cfgHealCostFactor = Config.Bind("Healing", "CostFactor", 0.5f, "Factor relative to original healing cost. 0.5 = half.");
            EnableHealCostAdjust = _cfgEnableHealCostAdjust.Value;
            HealCostFactor = Math.Max(0f, _cfgHealCostFactor.Value);
            _cfgEnableHealCostAdjust.SettingChanged += (_, __) => EnableHealCostAdjust = _cfgEnableHealCostAdjust.Value;
            _cfgHealCostFactor.SettingChanged += (_, __) => HealCostFactor = Math.Max(0f, _cfgHealCostFactor.Value);

            // Equip Anywhere
            _cfgEnableEquipAnywhere = Config.Bind("Inventory", "EnableEquipAnywhere", true, "Allow equipping any tool in any slot, ignoring color restrictions.");
            EnableEquipAnywhere = _cfgEnableEquipAnywhere.Value;
            _cfgEnableEquipAnywhere.SettingChanged += (_, __) => EnableEquipAnywhere = _cfgEnableEquipAnywhere.Value;
        }
        // BindCost override helper removed
        internal static int RoundRandomly(float v)
        {
            int floor = (int)Math.Floor(v);
            float frac = v - floor;
            return (frac > 0f && _rng.NextDouble() < frac) ? floor + 1 : floor;
        }
        static readonly System.Random _rng = new System.Random();
    }
}
