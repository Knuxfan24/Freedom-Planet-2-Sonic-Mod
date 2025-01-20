using System;

namespace FP2_Sonic_Mod.Patchers
{
    internal class FPSaveManagerPatcher
    {
        /// <summary>
        /// Gives Power Up Start a proper description for Sonic.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPSaveManager), nameof(FPSaveManager.GetItemDescription))]
        private static void PowerUpStartDescription(ref FPPowerup item, ref FPCharacterID ___character, ref string __result)
        {
            // Check if the item being called for is Power Up Start and that the player is Sonic. If so, replace the description that gets returned.
            if (item == FPPowerup.POWERUP_START && ___character == Plugin.sonicCharacterID)
                __result = "You begin the stage with Power Sneakers.";
        }

        /// <summary>
        /// Gives the Story Mode item the Chaos Emeralds name.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPSaveManager), nameof(FPSaveManager.GetItemName))]
        private static void ChaosEmeraldName(ref FPPowerup item, ref string __result)
        {
            // Check if the item being called for is Story Mode. If so, replace the name that gets returned.
            if (item == FPPowerup.STORY_MODE)
                __result = "Chaos Emeralds";
        }

        /// <summary>
        /// Gives the Story Mode item the Chaos Emeralds description.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPSaveManager), nameof(FPSaveManager.GetItemDescription))]
        private static void ChaosEmeraldDescription(ref FPPowerup item, ref string __result)
        {
            // Check if the item being called for is Story Mode. If so, replace the description that gets returned.
            if (item == FPPowerup.STORY_MODE)
                __result = "Mysterious gems that grant the user limitless power.";
        }

        /// <summary>
        /// Unlocks Sonic's game clear badge.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPSaveManager), "GameClearBadgeCheck")]
        private static void UnlockGameClearBadge(ref FPCharacterID ___character)
        {
            // If the player is Sonic, then unlock his game clear badge.
            if (___character == Plugin.sonicCharacterID)
                FP2Lib.Badge.BadgeHandler.UnlockBadge("k24.badge_sonic_clear");
        }

        /// <summary>
        /// Handles unlocking the badge for Weapon's Core as Sonic and adding the unused Story Mode item to the inventory.
        /// Mostly copied from the original code for the True Ending badge and reformatted to my style.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPSaveManager), "BadgeCheck", new Type[] { typeof(int), typeof(int) })]
        private static void UnlockChaosEmeralds()
        {
            // If no time is recorded for Weapon's Core or we're not Sonic, then don't bother with the rest of this code.
            if (FPSaveManager.timeRecord[30] <= 0 || FPSaveManager.character != Plugin.sonicCharacterID)
                return;

            // Set up a value to track the time capsules that have been obtained.
            int timeCapsuleCount = 0;

            // Loop through each time capsule in the save and increment our counter if it's obtained.
            for (int timeCapsuleIndex = 0; timeCapsuleIndex < FPSaveManager.timeCapsules.Length; timeCapsuleIndex++)
                if (FPSaveManager.timeCapsules[timeCapsuleIndex] > 0)
                    timeCapsuleCount++;

            // Check that we have at least 13 time capsules.
            if (timeCapsuleCount >= 13)
            {
                // Unlock the Weapon's Core as Sonic badge.
                FP2Lib.Badge.BadgeHandler.UnlockBadge("k24.badge_sonic_emeralds");

                // Unlock item 31 (the Story Mode item) in the save's inventory.
                FPSaveManager.inventory[31] = 1;
            }
        }

        /// <summary>
        /// Handles unlocking the "Problem Solved. Story Over" achievement.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPSaveManager), "BadgeCheck", new Type[] { typeof(int), typeof(int) })]
        private static void PalaceCourtyardMerga(ref int badgeID)
        {
            if (badgeID == 64 && FPPlayerPatcher.isSuper)
                FP2Lib.Badge.BadgeHandler.UnlockBadge("k24.badge_sonic_palacemerga");
        }
    }
}
