namespace FP2_Sonic_Mod.Patchers
{
    internal class MenuCreditsPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MenuCredits), "Start")]
        static void EditCredits(ref AudioClip[] ___castVoice, ref float ___normalSpeed)
        {
            // If we're not Sonic, then don't make the edits.
            if (FPSaveManager.character != Plugin.sonicCharacterID)
                return;

            // Move the portrait down a bit. 
            var portraitPosition = GameObject.Find("Credits").transform.GetChild(19);
            portraitPosition.localPosition = new(portraitPosition.localPosition.x, 0, portraitPosition.localPosition.z);

            // Find and replace the text for Lilac with Sonic. Ideally I'd add him into the cast list, but that sounds painful to do.
            GameObject.Find("CharacterName (1)").GetComponent<TextMesh>().text = "Sonic The\nHedgehog";

            // Replace Lilac's cutscene animator with Sonic's.
            GameObject.Find("Cutscene_Lilac").GetComponent<Animator>().runtimeAnimatorController = Plugin.sonicAssetBundle.LoadAsset<RuntimeAnimatorController>("Sonic Animator");

            // Hide Lilac's hair.
            GameObject.Find("Cutscene_Lilac").GetComponent<Animator>().transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;

            // Change Sonic's voice actor credit and add a voice line depending on the voice option.
            switch (Plugin.sonicVAOption.Value)
            {
                case 0:
                    GameObject.Find("ActorName (1)").GetComponent<TextMesh>().text = "Himself";
                    break;
                case 1:
                    GameObject.Find("ActorName (1)").GetComponent<TextMesh>().text = "Ryan\n Drummond";
                    ___castVoice[1] = Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory2_ryan");
                    break;
                case 2:
                    GameObject.Find("ActorName (1)").GetComponent<TextMesh>().text = "Jason\n Griffith";
                    ___castVoice[1] = Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory1");
                    break;
                case 3:
                    GameObject.Find("ActorName (1)").GetComponent<TextMesh>().text = "Roger Craig\n Smith";
                    ___castVoice[1] = Plugin.sonicAssetBundle.LoadAsset<AudioClip>("victory1_roger");
                    break;
            }

            // Slightly speed up the credits scrolling to make it so that a fully completed file will sync perfectly with His World.
            ___normalSpeed = -0.72f;
        }
    }
}
