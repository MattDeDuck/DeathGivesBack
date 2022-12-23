using BepInEx.Logging;
using HarmonyLib;
using PotionCraft.ObjectBased.UIElements.PotionCraftPanel;
using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.Potion;
using PotionCraft.ManagersSystem.SaveLoad;

namespace DeathGivesBack
{
    class ClassPatches
    {
        static ManualLogSource Log => Plugin.Log;

        public static bool giveback = true;

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
                Plugin.GiveBackIngredients();
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
