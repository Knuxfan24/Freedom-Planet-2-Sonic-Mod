using System;

namespace FP2_Sonic_Mod.Patchers
{
    internal class FPResultsMenuPatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPResultsMenu), "Update")]
        private static void GravityBubbleNoRocket()
        {
            // Check if we're in Gravity Bubble and the Rocket Wisp hasn't been used as Sonic. If so, unlock the achievement.
            if (FPStage.currentStage.stageID == 23 && !FPPlayerPatcher.UsedRocketWisp && FPSaveManager.character == Plugin.sonicCharacterID)
                FP2Lib.Badge.BadgeHandler.UnlockBadge("k24.badge_sonic_nowisp");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPResultsMenu), "Update")]
        private static void ParTimeBadgeCheck(ref float ___badgeCheckTimer, ref FPHudMaster ___stageHud)
        {
            // Check that the badge chec timer is less than 60 and that the current stage doesn't disable badges.
            if (___badgeCheckTimer < 60f && !FPStage.currentStage.disableBadgeChecks)
            {
                // Increment the badge check timer by the stage's delta time.
                ___badgeCheckTimer += FPStage.deltaTime;

                // Cheeck if the badge check timer is above 60 and that the player is Sonic.
                if (___badgeCheckTimer >= 60f && ___stageHud.targetPlayer.characterID == Plugin.sonicCharacterID)
                {
                    // Check for a single par time.
                    ParTime();

                    // Check for a half par time.
                    ParTime(true);

                    // Check for every par time.
                    AllPars();
                }
            }
        }

        private static void ParTime(bool halfPar = false)
        {
            // Set up a flag to see if the badge should be unlocked.
            bool shouldUnlock = false;

            // Get the stage's clear time in milliseconds.
            int stageClearTime = FPStage.currentStage.milliSeconds + FPStage.currentStage.seconds * 100 + FPStage.currentStage.minutes * 6000;

            // Check that the stage clear time is above 0, but less than the stage's par time.
            if (stageClearTime > 0 && stageClearTime < FPSaveManager.GetStageParTime(FPStage.currentStage.stageID))
            {
                // If we're not checking for a half par time, then set our unlock flag.
                if (!halfPar)
                    shouldUnlock = true;

                // If we are checking for a half par time, then compare the stage's clear time to the par time, divided by 2 and set the flag if this passes.
                else if (stageClearTime < FPSaveManager.GetStageParTime(FPStage.currentStage.stageID) / 2)
                    shouldUnlock = true;
            }

            // Check if we set the should unlock flag.
            if (shouldUnlock)
            {
                // If we're not checking for a half par time, then unlock the par time badge.
                if (!halfPar)
                    FP2Lib.Badge.BadgeHandler.UnlockBadge("k24.badge_sonic_partime");

                // If we are checking for a half par time, then unlock the half par time badge.
                else
                    FP2Lib.Badge.BadgeHandler.UnlockBadge("k24.badge_sonic_halfpartime");
            }
        }

        private static void AllPars()
        {
            // Set up a value to track how many par times have been beaten.
            int stageParTimes = 0;

            // If Weapon's Core has a time recorded or is unlocked (I assume that's what story flag 47 is) then subtract 1 from our value to make it required.
            if (FPSaveManager.timeRecord[30] > 0 || FPSaveManager.storyFlag[47] > 0)
                stageParTimes--;

            // Loop through each value in the save's time records (up to and including index 32) and check if a time is recorded for it below the par time. If so, increment our value.
            for (int stageRecordIndex = 1; stageRecordIndex < FPSaveManager.timeRecord.Length && stageRecordIndex <= 32; stageRecordIndex++)
                if (stageRecordIndex != 31 && FPSaveManager.timeRecord[stageRecordIndex] > 0 && FPSaveManager.timeRecord[stageRecordIndex] < FPSaveManager.GetStageParTime(stageRecordIndex))
                    stageParTimes++;

            // If every stage has a record below the par time, then unlock Sonic's all par time badge.
            if (stageParTimes >= 30)
                FP2Lib.Badge.BadgeHandler.UnlockBadge("k24.badge_sonic_allpartime");
        }
    }
}
