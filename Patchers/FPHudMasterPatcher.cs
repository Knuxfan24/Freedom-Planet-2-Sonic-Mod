using System.Linq;
using UnityEngine.SceneManagement;

namespace FP2_Sonic_Mod.Patchers
{
    internal class FPHudMasterPatcher
    {
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
            string text4 = "-";

            if (player.velocity.y < player.jumpStrength && !player.jumpAbilityFlag && player.currentAnimation == "Rolling" && player.state == new FPObjectState(player.State_InAir))
                text1 = "Double Jump";

            if (player.state == new FPObjectState(player.State_InAir) && player.currentAnimation != "GuardAir" && FPPlayerPatcher.HomingAttackTarget != null && FPPlayerPatcher.HomingAttackFailsafeTimer != 0)
                text2 = "Homing Attack";

            if ((player.input.left || player.input.right) && player.state != new FPObjectState(FPPlayerPatcher.State_Sonic_Roll) && player.currentAnimation == "Spring")
                text2 = "Humming Top";

            if (player.input.up && player.state != new FPObjectState(FPPlayerPatcher.State_Sonic_Roll) && player.currentAnimation == "Spring")
                text2 = "Hop Jump";

            if (player.state == new FPObjectState(player.State_InAir))
                text3 = "Stomp";

            if (!FPPlayerPatcher.HasWisp && player.powerups.Contains(FPPowerup.STORY_MODE) && player.totalCrystals >= 50 && player.state != new FPObjectState(FPPlayerPatcher.State_Sonic_Roll) && player.currentAnimation == "Rolling" && !FPPlayerPatcher.isSuper && player.state == new FPObjectState(player.State_InAir))
                text4 = "<w><c=energy>Super Sonic</c></w>";
            if (!FPPlayerPatcher.HasWisp && player.state != new FPObjectState(FPPlayerPatcher.State_Sonic_Roll) && player.currentAnimation == "Rolling" && FPPlayerPatcher.isSuper && player.state == new FPObjectState(player.State_InAir))
                text4 = "<c=energy>Detransform</c>";
            if (FPPlayerPatcher.HasWisp)
                text4 = "<c=energy>Rocket Wisp</c>";

            if (player.input.down && (player.state == new FPObjectState(player.State_Crouching) || player.state == new FPObjectState(FPPlayerPatcher.State_Sonic_SpinDash)))
                text1 = "Spin Dash";

            if (player.IsKOd(false))
            {
                text1 = "-";
                text2 = "-";
                text3 = "-";
                text4 = "-";
            }

            // Set the text on the HUD Guide.
            ___hudGuide.text = text1 + "\n" + text2 + "\n" + text3 + "\n" + text4 + "\n ";
        }
    }
}
