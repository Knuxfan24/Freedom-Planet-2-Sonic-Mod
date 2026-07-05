// TODO: Test all the scenes to make sure Lilac gets replaced in all of them.
using UnityEngine.SceneManagement;

namespace FP2_Sonic_Mod.Patchers
{
    internal class FPEventSequencePatcher
    {
        // The event activator for the Kalaw intro never addresses Lilac's object, so the other replacement doesn't work on it.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPStage), "Start")]
        static void ReplaceKalawIntro()
        {
            // Only do this if we're Sonic and in the Kalaw intro cutscene.
            if (FPSaveManager.character != Plugin.sonicCharacterID || SceneManager.GetActiveScene().name != "Battlesphere_Kalaw")
                return;

            // Find Lilac's cutscene object.
            GameObject lilac = GameObject.Find("Cutscene_Lilac");

            // Swap out the animator and kill the hair object.
            lilac.GetComponent<Animator>().runtimeAnimatorController = Plugin.sonicAssetBundle.LoadAsset<RuntimeAnimatorController>("Event Sonic Animator");
            lilac.transform.GetChild(0).gameObject.SetActive(false);
        }

        /// <summary>
        /// Replaces the animator on Lilac's cutscene objects if we're playing as Sonic.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPEventSequence), "Action_StartEvent")]
        static void LilacEventReplacer(ref FPDialogEvent e)
        {
            // Don't do this if we're not Sonic and not in the Ending.
            if (FPSaveManager.character != Plugin.sonicCharacterID || SceneManager.GetActiveScene().name == "Cutscene_Ending2" || SceneManager.GetActiveScene().name == "Cutscene_Ending3")
                return;

            // Check that this event targets an object.
            if (e.targetObject != null)
            {
                // If we're in Gravity Bubble, change a few events in the scene.
                if (SceneManager.GetActiveScene().name == "GravityBubble")
                {
                    if (e.description == "Lilac pose")
                    {
                        e.pose = "Pose9";
                    }

                    if (e.description == "Lilac scream")
                    {
                        e.audio = null;
                    }

                    if (e.description == "Lilac charge")
                    {
                        e.audio = FPPlayerPatcher.player.sfxBoostCharge;
                        e.pose = "SpindashCharge";
                        e.activateObject = null;
                    }

                    if (e.description == "Lilac boost")
                    {
                        e.audio = FPPlayerPatcher.player.sfxBoostLaunch;
                        e.movePose = "Rolling";
                        e.activateObject = null;
                    }
                }

                // Get the target's animator and check that it actually has one.
                Animator animator = e.targetObject.gameObject.GetComponent<Animator>();
                if (animator != null)
                {
                    // DEBUG: Log the object, pose and move pose.
                    Plugin.consoleLog.LogDebug($"Object: {e.targetObject.name}\r\n\tPose: {e.pose}\r\n\tMove Pose: {e.movePose}");

                    // Check if this animator is Lilac's event one.
                    if (animator.runtimeAnimatorController.name == "Lilac")
                    {
                        // Kill the object for Lilac's hair.
                        if (e.targetObject.transform.GetChild(0).name == "tail")
                            e.targetObject.transform.GetChild(0).gameObject.SetActive(false);

                        // Swap to Sonic's animator depending on if we're Super or not.
                        if (!FPPlayerPatcher.isSuper) animator.runtimeAnimatorController = Plugin.sonicAssetBundle.LoadAsset<RuntimeAnimatorController>("Event Sonic Animator");
                        else animator.runtimeAnimatorController = Plugin.sonicAssetBundle.LoadAsset<RuntimeAnimatorController>("Event Super Sonic Animator");
                    }
                }
            }
        }
    }
}
