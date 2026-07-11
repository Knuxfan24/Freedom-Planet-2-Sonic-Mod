using UnityEngine.SceneManagement;

namespace FP2_Sonic_Mod.Patchers
{
    internal class DiscordPatcher
    {
        private static bool playedVoice;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Discord), "Start")]
        private static void ResetFlag() => playedVoice = false;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Discord), "State_Death")]
        private static void VoiceLine(ref float ___genericTimer)
        {
            // Don't do this if the voice isn't set to Roger and we've already played the sound.
            if (FPPlayerPatcher.player.characterID != Plugin.sonicCharacterID || Plugin.sonicVAOption.Value != 3 || playedVoice)
                return;

            // Check if the Discord's timer has reached 70 (when the slowdown is stopped) and we're in Phoenix Highway.
            if (___genericTimer >= 70f && SceneManager.GetActiveScene().name == "PhoenixHighway")
            {
                // Play the voice line and set the flag.
                FPPlayerPatcher.player.Action_PlayVoice(Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory_roger_phoenixhighway"));
                playedVoice = true;
            }

        }
    }
}
