namespace FP2_Sonic_Mod.Patchers
{
    internal class GBJetstreamPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GBJetstream), "State_Start")]
        private static void DisableRocketWisp(ref FPPlayer ___targetPlayer)
        {
            if (___targetPlayer != null)
            {
                // Stop the Rocket Wisp jingle if it's still playing.
                FPAudio.StopJingle();

                // Hide the Rocket Wisp effect.
                FPPlayerPatcher.RocketWispEffect.gameObject.SetActive(false);
            }
        }
    }
}
