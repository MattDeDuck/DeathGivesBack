using BepInEx.Logging;
using PotionCraft.Assemblies.DataBaseSystem;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapObject;
using PotionCraft.ObjectBased.UIElements.FloatingText;
using PotionCraft.LocalizationSystem;
using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.RecipeMap;
using PotionCraft.Settings;
using PotionCraft.ScriptableObjects;
using PotionCraft.ScriptableObjects.Potion;
using System;
using System.Collections.Generic;
using UnityEngine;
using Locale = PotionCraft.LocalizationSystem.LocalizationManager.Locale;

namespace DeathGivesBack
{
    class Functions
    {
        static ManualLogSource Log => Plugin.Log;

        // Create dictionary to add the used ingredients to, as well as amounts used
        public static Dictionary<InventoryItem, int> used = new();

        /// <summary>
        /// Adds text to the localisation fields
        /// </summary>
        /// <param name="key">The locale text key reference</param>
        /// <param name="text">The locale text</param>
        public static void AddLocaleText(string key, string text)
        {
            foreach (Locale lang in Enum.GetValues(typeof(Locale)))
            {
                LocalizationManager.localizationData.Add((int)lang, key, text);
            }
        }

        /// <summary>
        /// Sets the text in the locale system for the mod to use
        /// </summary>
        public static void SetModText()
        {
            AddLocaleText("DGB_text", "Ingredients retrieved!");
            AddLocaleText("DGB_resetpotion_text", "All the ingredients will be returned to your inventory. Reset potion?");
        }

        /// <summary>
        /// Gives the player back their ingredients
        /// </summary>
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
    }
}
