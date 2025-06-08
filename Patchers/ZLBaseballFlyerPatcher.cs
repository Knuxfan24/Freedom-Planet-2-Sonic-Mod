using FP2Lib.Player;

namespace FP2_Sonic_Mod.Patchers
{
    internal class ZLBaseballFlyerPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ZLBaseballFlyer), "Update")]
        private static void SwapSpriteIfSuper()
        {
            // If we're not Sonic, then don't make the edits.
            if (FPSaveManager.character != Plugin.sonicCharacterID)
                return;

            // Swap to the Super Sonic sprites if the player is super.
            if (FPPlayerPatcher.isSuper)
                PlayerHandler.currentCharacter.zaoBaseballSprite = Plugin.superZLBall;
            else
                PlayerHandler.currentCharacter.zaoBaseballSprite = Plugin.sonicZLBall;
        }
    }
}
