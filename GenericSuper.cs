using System;
using System.Linq;

namespace FP2_Sonic_Mod
{
    internal class GenericSuper
    {
        // Holds a reference to the player's object.
        public static FPPlayer player;

        // Values relating to the Super form.
        private static bool isSuper;
        private static float superTimeCounter;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "Start")]
        private static void Setup(FPPlayer __instance)
        {
            // Reset the Super flag.
            isSuper = false;

            // Get a reference to the player object.
            player = __instance;

            // If we have the Chaos Emeralds equipped, then strip us of any crystals so we can't turn Super immediately after reloading a checkpoint.
            if (player.powerups.Contains((FPPowerup)Plugin.chaosEmeraldID))
                player.totalCrystals = 0;
        }

        /// <summary>
        /// Activates the Super Form when in the guard action.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPPlayer), "Action_Guard")]
        private static void ActivateSuperForm()
        {
            // Check if we have the Chaos Emeralds equipped, have enough crystals and are NOT Sonic.
            if (player.powerups.Contains((FPPowerup)Plugin.chaosEmeraldID) && player.totalCrystals >= 50 && !isSuper && player.characterID != Plugin.sonicCharacterID)
            {
                // Set the Super flag.
                isSuper = true;

                // Shake the camera a bit.
                FPCamera.stageCamera.screenShake = Mathf.Max(FPCamera.stageCamera.screenShake, 10f);

                // Loop through four times.
                for (int sparkIndex = 0; sparkIndex < 4; sparkIndex++)
                {
                    // Create a spark.
                    Spark spark = (Spark)FPStage.CreateStageObject(Spark.classID, player.position.x, player.position.y);
                    spark.velocity.x = Mathf.Cos((float)Math.PI / 180f * ((float)sparkIndex * 90f + 45f)) * 20f;
                    spark.velocity.y = Mathf.Sin((float)Math.PI / 180f * ((float)sparkIndex * 90f + 45f)) * 20f;
                    spark.SetAngle();
                }

                // Play the Boost Breaker sound.
                player.Action_PlaySoundUninterruptable(player.sfxBoostExplosion);

                // Create the Boost Breaker explosion.
                BoostExplosion boostExplosion = (BoostExplosion)FPStage.CreateStageObject(BoostExplosion.classID, player.position.x, player.position.y);
                boostExplosion.attackKnockback.x = player.attackKnockback.x * 0.5f;
                boostExplosion.attackKnockback.y = player.attackKnockback.y * 0.5f;
                boostExplosion.attackEnemyInvTime = player.attackEnemyInvTime;
                boostExplosion.parentObject = player;
                boostExplosion.faction = player.faction;

                // Create the Invincibility stars.
                InvincibilityStar invincibilityStar = (InvincibilityStar)FPStage.CreateStageObject(InvincibilityStar.classID, -100f, -100f);
                invincibilityStar.parentObject = player;
                InvincibilityStar invincibilityStar2 = (InvincibilityStar)FPStage.CreateStageObject(InvincibilityStar.classID, -100f, -100f);
                invincibilityStar2.parentObject = player;
                invincibilityStar2.rotation = 180f;

                // Play the Superstars super music.
                if (FPAudio.GetCurrentMusic() != Plugin.genericSuperMusic)
                    FPAudio.PlayMusic(Plugin.genericSuperMusic);
            }
        }

        /// <summary>
        /// Handle being in the Super state.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "Update")]
        private static void SuperForm()
        {
            // If the player is Sonic, then don't do any of this.
            if (player.characterID == Plugin.sonicCharacterID)
                return;

            // Don't proceed if the player isn't Super or is in the victory animation.
            if (!isSuper || player.state == player.State_Victory)
                return;

            // Increase the player's stats.
            player.topSpeed = player.GetPlayerStat_Default_TopSpeed() * 2f;
            player.acceleration = player.GetPlayerStat_Default_Acceleration() * 2f;
            player.airAceleration = player.GetPlayerStat_Default_AirAceleration() * 2f;
            player.jumpStrength = player.GetPlayerStat_Default_JumpStrength() * 1.2f;
            typeof(FPPlayer).GetField("speedMultiplier", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(player, 1f + (float)(int)player.potions[6] * 0.05f);

            // Reset the player's invincibility time to 200 so it can never expire.
            player.invincibilityTime = 200f;

            // Set the flash timer to 1200 if its reached 0 so the character flashes.
            if (player.flashTime <= 0)
                player.flashTime = 1200f;

            // Reset the player's heat and oxygen levels.
            player.heatLevel = 0f;
            player.oxygenLevel = 1f;

            player.hbAttack.enabled = true;
            player.hbAttack.visible = true;
            player.hbAttack.left = -32;
            player.hbAttack.right = 32;
            player.hbAttack.top = 32;
            player.hbAttack.bottom = -32;

            // Increment the super timer.
            superTimeCounter += Time.deltaTime;

            // Check if the super timer goes above 1.
            while (superTimeCounter >= 1f)
            {
                // Subtract 1 from the timer.
                superTimeCounter -= 1f;

                // Remove a crystal from the player.
                player.totalCrystals--;
            }

            // Check if the player has run out of crystals.
            if (player.totalCrystals <= 0)
            {
                // If the super music is playing, then play the last used audio we stored.
                if (FPAudio.GetCurrentMusic() == Plugin.genericSuperMusic)
                    FPAudio.PlayMusic(Plugin.lastUsedAudio);

                // Remove the invincibility.
                player.invincibilityTime = 0;
                player.flashTime = 0;

                // Reset the player stats.
                player.topSpeed = player.GetPlayerStat_Default_TopSpeed();
                player.acceleration = player.GetPlayerStat_Default_Acceleration();
                player.airAceleration = player.GetPlayerStat_Default_AirAceleration();
                player.jumpStrength = player.GetPlayerStat_Default_JumpStrength();

                // Disable the Super flag.
                isSuper = false;

                // Shake the camera a bit.
                FPCamera.stageCamera.screenShake = Mathf.Max(FPCamera.stageCamera.screenShake, 10f);

                // Loop through four times.
                for (int sparkIndex = 0; sparkIndex < 4; sparkIndex++)
                {
                    // Create a spark.
                    Spark spark = (Spark)FPStage.CreateStageObject(Spark.classID, player.position.x, player.position.y);
                    spark.velocity.x = Mathf.Cos((float)Math.PI / 180f * ((float)sparkIndex * 90f + 45f)) * 20f;
                    spark.velocity.y = Mathf.Sin((float)Math.PI / 180f * ((float)sparkIndex * 90f + 45f)) * 20f;
                    spark.SetAngle();
                }

                // Play the Boost Breaker sound.
                player.Action_PlaySoundUninterruptable(player.sfxBoostExplosion);

                // Create the Boost Breaker explosion.
                BoostExplosion boostExplosion = (BoostExplosion)FPStage.CreateStageObject(BoostExplosion.classID, player.position.x, player.position.y);
                boostExplosion.attackKnockback.x = player.attackKnockback.x * 0.5f;
                boostExplosion.attackKnockback.y = player.attackKnockback.y * 0.5f;
                boostExplosion.attackEnemyInvTime = player.attackEnemyInvTime;
                boostExplosion.parentObject = player;
                boostExplosion.faction = player.faction;
            }
        }
    }
}
