using UnityEngine.SceneManagement;

namespace FP2_Sonic_Mod.Patchers
{
    internal class BossTaunts
    {
        private static bool playedVoice;
        private static PlayerDialog dialogBox;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Discord), "Start")]
        [HarmonyPatch(typeof(BFF2000), "Start")]
        [HarmonyPatch(typeof(TitanArmor), "Start")]
        [HarmonyPatch(typeof(GnawsaLock), "Start")]
        private static void ResetFlag()
        {
            playedVoice = false;
            dialogBox = GameObject.Instantiate(Plugin.sonicAssetBundle.LoadAsset<GameObject>("SonicDialogBox")).GetComponent<PlayerDialog>();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Discord), "State_Death")]
        private static void DiscordVoiceLine(ref float ___genericTimer)
        {
            // Don't do this if the voice isn't set to Roger and we've already played the sound.
            if (FPPlayerPatcher.player.characterID != Plugin.sonicCharacterID || Plugin.sonicVAOption.Value != 3 || playedVoice)
                return;

            // Check if the Discord's timer has reached 70 (when the slowdown is stopped) and we're in Phoenix Highway.
            if (___genericTimer >= 70f && SceneManager.GetActiveScene().name == "PhoenixHighway")
            {
                // Play the voice line and set the flag.
                dialogBox.queue[0].name = "Sonic";
                dialogBox.queue[0].text = "Sorry, I got bored and broke your robot!";
                dialogBox.queue[0].audio = Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory_roger_phoenixhighway");
                dialogBox.queue[0].portrait = Plugin.sonicAssetBundle.LoadAsset<Sprite>("file_icon");
                dialogBox.queue[0].active = true;
                playedVoice = true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BFF2000), "Update")]
        private static void BFF2000VoiceLine(ref float ___health)
        {
            // Don't do this if the voice isn't set to Jason and we've already played the sound.
            if (FPPlayerPatcher.player.characterID != Plugin.sonicCharacterID || Plugin.sonicVAOption.Value != 2 || playedVoice)
                return;

            // Check if the BFF2000's health has reached 105 (three bars left) and we're in Auditorium.
            if (___health <= 105f && SceneManager.GetActiveScene().name == "Auditorium")
            {
                // Play the voice line and set the flag.
                dialogBox.queue[0].name = "Sonic";
                dialogBox.queue[0].text = "Hey, what's wrong?\r\nIs that size just for show?";
                dialogBox.queue[0].audio = Plugin.sonicAssetBundle.LoadAsset<AudioClip>("bos02_e01_sn");
                dialogBox.queue[0].portrait = Plugin.sonicAssetBundle.LoadAsset<Sprite>("file_icon");
                dialogBox.queue[0].active = true;
                playedVoice = true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TitanArmor), "State_DestroyHead2")]
        private static void TitanArmorVoiceLine()
        {
            // Don't do this if the voice isn't set to Jason and we've already played the sound or aren't in Inversion Dynamo.
            if (FPPlayerPatcher.player.characterID != Plugin.sonicCharacterID || Plugin.sonicVAOption.Value != 2 || playedVoice || SceneManager.GetActiveScene().name != "Bakunawa3")
                return;

            // Play the voice line and set the flag.
            dialogBox.queue[0].name = "Sonic";
            dialogBox.queue[0].text = "All right, you're going down!";
            dialogBox.queue[0].audio = Plugin.sonicAssetBundle.LoadAsset<AudioClip>("bos07_e03_sn");
            dialogBox.queue[0].portrait = Plugin.sonicAssetBundle.LoadAsset<Sprite>("file_icon");
            dialogBox.queue[0].active = true;
            playedVoice = true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GnawsaLock), "State_RetreatB")]
        private static void GnawsaLockVoiceLine(ref float ___genericTimer)
        {
            // Don't do this if the voice isn't set to Ryan and we've already played the sound or aren't in Ancestral Forge.
            if (FPPlayerPatcher.player.characterID != Plugin.sonicCharacterID || Plugin.sonicVAOption.Value != 1 || playedVoice || SceneManager.GetActiveScene().name != "AncestralForge")
                return;

            // Play the voice line and set the flag.
            if (___genericTimer >= 35)
            {
                dialogBox.queue[0].name = "Sonic";
                dialogBox.queue[0].text = "Come on ya big drip!\r\nWhere ya goin'?";
                dialogBox.queue[0].audio = Plugin.sonicAssetBundle.LoadAsset<AudioClip>("e0002_01_01_sn");
                dialogBox.queue[0].portrait = Plugin.sonicAssetBundle.LoadAsset<Sprite>("file_icon");
                dialogBox.queue[0].active = true;
                playedVoice = true;
            }
        }
    }
}
