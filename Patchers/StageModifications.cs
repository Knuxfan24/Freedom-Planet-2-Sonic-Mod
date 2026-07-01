using FP2_Sonic_Mod.CustomObjectScripts;
using UnityEngine.SceneManagement;

namespace FP2_Sonic_Mod.Patchers
{
    internal class StageModifications
    {
        private static readonly GameObject wispCapsuleBase = Plugin.sonicAssetBundle.LoadAsset<GameObject>("wisp capsule");

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPStage), "Start")]
        private static void AddExtraObjects()
        {
            // If we're not Sonic, then don't make the edits.
            if (FPSaveManager.character != Plugin.sonicCharacterID)
                return;

            // Value to hold whatever object we read to duplicate.
            GameObject templateObject;

            switch (SceneManager.GetActiveScene().name)
            {
                // Add Rocket Wisp Capsules to the tutorial.
                case "Tutorial1Sonic":
                    CreateWispCapsule(new(6768, -2562, 0));
                    CreateWispCapsule(new(19488, -2466, 0));
                    break;

                // Add some Wisp Capsules to Globe Opera 1.
                case "GlobeOpera1":
                    CreateWispCapsule(new(15344, -1400, 0));
                    CreateWispCapsule(new(20168, -1584, 0), WispType.LASER);
                    CreateWispCapsule(new(26128, -2496, 0), WispType.LASER);
                    CreateWispCapsule(new(36264, -1808, 0), WispType.LASER);
                    break;

                // Add some Wisp Capsules to Tidal Gate.
                case "TidalGate":
                    CreateWispCapsule(new(42952, -3016, 0), WispType.LASER);
                    CreateWispCapsule(new(46152, -2648, 0), WispType.LASER);
                    CreateWispCapsule(new(47600, -2656, 0), WispType.DRILL);
                    CreateWispCapsule(new(58264, -2280, 0));
                    break;

                // Add Drill Wisp Capsules to Nalao Lake.
                case "NalaoLake":
                    CreateWispCapsule(new(7328, -3216, 0), WispType.DRILL);
                    CreateWispCapsule(new(21824, -3040, 0), WispType.DRILL);
                    CreateWispCapsule(new(21016, -2304, 0), WispType.DRILL);
                    CreateWispCapsule(new(23904, -3464, 0), WispType.DRILL);
                    CreateWispCapsule(new(26072, -3136, 0), WispType.DRILL);
                    break;

                // Add Rocket Wisp Capsules to Ancestral Forge.
                case "AncestralForge":
                    CreateWispCapsule(new(11984, -15968, 0));
                    CreateWispCapsule(new(3784, -60256, 0), WispType.ROCKET, [GameObject.Find("AF Keyfish Altar (4)").GetComponent<AFKeyfishLock>()]);
                    CreateWispCapsule(new(41232, -40996, 0));
                    CreateWispCapsule(new(39968, -40408, 0), WispType.ROCKET, [GameObject.Find("AF Keyfish Altar (14)").GetComponent<AFKeyfishLock>(), GameObject.Find("AF Keyfish Altar (13)").GetComponent<AFKeyfishLock>(), GameObject.Find("AF Keyfish Altar (12)").GetComponent<AFKeyfishLock>()]);
                    break;

                // Add some Wisp Capsules to Gravity Bubble.
                case "GravityBubble":
                    CreateWispCapsule(new(20140, -6498, 0));
                    CreateWispCapsule(new(26632, -8154, 0), WispType.DRILL);
                    CreateWispCapsule(new(26600, -6378, 0));
                    CreateWispCapsule(new(38642, -8580, 0));
                    CreateWispCapsule(new(40040, -8028, 0));
                    CreateWispCapsule(new(40040, -6728, 0));
                    CreateWispCapsule(new(40040, -5428, 0));
                    CreateWispCapsule(new(40040, -4128, 0));
                    CreateWispCapsule(new(46500, -7502, 0), WispType.DRILL);
                    break;

                // Add a Rocket Wisp to Inversion Dynamo.
                case "Bakunawa3":
                    CreateWispCapsule(new(31584, -2552, 0));
                    break;

                // Add an exit zone to the Merga fight as a failsafe for if the Homing Attack drags the player underneath the floor.
                case "Bakunawa4Boss":
                    templateObject = new GameObject("Bottomless Pit Failsafe");
                    templateObject.transform.position = new(688, -720, 0);
                    var exitZone = templateObject.AddComponent<FPExitZone>();
                    exitZone.range = new(1000, 100);
                    exitZone.returnToGround = true;
                    UnityEngine.Object.Instantiate(templateObject);
                    break;
            }
        }

        /// <summary>
        /// Moves the spawn point in Gravity Bubble to the ground so Sonic doesn't fall down in his standing pose.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerSpawnPoint), "Start")]
        private static void AdjustGravityBubbleSpawn(PlayerSpawnPoint __instance)
        {
            if (FPSaveManager.character != Plugin.sonicCharacterID || SceneManager.GetActiveScene().name != "GravityBubble")
                return;

            __instance.transform.position = new(128, -564, 0);
            __instance.powerupOffset = new(182, -16);
        }

        /// <summary>
        /// Changes the water level at a certain point in Sonic's tutorial.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPWaterSurface), "Update")]
        private static void TutorialWaterLevel(FPWaterSurface __instance)
        {
            // Only do this if we're in Sonic's tutorial and have a player object.
            if (SceneManager.GetActiveScene().name != "Tutorial1Sonic" || FPPlayerPatcher.player == null)
                return;

            // Set the water's y position depending on the player's x position.
            if (FPPlayerPatcher.player.transform.position.x > 15360)
                __instance.transform.position = new(__instance.transform.position.x, -1472, __instance.transform.position.z);
            else
                __instance.transform.position = new(__instance.transform.position.x, -2864, __instance.transform.position.z);
        }

        /// <summary>
        /// Creates a Wisp Capsule
        /// </summary>
        /// <param name="position">The position to place the capsule.</param>
        /// <param name="type">What type of Wisp this capsule gives.</param>
        /// <param name="activators">What AFKeyfishLock objects (if any) need a key in to make this capsule spawn.</param>
        private static void CreateWispCapsule(Vector3 position, WispType type = WispType.ROCKET, AFKeyfishLock[] activators = null)
        {
            // Create the Wisp Capsule object.
            GameObject wispCapsule = UnityEngine.Object.Instantiate(wispCapsuleBase);

            // Add the RocketWispCapsule script to it.
            WispCapsule wispScript = wispCapsule.AddComponent<WispCapsule>();

            // Set the type of this Wisp Capsule.
            wispScript.type = type;

            // Set the Wisp Capsule's position.
            wispCapsule.transform.position = position;

            // If we have any activators, then set them too.
            if (activators != null)
                wispScript.activators = activators;

            // Print some debug information about the capsule we just spawned.
            Plugin.consoleLog.LogDebug($"Created Wisp Capsule of type {type} at {position}");
            if (activators != null)
            {
                Plugin.consoleLog.LogDebug($"\tActivators:");
                foreach (var activator in activators)
                    Plugin.consoleLog.LogDebug($"\t{activator.name}");
            }
        }
    }
}
