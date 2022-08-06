using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.IndicatorMapItem;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapObject;
using PotionCraft.ObjectBased.UIElements.FloatingText;
using PotionCraft.ObjectBased.UIElements.PotionCraftPanel;
using PotionCraft.LocalizationSystem;
using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.RecipeMap;
using PotionCraft.Settings;
using PotionCraft.ScriptableObjects.Ingredient;
using PotionCraft.ScriptableObjects;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace DeathGivesBack
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, "1.1.1.0")]
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
        public static void PotionFailed_Prefix()
        {
            GiveBackIngredients();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(RecipeMapObject), "Awake")]
        public static void Awake_Postfix()
        {
            LocalizationManager.textData[LocalizationManager.Locale.en].AddText("DGB_text", "Ingredients retrieved!");
        }

        public static void GiveBackIngredients()
        {
            List<Potion.UsedComponent> usedComponents = Managers.Potion.usedComponents;

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

            RecipeMapObject recipeMapObject = Managers.RecipeMap.recipeMapObject;
            RecipeMapManagerPotionBasesSettings asset = Settings<RecipeMapManagerPotionBasesSettings>.Asset;
            Vector2 v = recipeMapObject.transmitterWindow.ViewRect.center + asset.potionFailTextPos;
            CollectedFloatingText.SpawnNewText(asset.floatingTextSelectBase, v + new Vector2(0f, -1f), new CollectedFloatingText.FloatingTextContent(new Key("DGB_text", null, false).GetText(), CollectedFloatingText.FloatingTextContent.Type.Text, 0f), Managers.Game.Cam.transform, false, false);
        }
    }
}