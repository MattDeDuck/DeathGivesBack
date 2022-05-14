using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using ObjectBased.UIElements.FloatingText;
using LocalizationSystem;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DeathGivesBack
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, "1.0.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log { get; set; }
        public static string pluginLoc = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static Texture2D mainButtonTexture;
        public static Sprite mainButtonSprite;

        public static GameObject retrievalButtonGO;

        // Create dictionary to add the used ingredients to, as well as amounts used
        public static Dictionary<Ingredient, int> used = new Dictionary<Ingredient, int>();

        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            Log = this.Logger;

            // Add the text used by the floating text window
            LocalizationSystem.LocalizationSystem.textData[LocalizationSystem.LocalizationSystem.Locale.en].AddText("dgb_text", "Ingredients retrieved!");

            Harmony.CreateAndPatchAll(typeof(Plugin));
        }

        [HarmonyPrefix, HarmonyPatch(typeof(IndicatorMapItem), "OnIndicatorRuined")]
        public static void OnIndicatorRuined_Prefix()
        {
            // Get list of used ingredients
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

                // Display some text for feedback
                RecipeMapObject recipeMapObject = Managers.RecipeMap.recipeMapObject;
                Vector2 v = recipeMapObject.transmitterWindow.ViewRect.center + Managers.RecipeMap.potionBasesSettings.potionFailTextPos;
                CollectedFloatingText.SpawnNewText(Managers.RecipeMap.potionBasesSettings.floatingTextSelectBase, v + new Vector2(0f, -1f), new CollectedFloatingText.FloatingTextContent(new Key("dgb_text", null).GetText(), CollectedFloatingText.FloatingTextContent.Type.Text, 0f), Managers.Game.Cam.transform, false, false);

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