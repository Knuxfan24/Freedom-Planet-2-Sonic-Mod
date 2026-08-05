using FP2_Sonic_Mod.CustomObjectScripts;
using FP2Lib.Stage;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

namespace FP2_Sonic_Mod.Patchers
{
    internal class GreenHill
    {
        /// <summary>
        /// Handles giving the achievement for completing Green Hill.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPResultsMenu), "Update")]
        private static void Achievement()
        {
            // Check if the Results Menu has been called from Green Hill and unlock the Home Sweet Home achievement.
            if (FPStage.currentStage.stageName == "Green Hill Zone")
                FP2Lib.Badge.BadgeHandler.UnlockBadge("k24.badge_sonic_greenhill");
        }

        /// <summary>
        /// Creates Green Hill's panel on the map.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuClassic), "Start")]
        private static void GreenHillMapEdits(MenuClassic __instance)
        {
            // Only do these edits if we're Sonic.
            if (FPSaveManager.character != Plugin.sonicCharacterID)
                return;

            // Create the map background for Green Hill.
            GameObject ghzMapBackground = new("BG_GreenHill");
            ghzMapBackground.AddComponent<SpriteRenderer>().sprite = Plugin.sonicAssetBundle.LoadAsset<Sprite>("map_background");
            ghzMapBackground.layer = LayerMask.NameToLayer("BG Layer 0");
            ghzMapBackground.SetActive(false);

            // Instantiate the Emerald Hill panel from the asset bundle.
            GameObject mapPanel = GameObject.Instantiate(Plugin.sonicAssetBundle.LoadAsset<GameObject>("StageIcon_GreenHill"));

            // Create a tile entry for Green Hill.
            MenuClassicTile levelSelectTile = new MenuClassicTile
            {
                // Set the BGM and Background for Green Hill's tile.
                bgm = Plugin.sonicGHZMapMusic,
                background = ghzMapBackground.transform,

                // Tell the game that Green Hill doesn't have a Vinyl.
                hudVinylID = -1,

                // Attach the stage to Weapon's Core.
                left = 30,
                right = -1,
                down = -1,
                up = -1,
                stageRequirement = [30],

                // Get the icon from the panel.
                icon = mapPanel.GetComponent<SpriteRenderer>(),

                // Get Green Hill's ID.
                stageID = StageHandler.getCustomStageByUid("K24_Sonic_GreenHill").id
            };

            // Add Green Hill to the stage list.
            __instance.stages = __instance.stages.AddToArray(levelSelectTile);

            // Attach Green Hill to Weapon's Core's entry.
            __instance.stages[30].right = Array.IndexOf(__instance.stages, levelSelectTile);
        }

        /// <summary>
        /// Forces crystal shard Item Boxes to give 10 if in Green Hill.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ItemBox), "Action_ReleaseCrystals")]
        private static void GreenHillRingCapsule(ref int amount)
        {
            // Check if we're in the GreenHill scene, if so, replace the amount value with 10.
            if (SceneManager.GetActiveScene().name == "GreenHill")
                amount = 10;
        }

        /// <summary>
        /// Sets the Star Post to the On state when reloading from it.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPCheckpoint), "Start")]
        private static void StarpostReload(FPCheckpoint __instance)
        {
            // Check if we're in Green Hill and that the active checkpoint's x position is at least equal to this one's. If so, play the on noanim animation.
            if (SceneManager.GetActiveScene().name == "GreenHill" && FPStage.checkpointPos.x >= __instance.transform.position.x)
                __instance.GetComponent<Animator>().Play("starpost_on_noanim");
        }

        /// <summary>
        /// Sets the Star Post to the On state when passing it.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPCheckpoint), "Set_Checkpoint")]
        private static void Starpost(FPCheckpoint __instance)
        {
            // Don't do this if we're not in Green Hill.
            if (SceneManager.GetActiveScene().name != "GreenHill")
                return;

            // Play the on animation.
            __instance.GetComponent<Animator>().Play("starpost_on");

            // Play the Star Post sound.
            FPAudio.PlaySfx(Plugin.sonicAssetBundle.LoadAsset<AudioClip>("starpost"));
        }
                
        /// <summary>
        /// Adds the custom Falling Platform component to the platforms in Green Hill that need it.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPStage), "Start")]
        private static void CollapsingPlatforms()
        {
            if (SceneManager.GetActiveScene().name != "GreenHill")
                return;

            GameObject[] platforms = [.. UnityEngine.Object.FindObjectsOfType<GameObject>().Where(x => x.name.StartsWith("collapsing platform"))];
            
            foreach (var platform in platforms)
                platform.AddComponent<FallingPlatform>();
        }

        /// <summary>
        /// Adds the custom Zoom Tube component to the dummy objects for them in Green Hill.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPStage), "Start")]
        private static void ZoomTubes()
        {
            if (SceneManager.GetActiveScene().name != "GreenHill" && SceneManager.GetActiveScene().name != "GreenHillTutorial")
                return;

            GameObject[] tubes = [.. UnityEngine.Object.FindObjectsOfType<GameObject>().Where(x => x.name.StartsWith("zoom tube"))];
            
            foreach (var tube in tubes)
                tube.AddComponent<ZoomTube>();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPStage), "Start")]
        private static void SignPost()
        {
            if (SceneManager.GetActiveScene().name != "GreenHill" && SceneManager.GetActiveScene().name != "GreenHillTutorial")
                return;

            var sign = UnityEngine.GameObject.Find("ghz_signpost_0").AddComponent<SignPost>();
            sign.signSound = Plugin.sonicAssetBundle.LoadAsset<AudioClip>("classic_signpost");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPStage), "Start")]
        private static void Badniks()
        {
            if (SceneManager.GetActiveScene().name != "GreenHill" && SceneManager.GetActiveScene().name != "GreenHillTutorial")
                return;

            GameObject[] motobugs = [.. UnityEngine.Object.FindObjectsOfType<GameObject>().Where(x => x.name.StartsWith("Motobug"))];
            GameObject[] choppers = [.. UnityEngine.Object.FindObjectsOfType<GameObject>().Where(x => x.name.StartsWith("Chopper"))];
            GameObject[] newtronsB = [.. UnityEngine.Object.FindObjectsOfType<GameObject>().Where(x => x.name.StartsWith("Blue Newtron"))];
            GameObject[] newtronsG = [.. UnityEngine.Object.FindObjectsOfType<GameObject>().Where(x => x.name.StartsWith("Green Newtron"))];
            GameObject[] buzzBombers = [.. UnityEngine.Object.FindObjectsOfType<GameObject>().Where(x => x.name.StartsWith("Buzz Bomber"))];
            GameObject[] crabmeats = [.. UnityEngine.Object.FindObjectsOfType<GameObject>().Where(x => x.name.StartsWith("Crabmeat"))];

            foreach (var motobug in motobugs) motobug.AddComponent<Motobug>();
            foreach (var chopper in choppers) chopper.AddComponent<Chopper>();
            foreach (var newtron in newtronsB) { var component = newtron.AddComponent<Newtron>(); component.type = 0; }
            foreach (var newtron in newtronsG) { var component = newtron.AddComponent<Newtron>(); component.type = 1; }
            foreach (var buzzBomber in buzzBombers) buzzBomber.AddComponent<BuzzBomber>();
            foreach (var crabmeat in crabmeats) crabmeat.AddComponent<Crabmeat>();
        }
    }
}
