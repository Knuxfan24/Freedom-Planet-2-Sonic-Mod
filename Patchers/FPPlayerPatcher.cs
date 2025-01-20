using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine.SceneManagement;

namespace FP2_Sonic_Mod.Patchers
{
    public class FPPlayerPatcher
    {
        // Holds a reference to the player's object.
        public static FPPlayer player;

        // Holds references to the Apply Force functions, as they're private.
        private static readonly MethodInfo applyGround = typeof(FPPlayer).GetMethod("ApplyGroundForces", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo applyAir = typeof(FPPlayer).GetMethod("ApplyAirForces", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo applyGravity = typeof(FPPlayer).GetMethod("ApplyGravityForce", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo rollAttackStats = typeof(FPPlayer).GetMethod("AttackStats_CarolRoll", BindingFlags.NonPublic | BindingFlags.Instance);

        // A multiplier for how strong the Spin Dash's release should be.
        private static float SpinDashMultiplier = 1f;

        // Values for Super Sonic.
        public static bool isSuper;
        private static float SuperStartTimer;
        private static float superTimeCounter;
        private static bool createdSparks;

        // Values for the Air Dash.
        private static int LTap;
        private static int RTap;
        private static float DashTimer;

        // Chaos Emerald GameObjects for the Super Transformation animation.
        private static Transform BlueChaosEmerald;
        private static Transform CyanChaosEmerald;
        private static Transform GreenChaosEmerald;
        private static Transform GreyChaosEmerald;
        private static Transform PurpleChaosEmerald;
        private static Transform RedChaosEmerald;
        private static Transform YellowChaosEmerald;

        // Values for the Homing Attack.
        public static float HomingAttackFailsafeTimer;
        public static FPBaseEnemy HomingAttackTarget;
        private static Transform HomingAttackCursor;
        private static Transform HomingAttackArrows1;
        private static Transform HomingAttackArrows2;

        // The Stomp's visual effect.
        private static Transform StompEffect;

        // Values for the Rocket Wisp.
        public static bool HasWisp;
        public static Transform RocketWispEffect;
        public static bool UsedRocketWisp;

        // Value to see if the drowning jingle is apparently playing.
        private static bool DrowningJingle;

        /// <summary>
        /// Sets up various elements for the rest of the functions in here.
        /// </summary>
        /// <param name="__instance">The player object that called this start function.</param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "Start")]
        private static void Setup(FPPlayer __instance)
        {
            // Get the player's object.
            player = __instance;

            // Reset the Super Sonic flags.
            isSuper = false;
            SuperStartTimer = 0f;
            createdSparks = false;

            // Reset the Homing Attack flags.
            HomingAttackFailsafeTimer = 0f;
            HomingAttackTarget = null;

            // Reset the Wisp flag.
            HasWisp = false;

            // If the player is Sonic, then handle some other things.
            if (player.characterID == Plugin.sonicCharacterID)
            {
                // Set the clear and results jingles.

                if (SceneManager.GetActiveScene().name != "GreenHill")
                    UnityEngine.Object.FindObjectOfType<FPAudio>().jingleStageComplete = Plugin.sonicClearJingle;
                
                player.bgmResults = Plugin.sonicResultsMusic;

                // Get references to the Chaos Emerald objects.
                BlueChaosEmerald = player.gameObject.transform.GetChild(0);
                CyanChaosEmerald = player.gameObject.transform.GetChild(1);
                GreenChaosEmerald = player.gameObject.transform.GetChild(2);
                GreyChaosEmerald = player.gameObject.transform.GetChild(3);
                PurpleChaosEmerald = player.gameObject.transform.GetChild(4);
                RedChaosEmerald = player.gameObject.transform.GetChild(5);
                YellowChaosEmerald = player.gameObject.transform.GetChild(6);

                // Get references to the Homing Attack cursor objects.
                HomingAttackCursor = GameObject.Instantiate(Plugin.sonicAssetBundle.LoadAsset<GameObject>("homing attack cursor")).transform;
                HomingAttackArrows1 = HomingAttackCursor.gameObject.transform.GetChild(0);
                HomingAttackArrows2 = HomingAttackCursor.gameObject.transform.GetChild(1);

                // Get references to the Stomp's effect.
                StompEffect = player.gameObject.transform.GetChild(7);
                StompEffect.gameObject.SetActive(false);

                // Get references to the Rocket Wisp's effect.
                RocketWispEffect = player.gameObject.transform.GetChild(8);
                RocketWispEffect.gameObject.SetActive(false);

                // Set the voice bank depending on the config option.
                switch (Plugin.sonicVAOption.Value)
                {
                    case 0:
                        player.vaKO = null;
                        player.vaSpecialA = new AudioClip[1];
                        player.vaSpecialB = new AudioClip[1];
                        player.vaHit = new AudioClip[1];
                        player.vaRevive = new AudioClip[1];
                        player.vaItemGet = new AudioClip[1];
                        player.vaClear = new AudioClip[1];
                        player.vaJackpotClear = new AudioClip[1];
                        player.vaLowDamageClear = new AudioClip[1];
                        break;

                    case 1:
                        player.vaKO = Plugin.sonicAssetBundle.LoadAsset<AudioClip>("ko_ryan");
                        player.vaSpecialA = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("hummingtop1_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("hummingtop2_ryan")];
                        player.vaSpecialB = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("vo_super_ryan")];
                        player.vaHit = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("hit1_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("hit2_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("hit3_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("hit4_ryan")];
                        player.vaRevive = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("ko_recover1_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("ko_recover2_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("ko_recover3_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("ko_recover4_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("ko_recover5_ryan")];
                        player.vaItemGet = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("item1_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("item2_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("item3_ryan")];
                        player.vaClear = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory1_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory2_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory3_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory4_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory5_ryan")];
                        player.vaJackpotClear = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory2_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory3_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory4_ryan")];
                        player.vaLowDamageClear = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory2_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory3_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory4_ryan")];

                        if (SceneManager.GetActiveScene().name == "Tutorial1Sonic") player.vaClear = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("tutorialdone_ryan")];
                        break;

                    case 2:
                        if (SceneManager.GetActiveScene().name == "Tutorial1Sonic") player.vaClear = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("tutorialdone")];
                        break;

                    case 3:
                        player.vaKO = Plugin.sonicAssetBundle.LoadAsset<AudioClip>("ko_roger");
                        player.vaSpecialA = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("hummingtop1_roger")];
                        player.vaSpecialB = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("vo_super_roger")];
                        player.vaHit = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("hit1_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("hit2_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("hit3_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("hit4_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("hit5_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("hit6_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("hit7_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("hit8_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("hit9_roger")];
                        player.vaRevive = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("ko_recover1_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("ko_recover2_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("ko_recover3_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("ko_recover4_roger")];
                        player.vaItemGet = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("item1_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("item2_roger")];
                        player.vaClear = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory1_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory2_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory3_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory4_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory5_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory6_roger")];
                        player.vaJackpotClear = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory2_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory3_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory4_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory5_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory6_roger")];
                        player.vaLowDamageClear = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory2_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory3_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory4_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory5_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory6_roger")];

                        if (SceneManager.GetActiveScene().name == "Tutorial1Sonic") player.vaClear = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("tutorialdone_roger")];
                        break;
                }

                // Set the jump sound depending on the config option.
                switch (Plugin.sonicJumpSFXOption.Value)
                {
                    case 0: player.sfxJump = Plugin.sonicAssetBundle.LoadAsset<AudioClip>("jump_classic"); break;
                    case 1: player.sfxJump = Plugin.sonicAssetBundle.LoadAsset<AudioClip>("jump_adventure"); break;
                }

                // Green Hill Zone edits for the SA2-esque Easter Egg.
                if (SceneManager.GetActiveScene().name == "GreenHill")
                {
                    player.sfxJump = Plugin.sonicAssetBundle.LoadAsset<AudioClip>("jump_classic");
                    player.sfxSkid = Plugin.sonicAssetBundle.LoadAsset<AudioClip>("classic_skid");

                    player.vaKO = null;
                    player.vaSpecialA = new AudioClip[1];
                    player.vaSpecialB = new AudioClip[1];
                    player.vaHit = new AudioClip[1];
                    player.vaRevive = new AudioClip[1];
                    player.vaItemGet = new AudioClip[1];
                    player.vaClear = new AudioClip[1];
                    player.vaJackpotClear = new AudioClip[1];
                    player.vaLowDamageClear = new AudioClip[1];

                    player.bgmResults = Plugin.sonicAssetBundle.LoadAsset<AudioClip>("M_ClearSilent");

                    player.extraLifeCost = 100;
                    player.crystals = 100;
                }
            }
        }

        /// <summary>
        /// Makes Sonic jump with the rolling animation instead of the normal one.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "Action_Jump")]
        private static void MakeJumpRoll()
        {
            // If we're in the jumping animation from Action_Jump and the player is Sonic, then swap to the rolling animation instead.
            if (player.currentAnimation == "Jumping" && player.characterID == Plugin.sonicCharacterID)
                player.currentAnimation = "Rolling";
        }

        /// <summary>
        /// Lets Sonic jump again if in a waterfall.
        /// </summary>
        private static void Action_Sonic_WaterfallJump()
        {
            // Check that the player has a water surface.
            if (player.targetWaterSurface != null)
            {
                // Check that the type of water the player is in is specifically a Water Square.
                // Also check that we aren't in a few specific scenes, as they have ones that aren't waterfalls, but still use WaterSquare.
                if (player.targetWaterSurface.GetType() == typeof(FPWaterSquare)
                    && SceneManager.GetActiveScene() is { name: not "GlobeOpera1",
                                                          name: not "AncestralForge",
                                                          name: not "Battlesphere_Boss",
                                                          name: not "Battlesphere_Bossrush_Div1" })
                {
                    // Check if the player is pressing the jump button.
                    if (player.input.jumpPress)
                    {
                        // Play the jump sound.
                        player.Action_PlaySound(player.sfxJump);

                        // Give the player upwards momentum.
                        player.velocity = new(0, 9f);
                        
                        // Reset the player to the rolling animation.
                        player.SetPlayerAnimation("Rolling");
                    }
                }
            }
        }

        /// <summary>
        /// Returns a custom value for Sonic from GetPlayerStat_Default_TopSpeed.
        /// This only needs to be done for GetPlayerStat_Default_TopSpeed, as the rest of Sonic's speed stats match Lilac's, which are the default return values for those functions.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "GetPlayerStat_Default_TopSpeed", new Type[] { typeof(FPCharacterID) })]
        private static void IncreasePlayerTopSpeed(ref FPCharacterID character, ref float __result)
        {
            // If the player is Sonic, then return 10 for this function.
            if (character == Plugin.sonicCharacterID)
                __result = 10f;
        }

        /// <summary>
        /// Increases Sonic's jump height when in water.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "Action_SetWaterFlag", new Type[] { typeof(FPBaseObject), typeof(bool) })]
        private static void BuffWaterJumpHeight(ref FPBaseObject surface)
        {
            /// If the player is Sonic and in water, then set their jump strength up to 12.
            if (player.characterID == Plugin.sonicCharacterID && surface != null && surface.isActiveAndEnabled)
                player.jumpStrength = 12f;
        }

        /// <summary>
        /// Stops Sonic from jumping when crouching so he can Spin Dash.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPPlayer), "Action_Jump")]
        private static bool StopCrouchJump()
        {
            // If the player is Sonic and is crouching, then stop the original function from running.
            if (player.state == new FPObjectState(player.State_Crouching) && player.characterID == Plugin.sonicCharacterID)
                return false;

            // Otherwise, run the original function as normal.
            return true;
        }

        /// <summary>
        /// Creates the smoke for the Spin Dash.
        /// </summary>
        /// <param name="player">The player object creating the smoke.</param>
        /// <param name="xOffset">How far offset on the X axis this smoke should be.</param>
        /// <param name="scaleModifier">The scale modifier for this smoke.</param>
        private static void CreateSpinDashSmoke(float xOffset, float scaleModifier)
        {
            // Create a dust object and set up the static values.
            Dust dust = (Dust)FPStage.CreateStageObject(Dust.classID, player.position.x, player.position.y);
            dust.SetParentObject(player);
            dust.yOffset = -32f;

            // Set the scale of the dust.
            dust.scale = new Vector3(scaleModifier, scaleModifier, scaleModifier);

            // Set the velocity and x offset of the dust depending on the player's direction.
            if (player.direction == FPDirection.FACING_RIGHT)
            {
                dust.velocity = new Vector2(-0.2f, 0f);
                dust.xOffset = -xOffset;
            }
            else
            {
                dust.velocity = new Vector2(0.2f, 0f);
                dust.xOffset = xOffset;
            }
        }

        /// <summary>
        /// Sonic's moves when in the air state.
        /// </summary>
        public static void Action_Sonic_AirMoves()
        {
            Action_Sonic_WaterfallJump();

            #region Double Jump
            // Check that the player presses jump, has a lower y velocity than their jump strength, hasn't already used the double jump and in the rolling animation.
            if (player.input.jumpPress && player.velocity.y < player.jumpStrength && !player.jumpAbilityFlag && player.currentAnimation == "Rolling")
            {
                // Set the jump ability flag.
                player.jumpAbilityFlag = true;

                // Set the player's y velocity.
                player.velocity.y = Mathf.Max(player.jumpStrength * (float)typeof(FPPlayer).GetField("jumpMultiplier", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(player), player.velocity.y);

                // Set the player to the Jumping animation to make Sonic uncurl.
                player.SetPlayerAnimation("Jumping");

                // Play the double jump sound.
                player.Action_PlaySoundUninterruptable(player.sfxDoubleJump);
            }
            #endregion

            #region Humming Top
            // Check that the player presses attack, is pressing left or right and is in the spring animation.
            if (player.input.attackPress && (player.input.left || player.input.right) && player.currentAnimation == "Spring")
            {
                // Set the player to the Cyclone animation.
                player.SetPlayerAnimation("Cyclone");

                // Reset the player's velocity, as the start up lag of this move in Advance 2 (which I'm not replicating), does reset Sonic's movement.
                player.velocity = Vector2.zero;

                // Set the player's x velocity depending on direction.
                if (player.input.left) player.velocity.x = -20f;
                if (player.input.right) player.velocity.x = 20f;

                // Play the cyclone sound.
                player.Action_PlaySound(player.sfxCyclone);

                // Check if the player's voice timer is less than or equal to 0.
                if (player.voiceTimer <= 0f)
                {
                    // Set the player's voice timer up to 900.
                    player.voiceTimer = 900f;

                    // Play a sound from the SpecialA voice array.
                    player.Action_PlayVoiceArray("SpecialA");
                }
            }
            #endregion

            #region Hop Jump
            // Check that the player presses attack, is pressing up and is in the spring animation.
            if (player.input.attackPress && player.input.up && player.currentAnimation == "Spring")
            {
                // Set the player to the HopStart animation.
                player.SetPlayerAnimation("HopStart");

                // Reset the player's velocity, as the start up lag of this move in Advance 2 (which I'm not replicating), does reset Sonic's movement.
                player.velocity = Vector2.zero;

                // Set the player's y velocity to 12.
                player.velocity.y = 12f;

                // Play the double jump sound.
                player.Action_PlaySound(player.sfxDoubleJump);
            }
            #endregion

            #region Air Dash
            // Check that the player isn't rolling but is in the rolling animation.
            if (player.state != new FPObjectState(State_Sonic_Roll) && player.currentAnimation == "Rolling")
            {
                // Check the player pressed right.
                if (player.input.rightPress)
                {
                    // Reset the timer and left taps.
                    DashTimer = 0f;
                    LTap = 0;

                    // Increment our right taps.
                    RTap++;

                    // Check if right has been tapped at least twice.
                    if (RTap >= 2)
                    {
                        // Get the player's y velocity. If it's higher than 0, then reduce it to 20%.
                        float upwardsVelocity = player.velocity.y;
                        if (upwardsVelocity > 0f)
                            upwardsVelocity *= 0.2f;

                        // Set the player to the AirDash animation.
                        player.SetPlayerAnimation("Airdash");

                        // Set the player's velocity, adding 5 to their x and replacing y with our calculated value.
                        if (!isSuper) player.velocity = new(player.velocity.x + 5f, upwardsVelocity);
                        if (isSuper) player.velocity = new(player.velocity.x + 15f, upwardsVelocity);

                        // Reset the right taps.
                        RTap = 0;

                        // Play the double jump sound.
                        player.Action_PlaySoundUninterruptable(player.sfxDoubleJump);
                    }
                }

                // Check the player pressed left.
                if (player.input.leftPress)
                {
                    // Reset the timer and right taps.
                    DashTimer = 0f;
                    RTap = 0;

                    // Increment our left taps.
                    LTap++;

                    // Check if left has been tapped at least twice.
                    if (LTap >= 2)
                    {
                        // Get the player's y velocity. If it's higher than 0, then reduce it to 20%.
                        float upwardsVelocity = player.velocity.y;
                        if (upwardsVelocity > 0f)
                            upwardsVelocity *= 0.2f;

                        // Set the player to the AirDash animation.
                        player.SetPlayerAnimation("Airdash");

                        // Set the player's velocity, removing 5 from their x and replacing y with our calculated value.
                        if (!isSuper) player.velocity = new(player.velocity.x - 5f, upwardsVelocity);
                        if (isSuper) player.velocity = new(player.velocity.x - 15f, upwardsVelocity);

                        // Reset the left taps.
                        LTap = 0;

                        // Play the double jump sound.
                        player.Action_PlaySoundUninterruptable(player.sfxDoubleJump);
                    }
                }
            }
            #endregion

            #region Super Sonic
            // Check that the player has the Story Mode item equipped, has at least 50 crystal shards, has pressed the special button, isn't rolling but is in the rolling animation and isn't already super.
            if (!HasWisp && player.powerups.Contains(FPPowerup.STORY_MODE) && player.totalCrystals >= 50 && player.input.guardPress && player.state != new FPObjectState(State_Sonic_Roll) && player.currentAnimation == "Rolling" && !isSuper)
            {
                // Set the player to the Super Transformation state.
                player.state = State_Sonic_SuperTransform;

                // Kill the player's velocity.
                player.velocity = Vector2.zero;

                // Set the player to the SuperStart animation.
                player.SetPlayerAnimation("SuperStart");

                // Set the player's invincibility time high so that Sonic can't be knocked out of the Super Transformation state.
                player.invincibilityTime = 9999;
            }

            if (!HasWisp && player.input.guardPress && player.state != new FPObjectState(State_Sonic_Roll) && player.currentAnimation == "Rolling" && isSuper)
            {
                // Reset the player's animator to normal Sonic's.
                player.animator.runtimeAnimatorController = Plugin.sonicAssetBundle.LoadAsset<RuntimeAnimatorController>("Sonic Animator");

                // Reset the player's jump strength.
                player.jumpStrength = player.GetPlayerStat_Default_JumpStrength();

                // If the super music is playing, then play the last used audio we stored.
                if (FPAudio.GetCurrentMusic() == Plugin.sonicSuperMusic)
                    FPAudio.PlayMusic(Plugin.lastUsedAudio);

                // Reset the player's velocity.
                player.velocity = Vector2.zero;

                // Set the player's state to the Super Detransformation one.
                player.state = State_Sonic_SuperDetransform;
            }
            #endregion

            #region Homing Attack
            UpdateHomingAttackTargetedEnemies();

            if (HomingAttackTarget != null && player.input.attackPress && player.currentAnimation != "GuardAir" && player.currentAnimation != "Cyclone" && player.currentAnimation != "Spring" && player.currentAnimation != "HopStart" && player.currentAnimation != "HopLoop" && player.currentAnimation != "Airdash")
            {
                HomingAttackFailsafeTimer = 0f;
                player.velocity = Vector2.zero;
                player.state = State_Sonic_HomingAttack;
                player.SetPlayerAnimation("Rolling");
                player.Action_PlaySoundUninterruptable(player.sfxBigBoostLaunch);
            }
            #endregion

            #region Stomp
            // Check if the player has pressed the special button.
            if (player.input.specialPress)
            {
                // Give the player a set downwards velocity.
                player.velocity = new(0, -12);

                // Set the player to the stomp state.
                player.state = State_Sonic_Stomp;

                // Play the stomp animation.
                player.SetPlayerAnimation("Stomp");

                // Play the stomp sound.
                player.Action_PlaySound(player.sfxDivekick1);

                // Make the stomp effect visible.
                StompEffect.gameObject.SetActive(true);

                // Reset the failsafe timer.
                player.genericTimer = 0f;
            }
            #endregion

            #region Rocket Wisp
            // Check if we have a wisp, have pressed the guard button and have a full energy gauge.
            if (HasWisp && player.input.guardPress && player.energy == 100)
            {
                // Set the flag for the Gravity Bubble achievement.
                UsedRocketWisp = true;

                // Set the player into the Rocket Wisp Start state.
                player.state = State_Sonic_RocketWispStart;

                // Play the announcer call for the Rocket Wisp.
                player.Action_PlaySound(Plugin.sonicAssetBundle.LoadAsset<AudioClip>("vo_rocket_wisp"));

                // Reset the HasWisp flag.
                HasWisp = false;

                // Set the player animation to the UseWisp one.
                player.SetPlayerAnimation("UseWisp");

                // Reset our generic timer.
                player.genericTimer = 0f;
            }
            #endregion
        }

        /// <summary>
        /// Logic for the Homing Attack.
        /// </summary>
        private static void State_Sonic_HomingAttack()
        {
            // Get the state of the target (if it has one).
            string targetState = null;
            if (HomingAttackTarget.state != null)
                targetState = HomingAttackTarget.state.Method.Name;

            // Increment the failsafe timer.
            HomingAttackFailsafeTimer += FPStage.deltaTime;

            // Check if the failsafe timer has reached 60.
            if (HomingAttackFailsafeTimer >= 60f)
            {
                // Set the player to the InAir state.
                player.state = player.State_InAir;

                // Kill the player's velocity.
                player.velocity = Vector2.zero;

                // Uncurl the player.
                player.SetPlayerAnimation("Jumping");
            }

            // Move the player towards their Homing Attack target.
            player.position = Vector2.MoveTowards(player.position, new(HomingAttackTarget.transform.position.x + (float)Math.Round((HomingAttackTarget.hbWeakpoint.right + HomingAttackTarget.hbWeakpoint.left) / 2), HomingAttackTarget.transform.position.y + (float)Math.Round((HomingAttackTarget.hbWeakpoint.top + HomingAttackTarget.hbWeakpoint.bottom) / 2)), 20f * FPStage.deltaTime);

            // Check if the player hasn't collided with a solid surface, that their position now matches the Homing Attack target's or if the target is in its Death state.
            if (player.position == new Vector2(HomingAttackTarget.transform.position.x + (float)Math.Round((HomingAttackTarget.hbWeakpoint.right + HomingAttackTarget.hbWeakpoint.left) / 2), HomingAttackTarget.transform.position.y + (float)Math.Round((HomingAttackTarget.hbWeakpoint.top + HomingAttackTarget.hbWeakpoint.bottom) / 2)) || targetState == "State_Death" || player.colliderWall != null || player.colliderRoof != null || player.colliderGround != null)
            {
                // Set the player to the InAir state.
                player.state = player.State_InAir;

                // Give the player some upward's velocity.
                player.velocity = new(0, 9f);

                // Set the player to their Guard Air animation to act as a Homing flip.
                player.SetPlayerAnimation("GuardAir");
            }
        }

        /// <summary>
        /// Logic for the Stomp.
        /// </summary>
        private static void State_Sonic_Stomp()
        {
            // Increment the failsafe timer.
            player.genericTimer += FPStage.deltaTime;

            // Apply gravity to the player.
            applyGravity.Invoke(player, new object[] { });

            // Set the player's attack stats to Carol's roll.
            player.attackStats = (FPObjectState)rollAttackStats.Invoke(player, new object[] { });

            // Check if the player is on the ground at this point.
            if (player.onGround)
            {
                // Set the player to the Ground state.
                player.state = player.State_Ground;

                // Stop the stomp sound if its still playing.
                player.Action_StopSound();

                // Play the stomp landing sound.
                player.Action_PlaySound(player.sfxDivekick2);

                // Create the two dust particles.
                Dust dust = (Dust)FPStage.CreateStageObject(Dust.classID, player.position.x, player.position.y);
                dust.SetParentObject(player);
                dust.yOffset = -32f;
                dust.velocity = new Vector2(-1f, 0f);
                Dust dust2 = (Dust)FPStage.CreateStageObject(Dust.classID, player.position.x, player.position.y);
                dust2.SetParentObject(player);
                dust2.yOffset = -32f;
                dust2.velocity = new Vector2(1f, 0f);

                // Return so we don't bother with the failsafe check.
                return;
            }

            // Check if the player's velocity has become positive.
            // Additionally, check if we're in Clockwork Arboretum and have gone a certain distance down (so we don't infinitely fall in the Cory fight).
            // Also also, check if the generic timer has gone above 300 (which should be roughly 5 seconds).
            if (player.velocity.y >= 0 || (SceneManager.GetActiveScene().name == "Bakunawa2" && player.position.y <= -28088) || player.genericTimer > 300f)
            {
                // Set the player to the InAir state.
                player.state = player.State_InAir;

                // Set the player to the Spring animation.
                player.SetPlayerAnimation("Spring");
            }
        }

        /// <summary>
        /// State for transforming into Super Sonic.
        /// </summary>
        private static void State_Sonic_SuperTransform()
        {
            // Increment our super timer by the stage's delta time.
            SuperStartTimer += FPStage.deltaTime;

            // Check if the timer has gone above 17.
            if (SuperStartTimer >= 17)
            {
                // Hide the Chaos Emeralds.
                BlueChaosEmerald.GetComponent<SpriteRenderer>().enabled = false;
                CyanChaosEmerald.GetComponent<SpriteRenderer>().enabled = false;
                GreenChaosEmerald.GetComponent<SpriteRenderer>().enabled = false;
                GreyChaosEmerald.GetComponent<SpriteRenderer>().enabled = false;
                PurpleChaosEmerald.GetComponent<SpriteRenderer>().enabled = false;
                RedChaosEmerald.GetComponent<SpriteRenderer>().enabled = false;
                YellowChaosEmerald.GetComponent<SpriteRenderer>().enabled = false;

                // Reset the Chaos Emerald's positions.
                BlueChaosEmerald.localPosition = new(-144, -180, 0);
                CyanChaosEmerald.localPosition = new(144, -180, 0);
                GreenChaosEmerald.localPosition = new(320, -54, 0);
                GreyChaosEmerald.localPosition = new(252, 107, 0);
                PurpleChaosEmerald.localPosition = new(0, 180, 0);
                RedChaosEmerald.localPosition = new(-252, 107, 0);
                YellowChaosEmerald.localPosition = new(-320, -54, 0);

                // If the super music isn't playing, then play it.
                if (FPAudio.GetCurrentMusic() != Plugin.sonicSuperMusic)
                    FPAudio.PlayMusic(Plugin.sonicSuperMusic);

                // Check if we haven't already created the things from Lilac's Boost Breaker.
                if (!createdSparks)
                {
                    // Play the Super Transformation sound.
                    player.Action_PlaySoundUninterruptable(player.sfxCarolAttack2);

                    // Play a sound from the SpecialB voice array.
                    player.Action_PlayVoiceArray("SpecialB");

                    // Set the created sparks flag.
                    createdSparks = true;

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

            // If the timer hasn't yet hit 17.
            else
            {
                // Make the Chaos Emeralds visible.
                BlueChaosEmerald.GetComponent<SpriteRenderer>().enabled = true;
                CyanChaosEmerald.GetComponent<SpriteRenderer>().enabled = true;
                GreenChaosEmerald.GetComponent<SpriteRenderer>().enabled = true;
                GreyChaosEmerald.GetComponent<SpriteRenderer>().enabled = true;
                PurpleChaosEmerald.GetComponent<SpriteRenderer>().enabled = true;
                RedChaosEmerald.GetComponent<SpriteRenderer>().enabled = true;
                YellowChaosEmerald.GetComponent<SpriteRenderer>().enabled = true;

                // Move the Chaos Emeralds towards Sonic's position.
                BlueChaosEmerald.transform.position = Vector2.MoveTowards(BlueChaosEmerald.transform.position, player.transform.position, 20f * FPStage.deltaTime);
                CyanChaosEmerald.transform.position = Vector2.MoveTowards(CyanChaosEmerald.transform.position, player.transform.position, 20f * FPStage.deltaTime);
                GreenChaosEmerald.transform.position = Vector2.MoveTowards(GreenChaosEmerald.transform.position, player.transform.position, 20f * FPStage.deltaTime);
                GreyChaosEmerald.transform.position = Vector2.MoveTowards(GreyChaosEmerald.transform.position, player.transform.position, 20f * FPStage.deltaTime);
                PurpleChaosEmerald.transform.position = Vector2.MoveTowards(PurpleChaosEmerald.transform.position, player.transform.position, 20f * FPStage.deltaTime);
                RedChaosEmerald.transform.position = Vector2.MoveTowards(RedChaosEmerald.transform.position, player.transform.position, 20f * FPStage.deltaTime);
                YellowChaosEmerald.transform.position = Vector2.MoveTowards(YellowChaosEmerald.transform.position, player.transform.position, 20f * FPStage.deltaTime);
            }

            // Check if the start timer has gone above 65.
            if (SuperStartTimer >= 65)
            {
                // Set the isSuper flag.
                isSuper = true;

                // Reset the super time counters.
                superTimeCounter = 0f;
                SuperStartTimer = 0;

                // Change the player's animator to Super Sonic's.
                player.animator.runtimeAnimatorController = Plugin.sonicAssetBundle.LoadAsset<RuntimeAnimatorController>("Super Sonic Animator");

                // Set the player to the InAir state.
                player.state = player.State_InAir;

                // Set the player to the Jumping animation, specifically the looping part.
                player.SetPlayerAnimation("Jumping_Loop");
            }
        }

        /// <summary>
        /// State for transforming back into regular Sonic.
        /// </summary>
        private static void State_Sonic_SuperDetransform()
        {
            // Check if the start timer is 0.
            if (SuperStartTimer == 0)
            {
                // Play the Super Detransformation sound.
                player.Action_PlaySoundUninterruptable(player.sfxCarolAttack3);

                // Set the player to the Jumping animation, specifically the looping part.
                player.SetPlayerAnimation("Jumping_Loop");

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

                // Show the Chaos Emeralds.
                BlueChaosEmerald.GetComponent<SpriteRenderer>().enabled = true;
                CyanChaosEmerald.GetComponent<SpriteRenderer>().enabled = true;
                GreenChaosEmerald.GetComponent<SpriteRenderer>().enabled = true;
                GreyChaosEmerald.GetComponent<SpriteRenderer>().enabled = true;
                PurpleChaosEmerald.GetComponent<SpriteRenderer>().enabled = true;
                RedChaosEmerald.GetComponent<SpriteRenderer>().enabled = true;
                YellowChaosEmerald.GetComponent<SpriteRenderer>().enabled = true;

                // Set the Chaos Emerald's positions to the player's position.
                BlueChaosEmerald.transform.position = player.position;
                CyanChaosEmerald.transform.position = player.position;
                GreenChaosEmerald.transform.position = player.position;
                GreyChaosEmerald.transform.position = player.position;
                PurpleChaosEmerald.transform.position = player.position;
                RedChaosEmerald.transform.position = player.position;
                YellowChaosEmerald.transform.position = player.position;
            }

            // Increment our super timer by the stage's delta time.
            SuperStartTimer += FPStage.deltaTime;

            // Move the Chaos Emeralds.
            BlueChaosEmerald.transform.position = new(BlueChaosEmerald.transform.position.x - (15f * FPStage.deltaTime), BlueChaosEmerald.transform.position.y - (22.5f * FPStage.deltaTime));
            CyanChaosEmerald.transform.position = new(CyanChaosEmerald.transform.position.x + (15f * FPStage.deltaTime), CyanChaosEmerald.transform.position.y - (22.5f * FPStage.deltaTime));
            GreenChaosEmerald.transform.position = new(GreenChaosEmerald.transform.position.x + (30f * FPStage.deltaTime), GreenChaosEmerald.transform.position.y - (7.5f * FPStage.deltaTime));
            GreyChaosEmerald.transform.position = new(GreyChaosEmerald.transform.position.x + (27.5f * FPStage.deltaTime), GreyChaosEmerald.transform.position.y + (12.5f * FPStage.deltaTime));
            PurpleChaosEmerald.transform.position = new(PurpleChaosEmerald.transform.position.x, PurpleChaosEmerald.transform.position.y + (20f * FPStage.deltaTime));
            RedChaosEmerald.transform.position = new(RedChaosEmerald.transform.position.x - (27.5f * FPStage.deltaTime), RedChaosEmerald.transform.position.y + (12.5f * FPStage.deltaTime));
            YellowChaosEmerald.transform.position = new(YellowChaosEmerald.transform.position.x - (30f * FPStage.deltaTime), YellowChaosEmerald.transform.position.y - (7.5f * FPStage.deltaTime));

            // Check if the timer has reached 60.
            if (SuperStartTimer >= 60)
            {
                // Reset the super start flags.
                isSuper = false;
                createdSparks = false;

                // Remove the player's invincibility.
                player.invincibilityTime = 0f;

                // Set the player to the InAir state.
                player.state = player.State_InAir;

                // Reset the player's top speed and jump strength.
                player.topSpeed = player.GetPlayerStat_Default_TopSpeed();
                player.jumpStrength = player.GetPlayerStat_Default_JumpStrength();

                // Reset the super start timer.
                SuperStartTimer = 0;

                // Hide the Chaos Emeralds.
                BlueChaosEmerald.GetComponent<SpriteRenderer>().enabled = false;
                CyanChaosEmerald.GetComponent<SpriteRenderer>().enabled = false;
                GreenChaosEmerald.GetComponent<SpriteRenderer>().enabled = false;
                GreyChaosEmerald.GetComponent<SpriteRenderer>().enabled = false;
                PurpleChaosEmerald.GetComponent<SpriteRenderer>().enabled = false;
                RedChaosEmerald.GetComponent<SpriteRenderer>().enabled = false;
                YellowChaosEmerald.GetComponent<SpriteRenderer>().enabled = false;

                // Reset the Chaos Emerald's positions.
                BlueChaosEmerald.localPosition = new(-144, -180, 0);
                CyanChaosEmerald.localPosition = new(144, -180, 0);
                GreenChaosEmerald.localPosition = new(320, -54, 0);
                GreyChaosEmerald.localPosition = new(252, 107, 0);
                PurpleChaosEmerald.localPosition = new(0, 180, 0);
                RedChaosEmerald.localPosition = new(-252, 107, 0);
                YellowChaosEmerald.localPosition = new(-320, -54, 0);
            }
        }

        /// <summary>
        /// Logic for when the player is Super Sonic.
        /// Most of the stat resetting for when Sonic detransforms is handled in LateUpdate by the PowerSneakers function instead.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "Update")]
        private static void SuperSonic()
        {
            // If the player isn't Sonic, then don't do any of this.
            if (player.characterID != Plugin.sonicCharacterID)
                return;

            // Don't proceed if the player isn't Super or is in the victory animation.
            if (!isSuper || player.state == player.State_Victory || player.state == State_Sonic_SuperDetransform || player.state == State_Sonic_RocketWisp || player.state == State_Sonic_RocketWispStart)
                return;

            // Increase the player's stats.
            player.topSpeed = player.GetPlayerStat_Default_TopSpeed() * 2f;
            player.acceleration = player.GetPlayerStat_Default_Acceleration() * 2f;
            player.airAceleration = player.GetPlayerStat_Default_AirAceleration() * 2f;
            player.jumpStrength = player.GetPlayerStat_Default_JumpStrength() * 1.2f;
            typeof(FPPlayer).GetField("speedMultiplier", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(player, 1f + (float)(int)player.potions[6] * 0.05f);

            // Reset the player's invincibility time to 200 so it can never expire.
            player.invincibilityTime = 200f;

            // Reset the player's heat and oxygen levels.
            player.heatLevel = 0f;
            player.oxygenLevel = 1f;

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
                // Reset the player's animator to normal Sonic's.
                player.animator.runtimeAnimatorController = Plugin.sonicAssetBundle.LoadAsset<RuntimeAnimatorController>("Sonic Animator");

                // If the super music is playing, then play the last used audio we stored.
                if (FPAudio.GetCurrentMusic() == Plugin.sonicSuperMusic)
                    FPAudio.PlayMusic(Plugin.lastUsedAudio);

                // Reset the player's velocity.
                player.velocity = Vector2.zero;

                // Set the player's state to the Super Detransformation one.
                player.state = State_Sonic_SuperDetransform;
            }
        }

        /// <summary>
        /// Sonic's moves when in the ground state.
        /// </summary>
        public static void Action_Sonic_GroundMoves()
        {
            #region Spin Dash
            // Check that the player is holding down, has pressed jump and are in the crouching state.
            if (player.input.down && player.input.jumpPress && player.state == new FPObjectState(player.State_Crouching))
            {
                // Set the player to the SpindashCharge animation.
                player.SetPlayerAnimation("SpindashCharge");

                // Reset the generic timer.
                player.genericTimer = 0f;

                // Set the player's state to Sonic's Spin Dash state.
                player.state = State_Sonic_SpinDash;

                // Play the Spin Dash charge sound.
                player.Action_PlaySound(player.sfxBoostCharge);

                // Create the smoke particles.
                CreateSpinDashSmoke(24, 1);
                CreateSpinDashSmoke(32, 1.25f);
                CreateSpinDashSmoke(48, 1.5f);
                CreateSpinDashSmoke(64, 1.75f);

                // If the player is Super, then set the multiplier straight up to 2.
                if (isSuper)
                    SpinDashMultiplier = 2f;
            }
            #endregion

            #region Rolling
            // Check that the player holding down, isn't crouching or rolling and has at least 3 ground velocity.
            if (player.input.down && player.state != new FPObjectState(player.State_Crouching) && player.state != new FPObjectState(State_Sonic_Roll) && Mathf.Abs(player.groundVel) > 3f)
            {
                // Reset the generic timer.
                player.genericTimer = 0f;

                // Set the player's state to Sonic's rolling state.
                player.state = State_Sonic_Roll;

                // Play the rolling sound effect.
                player.Action_PlaySoundUninterruptable(player.sfxRolling);
            }
            #endregion

            #region Rocket Wisp
            // Check if we have a wisp, have pressed the guard button and have a full energy gauge.
            if (HasWisp && player.input.guardPress && player.energy == 100)
            {
                // Set the player into the Rocket Wisp Start state.
                player.state = State_Sonic_RocketWispStart;

                // Play the announcer call for the Rocket Wisp.
                player.Action_PlaySound(Plugin.sonicAssetBundle.LoadAsset<AudioClip>("vo_rocket_wisp"));

                // Reset the HasWisp flag.
                HasWisp = false;

                // Set the player animation to the UseWisp one.
                player.SetPlayerAnimation("UseWisp");

                // Reset our generic timer.
                player.genericTimer = 0f;
            }
            #endregion
        }

        /// <summary>
        /// Logic for the beginning of the Rocket Wisp's activation.
        /// </summary>
        private static void State_Sonic_RocketWispStart()
        {
            // Set the player's invincibility timer to something absurdly high.
            player.invincibilityTime = 9999;

            // Kill the player's velocity.
            player.velocity = Vector2.zero;
            player.groundVel = 0;

            // Reset the player's angles.
            player.angle = 0;
            player.groundAngle = 0;

            // Reset the player's oxygen level, as the Rocket Wisp in Sonic Colours does this.
            player.oxygenLevel = 1;

            // Increment our generic timer.
            player.genericTimer += FPStage.deltaTime;

            // Check if our generic timer has gone above 65.
            if (player.genericTimer >= 65)
            {
                // Set the player to the Rocket Wisp animation.
                player.SetPlayerAnimation("RocketWisp");

                // Play the Rocket Wisp jingle.
                FPAudio.PlayJingle(Plugin.sonicRocketJingle);

                // Set the player to the Rocket Wisp state.
                player.state = State_Sonic_RocketWisp;

                // Make the Rocket Wisp effect visible.
                RocketWispEffect.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Logic for the Rocket Wisp.
        /// </summary>
        private static void State_Sonic_RocketWisp()
        {
            // Set the player's invincibility timer to something absurdly high.
            player.invincibilityTime = 9999;

            // Give the player upwards velocity.
            player.velocity = new(0, 9f);

            // Unset the onGround flag.
            player.onGround = false;

            // Reset the player's oxygen level, as the Rocket Wisp in Sonic Colours does this.
            player.oxygenLevel = 1;

            // Reset the Wisp flag.
            HasWisp = false;

            // Check if we've run out of energy.
            if (player.energy <= 0)
            {
                // Remove the player's invincibility.
                player.invincibilityTime = 0;

                // Set the player to the InAir state.
                player.state = player.State_InAir;

                // Set the player to the GuardAir animation for the flip.
                player.SetPlayerAnimation("GuardAir");

                // Stop the Rocket Wisp jingle if it's still playing.
                FPAudio.StopJingle();

                // Hide the Rocket Wisp effect.
                RocketWispEffect.gameObject.SetActive(false);

                // Don't run the rest of this function.
                return;
            }

            // Reduce the player's energy bar.
            player.energy -= 0.8f;

            // If the player is colliding with a roof, then double the drain rate.
            if (player.colliderRoof != null)
                player.energy -= 0.8f;
        }

        /// <summary>
        /// Logic for collecting the Power Up item (AKA Power Sneakers).
        /// </summary>
        public static void Action_Sonic_Fuel()
        {
            // Stop any playing jingles then play our Power Sneakers jingle.
            FPAudio.StopJingle();
            FPAudio.PlayJingle(Plugin.sonicSpeedUpJingle);

            // Set our power up timer to 900 (roughly 15 seconds).
            player.powerupTimer = 900f;
        }

        /// <summary>
        /// Logic for being under the effect of the Power Sneakers.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "LateUpdate")]
        private static void PowerSneakers()
        {
            // If the player isn't Sonic, then don't do any of this.
            if (player.characterID != Plugin.sonicCharacterID)
                return;

            // If the player is under the effects of the Power Sneakers, then run Neera's function for them.
            if (player.powerupTimer > 0f)
                player.Action_SpeedShoes();

            // If not (and they're not Super Sonic), then reset the player's values.
            else if (!isSuper)
            {
                player.acceleration = player.GetPlayerStat_Default_Acceleration();
                player.deceleration = player.GetPlayerStat_Default_Deceleration();
                player.airAceleration = player.GetPlayerStat_Default_AirAceleration();
                typeof(FPPlayer).GetField("speedMultiplier", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(player, 1f + (float)(int)player.potions[6] * 0.05f);
            }
        }

        /// <summary>
        /// Logic for Sonic's Spin Dash charging state.
        /// </summary>
        public static void State_Sonic_SpinDash()
        {
            // Increment the generic timer.
            player.genericTimer += FPStage.deltaTime;

            // If the player has any horizontal momentum, then halt it.
            if (player.velocity.x != 0)
                player.velocity = new(0, player.velocity.y);
            player.groundVel = 0;

            // If the player isn't on the ground, then apply gravity to them and stop the rest of the function from running.
            if (!player.onGround)
            {
                applyGravity.Invoke(player, new object[] { });
                return;
            }

            // Check that the player is pressing down and jump.
            if (player.input.down && player.input.jumpPress)
            {
                // Create the smoke particles.
                CreateSpinDashSmoke(24, 1);
                CreateSpinDashSmoke(32, 1.25f);
                CreateSpinDashSmoke(48, 1.5f);
                CreateSpinDashSmoke(64, 1.75f);

                // Play the Spin Dash charge sound again.
                player.Action_PlaySound(player.sfxBoostCharge);

                // Reset the Spin Dash animation.
                player.SetPlayerAnimation("SpindashCharge", 0, 0, true);

                // Increment the Spin Dash's multiplier up to a maximum of 2 (the check is 1.9, but that's so the decaying force can't push it over.
                if (SpinDashMultiplier < 1.9f)
                    SpinDashMultiplier += 0.2f;

                // Reset the generic timer.
                player.genericTimer = 0f;
            }

            // Check if the generic timer has reached 15.
            if (player.genericTimer >= 15)
            {
                // Reset the generic timer.
                player.genericTimer = 0f;

                // Decay a bit of force from the Spin Dash.
                if (SpinDashMultiplier > 1)
                    SpinDashMultiplier -= 0.1f;
            }

            // Check if the player has released down.
            if (!player.input.down)
            {
                // Set the player's ground velocity based on direction.
                if (player.direction == FPDirection.FACING_LEFT)
                    player.groundVel = Mathf.Min(Mathf.Min(player.groundVel, 0f) * 0.5f - 15f, player.groundVel) * SpinDashMultiplier;
                else
                    player.groundVel = Mathf.Max(Mathf.Max(player.groundVel, 0f) * 0.5f + 15f, player.groundVel) * SpinDashMultiplier;

                // Reset the generic timer.
                player.genericTimer = 0f;

                // Set the player's state to Sonic's rolling state.
                player.state = State_Sonic_Roll;

                // Stop the Spin Dash charge sound.
                player.Action_StopSound();

                // Play the Spin Dash release sound.
                player.Action_PlaySoundUninterruptable(player.sfxBoostLaunch);

                // Reset the Spin Dash's multiplier.
                SpinDashMultiplier = 1f;
            }
        }

        /// <summary>
        /// Logic for Sonic's rollling state. Copied and modified from Carol's rolling state.
        /// </summary>
        public static void State_Sonic_Roll()
        {
            // Increment the generic timer.
            player.genericTimer += FPStage.deltaTime;

            // Set the player to the rolling animation.
            player.SetPlayerAnimation("Rolling");

            // Check if the player is on the ground.
            if (player.onGround)
            {
                // Set the animator's speed based on the player's ground velocity.
                player.animator.SetSpeed(Mathf.Abs(player.groundVel) * 0.15f);

                // Run the Ground Moves action.
                Action_Sonic_GroundMoves();

                // Check if the player presses jump.
                if (player.input.jumpPress)
                {
                    // Perform the soft jump action.
                    player.Action_SoftJump();

                    // Set the animator speed to 2.
                    player.animator.SetSpeed(2f);
                }

                // Check if the player hasn't pressed jump.
                else
                {
                    // If the generic timer has gone above 15, apply ground forces onto the player.
                    if (player.genericTimer > 15f)
                        applyGround.Invoke(player, new object[] { false });
                    
                    // Set the player's angle to their ground angle.
                    player.angle = player.groundAngle;
                }
            }

            // Check if the player is in the air.
            else
            {
                // Apply the air and gravity forces onto the player.
                applyAir.Invoke(player, new object[] { false });
                applyGravity.Invoke(player, new object[] { });

                // Check if the player isn't holding jump and has just released it.
                if (!player.input.jumpHold && player.jumpReleaseFlag)
                {
                    // Reset the jump release flag.
                    player.jumpReleaseFlag = false;

                    // Cap the player's y velocity.
                    if (player.velocity.y > player.jumpRelease)
                        player.velocity.y = player.jumpRelease;
                }
            }

            // If the player is on the ground below a certain speed, set them back to the normal ground state.
            if (player.onGround && Mathf.Abs(player.groundVel) <= 1.5f)
                player.state = player.State_Ground;
                
            // If the player isn't on the ground, then set them to the in air state.
            if (!player.onGround)
                player.state = player.State_InAir;
        }

        /// <summary>
        /// Handles the timer to reset the Air Dash's double tap.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPPlayer), "Update")]
        private static void AirDashTimer()
        {
            // If the player isn't Sonic, then don't do any of this.
            if (player.characterID != Plugin.sonicCharacterID)
                return;

            // Increment the dash timer.
            DashTimer += Time.deltaTime;

            // Check if the dash timer goes above 0.25.
            while (DashTimer >= 0.25f)
            {
                // Subtract 0.25 from the timer.
                DashTimer -= 0.25f;

                // Reset the right and left tap values.
                LTap = 0;
                RTap = 0;
            }
        }

        /// <summary>
        /// Increases the damage of the roll, as its Sonic's only real attack.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "AttackStats_CarolRoll")]
        private static void BuffRollDamage()
        {
            // If the player isn't Sonic, then don't perform these edits.
            if (player.characterID != Plugin.sonicCharacterID)
                return;

            // If the player ISN'T in the Homing Attack state, then set the roll's attack power to 4.
            if (player.state != State_Sonic_HomingAttack)
                player.attackPower = 4f;

            // If they are, then set it to 8.
            else
                player.attackPower = 8f;
        }

        /// <summary>
        /// Gets all the enemies in range of the Homing Attack.
        /// </summary>
        private static List<FPBaseEnemy> GetEnemyListInHomingAttackRange()
        {
            // Create a list of enemies.
            List<FPBaseEnemy> enemyList = [];

            // Loop through every active enemy in the stage.
            foreach (FPBaseEnemy fpbaseEnemy in FPStage.GetActiveEnemies(false, false))
            {
                // Check if the enemy has health, and active weakpoint, can be targeted, is of a different faction to the player and that the squared length of the player's position minus the enemy's is less than or equal to 65536.
                if (fpbaseEnemy.health > 0f && fpbaseEnemy.hbWeakpoint.enabled && fpbaseEnemy.CanBeTargeted() && fpbaseEnemy.faction != player.faction && Vector2.SqrMagnitude(player.position - fpbaseEnemy.position) <= 65536)
                {
                    // If the enemy's position is valid (depending on the player's direction), then add them to the list.
                    if (player.direction == FPDirection.FACING_RIGHT && fpbaseEnemy.position.x + 8 > player.position.x) enemyList.Add(fpbaseEnemy);
                    if (player.direction == FPDirection.FACING_LEFT && fpbaseEnemy.position.x - 8 < player.position.x) enemyList.Add(fpbaseEnemy);
                }
            }

            // Return our list of enemies.
            return enemyList;
        }

        /// <summary>
        /// Compares two potential Homing Attack targets.
        /// TODO: This code comes from the BFF2000 missiles, how does it work?
        /// </summary>
        private static int CompareHomingAttackTargets(FPBaseEnemy enemy1, FPBaseEnemy enemy2)
        {
            if (ReferenceEquals(enemy1, enemy2))
                return 0;

            if (enemy1 == null)
                return 1;

            if (enemy2 == null)
                return -1;

            float num = Vector2.SqrMagnitude(player.position - enemy1.position);
            float num2 = Vector2.SqrMagnitude(player.position - enemy2.position);

            if (num < num2)
                return -1;

            if (num > num2)
                return 1;

            if (enemy1.stageListPos < enemy2.stageListPos)
                return -1;

            if (enemy1.stageListPos > enemy2.stageListPos)
                return 1;

            return 0;
        }

        /// <summary>
        /// Updates the list of Homing Attackable enemies.
        /// TODO: This code comes from the BFF2000 missiles, how does it work?
        /// </summary>
        private static void UpdateHomingAttackTargetedEnemies()
        {
            List<FPBaseEnemy> enemyListInHARange = GetEnemyListInHomingAttackRange();

            enemyListInHARange.Sort(new Comparison<FPBaseEnemy>(CompareHomingAttackTargets));

            int i = 0;
            while (i < enemyListInHARange.Count - 1)
            {
                if (ReferenceEquals(enemyListInHARange[i], enemyListInHARange[i + 1]))
                    enemyListInHARange.RemoveAt(i + 1);
                else
                    i++;
            }

            if (enemyListInHARange.Count == 0)
            {
                HomingAttackTarget = null;
                return;
            }

            if (enemyListInHARange[0] != null)
                    HomingAttackTarget = enemyListInHARange[i];
        }
        
        /// <summary>
        /// Handles playing/stopping the Drowning jingle.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPPlayer), "Update")]
        private static void HandleDrowningJingle()
        {
            // If the player isn't Sonic, then don't do any of this.
            if (player.characterID != Plugin.sonicCharacterID)
                return;

            // Check if the player has more than a quarter the oxygen gauge and that we're apparently playing the Drowning jingle.
            if (player.oxygenLevel > 0.25 && DrowningJingle)
            {
                // Stop the jingle.
                FPAudio.StopJingle();

                // Reset our drowning jingle flag.
                DrowningJingle = false;
            }

            // Check if the player has less than a quarter the oxygen gauge and that we're not playing the Drowning jingle.
            if (player.oxygenLevel <= 0.25 && !DrowningJingle)
            {
                // Play the jingle.
                FPAudio.PlayJingle(Plugin.sonicDrowningJingle);

                // Set our drowning jingle flag.
                DrowningJingle = true;
            }
        }

        /// <summary>
        /// Logic for the Homing Attack's cursor.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPPlayer), "Update")]
        private static void UpdateHomingAttackCursor()
        {
            // If the player isn't Sonic, then don't do any of this.
            if (player.characterID != Plugin.sonicCharacterID)
                return;

            // If the player isn't in the air, then disable the cursor.
            if (player.state != player.State_InAir)
                ControlHomingAttackCursor(false);

            // Check if the player isn't in the air.
            else
            {
                // Check that the player isn't in an animation that they're not allowed to Homing Attack in.
                if (player.currentAnimation != "GuardAir" && player.currentAnimation != "Cyclone" && player.currentAnimation != "Spring" && player.currentAnimation != "HopStart" && player.currentAnimation != "HopLoop" && player.currentAnimation != "Airdash")
                {
                    // If there's actually a Homing Attack target, then make the cursor visible.
                    if (HomingAttackTarget != null)
                        ControlHomingAttackCursor(true);

                    // If not, then hide the cursor.
                    else
                        ControlHomingAttackCursor(false);
                }

                // If the player is in an invalid animation, then hide the cursor.
                else
                    ControlHomingAttackCursor(false);
            }

            // Rotate the two arrows.
            HomingAttackArrows1.Rotate(new Vector3(0, 0, FPStage.deltaTime * 4));
            HomingAttackArrows2.Rotate(new Vector3(0, 0, -FPStage.deltaTime * 4));

            // Internal function for controling the cursor's visiblity.
            static void ControlHomingAttackCursor(bool visible)
            {
                // Check if we're making the cursor visible.
                if (visible)
                {
                    // Update the cursor's position.
                    HomingAttackCursor.transform.position = new(HomingAttackTarget.transform.position.x + (float)Math.Round((HomingAttackTarget.hbWeakpoint.right + HomingAttackTarget.hbWeakpoint.left) / 2), HomingAttackTarget.transform.position.y + (float)Math.Round((HomingAttackTarget.hbWeakpoint.top + HomingAttackTarget.hbWeakpoint.bottom) / 2), 0);
                    
                    // If we're making the cursor visible and it isn't already, then play the sound effect for it.
                    if (HomingAttackCursor.GetComponent<SpriteRenderer>().enabled == false)
                        player.Action_PlaySoundUninterruptable(player.sfxCarolAttack1);
                }

                // Make the three parts of the cursor visible or invisible.
                HomingAttackCursor.GetComponent<SpriteRenderer>().enabled = visible;
                HomingAttackArrows1.GetComponent<SpriteRenderer>().enabled = visible;
                HomingAttackArrows2.GetComponent<SpriteRenderer>().enabled = visible;
            }
        }

        /// <summary>
        /// Handles hiding Stomp's visual effect.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPPlayer), "Update")]
        private static void HandleStompEffect()
        {
            // If the player isn't Sonic, then don't do any of this.
            if (player.characterID != Plugin.sonicCharacterID)
                return;

            // If we're not in the Stomp state, then hide the effect.
            if (player.state != State_Sonic_Stomp)
                StompEffect.gameObject.SetActive(false);
        }

        /// <summary>
        /// Replaces the Airship in Bakunawa Chase with the Tornado.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerShip), "Start")]
        private static void ModShip(PlayerShip __instance)
        {
            // If the player isn't Sonic, then don't do any of this.
            if (player.characterID != Plugin.sonicCharacterID)
                return;

            // Change the Airship's animator to the Tornado's.
            __instance.GetComponent<Animator>().runtimeAnimatorController = Plugin.sonicAssetBundle.LoadAsset<RuntimeAnimatorController>("Tornado Animator");

            // Shift the origin point for the bullets a bit.
            __instance.gameObject.transform.GetChild(0).gameObject.transform.localPosition = new(136, 16, 0);

            // Scale down the sprite.
            __instance.gameObject.transform.GetChild(2).gameObject.transform.localScale = Vector3.one;

            // Hide the booster and tiny characters.
            __instance.gameObject.transform.GetChild(3).gameObject.SetActive(false);
            __instance.gameObject.transform.GetChild(4).gameObject.SetActive(false);
        }

        // Transpiles to remove the swimming state.
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(FPPlayer), nameof(FPPlayer.State_InAir))]
        static IEnumerable<CodeInstruction> RemoveSwimState_InAir(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            // Nop out the check that puts the player into the swimming state.
            for (int i = 104; i < 122; i++)
                codes[i].opcode = OpCodes.Nop;

            return codes.AsEnumerable();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(FPPlayer), nameof(FPPlayer.ReturnToGeneralState), new Type[] { typeof(bool), typeof(bool) })]
        static IEnumerable<CodeInstruction> RemoveSwimState_ReturnToGeneralState(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            
            codes[34] = codes[28]; // Swaps out the call to the swimming state to the InAir state instead.

            return codes.AsEnumerable();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(FPPlayer), nameof(FPPlayer.Action_Jump))]
        static IEnumerable<CodeInstruction> RemoveSwimState_ActionJump(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            codes[129] = codes[162]; // Swaps out the call to the swimming state to the InAir state instead.
            codes[137] = codes[149]; // Swaps out the call to the swimming animation to the jumping one instead.

            return codes.AsEnumerable();
        }

        /// <summary>
        /// Restores the swap to the Swim State from State_InAir if our character isn't Sonic and has the Swimming animation.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPPlayer), nameof(FPPlayer.State_InAir))]
        private static bool RestoreSwimState_InAir()
        {
            // Only do this if the player isn't Sonic.
            if (player.characterID != Plugin.sonicCharacterID)
            {
                // Loop through each animation in this player's animation controller.
                foreach (AnimationClip animation in player.animator.runtimeAnimatorController.animationClips)
                {
                    // Check if this animation is the Swimming one or that the player is a base game character.
                    if (animation.name == "Swimming" || player.characterID <= (FPCharacterID)4)
                    {
                        // Check if the player is not on the ground, in the Spring animation, is in water and has at most -2 on their Y velocity.
                        if (!player.onGround && player.currentAnimation != "Spring" && player.targetWaterSurface != null && player.velocity.y < -2f)
                        {
                            // Set the player to their Swimming state.
                            player.state = player.State_Swimming;

                            // Stop the rest of the InAir state function from running.
                            return false;
                        }
                    }
                }
            }

            // If we've reached here, then the player is either Sonic or doesn't have a Swimming animation, so run the original (transpiled) code.
            return true;
        }

        /// <summary>
        /// Restores the swap to the Swim State from ReturnToGeneralState if our character isn't Sonic and has the Swimming animation.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPPlayer), nameof(FPPlayer.ReturnToGeneralState), new Type[] { typeof(bool), typeof(bool) })]
        private static bool RestoreSwimReturnToGeneralState(ref bool overrideOnGroundStatus, ref bool onGroundValue)
        {
            // Only do this if the player isn't Sonic.
            if (player.characterID != Plugin.sonicCharacterID)
            {
                // Loop through each animation in this player's animation controller.
                foreach (AnimationClip animation in player.animator.runtimeAnimatorController.animationClips)
                {
                    // Check if this animation is the Swimming one or that the player is a base game character.
                    if (animation.name == "Swimming" || player.characterID <= (FPCharacterID)4)
                    {
                        // Check the original values and that we're in water.
                        if ((overrideOnGroundStatus || !player.onGround) && (!overrideOnGroundStatus || !onGroundValue) && player.targetWaterSurface != null)
                        {
                            // Set the player to their Swimming state.
                            player.state = player.State_Swimming;

                            // Stop the rest of the ReturnToGeneralState state function from running.
                            return false;
                        }
                    }
                }
            }

            // If we've reached here, then the player is either Sonic or doesn't have a Swimming animation, so run the original (transpiled) code.
            return true;
        }

        /// <summary>
        /// Restores the swap to the Swim State from Action_Jump if our character isn't Sonic and has the Swimming animation.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), nameof(FPPlayer.Action_Jump))]
        private static void RestoreSwimAction_Jump()
        {
            // Only do this if the player isn't Sonic.
            if (player.characterID != Plugin.sonicCharacterID)
            {
                // Loop through each animation in this player's animation controller.
                foreach (AnimationClip animation in player.animator.runtimeAnimatorController.animationClips)
                {
                    // Check if this animation is the Swimming one or that the player is a base game character.
                    if (animation.name == "Swimming" || player.characterID <= (FPCharacterID)4)
                    {
                        // Check if the player is in water.
                        if (player.targetWaterSurface != null)
                        {
                            // Set the player to their Swimming state.
                            player.state = player.State_Swimming;

                            // Set the player's animation depending on if they're Carol on her bike or not.
                            if (player.characterID != FPCharacterID.BIKECAROL)
                                player.SetPlayerAnimation("Swimming");
                            else
                                player.SetPlayerAnimation("Jumping");
                        }

                        // Return so we don't pointlessly loop through the remaining animations.
                        return;
                    }
                }
            }
        }
    }
}
