namespace FP2_Sonic_Mod.Patchers
{
    internal class ItemStarCardPatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ItemStarCard), "Collect")]
        private static void DisableRocketWisp()
        {
            // If we're not Sonic, then don't bother with this.
            if (FPSaveManager.character != Plugin.sonicCharacterID)
                return;

            // Stop the Rocket Wisp jingle if it's still playing.
            FPAudio.StopJingle();

            // Hide the Rocket Wisp effect.
            FPPlayerPatcher.RocketWispEffect.gameObject.SetActive(false);
        }
    }
}
