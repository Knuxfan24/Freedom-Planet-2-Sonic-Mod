using System.Linq;

namespace FP2_Sonic_Mod.Patchers
{
    internal class FPHudMasterPatcher
    {
        private static readonly Sprite[] wispSprites = Plugin.sonicAssetBundle.LoadAssetWithSubAssets<Sprite>("hud_wisp");
        private static readonly Sprite chaosEmeraldSprite = Plugin.sonicAssetBundle.LoadAsset<Sprite>("hud_chaosemeralds");

        /// <summary>
        /// Handles displaying the Wisp icon if one is in the player's possession or the Chaos Emeralds if we can become Super.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPHudMaster), "LateUpdate")]
        private static void HUDIcons(FPHudMaster __instance)
        {
            // Handle the generic super icon.
            if (FPPlayerPatcher.player.powerups.Contains((FPPowerup)Plugin.chaosEmeraldID) && FPPlayerPatcher.player.characterID != Plugin.sonicCharacterID)
            {
                if ((!GenericSuper.isSuper && GenericSuper.player.totalCrystals >= 50) || GenericSuper.isSuper)
                {
                    __instance.hudBike[0].GetRenderer().enabled = true;
                    __instance.hudBike[0].GetComponent<SpriteRenderer>().sprite = chaosEmeraldSprite;
                }
            }

            // Only do this if we're Sonic and have a Wisp.
            if (FPPlayerPatcher.player == null)
                return;
            if (FPPlayerPatcher.player.characterID != Plugin.sonicCharacterID)
                return;
            if (FPPlayerPatcher.HasWisp != WispType.NONE)
            {
                // Show Carol's bike display.
                __instance.hudBike[0].GetRenderer().enabled = true;

                // Swap out its sprite depending on the held Wisp.
                switch (FPPlayerPatcher.HasWisp)
                {
                    case WispType.DRILL: __instance.hudBike[0].GetComponent<SpriteRenderer>().sprite = wispSprites[0]; break;
                    case WispType.ROCKET: __instance.hudBike[0].GetComponent<SpriteRenderer>().sprite = wispSprites[2]; break;
                    case WispType.LASER: __instance.hudBike[0].GetComponent<SpriteRenderer>().sprite = wispSprites[1]; break;
                }
            }
            else if ((!FPPlayerPatcher.isSuper && FPPlayerPatcher.player.totalCrystals >= 50 && FPPlayerPatcher.player.powerups.Contains((FPPowerup)Plugin.chaosEmeraldID)) || FPPlayerPatcher.isSuper)
            {
                __instance.hudBike[0].GetRenderer().enabled = true;
                __instance.hudBike[0].GetComponent<SpriteRenderer>().sprite = chaosEmeraldSprite;
            }
            else
            {
                // Hide Carol's bike display.
                __instance.hudBike[0].GetRenderer().enabled = false;
            }
        }
    }
}
