using BepInEx.Logging;
using DarkScreenSystem;
using HarmonyLib;
using PotionCraft.LocalizationSystem;
using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.Potion;
using PotionCraft.ManagersSystem.SaveLoad;
using PotionCraft.ObjectBased.UIElements.ConfirmationWindow;
using PotionCraft.ObjectBased.UIElements.PotionCraftPanel;
using PotionCraft.ManagersSystem.Game;
using PotionCraft.Settings;

namespace DeathGivesBack
{
    class ClassPatches
    {
        static ManualLogSource Log => Plugin.Log;

        public static bool giveback = true;

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
                Functions.GiveBackIngredients();
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
