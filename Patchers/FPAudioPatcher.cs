using System;

namespace FP2_Sonic_Mod.Patchers
{
    internal class FPAudioPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPAudio), nameof(FPAudio.PlayMusic), new Type[] { typeof(AudioClip), typeof(float) })]
        private static void GetLastTrack(ref AudioClip bgmMusic)
        {
            // If there isn't actually any audio being made to play, then return.
            if (bgmMusic == null)
                return;

            // Check that the audio being made to play doesn't match what we've previously saved and isn't the super theme then lastUsedAudio to the audio being made to play.
            if (bgmMusic != Plugin.lastUsedAudio && bgmMusic != Plugin.sonicSuperMusic)
                Plugin.lastUsedAudio = bgmMusic;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPAudio), nameof(FPAudio.PlayMusic), new Type[] { typeof(AudioClip), typeof(float) })]
        private static void CustomMusicLoopSet(ref AudioClip bgmMusic, ref float ___loopStart, ref float ___loopEnd)
        {
            // Check that the play music function has a bgm assigned.
            if (bgmMusic != null)
            {
                // Check that the assigned bgm is our super theme.
                if (bgmMusic == Plugin.sonicSuperMusic)
                {
                    // Set the loop points for the super theme.
                    ___loopStart = 10.787f;
                    ___loopEnd = 79.547f;
                }

                // Check that the assigned bgm is our results theme.
                if (bgmMusic == Plugin.sonicResultsMusic)
                {
                    // Set the loop points for the results theme.
                    ___loopStart = 6.864f;
                    ___loopEnd = 55.467f;
                }

                // Check that the assigned bgm is our results theme.
                if (bgmMusic == Plugin.sonicGHZMapMusic)
                {
                    // Set the loop points for the results theme.
                    ___loopStart = 1.739f;
                    ___loopEnd = 19.130f;
                }
            }
        }
    }
}
