using System.Linq;
using UnityEngine.SceneManagement;

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

        // Probably possible to refactor the stuff in FPPlayerPatcher to change the displayMove variables directly, but eh.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPHudMaster), "GuideUpdate")]
        private static void SonicGuide(ref FPPlayer player, ref SuperTextMesh ___hudGuide)
        {
            // Don't continue if we haven't found the player.
            if (player == null)
                return;

            // Don't continue if the player isn't Sonic.
            if (player.characterID != Plugin.sonicCharacterID)
                return;

            // If the player is in control of the BFF2000 or in Bakunawa Chase, then don't continue.
            if (player.displayMoveAttack is "Spark Shot" or "<w>Missiles</w>" or "-" || SceneManager.GetActiveScene().name == "Bakunawa_Chase")
                return;

            string text1 = "Jump";
            string text2 = "-";
            string text3 = "-";
            string text4 = "Guard";

            if (player.onGround && player.state != new FPObjectState(FPPlayerPatcher.State_Sonic_SweepKick) && player.state != new FPObjectState(FPPlayerPatcher.State_Sonic_Roll) && Mathf.Abs(player.groundVel) >= 4f)
                text3 = "Anti-Gravity";

            if (player.onGround && player.state != new FPObjectState(FPPlayerPatcher.State_Sonic_SweepKick) && !player.input.up && player.groundVel < 10 && player.groundVel > -10)
                text2 = "Sweep Kick";
            if (player.onGround && player.state != new FPObjectState(FPPlayerPatcher.State_Sonic_SweepKick) && !player.input.up && (player.groundVel >= 10 || player.groundVel <= -10))
                text2 = "Windmill";

            if (player.velocity.y < player.jumpStrength && !player.jumpAbilityFlag && player.currentAnimation == "Rolling" && player.state == new FPObjectState(player.State_InAir))
                text1 = "Double Jump";

            if (player.state == new FPObjectState(player.State_InAir) && player.currentAnimation != "GuardAir" && FPPlayerPatcher.HomingAttackTarget != null && FPPlayerPatcher.HomingAttackFailsafeTimer != 0)
                text2 = "Homing Attack";

            if ((player.input.left || player.input.right) && player.state != new FPObjectState(FPPlayerPatcher.State_Sonic_Roll) && player.currentAnimation == "Spring")
                text2 = "Humming Top";

            if (player.input.up && player.state != new FPObjectState(FPPlayerPatcher.State_Sonic_Roll) && player.currentAnimation == "Spring")
                text2 = "Hop Jump";

            if (player.input.up && player.currentAnimation != "Spring" && player.currentAnimation != "HopStart" && player.currentAnimation != "Cyclone" && player.state != new FPObjectState(FPPlayerPatcher.State_Sonic_UpKick))
                text2 = "Sonic Updraft";

            if (player.input.down && player.currentAnimation != "Spring" && player.currentAnimation != "HopStart" && player.currentAnimation != "Cyclone" && player.state != new FPObjectState(FPPlayerPatcher.State_Sonic_UpKick))
                text2 = "Sonic Rocket";

            if (player.state == new FPObjectState(player.State_InAir))
                text3 = "Stomp";

            if (player.state == new FPObjectState(FPPlayerPatcher.State_Sonic_Roll) || player.state == new FPObjectState(FPPlayerPatcher.State_Sonic_SpinDash) || FPPlayerPatcher.isSuper)
                text4 = "-";

            if (FPPlayerPatcher.HasWisp == WispType.NONE && player.powerups.Contains((FPPowerup)Plugin.chaosEmeraldID) && player.totalCrystals >= 50 && player.state != new FPObjectState(FPPlayerPatcher.State_Sonic_Roll) && player.currentAnimation == "Rolling" && !FPPlayerPatcher.isSuper && player.state == new FPObjectState(player.State_InAir))
                text4 = "<w><c=energy>Super Sonic</c></w>";
            if (FPPlayerPatcher.HasWisp == WispType.NONE && player.state != new FPObjectState(FPPlayerPatcher.State_Sonic_Roll) && player.currentAnimation == "Rolling" && FPPlayerPatcher.isSuper && player.state == new FPObjectState(player.State_InAir))
                text4 = "<c=energy>Detransform</c>";
            if (FPPlayerPatcher.HasWisp == WispType.ROCKET)
                text4 = "<c=energy>Rocket Wisp</c>";
            if (FPPlayerPatcher.HasWisp == WispType.DRILL)
                text4 = "<c=energy>Drill Wisp</c>";

            if (player.input.down && (player.state == new FPObjectState(player.State_Crouching) || player.state == new FPObjectState(FPPlayerPatcher.State_Sonic_SpinDash)))
                text1 = "Spin Dash";

            if (player.IsKOd(false))
            {
                text1 = "-";
                text2 = "-";
                text3 = "-";
                text4 = "-";
            }

            // Debug read out of Sonic's attack stats.
            //text1 = $"Power: {player.attackPower}";
            //text2 = $"Hitstun: {player.attackHitstun}";
            //text3 = $"EnemyInvTime: {player.attackEnemyInvTime}";
            //text4 = "-";

            // Set the text on the HUD Guide.
            ___hudGuide.text = text1 + "\n" + text2 + "\n" + text3 + "\n" + text4 + "\n ";
        }
    }
}
