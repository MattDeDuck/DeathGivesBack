using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using PotionCraft.LocalizationSystem;

namespace DeathGivesBack
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, "PotionCraftDeathGivesBack", "1.1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log { get; set; }

        public const string PLUGIN_GUID = "com.mattdeduck.potioncraftdeathgivesback";

        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            Log = this.Logger;

            LocalizationManager.OnInitialize.AddListener(Functions.SetModText);

            Log.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");
            Harmony.CreateAndPatchAll(typeof(Plugin));
            Harmony.CreateAndPatchAll(typeof(Functions));
            Harmony.CreateAndPatchAll(typeof(ClassPatches));
            Log.LogInfo($"Plugin {PLUGIN_GUID}: Patch Succeeded!");
        }
    }
}