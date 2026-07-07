global using BepInEx;
global using HarmonyLib;
global using UnityEngine;
using BepInEx.Configuration;
using BepInEx.Logging;
using FP2_Sonic_Mod.Patchers;
using FP2Lib.Item;
using System.IO;

namespace FP2_Sonic_Mod
{
    /* TODOs for potential updates:
    TODO: Proper event activators so that Sonic can appear in the few Classic Mode cutscenes that exist rather than just hijacking Lilac's. I have... NO idea how that works. - Low Priority
    TODO: Try and make the Homing Attack less likely to send Sonic through solid matter. Tried Raycast checks but they were either always coming up positive or detecting nothing. - Medium Priority
    TODO: The Shadow Guard is broken as it uses the animation names rather than the state names, how do we fix this cleanly? Hitbox is also borked. - Medium Priority
    TODO: Fix Green Hill's bottomless pit in a way that isn't just "Add chunks there so the camera doesn't show the background". - Low Priority
    TODO: Replace the item boxes in Green Hill with monitors? - Low Priority
    TODO: Update the tutorial to mention the Sonic Rocket and adjust the Sweep Kick text to mention the Windmill. - High Priority, mandatory for 1.1's release.
    TODO: Redo the README. - High Priority, mandatory for 1.1's release.
    */
    [BepInPlugin("K24_FP2_Sonic", "Sonic The Hedgehog", "1.1.0")]
    [BepInDependency("000.kuborro.libraries.fp2.fp2lib")]
    public class Plugin : BaseUnityPlugin
    {
        // The asset bundle exported from the Unity project.
        public static AssetBundle sonicAssetBundle;
        public static AssetBundle sonicSceneBundle;

        // The music and jingles.
        public static AudioClip sonicSpeedUpJingle;
        public static AudioClip sonicSuperMusic;
        public static AudioClip sonicClearJingle;
        public static AudioClip sonicResultsMusic;
        public static AudioClip sonicCreditsMusic;
        public static AudioClip sonicDrowningJingle;
        public static AudioClip sonicRocketJingle;
        public static AudioClip sonicDrillJingle;
        public static AudioClip sonicLaserJingle;
        public static AudioClip sonicGHZMapMusic;
        public static AudioClip sonicGHZMusic;
        public static AudioClip sonicGHZClearJingle;
        public static AudioClip genericSuperMusic;

        // The last audio that FPAudio played in its PlayMusic function.
        public static AudioClip lastUsedAudio;

        // The playable character created through FP2Lib.
        public static FP2Lib.Player.PlayableChara playerSonic;
        internal static FPCharacterID sonicCharacterID;

        // Config options.
        public static ConfigEntry<int> sonicVAOption;
        public static ConfigEntry<int> sonicJumpSFXOption;

        // Other object player sprites.
        public static Sprite sonicPieNormal;
        public static Sprite sonicPieStruggle;
        public static Sprite superPieNormal;
        public static Sprite superPieStruggle;
        public static Sprite sonicZLBall;
        public static Sprite superZLBall;
        public static Sprite[] lifeIcons;

        // Photo Mode Poses.
        public static MenuPhotoPose sonicPhotoPoses;
        public static MenuPhotoPose superPhotoPoses;

        // Chaos Emerald Item ID.
        public static int chaosEmeraldID;

        // A copy of Lilac's prefab for the Super sound.
        public static GameObject lilacPrefab;

        // Logger.
        public static ManualLogSource consoleLog;

