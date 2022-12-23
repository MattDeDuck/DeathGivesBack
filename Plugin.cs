using BepInEx;
using BepInEx.Logging;
using DarkScreenSystem;
using HarmonyLib;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapObject;
using PotionCraft.ObjectBased.UIElements.ConfirmationWindow;
using PotionCraft.ObjectBased.UIElements.FloatingText;
using PotionCraft.ObjectBased.UIElements.PotionCraftPanel;
using PotionCraft.LocalizationSystem;
using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.Game;
using PotionCraft.ManagersSystem.RecipeMap;
using PotionCraft.Settings;
using PotionCraft.ScriptableObjects;
using PotionCraft.ScriptableObjects.Potion;
using System.Collections.Generic;
using UnityEngine;


namespace DeathGivesBack
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, "PotionCraftDeathGivesBack", "1.0.3.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log { get; set; }

        public const string PLUGIN_GUID = "com.mattdeduck.potioncraftdeathgivesback";

        // Create dictionary to add the used ingredients to, as well as amounts used
        public static Dictionary<InventoryItem, int> used = new();

        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            Log = this.Logger;

            Log.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");
            Harmony.CreateAndPatchAll(typeof(Plugin));
            Harmony.CreateAndPatchAll(typeof(GameManagerStartPatch));
            Harmony.CreateAndPatchAll(typeof(ClassPatches));
            Log.LogInfo($"Plugin {PLUGIN_GUID}: Patch Succeeded!");
        }

        public static void AddLocaleText()
        {
            // Adds the localisation for the two key values
            if (!LocalizationManager.localizationData.Contains("DGB_text", 0, 0))
            {
                LocalizationManager.localizationData.Add(0, "DGB_text", "Ingredients retrieved!");
            }

            if (!LocalizationManager.localizationData.Contains("DGB_resetpotion_text", 0, 0))
            {
                LocalizationManager.localizationData.Add(0, "DGB_resetpotion_text", "All the ingredients will be returned to your inventory. Reset potion?");
            }
        }

        public static void GiveBackIngredients()
        {
            List<PotionUsedComponent> usedComponents = Managers.Potion.usedComponents;

            Log.LogInfo("# Ingredients retrieved:");

            // Loop through used ingredients
            for (int i = 0; i < usedComponents.Count; i++)
            {
                // Ignore potion base component
                if (usedComponents[i].componentType == PotionUsedComponent.ComponentType.InventoryItem)
                {
                    used.Add((InventoryItem)usedComponents[i].componentObject, usedComponents[i].amount);
                }

                // Add the ingredients back to the player inventory
                foreach (KeyValuePair<InventoryItem, int> usedIng in used)
                {
                    Managers.Player.inventory.AddItem(usedIng.Key, usedIng.Value, true, true);
                    Log.LogInfo($"  > {usedIng.Key.name} ({usedIng.Value})");
                }

                // Wipe the dictionary for next use
                used.Clear();
            }

            // Spawns the text for the player notifying them of ingredient retrieval
            RecipeMapObject recipeMapObject = Managers.RecipeMap.recipeMapObject;
            RecipeMapManagerPotionBasesSettings asset = Settings<RecipeMapManagerPotionBasesSettings>.Asset;
            Vector2 v = recipeMapObject.transmitterWindow.ViewRect.center + asset.potionFailTextPos;
            CollectedFloatingText.SpawnNewText(asset.floatingTextSelectBase,
                                               v + new Vector2(0f, -1f),
                                               new CollectedFloatingText.FloatingTextContent(new Key("DGB_text", null, false).GetText(), CollectedFloatingText.FloatingTextContent.Type.Text, 0f),
                                               Managers.Game.Cam.transform,
                                               false,
                                               false);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PotionResetButton), "OnButtonReleasedPointerInside")]
        public static void OnButtonReleasedPointerInside_Postfix()
        {
            // Replaces the standard reset potion localisation text
            ConfirmationWindow.Show(DarkScreenLayer.Lower,
                                    new Key("#reset_potion_warning_title", null, false),
                                    new Key("DGB_resetpotion_text", null, false),
                                    Settings<GameManagerSettings>.Asset.confirmationWindowPosition,
                                    delegate ()
                                    {
                                        Managers.Potion.ResetPotion(true);
                                    }, null, DarkScreenType.Scene, 0f, false);
        }
    }
}