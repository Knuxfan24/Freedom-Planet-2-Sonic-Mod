using System;
using FP2_Sonic_Mod.CustomObjectScripts;
using UnityEngine.SceneManagement;

namespace FP2_Sonic_Mod.Patchers
{
    internal class StageModifications
    {
        private static readonly GameObject wispCapsuleBase = Plugin.sonicAssetBundle.LoadAsset<GameObject>("rocket wisp capsule");

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
                // Add the Wisp Capsules to the tutorial.
                case "Tutorial1Sonic":
                    CreateWispCapsule(new(6768, -2562, 0));
                    CreateWispCapsule(new(19488, -2466, 0));
                    break;

                // Remove a Metal Shield from Shade Armoury that Sonic can't get.
                case "ShadeArmory":
                    GameObject.Find("BoxMetalShield").gameObject.SetActive(false);
                    break;

                // Add a single Spring to Globe Opera 1.
                case "GlobeOpera1":
                    templateObject = GameObject.Find("High Spring Up");
                    _ = GameObject.Instantiate(templateObject, new Vector3(36496, -1816, 0), Quaternion.identity);
                    break;

                // Add a few Springs to Tidal Gate.
                case "TidalGate":
                    templateObject = GameObject.Find("High Spring Up");
                    GameObject extraTGSpring1 = GameObject.Instantiate(templateObject, new Vector3(42952, -3032, 0), Quaternion.identity);
                    extraTGSpring1.GetComponent<Spring>().springStrength = 20f;
                    _ = GameObject.Instantiate(templateObject, new Vector3(46154, -2662, 0), Quaternion.identity);
                    break;

                // Various adjustments to Nalao Lake.
                case "NalaoLake":
                    templateObject = GameObject.Find("NL_RisingBubble (26)");
                    _ = GameObject.Instantiate(templateObject, new Vector3(25256, -3232, 0), Quaternion.identity);
                    GameObject.Find("NL Thorns (26)").GetComponent<SpikeTerrain>().enabled = false;
                    GameObject.Find("NL Thorns (26)").GetComponent<MeshRenderer>().material = Plugin.sonicAssetBundle.LoadAsset<Material>("bf1_asteroid_fill");
                    GameObject.Find("NL Thorns (19)").GetComponent<SpikeTerrain>().enabled = false;
                    GameObject.Find("NL Thorns (19)").GetComponent<MeshRenderer>().material = Plugin.sonicAssetBundle.LoadAsset<Material>("bf1_asteroid_fill");
                    GameObject.Find("Crystal (276)").gameObject.SetActive(false);
                    GameObject.Find("Crystal (277)").gameObject.SetActive(false);
                    GameObject.Find("Crystal (278)").gameObject.SetActive(false);
                    GameObject.Find("Crystal (279)").gameObject.SetActive(false);
                    break;

                // Add Wisp Capsules to Ancestral Forge.
                case "AncestralForge":
                    CreateWispCapsule(new(11984, -15968, 0));
                    CreateWispCapsule(new(3784, -60256, 0), [GameObject.Find("AF Keyfish Altar (4)").GetComponent<AFKeyfishLock>()]);
                    CreateWispCapsule(new(41232, -40996, 0));
                    CreateWispCapsule(new(39968, -40408, 0), [GameObject.Find("AF Keyfish Altar (14)").GetComponent<AFKeyfishLock>(), GameObject.Find("AF Keyfish Altar (13)").GetComponent<AFKeyfishLock>(), GameObject.Find("AF Keyfish Altar (12)").GetComponent<AFKeyfishLock>()]);
                    break;

                // Various adjustments to Gravity Bubble.
                case "GravityBubble":
                    GameObject.Find("High Spring Up (2)").gameObject.SetActive(false);
                    GameObject.Find("High Spring Up (3)").gameObject.SetActive(false);
                    CreateWispCapsule(new(20140, -6530, 0));
                    CreateWispCapsule(new(26800, -8154, 0));
                    GameObject.Find("BoxCrystals (2)").gameObject.SetActive(false);
                    CreateWispCapsule(new(26600, -6378, 0));
                    CreateWispCapsule(new(38642, -8580, 0));
                    CreateWispCapsule(new(40040, -8028, 0));
                    CreateWispCapsule(new(40040, -6728, 0));
                    CreateWispCapsule(new(40040, -5428, 0));
                    CreateWispCapsule(new(40040, -4128, 0));
                    GameObject.Find("BubbleWall (7)").gameObject.SetActive(false);
                    CreateWispCapsule(new(46500, -7502, 0));
                    break;

                // Add a single Dash Ring to Inversion Dynamo.
                case "Bakunawa3":
                    templateObject = GameObject.Find("BoostRing (16)");
                    _ = GameObject.Instantiate(templateObject, new Vector3(31264, -2616, 0), Quaternion.identity);
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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPStage), "Start")]
        private static void OverrideSyntaxImpale()
        {
            // If we're not Sonic, then don't make the edits.
            if (FPSaveManager.character != Plugin.sonicCharacterID)
                return;

            if (SceneManager.GetActiveScene().name == "Bakunawa5")
            {
                foreach (GameObject obj in UnityEngine.Object.FindObjectsOfType<GameObject>())
                {
                    SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();

                    if (spriteRenderer != null)
                        if (spriteRenderer.sprite != null)
                            if (spriteRenderer.sprite.name == "lilac_ko_0" || spriteRenderer.sprite.name == "lilac_hit2_0")
                                spriteRenderer.sprite = Plugin.sonicAssetBundle.LoadAssetWithSubAssets<Sprite>("hit1")[4];
                }
            }
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
        /// Creates a Rocket Wisp Capsule
        /// </summary>
        /// <param name="position">The position to place the capsule.</param>
        /// <param name="activators">What AFKeyfishLock objects (if any) need a key in to make this capsule spawn.</param>
        private static void CreateWispCapsule(Vector3 position, AFKeyfishLock[] activators = null)
        {
            // Create the Wisp Capsule object.
            GameObject wispCapsule = UnityEngine.Object.Instantiate(wispCapsuleBase);

            // Add the RocketWispCapsule script to it.
            RocketWispCapsule wispScript = wispCapsule.AddComponent<RocketWispCapsule>();

            // Set the Wisp Capsule's position.
            wispCapsule.transform.position = position;

            // If we have any activators, then set them too.
            if (activators != null)
                wispScript.activators = activators;

            // If this is a debug build, then print some information about the capsule we just spawned.
            #if DEBUG
            Plugin.consoleLog.LogInfo($"Created Rocket Wisp Capsule at {position}");
            if (activators != null)
            {
                Plugin.consoleLog.LogInfo($"\tActivators:");
                foreach (var activator in activators)
                    Plugin.consoleLog.LogInfo($"\t{activator.name}");
            }
            #endif
        }
    }
}