        private void Awake()
        {
            // Set up the logger.
            consoleLog = Logger;

            // Check for the asset bundles..
            if (!File.Exists($@"{Paths.GameRootPath}\mod_overrides\sonic.assets") || !File.Exists($@"{Paths.GameRootPath}\mod_overrides\sonic.scene"))
            {
                consoleLog.LogError("Failed to find either the Assets or Scene files! Please ensure they are correctly located in your Freedom Planet 2's mod_overrides folder.");
                return;
            }

            // Get the config options.
            sonicVAOption = Config.Bind("Sound",
                                        "Voice",
                                        2,
                                        "Determines which voice set to use.\n0: No Voice\n1: Ryan Drummond\n2: Jason Griffith\n3: Roger Craig Smith");

            sonicJumpSFXOption = Config.Bind("Sound",
                                             "Jump",
                                             2,
                                             "Determines which jump sound to use.\n0: Classic\n1: Adventure\n2: Modern");

            // Load our asset bundle.
            sonicAssetBundle = AssetBundle.LoadFromFile($@"{Paths.GameRootPath}\mod_overrides\sonic.assets");
            sonicSceneBundle = AssetBundle.LoadFromFile($@"{Paths.GameRootPath}\mod_overrides\sonic.scene");

            // Load our music and jingles.
            sonicSpeedUpJingle = sonicAssetBundle.LoadAsset<AudioClip>("speedup");
            sonicSuperMusic = sonicAssetBundle.LoadAsset<AudioClip>("super");
            sonicClearJingle = sonicAssetBundle.LoadAsset<AudioClip>("clear");
            sonicResultsMusic = sonicAssetBundle.LoadAsset<AudioClip>("result");
            sonicCreditsMusic = sonicAssetBundle.LoadAsset<AudioClip>("credits");
            sonicDrowningJingle = sonicAssetBundle.LoadAsset<AudioClip>("drowning");
            sonicRocketJingle = sonicAssetBundle.LoadAsset<AudioClip>("rocket");
            sonicDrillJingle = sonicAssetBundle.LoadAsset<AudioClip>("drill");
            sonicLaserJingle = sonicAssetBundle.LoadAsset<AudioClip>("laser");
            sonicGHZMapMusic = sonicAssetBundle.LoadAsset<AudioClip>("ghz_map");
            sonicGHZMusic = sonicAssetBundle.LoadAsset<AudioClip>("ghz");
            sonicGHZClearJingle = sonicAssetBundle.LoadAsset<AudioClip>("clear_ghz");
            genericSuperMusic = sonicAssetBundle.LoadAsset<AudioClip>("super_generic");

            // Load the misc. sprites.
            sonicPieNormal = sonicAssetBundle.LoadAsset<Sprite>("pie_normal");
            sonicPieStruggle = sonicAssetBundle.LoadAsset<Sprite>("pie_hit");
            superPieNormal = sonicAssetBundle.LoadAsset<Sprite>("pie_normal_super");
            superPieStruggle = sonicAssetBundle.LoadAsset<Sprite>("pie_hit_super");
            sonicZLBall = sonicAssetBundle.LoadAsset<Sprite>("zl_ball");
            superZLBall = sonicAssetBundle.LoadAsset<Sprite>("zl_ball_super");

            // Print all the asset names from the asset bundle as debug writes.
            foreach (string assetName in sonicAssetBundle.GetAllAssetNames())
                consoleLog.LogDebug(assetName);

            // Set up Sonic's photo mode poses.
            sonicPhotoPoses = new()
            {
                groundSprites = [sonicAssetBundle.LoadAsset<Sprite>("item"), sonicAssetBundle.LoadAsset<Sprite>("photo_ground1"), sonicAssetBundle.LoadAsset<Sprite>("photo_ground2"), sonicAssetBundle.LoadAsset<Sprite>("photo_ground3"), sonicAssetBundle.LoadAsset<Sprite>("photo_ground4"), sonicAssetBundle.LoadAsset<Sprite>("photo_ground5")],
                airSprites = [sonicAssetBundle.LoadAsset<Sprite>("photo_air1"), sonicAssetBundle.LoadAsset<Sprite>("photo_air2")]
            };
            superPhotoPoses = new()
            {
                groundSprites = [sonicAssetBundle.LoadAsset<Sprite>("item_super"), sonicAssetBundle.LoadAsset<Sprite>("idle_fight_super"), sonicAssetBundle.LoadAssetWithSubAssets<Sprite>("chat_super")[2]],
                airSprites = [sonicAssetBundle.LoadAsset<Sprite>("idle_fight_super"), sonicAssetBundle.LoadAssetWithSubAssets<Sprite>("wisp_super")[9]]
            };

            // Load the various life icons.
            lifeIcons = [sonicAssetBundle.LoadAsset<Sprite>("life_icon_0"), sonicAssetBundle.LoadAsset<Sprite>("life_icon_2"), sonicAssetBundle.LoadAsset<Sprite>("life_icon_1"), sonicAssetBundle.LoadAsset<Sprite>("life_icon_super_0"), sonicAssetBundle.LoadAsset<Sprite>("life_icon_super_2"), sonicAssetBundle.LoadAsset<Sprite>("life_icon_super_1")];

            // Construct Sonic's player object.
            playerSonic = new()
            {
                uid = "k24.sonic",
                Name = "Sonic",
                characterType = "SPEED Type",
                skill2 = "Double Jump",
                skill1 = "Homing Attack",
                skill3 = "Stomp",
                skill4 = "Rocket Wisp",
                powerupStartDescription = "Begin the stage with a set of Power Sneakers.",
                AirMoves = FPPlayerPatcher.Action_Sonic_AirMoves,
                GroundMoves = FPPlayerPatcher.Action_Sonic_GroundMoves,
                ItemFuelPickup = FPPlayerPatcher.Action_Sonic_Fuel,
                Gender = FP2Lib.Player.CharacterGender.MALE,
                element = FP2Lib.Player.CharacterElement.EARTH,
                profilePic = sonicAssetBundle.LoadAsset<Sprite>("file_icon"),
                keyArtSprite = sonicAssetBundle.LoadAsset<Sprite>("character_select"),
                endingKeyArtSprite = sonicAssetBundle.LoadAsset<Sprite>("character_select"),
                charSelectName = sonicAssetBundle.LoadAsset<Sprite>("character_name"),
                prefab = sonicAssetBundle.LoadAsset<GameObject>("player sonic"),
                characterSelectPrefab = sonicAssetBundle.LoadAsset<GameObject>("select sonic"),
                dataBundle = sonicAssetBundle,
                itemFuel = sonicAssetBundle.LoadAsset<Sprite>("power_sneakers"),
                livesIconAnim = [lifeIcons[0], lifeIcons[2], lifeIcons[1]],
                resultsTrack = sonicResultsMusic,
                menuPhotoPose = sonicPhotoPoses,
                endingTrack = sonicCreditsMusic,
                airshipSprite = 0,
                enabledInAventure = false, // While getting Adventure Mode fully working would be awesome, it feels like it'd be a nightmarish task.
                enabledInClassic = true,
                disableSwimming = true,
                eventActivatorCharacter = FPCharacterID.LILAC,
                EventSequenceStart = null, // Would really like this if only so Sonic can appear in the few Classic Mode events, but I don't understand how to make the event stuff work.
                piedHurtSprite = sonicPieStruggle,
                piedSprite = sonicPieNormal,
                sagaBlock = sonicAssetBundle.LoadAsset<RuntimeAnimatorController>("Saga Sonic"),
                sagaBlockSyntax = sonicAssetBundle.LoadAsset<RuntimeAnimatorController>("Syntax Saga Sonic"),
                TutorialScene = "GreenHillTutorial",
                useOwnCutsceneActivators = false, // See EventSequenceStart.
                worldMapIdle = [sonicAssetBundle.LoadAsset<Sprite>("worldmap_idle")],
                worldMapPauseSprite = sonicAssetBundle.LoadAsset<Sprite>("item"),
                worldMapWalk = [sonicAssetBundle.LoadAsset<Sprite>("worldmap_move1"), sonicAssetBundle.LoadAsset<Sprite>("worldmap_move2"), sonicAssetBundle.LoadAsset<Sprite>("worldmap_move3"), sonicAssetBundle.LoadAsset<Sprite>("worldmap_move4"), sonicAssetBundle.LoadAsset<Sprite>("worldmap_move5"), sonicAssetBundle.LoadAsset<Sprite>("worldmap_move6"), sonicAssetBundle.LoadAsset<Sprite>("worldmap_move7"), sonicAssetBundle.LoadAsset<Sprite>("worldmap_move8")],
                zaoBaseballSprite = sonicZLBall,
                menuInstructionPrefab = sonicAssetBundle.LoadAsset<GameObject>("guide sonic"),
                bfImpaleSprite = sonicAssetBundle.LoadAsset<Sprite>("impale"),
                statDefaultTopSpeed = 10f
            };

            // Register Sonic's player object with FP2Lib's player handler.
            FP2Lib.Player.PlayerHandler.RegisterPlayableCharacterDirect(playerSonic);

            // Get the ID that FP2Lib assigned to Sonic.
            sonicCharacterID = (FPCharacterID)FP2Lib.Player.PlayerHandler.GetPlayableCharaByUid(playerSonic.uid).id;

            // Register Sonic's Vinyls.
            FP2Lib.Vinyl.VinylHandler.RegisterVinyl("k24.vinyl_sonic_speedup", "Power Sneakers", sonicSpeedUpJingle, FP2Lib.Vinyl.VAddToShop.All, 1);
            FP2Lib.Vinyl.VinylHandler.RegisterVinyl("k24.vinyl_sonic_clear", "Stage Clear - Sonic", sonicClearJingle, FP2Lib.Vinyl.VAddToShop.All, 1);
            FP2Lib.Vinyl.VinylHandler.RegisterVinyl("k24.vinyl_sonic_results", "Results - Sonic", sonicResultsMusic, FP2Lib.Vinyl.VAddToShop.All, 1);
            FP2Lib.Vinyl.VinylHandler.RegisterVinyl("k24.vinyl_sonic_credits", "His World (Sonic's Theme)", sonicCreditsMusic, FP2Lib.Vinyl.VAddToShop.All, 31);
            FP2Lib.Vinyl.VinylHandler.RegisterVinyl("k24.vinyl_sonic_super", "Super Sonic", sonicSuperMusic, FP2Lib.Vinyl.VAddToShop.All, 32);
            FP2Lib.Vinyl.VinylHandler.RegisterVinyl("k24.vinyl_sonic_drowning", "Drowning", sonicDrowningJingle, FP2Lib.Vinyl.VAddToShop.All, 1);
            FP2Lib.Vinyl.VinylHandler.RegisterVinyl("k24.vinyl_sonic_rocket", "Colour Power - Orange Rocket", sonicRocketJingle, FP2Lib.Vinyl.VAddToShop.All, 24);
            FP2Lib.Vinyl.VinylHandler.RegisterVinyl("k24.vinyl_sonic_drill", "Color Power - Yellow Drill (Submarine Ver.)", sonicDrillJingle, FP2Lib.Vinyl.VAddToShop.All, 24);
            FP2Lib.Vinyl.VinylHandler.RegisterVinyl("k24.vinyl_sonic_laser", "Color Power - Cyan Laser", sonicLaserJingle, FP2Lib.Vinyl.VAddToShop.All, 24);
            FP2Lib.Vinyl.VinylHandler.RegisterVinyl("k24.vinyl_sonic_ghzmap", "Map - Green Hill", sonicGHZMapMusic, FP2Lib.Vinyl.VAddToShop.All, 32);
            FP2Lib.Vinyl.VinylHandler.RegisterVinyl("k24.vinyl_sonic_greenhill", "Green Hill Zone", sonicGHZMusic, FP2Lib.Vinyl.VAddToShop.All, 32);
            FP2Lib.Vinyl.VinylHandler.RegisterVinyl("k24.vinyl_sonic_greenhillclear", "Stage Clear - Green Hill", sonicGHZClearJingle, FP2Lib.Vinyl.VAddToShop.All, 32);
            FP2Lib.Vinyl.VinylHandler.RegisterVinyl("k24.vinyl_sonic_supergeneric", "Super Form", genericSuperMusic, FP2Lib.Vinyl.VAddToShop.All, 32);

            // Register Sonic's Badges.
            FP2Lib.Badge.BadgeHandler.RegisterBadge("k24.badge_sonic_clear", "Blue Blur", "Clear the game as Sonic.", sonicAssetBundle.LoadAsset<Sprite>("badge_clear"), FP2Lib.Badge.FPBadgeType.GOLD, FP2Lib.Badge.FPBadgeVisible.ALWAYS);
            FP2Lib.Badge.BadgeHandler.RegisterBadge("k24.badge_sonic_partime", "Greased Lightning", "Beat any stage's par time as Sonic.", sonicAssetBundle.LoadAsset<Sprite>("badge_partime"), FP2Lib.Badge.FPBadgeType.SILVER, FP2Lib.Badge.FPBadgeVisible.ALWAYS);
            FP2Lib.Badge.BadgeHandler.RegisterBadge("k24.badge_sonic_halfpartime", "Sonic Boom", "Beat any stage as Sonic in less than half of the par time.", sonicAssetBundle.LoadAsset<Sprite>("badge_halfpartime"), FP2Lib.Badge.FPBadgeType.SILVER, FP2Lib.Badge.FPBadgeVisible.ALWAYS);
            FP2Lib.Badge.BadgeHandler.RegisterBadge("k24.badge_sonic_allpartime", "Fastest Thing Alive", "Beat the par times in all stages as Sonic.", sonicAssetBundle.LoadAsset<Sprite>("badge_allpartime"), FP2Lib.Badge.FPBadgeType.GOLD, FP2Lib.Badge.FPBadgeVisible.ALWAYS);
            FP2Lib.Badge.BadgeHandler.RegisterBadge("k24.badge_sonic_emeralds", "The Servers Are...", "Clear Weapon's Core and obtain the Chaos Emeralds.", sonicAssetBundle.LoadAsset<Sprite>("badge_emeralds"), FP2Lib.Badge.FPBadgeType.GOLD, FP2Lib.Badge.FPBadgeVisible.HIDDEN);
            FP2Lib.Badge.BadgeHandler.RegisterBadge("k24.badge_sonic_palacemerga", "Problem Solved, Story Over", "Defeat Merga in Palace Courtyard as Super Sonic.", sonicAssetBundle.LoadAsset<Sprite>("badge_palacemerga"), FP2Lib.Badge.FPBadgeType.GOLD, FP2Lib.Badge.FPBadgeVisible.ALWAYS);
            FP2Lib.Badge.BadgeHandler.RegisterBadge("k24.badge_sonic_nowisp", "Oversaturated", "Beat Gravity Bubble without using any Wisps.", sonicAssetBundle.LoadAsset<Sprite>("badge_nowisp"), FP2Lib.Badge.FPBadgeType.GOLD, FP2Lib.Badge.FPBadgeVisible.ALWAYS);
            FP2Lib.Badge.BadgeHandler.RegisterBadge("k24.badge_sonic_greenhill", "Home Sweet Home", "Unlock and complete Green Hill Zone.", sonicAssetBundle.LoadAsset<Sprite>("badge_greenhill"), FP2Lib.Badge.FPBadgeType.GOLD, FP2Lib.Badge.FPBadgeVisible.HIDDEN);

            // Create and register the Chaos Emeralds item.
            FP2Lib.Item.ItemHandler.RegisterItem("k24.sonic.chaosemeralds", "Chaos Emeralds", sonicAssetBundle.LoadAsset<Sprite>("chaos_emeralds"), "Mysterious gems that grant the user limitless power.", IAddToShop.None);
            chaosEmeraldID = FP2Lib.Item.ItemHandler.GetItemDataByUid("k24.sonic.chaosemeralds").itemID;

            // Find and store Lilac's prefab.
            foreach (GameObject obj in UnityEngine.Resources.FindObjectsOfTypeAll<GameObject>())
                if (obj.name is "Player Lilac") lilacPrefab = obj;

            // Patch our classes.
            Harmony.CreateAndPatchAll(typeof(AcrabellePieTrapPatcher));
            Harmony.CreateAndPatchAll(typeof(DiscordPatcher));
            Harmony.CreateAndPatchAll(typeof(FPAudioPatcher));
            Harmony.CreateAndPatchAll(typeof(FPEventSequencePatcher));
            Harmony.CreateAndPatchAll(typeof(FPHudMasterPatcher));
            Harmony.CreateAndPatchAll(typeof(FPPlayerPatcher));
            Harmony.CreateAndPatchAll(typeof(FPResultsMenuPatcher));
            Harmony.CreateAndPatchAll(typeof(FPSaveManagerPatcher));
            Harmony.CreateAndPatchAll(typeof(GBJetstreamPatcher));
            Harmony.CreateAndPatchAll(typeof(GreenHill));
            Harmony.CreateAndPatchAll(typeof(ItemStarCardPatcher));
            Harmony.CreateAndPatchAll(typeof(MenuClassicPatcher));
            Harmony.CreateAndPatchAll(typeof(MenuCreditsPatcher));
            Harmony.CreateAndPatchAll(typeof(MenuGlobalPausePatcher));
            Harmony.CreateAndPatchAll(typeof(MenuPhotoPatcher));
            Harmony.CreateAndPatchAll(typeof(SagaPatcher));
            Harmony.CreateAndPatchAll(typeof(StageModifications));
            Harmony.CreateAndPatchAll(typeof(ZLBallonAnchorPatcher));
            Harmony.CreateAndPatchAll(typeof(ZLBaseballFlyerPatcher));
            Harmony.CreateAndPatchAll(typeof(GenericSuper));
        }
    }
}
