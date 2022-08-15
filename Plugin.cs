﻿using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapObject;
using PotionCraft.ObjectBased.UIElements.FloatingText;
using PotionCraft.ObjectBased.UIElements.PotionCraftPanel;
using PotionCraft.LocalizationSystem;
using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.Potion;
using PotionCraft.ManagersSystem.RecipeMap;
using PotionCraft.ManagersSystem.SaveLoad;
using PotionCraft.Settings;
using PotionCraft.ScriptableObjects.Ingredient;
using PotionCraft.ScriptableObjects;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace DeathGivesBack
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, "1.1.3.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log { get; set; }

        // Create dictionary to add the used ingredients to, as well as amounts used
        public static Dictionary<Ingredient, int> used = new Dictionary<Ingredient, int>();

        public static bool giveback = true;

        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            Log = this.Logger;

            Harmony.CreateAndPatchAll(typeof(Plugin));
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

        // Thanks to rswallen#6112 for this
        [HarmonyPrefix, HarmonyPatch(typeof(PotionManager), "ResetPotion")]
        public static void ResetPotion_Prefix(bool resetEffectMapItems = true)
        {
            if (Managers.SaveLoad.SystemState == SaveLoadManager.SystemStateEnum.Loading)
            {
                Log.LogInfo("Nope. Not gonna try while the save is loading");
            }
            else if (giveback)
            {
                GiveBackIngredients();
            }
        }

        // Thanks to rswallen#6112 for this
        [HarmonyPrefix, HarmonyPatch(typeof(PotionCraftPanel), "MakePotion")]
        public static void MakePotion_Prefix(bool addToInventory)
        {
            Log.LogInfo("Giveback disabled for potion creation");
            giveback = false;
        }

        // Thanks to rswallen#6112 for this
        [HarmonyPostfix, HarmonyPatch(typeof(PotionCraftPanel), "MakePotion")]
        public static void MakePotion_Postfix(bool addToInventory)
        {
            Log.LogInfo("Giveback re-enabled after potion creation");
            giveback = true;
        }
    }
}