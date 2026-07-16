using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public static FPBaseObject HomingAttackTarget;
        private static Transform HomingAttackCursor;
        private static Transform HomingAttackArrows1;
        private static Transform HomingAttackArrows2;

        // The Stomp's visual effect.
        private static Transform StompEffect;

        // Values for the Rocket Wisp.
        public static WispType HasWisp;
        public static Transform RocketWispEffect;
        public static bool UsedWisp;
        public static FPObjectState LastWispState;
        private static Transform LaserWispEffect;

        // Value to see if the drowning jingle is apparently playing.
        private static bool DrowningJingle;

        // Value to see if we're charing the Drop Dash or not.
        private static bool DropDashCharge;

        // DEBUG: Value to make Sonic's attacks one shot everything.
        private static bool DebugOHKO;

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

            // If we have the Chaos Emeralds equipped, then strip us of any crystals so we can't turn Super immediately after reloading a checkpoint.
            if (player.powerups.Contains((FPPowerup)Plugin.chaosEmeraldID))
                player.totalCrystals = 0;

            // Reset the Homing Attack flags.
            HomingAttackFailsafeTimer = 0f;
            HomingAttackTarget = null;

            // Reset the Wisp flag.
            HasWisp = WispType.NONE;

            // Set up Sonic's special assets.
            if (player.characterID == Plugin.sonicCharacterID)
                SonicAssetSetup();
        }

        /// <summary>
        /// Creates the various objects used by Sonic.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPPlayer), "Update")]
        private static void SonicAssetSetupFailsafe()
        {
            // If the Homing Attack cursor hasn't been created for whatever reason, then go and redo most of Sonic's setup.
            if (HomingAttackCursor == null && player.characterID == Plugin.sonicCharacterID)
                SonicAssetSetup();
        }
        private static void SonicAssetSetup()
        {
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

                // Get references to the Laser Wisp's effect.
                LaserWispEffect = player.gameObject.transform.GetChild(9);
                LaserWispEffect.gameObject.SetActive(false);

                // Set the voice bank depending on the config option.
                switch (Plugin.sonicVAOption.Value)
                {
                    case 0:
                        player.vaKO = null;
                        player.vaAttack = new AudioClip[1];
                        player.vaHardAttack = new AudioClip[1];
                        player.vaSpecialA = new AudioClip[1];
                        player.vaSpecialB = new AudioClip[1];
                        player.vaHit = new AudioClip[1];
                        player.vaRevive = new AudioClip[1];
                        player.vaStart = new AudioClip[1];
                        player.vaItemGet = new AudioClip[1];
                        player.vaClear = new AudioClip[1];
                        player.vaJackpotClear = new AudioClip[1];
                        player.vaLowDamageClear = new AudioClip[1];
                        break;

                    case 1:
                        player.vaKO = Plugin.sonicAssetBundle.LoadAsset<AudioClip>("ko_ryan");
                        player.vaAttack = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("homingattack1_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("homingattack2_ryan")];
                        player.vaHardAttack = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("slide2_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("homingattack2_ryan")];
                        player.vaSpecialA = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("hummingtop1_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("hummingtop2_ryan")];
                        player.vaSpecialB = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("vo_super_ryan")];
                        player.vaHit = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("hit1_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("hit2_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("hit3_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("hit4_ryan")];
                        player.vaRevive = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("ko_recover1_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("ko_recover2_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("ko_recover3_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("ko_recover4_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("ko_recover5_ryan")];
                        player.vaStart = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("slide1_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("slide2_ryan")];
                        player.vaItemGet = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("item1_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("item2_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("item3_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("item4_ryan")];
                        player.vaClear = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory1_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory2_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory3_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory4_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory5_ryan")];
                        player.vaJackpotClear = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory2_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory3_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory4_ryan")];
                        player.vaLowDamageClear = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory2_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory3_ryan"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory4_ryan")];

                        if (SceneManager.GetActiveScene().name == "GreenHillTutorial")
                        {
                            player.vaClear = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("tutorialdone_ryan")];
                            player.vaJackpotClear = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("tutorialdone_ryan")];
                            player.vaLowDamageClear = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("tutorialdone_ryan")];
                        }

                        break;

                    case 2:
                        if (SceneManager.GetActiveScene().name == "GreenHillTutorial")
                        {
                            player.vaClear = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("tutorialdone")];
                            player.vaJackpotClear = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("tutorialdone")];
                            player.vaLowDamageClear = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("tutorialdone")];
                        }

                        break;

                    case 3:
                        player.vaKO = Plugin.sonicAssetBundle.LoadAsset<AudioClip>("ko_roger");
                        player.vaAttack = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("homingattack1_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("homingattack2_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("homingattack3_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("homingattack4_roger")];
                        player.vaHardAttack = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("sweepkick1_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("sweepkick2_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("sweepkick3_roger")];
                        player.vaSpecialA = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("hummingtop1_roger")];
                        player.vaSpecialB = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("vo_super_roger")];
                        player.vaHit = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("hit1_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("hit2_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("hit3_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("hit4_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("hit5_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("hit6_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("hit7_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("hit8_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("hit9_roger")];
                        player.vaStart = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("homingattack2_roger")];
                        player.vaRevive = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("ko_recover1_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("ko_recover2_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("ko_recover3_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("ko_recover4_roger")];
                        player.vaItemGet = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("item1_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("item2_roger")];
                        player.vaClear = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory1_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory2_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory3_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory4_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory5_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory6_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory7_roger")];
                        player.vaJackpotClear = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory2_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory3_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory4_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory5_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory6_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory7_roger")];
                        player.vaLowDamageClear = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory2_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory3_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory4_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory5_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory6_roger"), Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory7_roger")];

                        if (SceneManager.GetActiveScene().name == "GreenHillTutorial")
                        {
                            player.vaClear = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("tutorialdone_roger")];
                            player.vaJackpotClear = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("tutorialdone_roger")];
                            player.vaLowDamageClear = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("tutorialdone_roger")];
                        }

                        // Change the arrays for Phoenix Highway and Zao Land specifically.
                        if (SceneManager.GetActiveScene().name == "PhoenixHighway")
                        {
                            player.vaClear = new AudioClip[1];
                            player.vaJackpotClear = new AudioClip[1];
                            player.vaLowDamageClear = new AudioClip[1];
                        }
                        if (SceneManager.GetActiveScene().name == "ZaoLand")
                        {
                            player.vaClear = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory_roger_zaoland")];
                            player.vaJackpotClear = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory_roger_zaoland")];
                            player.vaLowDamageClear = [Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory_roger_zaoland")];
                        }

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
                    player.vaAttack = new AudioClip[1];
                    player.vaHardAttack = new AudioClip[1];
                    player.vaSpecialA = new AudioClip[1];
                    player.vaSpecialB = new AudioClip[1];
                    player.vaHit = new AudioClip[1];
                    player.vaRevive = new AudioClip[1];
                    player.vaStart = new AudioClip[1];
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
                
        #region Air States/Actions
        /// <summary>
        /// Sonic's moves when in the air state.
        /// </summary>
        public static void Action_Sonic_AirMoves()
        {
            Action_Sonic_WaterfallJump();

            #region Drop Dash
            // If we're charging the Drop Dash but let go of the Jump button, then stop charging it.
            if (!player.input.jumpHold && DropDashCharge)
            {
                DropDashCharge = false;
                player.SetPlayerAnimation("Jumping_Loop");
                player.audioChannel[1].Stop();
            }

            // Swap to the rolling animation if we're still holding the jump button and have finished the Drop Dash charge one.
            if (player.input.jumpHold && player.currentAnimation == "DropDash" && player.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f)
            {
                player.SetPlayerAnimation("Rolling");
                DropDashCharge = true;
            }

            // Start charging the Drop Dash if we've pressed Jump after a double jump or are holding down.
            if ((player.jumpAbilityFlag || player.input.down) && player.input.jumpPress)
            {
                player.jumpAbilityFlag = true;
                player.Action_PlaySound(Plugin.sonicAssetBundle.LoadAsset<AudioClip>("drop_dash_prepare"));
                player.SetPlayerAnimation("DropDash");
            }
            #endregion

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
            // Check that the player has the Chaos Emeralds equipped, has at least 50 crystal shards, has pressed the special button, isn't rolling but is in the rolling animation and isn't already super.
            if (HasWisp == WispType.NONE && player.powerups.Contains((FPPowerup)Plugin.chaosEmeraldID) && player.totalCrystals >= 50 && player.input.guardPress && player.state != new FPObjectState(State_Sonic_Roll) && player.currentAnimation == "Rolling" && !isSuper)
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

            if (HasWisp == WispType.NONE && player.input.guardPress && player.state != new FPObjectState(State_Sonic_Roll) && player.currentAnimation == "Rolling" && isSuper)
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
            // Determine what the Homing Attack should target.
            UpdateHomingAttackTarget();

            // Check that we have a Homing Attack target, are pressing the attack button but not up or down and not in various animations we don't want to Homing Attack cancel.
            if (HomingAttackTarget != null && player.input.attackPress && !player.input.up && !player.input.down && player.currentAnimation != "GuardAir" && player.currentAnimation != "Cyclone" && player.currentAnimation != "Spring" && player.currentAnimation != "HopStart" && player.currentAnimation != "HopLoop" && player.currentAnimation != "Airdash")
            {
                // Reset the failsafe timer.
                HomingAttackFailsafeTimer = 0f;

                // Kill our velocity.
                player.velocity = Vector2.zero;

                // Set our state to the Homing Attack's.
                player.state = State_Sonic_HomingAttack;

                // Set our animation to the rolling one.
                player.SetPlayerAnimation("Rolling");

                // Play the big boost sound.
                player.Action_PlaySoundUninterruptable(player.sfxBigBoostLaunch);

                // Play a line from the attack array.
                player.audioChannel[0].PlayOneShot(player.vaAttack[UnityEngine.Random.Range(0, player.vaAttack.Length)]);
            }
            #endregion

            #region Stomp
            // Check if the player has pressed the special button.
            if ((player.input.specialHold && player.velocity.y <= 0) || player.input.specialPress)
            {
                // Give the player a set downwards velocity.
                player.velocity = new(0, -12);
                player.groundVel = 0;

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
            // Check if we have a Rocket Wisp, have pressed the guard button and have a full energy gauge.
            if (HasWisp == WispType.ROCKET && player.input.guardPress && player.energy == 100)
            {
                // Set the flag for the Gravity Bubble achievement.
                UsedWisp = true;

                // Set the player into the Rocket Wisp Start state.
                player.state = State_Sonic_WispStart;

                // Play the announcer call for the Rocket Wisp.
                player.Action_PlaySound(Plugin.sonicAssetBundle.LoadAsset<AudioClip>("vo_rocket_wisp"));

                // Set the player animation to the UseWisp one.
                player.SetPlayerAnimation("UseWisp");

                // Reset our generic timer.
                player.genericTimer = 0f;

                // Play the Wisp activation sound.
                player.Action_PlaySoundUninterruptable(player.sfxMillaCubeSpawn);
            }
            #endregion

            #region Drill Wisp
            if (HasWisp == WispType.DRILL && player.input.guardPress && player.energy == 100 && player.targetWaterSurface != null)
            {
                // Set the flag for the Gravity Bubble achievement.
                UsedWisp = true;

                // Play the announcer call for the Drill Wisp.
                player.Action_PlaySound(Plugin.sonicAssetBundle.LoadAsset<AudioClip>("vo_drill_wisp"));

                // Set the player animation to the UseWisp one.
                player.SetPlayerAnimation("UseWisp");

                // Reset our generic timer.
                player.genericTimer = 0f;

                // Set the player into the Drill Wisp Start state.
                player.state = State_Sonic_WispStart;

                // Play the Wisp activation sound.
                player.Action_PlaySoundUninterruptable(player.sfxMillaCubeSpawn);
            }
            #endregion

            #region Laser Wisp
            // Check if we have a Laser Wisp, have pressed the guard button and have a full energy gauge.
            if (HasWisp == WispType.LASER && player.input.guardPress && player.energy == 100)
            {
                // Set the flag for the Gravity Bubble achievement.
                UsedWisp = true;

                // Set the player into the Rocket Wisp Start state.
                player.state = State_Sonic_WispStart;

                // Play the announcer call for the Rocket Wisp.
                player.Action_PlaySound(Plugin.sonicAssetBundle.LoadAsset<AudioClip>("vo_laser_wisp"));

                // Set the player animation to the UseWisp one.
                player.SetPlayerAnimation("UseWisp");

                // Reset our generic timer.
                player.genericTimer = 0f;

                // Play the Wisp activation sound.
                player.Action_PlaySoundUninterruptable(player.sfxMillaCubeSpawn);
            }
            #endregion

            #region Sonic Updraft
            // Check that we're pressing up and attack, yet aren't in a Spring, Hop Jump or Humming Top animation.
            if (player.input.up && player.input.attackPress && player.currentAnimation != "Spring" && player.currentAnimation != "HopStart" && player.currentAnimation != "Cyclone")
            {
                // Play the standard Uppercut attack sound and a line from the Hard Attack set.
                player.Action_PlaySound(player.sfxUppercut);
                player.audioChannel[0].PlayOneShot(player.vaHardAttack[UnityEngine.Random.Range(0, player.vaHardAttack.Length)]);

                // If we haven't already used the Sonic Updraft or the Double Jump, then give Sonic a bit of upwards push and set the jump ability flag so we don't Sonic Boom Knuckles this shit.
                if (!player.jumpAbilityFlag)
                {
                    if (player.velocity.y > 0)
                        player.velocity.y += 6f;
                    else
                        player.velocity.y = 6f;

                    player.jumpAbilityFlag = true;
                }

                // Set our animation to the Air version of the Updraft (cuts a few frames for better flow into the air animations).
                player.SetPlayerAnimation("UpKick_Air");

                // Set our state to the UpKick state.
                player.state = State_Sonic_UpKick;
            }
            #endregion

            #region Sonic Rocket
            // Check that we're pressing down and attack, yet aren't in a Spring, Hop Jump or Humming Top animation.
            if (player.input.down && player.input.attackPress && player.currentAnimation != "Spring" && player.currentAnimation != "HopStart" && player.currentAnimation != "Cyclone")
            {
                // Play the standard Uppercut attack sound and a line from the Hard Attack set.
                player.Action_PlaySound(player.sfxUppercut);
                player.audioChannel[0].PlayOneShot(player.vaHardAttack[UnityEngine.Random.Range(0, player.vaHardAttack.Length)]);

                // Set our animation to the Sonic Rocket's.
                player.SetPlayerAnimation("DownKick");

                // Set our state to the DownKick state.
                player.state = State_Sonic_DownKick;
            }
            #endregion

            #region Guard
            // Check that we can guard and aren't in any state that we shouldn't be able to guard cancel out of (and that we aren't Super).
            if ((player.guardTime <= 0f || player.cancellableGuard) && player.state != new FPObjectState(State_Sonic_WispStart) && player.state != new FPObjectState(State_Sonic_SuperTransform) && (player.input.guardPress) && !isSuper)
            {
                // Play the Guard animation.
                player.SetPlayerAnimation("GuardAir", 0f, 0f);

                // Edit the animator's speed depending on the player velocity.
                player.animator.SetSpeed(Mathf.Max(1f, 0.7f + Mathf.Abs(player.velocity.x * 0.05f)));

                // Run the guard actions.
                player.Action_Guard();
                player.Action_ShadowGuard();

                // Create the guard flash and parent it to Sonic.
                GuardFlash guardFlash = (GuardFlash)FPStage.CreateStageObject(GuardFlash.classID, player.position.x, player.position.y);
                guardFlash.parentObject = player;

                // Stop playing sounds (this is just what the base game does so we're copying it).
                player.Action_StopSound();

                // Play the guard sound.
                FPAudio.PlaySfx(15);
            }
            #endregion
        }

        /// <summary>
        /// Makes Sonic jump with the rolling animation instead of the normal one.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "Action_Jump")]
        private static void MakeJumpRoll()
        {
            // Check if we're in the jumping animation from Action_Jump and the player is Sonic.
            if (player.currentAnimation == "Jumping" && player.characterID == Plugin.sonicCharacterID)
            {
                // Swap to the rolling animation.
                player.currentAnimation = "Rolling";

                // Set the player's attack stats to Sonic's roll.
                player.attackStats = AttackStats_SonicRoll;
            }
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
        /// Logic for the Homing Attack.
        /// </summary>
        private static void State_Sonic_HomingAttack()
        {
            // Set our attack stats to the Homing Attack's.
            player.attackStats = AttackStats_SonicHomingAttack;

            // Try cast the object to an enemy
            FPBaseEnemy? enemy = null;
            try { enemy = (FPBaseEnemy)HomingAttackTarget; } catch {}

            // Try cast the object to an item box so we can check if its been broken.
            try
            {
                ItemBox box = (ItemBox)HomingAttackTarget;
                if (box.state.Method.Name == "State_Done")
                {
                    // Set the player to the InAir state.
                    player.state = player.State_InAir;

                    // Give the player some upwards velocity.
                    player.velocity = new(0, 12f);

                    // Set the player to their Guard Air animation to act as a Homing flip.
                    player.SetPlayerAnimation("GuardAir");
                }
            }
            catch { }

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

            if (enemy != null)
            {
                // Get the state of the target (if it has one).
                string targetState = null;
                if (enemy.state != null)
                    targetState = enemy.state.Method.Name;

                // Move the player towards their Homing Attack target.
                player.position = Vector2.MoveTowards(player.position, new(enemy.transform.position.x + (float)Math.Round((enemy.hbWeakpoint.right + enemy.hbWeakpoint.left) / 2), enemy.transform.position.y + (float)Math.Round((enemy.hbWeakpoint.top + enemy.hbWeakpoint.bottom) / 2)), 20f * FPStage.deltaTime);

                // Check if the player hasn't collided with a solid surface, that their position now matches the Homing Attack target's or if the target is in its Death state.
                if (player.position == new Vector2(enemy.transform.position.x + (float)Math.Round((enemy.hbWeakpoint.right + enemy.hbWeakpoint.left) / 2), enemy.transform.position.y + (float)Math.Round((enemy.hbWeakpoint.top + enemy.hbWeakpoint.bottom) / 2)) || targetState == "State_Death" || player.colliderWall != null || player.colliderRoof != null || player.colliderGround != null)
                {
                    // Set the player to the InAir state.
                    player.state = player.State_InAir;

                    // Give the player some upwards velocity.
                    player.velocity = new(0, 12f);

                    // Set the player to their Guard Air animation to act as a Homing flip.
                    player.SetPlayerAnimation("GuardAir");
                }
            }
            else
            {
                // Move the player towards their Homing Attack target.
                player.position = Vector2.MoveTowards(player.position, HomingAttackTarget.transform.position, 20f * FPStage.deltaTime);

                // Check if the player hasn't collided with a solid surface or that their position now matches the Homing Attack target's.
                if (player.position == HomingAttackTarget.position || player.colliderWall != null || player.colliderRoof != null || player.colliderGround != null)
                {
                    // Set the player to the InAir state.
                    player.state = player.State_InAir;

                    // Give the player some upwards velocity.
                    player.velocity = new(0, 12f);

                    // Set the player to their Guard Air animation to act as a Homing flip.
                    player.SetPlayerAnimation("GuardAir");
                }
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

            // Set the player's attack stats to Sonic's roll.
            player.attackStats = AttackStats_SonicRoll;

            // Check if the player is on the ground at this point.
            if (player.onGround)
            {
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

                // Check if we're still holding the special button and the timer is at least 5.
                if (player.input.specialHold && player.genericTimer >= 5f)
                {
                    // Reset the jump ability flag so we can double jump out of the stomp.
                    player.jumpAbilityFlag = false;

                    // Return to the InAir state.
                    player.state = player.State_InAir;
                    player.onGround = false;

                    // Set our velocity to our jump strength, with 2 extra points added to it.
                    player.velocity.y = (player.jumpStrength * (float)typeof(FPPlayer).GetField("jumpMultiplier", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(player)) + 2f;

                    // Set the player to the rolling animation.
                    player.SetPlayerAnimation("Rolling");
                }

                // Set the player to the Ground state.
                else
                    player.state = player.State_Ground;

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
                    && SceneManager.GetActiveScene() is
                    {
                        name: not "GlobeOpera1",
                        name: not "AncestralForge",
                        name: not "Battlesphere_Boss",
                        name: not "Battlesphere_Bossrush_Div1"
                    })
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
        /// Forces the Rolling and Cyclone animations to use the correct attack stats.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "Update")]
        private static void SetAnimationDependentStats()
        {
            if (player.currentAnimation == "Rolling") player.attackStats = AttackStats_SonicRoll;
            if (player.currentAnimation == "Cyclone") player.attackStats = AttackStats_SonicHummingTop;
        }

        /// <summary>
        /// Logic for the Sonic Rocket.
        /// </summary>
        public static void State_Sonic_DownKick()
        {
            // Set the player's attack stats to the Sonic Rocket's.
            player.attackStats = AttackStats_SonicDownKick;

            // Check if we're on the ground and swap out of the state if so.
            if (player.onGround)
            {
                player.state = player.State_Ground;
                player.SetPlayerAnimation("Running");
            }

            // If we're in the air, then apply basic air stuff.
            else
            {
                applyAir.Invoke(player, new object[] { false });
                applyGravity.Invoke(player, new object[] { });
                if (!player.input.jumpHold && player.jumpReleaseFlag)
                {
                    player.jumpReleaseFlag = false;
                    if (player.velocity.y > player.jumpRelease)
                    {
                        player.velocity.y = player.jumpRelease;
                    }
                }

                // Cap the Y velocity to -9.
                player.velocity.y = Mathf.Max(-9f, player.velocity.y);
            }

            // Check if we've reached the end of the updraft animation.
            if (player.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f)
            {
                player.SetPlayerAnimation("Jumping_Loop");
                player.state = player.State_InAir;
            }
        }

        #endregion

        #region Ground States/Actions
        /// <summary>
        /// Sonic's moves when in the ground state.
        /// </summary>
        public static void Action_Sonic_GroundMoves()
        {
            #region Drop Dash
            if (DropDashCharge)
            {
                // Stop the charge sound and play the release one.
                player.audioChannel[1].Stop();
                player.Action_PlaySound(Plugin.sonicAssetBundle.LoadAsset<AudioClip>("drop_dash_release"));

                // Reset the Drop Dash Flag.
                DropDashCharge = false;

                // Set our velocity.
                if (player.direction == FPDirection.FACING_LEFT) player.groundVel = -16f;
                else player.groundVel = 16f;
                player.velocity = new Vector2(player.groundVel, 0);

                // Reset the generic timer.
                player.genericTimer = 0;

                // Set the player to the Drop Dash state.
                player.state = State_Sonic_DropDash;

                // Don't run any more of the ground moves.
                return;
            }
            #endregion

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
            if (HasWisp == WispType.ROCKET && player.input.guardPress && player.energy == 100)
            {
                // Set the flag for the Gravity Bubble achievement.
                UsedWisp = true;

                // Set the player into the Rocket Wisp Start state.
                player.state = State_Sonic_WispStart;

                // Play the announcer call for the Rocket Wisp.
                player.Action_PlaySound(Plugin.sonicAssetBundle.LoadAsset<AudioClip>("vo_rocket_wisp"));

                // Set the player animation to the UseWisp one.
                player.SetPlayerAnimation("UseWisp");

                // Reset our generic timer.
                player.genericTimer = 0f;

                // Play the Wisp activation sound.
                player.Action_PlaySoundUninterruptable(player.sfxMillaCubeSpawn);
            }
            #endregion

            #region Drill Wisp
            if (HasWisp == WispType.DRILL && player.input.guardPress && player.energy == 100 && player.targetWaterSurface != null)
            {
                // Set the flag for the Gravity Bubble achievement.
                UsedWisp = true;

                // Play the announcer call for the Drill Wisp.
                player.Action_PlaySound(Plugin.sonicAssetBundle.LoadAsset<AudioClip>("vo_drill_wisp"));

                // Set the player animation to the UseWisp one.
                player.SetPlayerAnimation("UseWisp");

                // Reset our generic timer.
                player.genericTimer = 0f;

                // Set the player into the Drill Wisp Start state.
                player.state = State_Sonic_WispStart;

                // Play the Wisp activation sound.
                player.Action_PlaySoundUninterruptable(player.sfxMillaCubeSpawn);
            }
            #endregion

            #region Laser Wisp
            // Check if we have a Laser Wisp, have pressed the guard button and have a full energy gauge.
            if (HasWisp == WispType.LASER && player.input.guardPress && player.energy == 100)
            {
                // Set the flag for the Gravity Bubble achievement.
                UsedWisp = true;

                // Set the player into the Rocket Wisp Start state.
                player.state = State_Sonic_WispStart;

                // Play the announcer call for the Rocket Wisp.
                player.Action_PlaySound(Plugin.sonicAssetBundle.LoadAsset<AudioClip>("vo_laser_wisp"));

                // Set the player animation to the UseWisp one.
                player.SetPlayerAnimation("UseWisp");

                // Reset our generic timer.
                player.genericTimer = 0f;

                // Play the Wisp activation sound.
                player.Action_PlaySoundUninterruptable(player.sfxMillaCubeSpawn);
            }
            #endregion

            #region Guard
            // Check that we can guard and aren't in any state that we shouldn't be able to guard cancel out of (and that we aren't Super).
            if ((player.guardTime <= 0f || player.cancellableGuard) && player.state != new FPObjectState(State_Sonic_WispStart) && (player.input.guardPress) && player.state != new FPObjectState(State_Sonic_SpinDash) && !isSuper)
            {
                // Check if we're moving slow enough to use a standing guard.
                if (Mathf.Abs(player.groundVel) < 3f)
                {
                    // Play the standing guard animation.
                    player.SetPlayerAnimation("Guard");

                    // Reset the idle timer.
                    player.idleTimer = Mathf.Min(player.idleTimer, 0f);

                    // Kill our ground velocity so we stop in place.
                    player.groundVel = 0f;
                }
                else
                {
                    // Play the running guard animation.
                    player.SetPlayerAnimation("GuardRun");

                    // Edit the animator's speed depending on the player velocity.
                    player.animator.SetSpeed(Mathf.Max(1f, 0.7f + Mathf.Abs(player.velocity.x * 0.05f)));
                }

                // Run the guard actions.
                player.Action_Guard();
                player.Action_ShadowGuard();

                // Create the guard flash and parent it to Sonic.
                GuardFlash guardFlash = (GuardFlash)FPStage.CreateStageObject(GuardFlash.classID, player.position.x, player.position.y);
                guardFlash.parentObject = player;

                // Stop playing sounds (this is just what the base game does so we're copying it).
                player.Action_StopSound();

                // Play the guard sound.
                FPAudio.PlaySfx(15);
            }
            #endregion

            #region Sweep Kick
            // Check that we're not already in the Sweep Kick state, but are pressing attack and not up.
            if (player.state != new FPObjectState(State_Sonic_SweepKick) && player.input.attackPress && !player.input.up)
            {
                // Play the uppercut sound.
                player.Action_PlaySound(player.sfxUppercut);

                // Reset the idle timer based on the fight stance time.
                player.idleTimer = 0f - player.fightStanceTime;

                // Play the Sweep Kick or Windmill animation depending on if our ground velocity.
                if (player.groundVel >= 10 || player.groundVel <= -10)
                    player.SetPlayerAnimation("Windmill");
                else
                    player.SetPlayerAnimation("SweepKick");

                // Set our state to the Sweep Kick's.
                player.state = State_Sonic_SweepKick;

                // Play a sound from the hard attack array.
                player.audioChannel[0].PlayOneShot(player.vaHardAttack[UnityEngine.Random.Range(0, player.vaHardAttack.Length)]);
            }
            #endregion

            #region Slide
            // Check that we're not sweep kicking or rolling, are moving and have pressed the special key.
            if (player.state != new FPObjectState(State_Sonic_SweepKick) && player.state != new FPObjectState(State_Sonic_Roll) && Mathf.Abs(player.groundVel) >= 3f && player.input.specialPress)
            {
                // Play Carol's pounce sound (which is replaced with the slide one in the prefab).
                player.Action_PlaySound(player.sfxPounce);

                // Reset the generic timer.
                player.genericTimer = 0f;

                // Set our state to the slide's.
                player.state = State_Sonic_Slide;

                // Play a sound from the start array.
                player.audioChannel[0].PlayOneShot(player.vaStart[UnityEngine.Random.Range(0, player.vaStart.Length)]);
            }
            #endregion

            #region Sonic Updraft
            // Check that we're pressing up and attack.
            if (player.input.up && player.input.attackPress)
            {
                // Play the standard Uppercut attack sound and a line from the Hard Attack set.
                player.Action_PlaySound(player.sfxUppercut);
                player.audioChannel[0].PlayOneShot(player.vaHardAttack[UnityEngine.Random.Range(0, player.vaHardAttack.Length)]);

                // Set our animation to the Updraft's.
                player.SetPlayerAnimation("UpKick");

                // Set our state to the UpKick state.
                player.state = State_Sonic_UpKick;
            }
            #endregion
        }

        public static void State_Sonic_DropDash()
        {
            player.SetPlayerAnimation("Rolling");
            player.attackStats = AttackStats_SonicRoll;
            player.genericTimer += FPStage.deltaTime;

            if (player.direction == FPDirection.FACING_LEFT) player.groundVel = -16f;
            else player.groundVel = 16f;

            if (player.genericTimer >= 15)
                player.state = State_Sonic_Roll;

            if (player.onGround)
            {
                // Set the animator's speed based on the player's ground velocity.
                player.animator.SetSpeed(Mathf.Abs(player.groundVel) * 0.15f);

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
                    // Set the player's angle to their ground angle.
                    player.angle = player.groundAngle;
                }
            }
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
        }

        /// <summary>
        /// Logic for Sonic's Spin Dash charging state.
        /// </summary>
        public static void State_Sonic_SpinDash()
        {
            // Set the player's attack stats to Sonic's roll.
            player.attackStats = AttackStats_SonicRoll;

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
        /// Logic for Sonic's Anti-Gravity sliding state.
        /// </summary>
        public static void State_Sonic_Slide()
        {
            // Set the player's attack stats to Sonic's slide.
            player.attackStats = AttackStats_SonicSlide;

            // Increment the generic timer.
            player.genericTimer += FPStage.deltaTime;

            // Make sure we're in the sliding animation.
            player.SetPlayerAnimation("Slide");

            // Check if we're on the ground.
            if (player.onGround)
            {
                // Force our ground velocity to 8 to lock the slide to a very specific speed like in '06.
                if (player.direction == FPDirection.FACING_RIGHT)
                    player.groundVel = 8;
                else
                    player.groundVel = -8;

                // Make sure our angle matches the ground.
                player.angle = player.groundAngle;

                // Change direction when pressing left or right.
                if (player.input.left) player.direction = FPDirection.FACING_LEFT;
                if (player.input.right) player.direction = FPDirection.FACING_RIGHT;

                // If we hit a wall, then swap to the KO Recover state.
                if (player.colliderWall != null)
                {
                    player.groundVel = 0;
                    player.SetPlayerAnimation("KO_Recover", 0f, 0f);
                    player.state = player.State_KO_Recover;
                }

                // Check if the player presses jump.
                if (player.input.jumpPress)
                {
                    // Perform the soft jump action.
                    player.Action_Jump();
                    player.SetPlayerAnimation("Rolling");
                }
            }

            // If the player isn't on the ground, then set them to the in air state.
            else
            {
                player.state = player.State_InAir;
                player.SetPlayerAnimation("Jumping");
            }

            // If we've been sliding for three seconds or press the special button again, then end the slide.
            if (player.genericTimer > 180f || player.input.specialPress)
            {
                // Swap to the standard ground state if we're moving.
                if (player.input.left || player.input.right)
                {
                    player.state = player.State_Ground;
                }
                // Swap to the KO Recover state if we're not pressing a direction.
                else
                {
                    player.groundVel = 0;
                    player.SetPlayerAnimation("KO_Recover", 0f, 0f);
                    player.state = player.State_KO_Recover;
                }
            }

            // Sweep Kick cancel if we press the attack button.
            if (player.input.attackPress)
            {
                // Play the uppercut sound.
                player.Action_PlaySound(player.sfxUppercut);

                // Reset the idle timer based on the fight stance time.
                player.idleTimer = 0f - player.fightStanceTime;

                // Play the Sweep Kick animation at a higher speed.
                player.SetPlayerAnimation("SweepKick");
                player.animator.SetSpeed(2);

                // Set our state to the Sweep Kick's.
                player.state = State_Sonic_SweepKick;

                // Play a sound from the hard attack array.
                player.audioChannel[0].PlayOneShot(player.vaHardAttack[UnityEngine.Random.Range(0, player.vaHardAttack.Length)]);
            }
        }

        /// <summary>
        /// Logic for Sonic's rollling state. Copied and modified from Carol's rolling state.
        /// </summary>
        public static void State_Sonic_Roll()
        {
            // Set the player's attack stats to Sonic's roll.
            player.attackStats = AttackStats_SonicRoll;

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
        /// Logic for Sonic's sweep kick state.
        /// </summary>
        public static void State_Sonic_SweepKick()
        {
            // Set the player's attack stats to Sonic's sweep kick.
            player.attackStats = AttackStats_SonicSweepKick;

            // Check if we're on the ground to apply basic ground stuff and allow jumping.
            if (player.onGround)
            {
                if (player.input.jumpPress)
                {
                    player.Action_SoftJump();
                }
                else
                {
                    applyGround.Invoke(player, new object[] { false });
                    player.angle = player.groundAngle;
                }
            }

            // Cancel out of the sweep kick if we end up in the air.
            else
            {
                player.state = player.State_InAir;
                player.SetPlayerAnimation("GuardAir");
            }

            // Check if we've reached the end of the sweep kick animation.
            if (player.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f)
            {
                // Check that we're on the ground still.
                if (player.onGround)
                {
                    // If we're holding down and barely moving, then go into a crouch.
                    if (player.input.down && Mathf.Abs(player.groundVel) <= 3f)
                    {
                        player.state = player.State_Crouching;
                        player.SetPlayerAnimation("Crouching", 0f, 0f, true);
                    }

                    // If not, then just go into the standard ground state.
                    else
                    {
                        player.state = player.State_Ground;
                    }
                }

                // If we're in the air (which in theory shouldn't be possible based on the earlier check) then set us to the air state and our animation to the jump loop.
                else
                {
                    player.state = player.State_InAir;
                    player.SetPlayerAnimation("Jumping_Loop");
                }
            }
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
        #endregion

        #region Misc. States/Actions
        /// <summary>
        /// Logic for collecting the Power Up item (AKA Power Sneakers).
        /// </summary>
        public static void Action_Sonic_Fuel()
        {
            // Stop any playing jingles then play our Power Sneakers jingle.
            FPAudio.StopJingle();
            FPAudio.PlayJingle(Plugin.sonicSpeedUpJingle);

            // Set our power up timer to 1080 (roughly 18 seconds).
            player.powerupTimer = 1080f;
        }

        /// <summary>
        /// Redirects any call to Carol's rolling attack stats (as the game sometimes calls it if the player is in the rolling animation) to Sonic's if needed.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPPlayer), "AttackStats_CarolRoll")]
        private static bool RedirectRollStats()
        {
            // If the player isn't Sonic, then use the original Carol Roll stats.
            if (player.characterID != Plugin.sonicCharacterID)
                return true;

            // If not, then redirect it to Sonic's custom set.
            player.attackStats = AttackStats_SonicRoll;
            return false;
        }

        /// <summary>
        /// Logic for the Sonic Updraft.
        /// </summary>
        public static void State_Sonic_UpKick()
        {
            // Set the player's attack stats to the Sonic Updraft's.
            player.attackStats = AttackStats_SonicUpKick;

            // Check if we're on the ground to apply basic ground stuff and allow jumping.
            if (player.onGround)
            {
                if (player.input.jumpPress)
                {
                    player.Action_SoftJump();
                    player.state = player.State_InAir;
                    player.SetPlayerAnimation("Rolling");
                }
                else
                {
                    applyGround.Invoke(player, new object[] { false });
                    player.angle = player.groundAngle;
                }
                player.jumpAbilityFlag = false;
            }

            // If we're in the air, then apply basic air stuff.
            else
            {
                applyAir.Invoke(player, new object[] { false });
                applyGravity.Invoke(player, new object[] { });
                if (!player.input.jumpHold && player.jumpReleaseFlag)
                {
                    player.jumpReleaseFlag = false;
                    if (player.velocity.y > player.jumpRelease)
                    {
                        player.velocity.y = player.jumpRelease;
                    }
                }
            }

            // Check if we've reached the end of the updraft animation.
            if (player.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f)
            {
                // If we're on the ground then set our state to the ground state and reset the idle timer.
                if (player.onGround)
                {
                    player.idleTimer = 0f - player.fightStanceTime;
                    player.state = player.State_Ground;
                }

                // If we're in the air then set our state to the air state and our animation to the jump loop.
                else
                {
                    player.SetPlayerAnimation("Jumping_Loop");
                    player.state = player.State_InAir;
                }
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

                    // Play the Boost Breaker sound from Lilac's prefab.
                    player.Action_PlaySoundUninterruptable(Plugin.lilacPrefab.GetComponent<FPPlayer>().sfxBoostExplosion);

                    // Create the Boost Breaker explosion.
                    BoostExplosion boostExplosion = (BoostExplosion)FPStage.CreateStageObject(BoostExplosion.classID, player.position.x, player.position.y);
                    boostExplosion.attackKnockback.x = player.attackKnockback.x * 0.5f;
                    boostExplosion.attackKnockback.y = player.attackKnockback.y * 0.5f;
                    boostExplosion.attackEnemyInvTime = player.attackEnemyInvTime;
                    boostExplosion.parentObject = player;
                    boostExplosion.faction = player.faction;

                    // Create the Invincibility stars and set the flash timer.
                    InvincibilityStar invincibilityStar = (InvincibilityStar)FPStage.CreateStageObject(InvincibilityStar.classID, -100f, -100f);
                    invincibilityStar.parentObject = player;
                    InvincibilityStar invincibilityStar2 = (InvincibilityStar)FPStage.CreateStageObject(InvincibilityStar.classID, -100f, -100f);
                    invincibilityStar2.parentObject = player;
                    invincibilityStar2.rotation = 180f;
                    player.flashTime = 1200f;
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
            // Kill the player velocity so the detransformation stops Sonic in place.
            player.velocity = Vector2.zero;
            player.groundVel = 0;

            // Disable the Super flag.
            isSuper = false;

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

                // Play the Boost Breaker sound from Lilac's prefab.
                player.Action_PlaySoundUninterruptable(Plugin.lilacPrefab.GetComponent<FPPlayer>().sfxBoostExplosion);

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
                createdSparks = false;

                // Remove the player's invincibility.
                player.invincibilityTime = 0f;
                player.flashTime = 0f;

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
        /// State for activating a Wisp power.
        /// </summary>
        private static void State_Sonic_WispStart()
        {
            // Set the player's invincibility timer to something absurdly high.
            player.invincibilityTime = 9999;

            // Kill the player's velocity.
            player.velocity = Vector2.zero;
            player.groundVel = 0;

            // Reset the player's angles.
            player.angle = 0;
            player.groundAngle = 0;

            // Increment our generic timer.
            player.genericTimer += FPStage.deltaTime;

            // Check if our generic timer has gone above 65.
            if (player.genericTimer >= 65)
            {
                switch (HasWisp)
                {
                    case WispType.ROCKET:
                        // Set the player to the Rocket Wisp animation.
                        player.SetPlayerAnimation("RocketWisp");

                        // Play the Rocket Wisp jingle.
                        FPAudio.PlayJingle(Plugin.sonicRocketJingle);

                        // Play the Rocket Wisp sounds.
                        player.Action_PlaySound(player.sfxMillaShieldFire);
                        player.Action_PlaySoundUninterruptable(player.sfxMillaSuperShield);

                        // Set the player to the Rocket Wisp state.
                        player.state = State_Sonic_RocketWisp;

                        // Make the Rocket Wisp effect visible.
                        RocketWispEffect.gameObject.SetActive(true);
                        break;

                    case WispType.DRILL:
                        // Set the player to the Drill Wisp animation.
                        player.SetPlayerAnimation("DrillWisp");

                        // Play the Drill Wisp jingle.
                        FPAudio.PlayJingle(Plugin.sonicDrillJingle);

                        // Play the Drill Wisp sound.
                        player.Action_PlaySoundUninterruptable(player.sfxMillaShieldSummon);

                        // Set the player to the Drill Wisp state.
                        player.state = State_Sonic_DrillWisp;
                        break;

                    case WispType.LASER:
                        // Set the player to the Laser Wisp Aim animation.
                        player.SetPlayerAnimation("LaserWispAim");

                        // Play the Laser Wisp jingle.
                        FPAudio.PlayJingle(Plugin.sonicLaserJingle);

                        // Play the Laser Wisp sound.
                        player.Action_PlaySound(Plugin.sonicAssetBundle.LoadAsset<AudioClip>("phantom_laser_charge"));

                        // Set the player to the Laser Wisp Aim state.
                        player.state = State_Sonic_LaserWispAim;

                        // Make the Laser Wisp effect visible.
                        LaserWispEffect.gameObject.SetActive(true);
                        LaserWispEffect.gameObject.GetComponent<Animator>().Play("Aim");
                        break;
                }

            }
        }

        /// <summary>
        /// Logic for the Rocket Wisp.
        /// </summary>
        private static void State_Sonic_RocketWisp()
        {
            // Store this in our last state value.
            LastWispState = State_Sonic_RocketWisp;

            // Set Sonic's attack stats to the Wisp one.
            player.attackStats = AttackStats_SonicWisp;

            // Zoom the camera out.
            FPCamera.stageCamera.RequestZoom(FPCamera.stageCamera.GetStandardZoomIncrementedValue(), FPCamera.ZoomPriority_VeryHigh);

            // Set the player's invincibility timer to something absurdly high.
            player.invincibilityTime = 9999;

            // Cap the player's x velocity.
            if (player.velocity.x > 2) player.velocity.x = 2;
            if (player.velocity.x < -2) player.velocity.x = -2;
            
            // Allow the player to shift the Rocket Wisp side to side like in Sonic Generations.
            if (player.input.right && player.velocity.x < 2) player.velocity.x += (0.25f * FPStage.deltaTime);
            if (player.input.left && player.velocity.x > -2) player.velocity.x -= (0.25f * FPStage.deltaTime);
            if (!player.input.right && !player.input.left)
            {
                if (player.velocity.x < 0) player.velocity.x += (0.1f * FPStage.deltaTime);
                if (player.velocity.x > 0) player.velocity.x -= (0.1f * FPStage.deltaTime);

                if (player.velocity.x < 0.1 && player.velocity.x > -0.1) player.velocity.x = 0;
            }

            // Give the player upwards velocity.
            player.velocity = new(player.velocity.x, 9f);

            // Unset the onGround flag.
            player.onGround = false;

            // Reset the player's oxygen level, as the Rocket Wisp in Sonic Colours does this.
            player.oxygenLevel = 1;

            // Check if we've run out of energy.
            if (player.energy <= 0)
            {
                // Reset the Wisp flag.
                HasWisp = WispType.NONE;

                // Remove the player's invincibility.
                player.invincibilityTime = 0;

                // Set the player to the InAir state.
                player.state = player.State_InAir;

                // Set the player to the GuardAir animation for the flip.
                player.SetPlayerAnimation("GuardAir");

                // Stop the Rocket Wisp jingle if it's still playing.
                FPAudio.StopJingle();

                // Stop the Rocket Wisp's sound.
                player.audioChannel[2].Stop();

                // Hide the Rocket Wisp effect.
                RocketWispEffect.gameObject.SetActive(false);

                // Clear our stored state.
                LastWispState = null;

                // Don't run the rest of this function.
                return;
            }

            // Reduce the player's energy bar.
            player.energy -= (0.8f * FPStage.deltaTime);

            // If the player is colliding with a roof, then double the drain rate.
            if (player.colliderRoof != null)
                player.energy -= (0.8f * FPStage.deltaTime);
        }

        /// <summary>
        /// Logic for the Drill Wisp.
        /// </summary>
        private static void State_Sonic_DrillWisp()
        {
            // Set Sonic's attack stats to the Wisp one.
            player.attackStats = AttackStats_SonicWisp;

            // Set the animation speed to 1.
            player.animator.SetSpeed(1f);

            // Store this in our last state value.
            LastWispState = State_Sonic_DrillWisp;

            // Zoom the camera out.
            FPCamera.stageCamera.RequestZoom(FPCamera.stageCamera.GetStandardZoomIncrementedValue(), FPCamera.ZoomPriority_VeryHigh);

            // Check if we've left the water or have run out of energy.
            if (player.targetWaterSurface == null || player.energy <= 0)
            {
                // Reset the Wisp flag.
                HasWisp = WispType.NONE;

                // Remove the player's invincibility.
                player.invincibilityTime = 0;

                // Set our state and animation to the air and jump ones.
                player.state = player.State_InAir;
                player.SetPlayerAnimation("Jumping");

                // If we've left water, then double our velocity.
                if (player.targetWaterSurface == null) player.velocity *= 2;

                // Stop the Drill Wisp jingle if it's still playing.
                FPAudio.StopJingle();

                // Stop the Drill Wisp's sound.
                player.audioChannel[2].Stop();

                // Clear our stored state.
                LastWispState = null;

                // Don't run the rest of this function.
                return;
            }

            // Drain some energy.
            player.energy -= 0.575f * FPStage.deltaTime;

            // Reset the player's oxygen level, as the Drill Wisp always does this.
            player.oxygenLevel = 1;

            // Make sure we're not on the ground.
            player.onGround = false;

            // Rotate the player based on input, capping it to 360.
            if (player.input.right) player.angle -= FPStage.deltaTime * 5;
            if (player.input.left) player.angle += FPStage.deltaTime * 5;
            if (player.angle <= -360 || player.angle >= 360) player.angle = 0;

            // Determine our speed and whether or not we need to speed up the animation too.
            float speedModifier = 5;
            if (player.input.attackHold || player.input.jumpHold || player.input.specialHold)
            {
                speedModifier = 10;
                player.animator.SetSpeed(2f);
            }

            // Move the player forward depending on their facing direction.
            if (player.direction == FPDirection.FACING_RIGHT)
                player.velocity = (Vector2)player.transform.right * FPStage.deltaTime * speedModifier;
            else
                player.velocity = -(Vector2)player.transform.right * FPStage.deltaTime * speedModifier;

            // Check if we've hit a solid surface.
            if (player.colliderGround != null || player.colliderWall != null || player.colliderRoof != null)
            {
                // Invert our velocity and direction.
                player.velocity.x = 0f - player.prevVelocity.x;
                player.velocity.y = 0f - player.prevVelocity.y;
                player.direction ^= FPDirection.FACING_RIGHT;

                // Play the rebound sound.
                player.Action_PlaySoundUninterruptable(player.sfxBoostRebound);

                // If we've hit a floor, shift the player up a bit so they don't stay on the ground.
                if (player.colliderGround != null)
                    player.position.y += 4;
            }
        }

        /// <summary>
        /// State for aiming the Laser Wisp.
        /// </summary>
        private static void State_Sonic_LaserWispAim()
        {
            // Zoom the camera out.
            FPCamera.stageCamera.RequestZoom(FPCamera.stageCamera.GetStandardZoomIncrementedValue(), FPCamera.ZoomPriority_VeryHigh);

            // Rotate the player based on input.
            if (player.input.right && player.angle > -80) player.angle -= FPStage.deltaTime * 2.5f;
            if (player.input.left && player.angle < 80) player.angle += FPStage.deltaTime * 2.5f;
            switch (player.direction)
            {
                case FPDirection.FACING_LEFT:
                    if (player.input.up && player.angle > -80) player.angle -= FPStage.deltaTime * 2.5f;
                    if (player.input.down && player.angle < 80) player.angle += FPStage.deltaTime * 2.5f;
                    break;

                case FPDirection.FACING_RIGHT:
                    if (player.input.down && player.angle > -80) player.angle -= FPStage.deltaTime * 2.5f;
                    if (player.input.up && player.angle < 80) player.angle += FPStage.deltaTime * 2.5f;
                    break;
            }

            // Drain the energy gauge.
            player.energy -= 1f * FPStage.deltaTime;

            // Check for any action button or the energy gauge running out.
            if (player.input.jumpPress || player.input.attackPress || player.input.specialPress || player.input.guardPress || player.energy <= 0)
            {
                // Refill the energy gauge so the actual Laser Wisp state lasts a consistent amount of time.
                player.energy = 100;

                // Shift us up if we're colliding with the ground.
                if (player.colliderGround != null)
                    player.position.y += 32;

                // Play the Laser Wisp release sound.
                player.Action_PlaySound(Plugin.sonicAssetBundle.LoadAsset<AudioClip>("phantom_laser_shot"));

                // Play the Boost Breaker sound from Lilac's prefab.
                player.Action_PlaySoundUninterruptable(Plugin.lilacPrefab.GetComponent<FPPlayer>().sfxBoostExplosion);

                // Create the Boost Breaker explosion.
                BoostExplosion boostExplosion = (BoostExplosion)FPStage.CreateStageObject(BoostExplosion.classID, player.position.x, player.position.y);
                boostExplosion.attackKnockback.x = player.attackKnockback.x * 0.5f;
                boostExplosion.attackKnockback.y = player.attackKnockback.y * 0.5f;
                boostExplosion.attackEnemyInvTime = player.attackEnemyInvTime;
                boostExplosion.parentObject = player;
                boostExplosion.faction = player.faction;

                // Hide Sonic's sprite and set us to the Laser Wisp state.
                player.SetPlayerAnimation("Hide");
                player.state = State_Sonic_LaserWisp;

                LaserWispEffect.gameObject.GetComponent<Animator>().Play("Launch");
            }
        }

        /// <summary>
        /// Logic for the Laser Wisp.
        /// </summary>
        private static void State_Sonic_LaserWisp()
        {
            // Set our attack states to the roll's.
            player.attackStats = AttackStats_SonicRoll;

            // Give us an attack hitbox.
            player.hbAttack.left = -8;
            player.hbAttack.top = 8;
            player.hbAttack.right = 104;
            player.hbAttack.bottom = -8;
            player.hbAttack.enabled = true;
            player.hbAttack.visible = true;

            // Set the player's invincibility to a high value.
            player.invincibilityTime = 999;

            // Set the player's angle, subtracting 180 from it if we're facing left.
            player.angle = (float)((Mathf.Atan2(player.velocity.y, player.velocity.x)) * (180 / Math.PI));
            if (player.direction == FPDirection.FACING_LEFT)
                player.angle -= 180;

            // Zoom the camera out.
            FPCamera.stageCamera.RequestZoom(FPCamera.stageCamera.GetStandardZoomIncrementedValue(), FPCamera.ZoomPriority_VeryHigh);

            // Store this in our last state value.
            LastWispState = State_Sonic_LaserWisp;

            // Check if we've left the water or have run out of energy.
            if (player.energy <= 0)
            {
                // Reset the Wisp flag.
                HasWisp = WispType.NONE;

                // Remove the player's invincibility.
                player.invincibilityTime = 0;

                // Set our state and animation to the air and jump ones.
                player.state = player.State_InAir;
                player.SetPlayerAnimation("Jumping");

                // Stop the Laser Wisp jingle if it's still playing.
                FPAudio.StopJingle();

                // Clear our stored state.
                LastWispState = null;

                // Hide the Laser Wisp's effect.
                LaserWispEffect.gameObject.SetActive(false);

                // Don't run the rest of this function.
                return;
            }

            // Forcibly give us velocity if we don't have any.
            if (player.velocity.x == 0)
            {
                if (player.direction == FPDirection.FACING_RIGHT)
                    player.velocity = (Vector2)player.transform.right * FPStage.deltaTime * 24;
                else
                    player.velocity = -(Vector2)player.transform.right * FPStage.deltaTime * 24;
            }

            // Drain the energy gauge.
            player.energy -= 1.5f * FPStage.deltaTime;

            // Forcibly remove the onGround flag.
            player.onGround = false;

            // Bounce off of surfaces.
            if (player.colliderWall != null)
            {
                player.velocity.x = 0f - player.prevVelocity.x;
                player.Action_PlaySoundUninterruptable(player.sfxBoostRebound);
            }
            else if (player.colliderRoof != null || player.colliderGround != null)
            {
                player.velocity.x = player.prevVelocity.x;
                player.velocity.y = 0f - player.prevVelocity.y;
                player.Action_PlaySoundUninterruptable(player.sfxBoostRebound);

                if (player.colliderGround != null)
                    player.position.y += 32;
            }
        }

        /// <summary>
        /// Handles cleaning up after a Wisp if it was ended prematurely by another state overriding it.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPPlayer), "Update")]
        private static void WispCleanUp()
        {
            // Check that we have a Wisp state stored.
            if (LastWispState != null)
            {
                // Stupid hack to make the Drill Wisp persist if hitting the spiral things in Nalao Lake.
                if (player.state == player.State_InAir && player.currentAnimation == "GuardAir")
                {
                    player.SetPlayerAnimation("DrillWisp");
                    player.state = State_Sonic_DrillWisp;
                }

                // Check that our current state doesn't match the stored one.
                if (player.state != LastWispState)
                {
                    // Hide the Rocket Wisp effect.
                    RocketWispEffect.gameObject.SetActive(false);

                    // Hide the Laser Wisp's effect.
                    LaserWispEffect.gameObject.SetActive(false);

                    // Reset the Wisp flag.
                    HasWisp = WispType.NONE;

                    // Reset the player's angle to 0.
                    player.angle = 0;

                    // Stop the Jingle.
                    FPAudio.StopJingle();

                    // Stop the Wisp's sound.
                    player.audioChannel[2].Stop();

                    // Remove our Invincibility.
                    player.invincibilityTime = 0;

                    // Clear the state reference.
                    LastWispState = null;
                }
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

            // Handle swapping the life icons from and to Super Sonic's.
            FPHudDigit hudLifeIcons = UnityEngine.Object.FindObjectOfType<FPHudMaster>().hudLifeIcon[0];
            for (int spriteIndex = 0; spriteIndex < hudLifeIcons.digitFrames.Length; spriteIndex++)
            {
                if (isSuper || (player.state == new FPObjectState(State_Sonic_SuperTransform) && SuperStartTimer >= 17))
                {
                    if (hudLifeIcons.digitFrames[spriteIndex] == Plugin.lifeIcons[0]) hudLifeIcons.digitFrames[spriteIndex] = Plugin.lifeIcons[3];
                    if (hudLifeIcons.digitFrames[spriteIndex] == Plugin.lifeIcons[1]) hudLifeIcons.digitFrames[spriteIndex] = Plugin.lifeIcons[4];
                    if (hudLifeIcons.digitFrames[spriteIndex] == Plugin.lifeIcons[2]) hudLifeIcons.digitFrames[spriteIndex] = Plugin.lifeIcons[5];
                }
                else
                {
                    if (hudLifeIcons.digitFrames[spriteIndex] == Plugin.lifeIcons[3]) hudLifeIcons.digitFrames[spriteIndex] = Plugin.lifeIcons[0];
                    if (hudLifeIcons.digitFrames[spriteIndex] == Plugin.lifeIcons[4]) hudLifeIcons.digitFrames[spriteIndex] = Plugin.lifeIcons[1];
                    if (hudLifeIcons.digitFrames[spriteIndex] == Plugin.lifeIcons[5]) hudLifeIcons.digitFrames[spriteIndex] = Plugin.lifeIcons[2];
                }
            }

            // Don't proceed if the player isn't Super or is in the victory animation or a Wisp form.
            if (!isSuper || player.state == player.State_Victory || player.state == State_Sonic_SuperDetransform || player.state == State_Sonic_WispStart || player.state == State_Sonic_RocketWisp || player.state == State_Sonic_DrillWisp || player.state == State_Sonic_LaserWispAim || player.state == State_Sonic_LaserWisp)
                return;

            // Increase the player's stats.
            player.topSpeed = player.GetPlayerStat_Default_TopSpeed() * 2f;
            player.acceleration = player.GetPlayerStat_Default_Acceleration() * 2f;
            player.airAceleration = player.GetPlayerStat_Default_AirAceleration() * 2f;
            player.jumpStrength = player.GetPlayerStat_Default_JumpStrength() * 1.2f;
            typeof(FPPlayer).GetField("speedMultiplier", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(player, 1f + (float)(int)player.potions[6] * 0.05f);
            player.attackStats = SetConstantAttackStats;

            // Reset the player's invincibility time to 200 so it can never expire.
            player.invincibilityTime = 200f;

            // Set the flash timer to 1200 if its reached 0 so the character flashes.
            if (player.flashTime <= 0)
                player.flashTime = 1200f;

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
        #endregion

        #region Homing Attack Methods
        /// <summary>
        /// Gets all the item boxes and enemies in range of the Homing Attack.
        /// </summary>
        private static List<FPBaseObject> GetHomingAttackTargets()
        {
            // Create a list of objects.
            List<FPBaseObject> potentialTargets = [];
            
            // Loop through all the item boxes in the stage.
            foreach (ItemBox itemBox in UnityEngine.GameObject.FindObjectsOfType<ItemBox>())
            {
                // Check if this item box isn't already opened, isn't a bomb and is in range.
                if (itemBox.state.Method.Name != "State_Done" && itemBox.itemType != FPItemBoxTypes.BOX_BOMB && Vector2.SqrMagnitude(player.position - itemBox.position) <= 65536)
                {
                    // If the item box's position is valid (depending on the player's direction), then add them to the list.
                    if (player.direction == FPDirection.FACING_RIGHT && itemBox.position.x + 8 > player.position.x) potentialTargets.Add(itemBox);
                    if (player.direction == FPDirection.FACING_LEFT && itemBox.position.x - 8 < player.position.x) potentialTargets.Add(itemBox);
                }
            }

            // Loop through every active enemy in the stage.
            foreach (FPBaseEnemy fpbaseEnemy in FPStage.GetActiveEnemies(false, false))
            {
                // Blacklist the Stahp if its in one of its movement states, as it stays a constant distance away, so the Homing Attack can never close the gap.
                if (fpbaseEnemy.GetType() == typeof(Stahp))
                    if (fpbaseEnemy.state.Method.Name is "State_Track" or "State_Reposition")
                        continue;

                // Blacklist some things which use FPBaseEnemy but really shouldn't be targeted.
                if (fpbaseEnemy.GetType() == typeof(WeightedPlatform)
                 || fpbaseEnemy.GetType() == typeof(ASBlockDoorKey)
                 || fpbaseEnemy.GetType() == typeof(AFKeyBlock)
                 || fpbaseEnemy.GetType() == typeof(ASBlockDoorKey))
                    continue;

                // Check if the enemy has health, and active weakpoint, can be targeted, is of a different faction to the player and that the squared length of the player's position minus the enemy's is less than or equal to 65536.
                if (fpbaseEnemy.health > 0f && fpbaseEnemy.hbWeakpoint.enabled && fpbaseEnemy.CanBeTargeted() && fpbaseEnemy.faction != player.faction && Vector2.SqrMagnitude(player.position - fpbaseEnemy.position) <= 65536)
                {
                    // If the enemy's position is valid (depending on the player's direction), then add them to the list.
                    if (player.direction == FPDirection.FACING_RIGHT && fpbaseEnemy.position.x + 8 > player.position.x) potentialTargets.Add(fpbaseEnemy);
                    if (player.direction == FPDirection.FACING_LEFT && fpbaseEnemy.position.x - 8 < player.position.x) potentialTargets.Add(fpbaseEnemy);
                }
            }

            // Return our list of objects.
            return potentialTargets;
        }

        /// <summary>
        /// Compares two potential Homing Attack targets.
        /// TODO: This code comes from the BFF2000 missiles, how does it work?
        /// </summary>
        private static int CompareHomingAttackTargets(FPBaseObject obj1, FPBaseObject obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return 0;

            if (obj1 == null)
                return 1;

            if (obj2 == null)
                return -1;

            float num = Vector2.SqrMagnitude(player.position - obj1.position);
            float num2 = Vector2.SqrMagnitude(player.position - obj2.position);

            if (num < num2)
                return -1;

            if (num > num2)
                return 1;

            if (obj1.stageListPos < obj2.stageListPos)
                return -1;

            if (obj1.stageListPos > obj2.stageListPos)
                return 1;

            return 0;
        }

        /// <summary>
        /// Updates the list of Homing Attackable targets.
        /// TODO: This code comes from the BFF2000 missiles, how does it work?
        /// </summary>
        private static void UpdateHomingAttackTarget()
        {
            List<FPBaseObject> enemyListInHARange = GetHomingAttackTargets();

            enemyListInHARange.Sort(new Comparison<FPBaseObject>(CompareHomingAttackTargets));

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
                HomingAttackTarget = enemyListInHARange[0];
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
                    // Try cast the object to an enemy.
                    FPBaseEnemy? enemy = null;
                    try { enemy = (FPBaseEnemy)HomingAttackTarget; } catch {}

                    // Update the cursor's position.
                    if (enemy != null)
                        HomingAttackCursor.transform.position = new(enemy.transform.position.x + (float)Math.Round((enemy.hbWeakpoint.right + enemy.hbWeakpoint.left) / 2), enemy.transform.position.y + (float)Math.Round((enemy.hbWeakpoint.top + enemy.hbWeakpoint.bottom) / 2), 0);
                    else
                        HomingAttackCursor.transform.position = HomingAttackTarget.transform.position;

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
        #endregion

        #region Attack Stats
        private static void AttackStats_SonicRoll()
        {
            player.attackPower = 2f;
            player.attackHitstun = 1.5f;
            player.attackEnemyInvTime = 6f;
            SetConstantAttackStats();
        }

        private static void AttackStats_SonicHomingAttack()
        {
            player.attackPower = 8f;
            player.attackHitstun = 2f;
            player.attackEnemyInvTime = 4f;
            SetConstantAttackStats();
        }

        private static void AttackStats_SonicHummingTop()
        {
            player.attackPower = 5f;
            player.attackHitstun = 1f;
            player.attackEnemyInvTime = 5f;
            SetConstantAttackStats();
        }

        private static void AttackStats_SonicSweepKick()
        {
            player.attackPower = 6f;
            player.attackHitstun = 3f;
            player.attackEnemyInvTime = 6f;
            SetConstantAttackStats();
        }

        private static void AttackStats_SonicUpKick()
        {
            player.attackPower = 6f;
            player.attackHitstun = 3f;
            player.attackEnemyInvTime = 6f;
            SetConstantAttackStats();
        }

        private static void AttackStats_SonicDownKick()
        {
            player.attackPower = 5f;
            player.attackHitstun = 3f;
            player.attackEnemyInvTime = 6f;
            SetConstantAttackStats();
        }

        private static void AttackStats_SonicSlide()
        {
            player.attackPower = 4f;
            player.attackHitstun = 1.5f;
            player.attackEnemyInvTime = 6f;
            SetConstantAttackStats();
        }

        private static void AttackStats_SonicWisp()
        {
            player.attackPower = 8f;
            player.attackHitstun = 1f;
            player.attackEnemyInvTime = 3f;
            SetConstantAttackStats();
        }

        private static void SetConstantAttackStats()
        {
            player.attackKnockback.x = Mathf.Max(Mathf.Abs(player.prevVelocity.x * 1.5f), 6f);
            if (player.direction == FPDirection.FACING_LEFT) player.attackKnockback.x = 0f - player.attackKnockback.x;
            player.attackKnockback.y = player.prevVelocity.y * 0.5f;
            player.attackSfx = 7;
            player.attackPower *= player.GetAttackModifier();

            if (isSuper)
            {
                player.attackPower = 8f;
                player.attackHitstun = 2f;
                player.attackEnemyInvTime = 4f;
            }

            // If the debug flag is set through Unity Explorer or something, then jack up the damage to an absurd value.
            if (DebugOHKO)
                player.attackPower = 2424f;
        }
        #endregion

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
        /// Replaces the Airship in Bakunawa Chase with the Tornado.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerShip), "Start")]
        private static void ModShip(PlayerShip __instance)
        {
            // If the player isn't Sonic, then don't do any of this.
            if (FPSaveManager.character != Plugin.sonicCharacterID)
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
    }
}
