using FP2Lib.Player;

namespace FP2_Sonic_Mod.Patchers
{
    internal class ZLBaseballFlyerPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ZLBaseballFlyer), "Update")]
        private static void SwapSpriteIfSuper()
        {
            // Only continue if the current character ID is Sonic's.
            if (PlayerHandler.currentCharacter.uid != "k24.sonic")
                return;

            // Swap to the Super Sonic sprites if the player is super.
            if (FPPlayerPatcher.isSuper)
                PlayerHandler.currentCharacter.zaoBaseballSprite = Plugin.superZLBall;
            else
                PlayerHandler.currentCharacter.zaoBaseballSprite = Plugin.sonicZLBall;
        }
    }
}
