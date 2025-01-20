namespace FP2_Sonic_Mod.Patchers
{
    internal class MenuPhotoPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MenuPhoto), "Start")]
        private static void SwapIfSuper(ref MenuPhotoPose[] ___poseList, ref FPPlayer ___targetPlayer)
        {
            // If we're not Sonic, then don't make the edits.
            if (FPSaveManager.character != Plugin.sonicCharacterID)
                return;

            // Check if the player is Super.
            if (FPPlayerPatcher.isSuper)
            {
                // Get the saved sprite from the post list entry..
                Sprite savedSprite = ___poseList[(int)___targetPlayer.characterID].savedSprite;

                // Replace the pose list entry.
                ___poseList[(int)___targetPlayer.characterID] = Plugin.superPhotoPoses;

                // Add the saved sprite back in.
                ___poseList[(int)___targetPlayer.characterID].savedSprite = savedSprite;
            }
        }
    }
}
