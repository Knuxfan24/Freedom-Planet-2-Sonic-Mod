using System.Collections.Generic;
using System.Linq;
using FP2_Sonic_Mod.CustomObjectScripts;
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
        /// Edits stuff on the Classic Map for Green Hill.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuClassic), "Start")]
        private static void GreenHillMapEdits(ref MenuClassicTile[] ___stages)
        {
            // Only do these edits if we're Sonic.
            if (FPSaveManager.character != Plugin.sonicCharacterID)
                return;

            // Replace the Guard Trial's unlock requirement with Weapon's Core.
            ___stages[33].stageRequirement = [30];

            // Set the music to the dummy White Space theme.
            ___stages[33].bgm = Plugin.sonicGHZMapMusic;

            // Move the icon and its connecting line to be after Weapon's Core.
            ___stages[33].icon.transform.position = new(4356, -400, 0);
            ___stages[33].icon.transform.GetChild(0).localPosition = new(-96, 0, 1);

            // Replace the Guard Trial, Weapon's Core and Tiger Falls' connecting directions.
            ___stages[33].right = -1;
            ___stages[33].left = 30;
            ___stages[30].right = 33;
            ___stages[4].left = -1;

            // Replace the Guard Trial's icon.
            ___stages[33].icon.sprite = Plugin.sonicAssetBundle.LoadAsset<Sprite>("ghz_icon");

            // Create the map background for Green Hill.
            GameObject ghzMapBackground = new("BG_GreenHill");
            ghzMapBackground.AddComponent<SpriteRenderer>().sprite = Plugin.sonicAssetBundle.LoadAsset<Sprite>("map_background");
            ghzMapBackground.layer = LayerMask.NameToLayer("BG Layer 0");
            ghzMapBackground.SetActive(false);

            // Set the Guard Trial's background.
            ___stages[33].background = ghzMapBackground.transform;
        }

        /// <summary>
        /// Edits the stage select confirmation dialog for Green Hill.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MenuWorldMapConfirm), "Start")]
        private static void GreenHillConfirm(ref Sprite[] ___hubIcon, ref string[] ___hubSceneToLoad)
        {
            // Only do these edits if we're Sonic.
            if (FPSaveManager.character != Plugin.sonicCharacterID)
                return;

            // Replace the Guard Trial's icon.
            ___hubIcon[12] = Plugin.sonicAssetBundle.LoadAsset<Sprite>("ghz_icon");

            // Redirect the Guard Trial to load the GreenHill scene instead of Tutorial2.
            ___hubSceneToLoad[12] = "GreenHill";
        }

        /// <summary>
        /// Replaces the Guard Trial's name with Green Hill's.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPSaveManager), nameof(FPSaveManager.GetHubName))]
        private static void GreenHillName(ref int hub, ref FPCharacterID ___character, ref string __result)
        {
            // Check if the HUB name being called for is the Guard Trial and that we're Sonic. If so, replace the Guard Trial text with Green Hill's.
            if (hub == 12 && ___character == Plugin.sonicCharacterID)
                __result = "Green Hill Zone";
        }

        /// <summary>
        /// Sets Green Hill's par time.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPSaveManager), nameof(FPSaveManager.GetStageParTime))]
        private static void GreenHillParTime(ref int __result)
        {
            // Check if we're in the GreenHill scene, if so, set the Par Time to 30 seconds.
            if (SceneManager.GetActiveScene().name == "GreenHill")
                __result = 3000;
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
