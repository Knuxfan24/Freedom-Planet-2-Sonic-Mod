using System.Collections.Generic;
using System.Linq;
using FP2_Sonic_Mod.CustomObjectScripts;
using UnityEngine.SceneManagement;

namespace FP2_Sonic_Mod.Patchers
{
    internal class GreenHill
    {
        // Dictionary of Koi Cannon State_Default methods, as all their states are private.
        static readonly Dictionary<string, FPObjectState> koiCannonDefault = [];

        /// <summary>
        /// Handles doing the stupid tutorial collision fix for Green Hill.
        /// An error gets thrown in this code thanks to it looping twice, but it seems to work fine despite it so OH WELL.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Ferr2DT_PathTerrain), "LegacyRecreateCollider2D")]
        private static void GreenHillCollisionHandler()
        {
            // Check if we're in Green Hill.
            if (SceneManager.GetActiveScene().name == "GreenHill")
            {
                // Get all of Green Hill's chunks.
                GameObject[] chunks = UnityEngine.Object.FindObjectsOfType<GameObject>().Where(x => x.name.StartsWith("chunk")).ToArray();

                // Set up a list of path arrays.
                List<Vector2[]> paths = [];
                List<Vector2[]> pathsSolid = [];

                // Loop through each chunk.
                foreach (GameObject chunk in chunks)
                {
                    var fullySolid = chunk.transform.Find("solid");
                    if (fullySolid != null)
                    {
                        // Find this chunk's collider.
                        PolygonCollider2D colliderSolid = fullySolid.gameObject.GetComponent<PolygonCollider2D>();

                        // If this chunk doesn't have a collider, then continue to the next chunk.
                        if (colliderSolid == null)
                            continue;

                        // Loop through each path in this collider.
                        for (int pathIndex = 0; pathIndex < colliderSolid.pathCount; pathIndex++)
                        {
                            // Store this path's values.
                            Vector2[] path = colliderSolid.GetPath(pathIndex);

                            // Loop through each point in our stored path.
                            for (int pointIndex = 0; pointIndex < path.Length; pointIndex++)
                            {
                                // Increment the points based on this chunk's position.
                                path[pointIndex].x += chunk.transform.position.x;
                                path[pointIndex].y += chunk.transform.position.y;
                            }

                            // Add this path to our list.
                            pathsSolid.Add(path);
                        }

                        // Disable this chunk's collider.
                        fullySolid.gameObject.GetComponent<PolygonCollider2D>().enabled = false;
                    }

                    // Find this chunk's collider.
                    PolygonCollider2D collider = chunk.gameObject.GetComponent<PolygonCollider2D>();

                    // If this chunk doesn't have a collider, then continue to the next chunk.
                    if (collider == null)
                        continue;

                    // Loop through each path in this collider.
                    for (int pathIndex = 0; pathIndex < collider.pathCount; pathIndex++)
                    {
                        // Store this path's values.
                        Vector2[] path = collider.GetPath(pathIndex);

                        // Loop through each point in our stored path.
                        for (int pointIndex = 0; pointIndex < path.Length; pointIndex++)
                        {
                            // Increment the points based on this chunk's position.
                            path[pointIndex].x += chunk.transform.position.x;
                            path[pointIndex].y += chunk.transform.position.y;
                        }

                        // Add this path to our list.
                        paths.Add(path);
                    }

                    // Disable this chunk's collider.
                    chunk.gameObject.GetComponent<PolygonCollider2D>().enabled = false;
                }

                // Find the Ferr2D terrain's polygon collider and overwrite its path count.
                GameObject.Find("New Terrain (2)").gameObject.GetComponent<PolygonCollider2D>().pathCount = paths.Count;

                // Loop through all the paths we saved and set them in the Ferr2D collider.
                for (int i = 0; i < paths.Count; i++)
                    GameObject.Find("New Terrain (2)").gameObject.GetComponent<PolygonCollider2D>().SetPath(i, paths[i]);

                // Find the Ferr2D terrain's polygon collider and overwrite its path count.
                GameObject.Find("New Terrain (3)").gameObject.GetComponent<PolygonCollider2D>().pathCount = pathsSolid.Count;

                // Loop through all the paths we saved and set them in the Ferr2D collider.
                for (int i = 0; i < pathsSolid.Count; i++)
                    GameObject.Find("New Terrain (3)").gameObject.GetComponent<PolygonCollider2D>().SetPath(i, pathsSolid[i]);
            }
        }

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
        /// Finds all the Koi Cannons and gets their default states.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(KoiCannon), "Start")]
        private static void GetKoiCannonStates(KoiCannon __instance)
        {
            if (!koiCannonDefault.ContainsKey(__instance.name) && SceneManager.GetActiveScene().name == "GreenHill")
                koiCannonDefault.Add(__instance.name, __instance.state);
        }

        /// <summary>
        /// Edits the behaviour of the Koi Cannon to let them jump again without needing to land on a solid surface first.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(KoiCannon), "State_Falling")]
        private static void EditKoiCannonBehaviour(KoiCannon __instance)
        {
            // Don't do this if we're not in Green Hill.
            if (SceneManager.GetActiveScene().name != "GreenHill")
                return;

            // Get the two private values I need.
            Vector2 start = (Vector2)typeof(KoiCannon).GetField("start", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(__instance);
            float startAngle = (float)typeof(KoiCannon).GetField("startAngle", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(__instance);
            
            // Check if this Koi Cannon has gone below its start position.
            if (__instance.transform.position.y <= start.y)
            {
                // Kill this Koi Cannon's velocity.
                __instance.velocity = new(0, 0);

                // Reset this Koi Cannon's position and angle.
                __instance.position = start;
                __instance.angle = startAngle;

                // Set this Koi Cannon's generic timer to 0.
                typeof(KoiCannon).GetField("genericTimer", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(__instance, 0f);
                
                // Set this Koi Cannon's state back to its default one.
                __instance.state = koiCannonDefault[__instance.name];
            }
        }

        /// <summary>
        /// Resets the Dictionary of Koi Cannon Default States.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPStage), "Start")]
        private static void ResetDictionary() => koiCannonDefault.Clear();
        
        /// <summary>
        /// Adds the custom Falling Platform component to the platforms in Green Hill that need it.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPStage), "Start")]
        private static void CollapsingPlatforms()
        {
            GameObject[] platforms = UnityEngine.Object.FindObjectsOfType<GameObject>().Where(x => x.name.StartsWith("collapsing platform")).ToArray();
            
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
            GameObject[] tubes = UnityEngine.Object.FindObjectsOfType<GameObject>().Where(x => x.name.StartsWith("zoom tube")).ToArray();
            
            foreach (var tube in tubes)
                tube.AddComponent<ZoomTube>();
        }
    }
}
