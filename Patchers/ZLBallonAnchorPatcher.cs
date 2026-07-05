using UnityEngine.SceneManagement;

namespace FP2_Sonic_Mod.Patchers
{
    internal class ZLBallonAnchorPatcher
    {
        /// <summary>
        /// Forces Balloons in Sonic's tutorial out of their inactive state, as they ignore the "Can't be killed" flag entirely.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ZLBalloonAnchor), "State_Inactive")]
        static void Indestructible(ref bool ___readyToReset)
        {
            if (SceneManager.GetActiveScene().name == "GreenHillTutorial")
                ___readyToReset = true;
        }
    }
}
