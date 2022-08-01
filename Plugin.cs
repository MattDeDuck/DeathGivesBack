using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.IndicatorMapItem;
using PotionCraft.ManagersSystem;
using PotionCraft.ScriptableObjects.Ingredient;
using PotionCraft.ScriptableObjects;

namespace DeathGivesBack
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, "1.1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log { get; set; }
        public static string pluginLoc = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        // Create dictionary to add the used ingredients to, as well as amounts used
        public static Dictionary<Ingredient, int> used = new Dictionary<Ingredient, int>();

        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            Log = this.Logger;

            Harmony.CreateAndPatchAll(typeof(Plugin));
        }

        [HarmonyPrefix, HarmonyPatch(typeof(IndicatorMapItem), "PotionFailed")]
        public static void OnIndicatorRuined_Prefix()
        {
            List<Potion.UsedComponent> usedComponents = Managers.Potion.usedComponents;
            for (int i = 0; i < usedComponents.Count; i++)
            {
                if (usedComponents[i].componentType == Potion.UsedComponent.ComponentType.InventoryItem)
                {
                    //used.Add((Ingredient)derp.usedComponents[i].componentObject, derp.usedComponents[i].amount);
                    Log.LogInfo(usedComponents[i].componentObject.name);
                }
            }

            Log.LogInfo("# Ingredients retrieved:");

            // Loop through used ingredients
            for (int i = 0; i < usedComponents.Count; i++)
            {
                // Ignore potion base component
                if (usedComponents[i].componentType == Potion.UsedComponent.ComponentType.InventoryItem)
                {
                    used.Add((Ingredient)usedComponents[i].componentObject, usedComponents[i].amount);
                }

                // Add the ingredients back to the player inventory
                foreach (KeyValuePair<Ingredient, int> usedIng in used)
                {
                    Managers.Player.inventory.AddItem(usedIng.Key, usedIng.Value, true, true);
                    Log.LogInfo($"  > {usedIng.Key.name} ({usedIng.Value})");
                }

                // Wipe the dictionary for next use
                used.Clear();
            }
        }
    }
}