namespace FP2_Sonic_Mod.Patchers
{
    internal class FPEventSequencePatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPEventSequence), "Action_StartEvent")]
        static void StupidAssEventAnimatorReplacement(FPEventSequence __instance)
        {
            // If we're Sonic dig through the activate and deactivate arrays to find and swap the animators.
            if (FPSaveManager.character == Plugin.sonicCharacterID)
            {
                foreach (FPBaseObject? obj in __instance.activateOnStart) HandleSwap(obj);
                foreach (FPBaseObject? obj in __instance.deactivateOnEnd) HandleSwap(obj);
            }

            static void HandleSwap(FPBaseObject? obj)
            {
                // Don't bother doing anything if we've somehow ended up with a null object.
                if (obj == null) return;

                // Get this object's animator.
                Animator animator = obj.GetComponent<Animator>();

                // Check that this object actually has an animator.
                if (animator != null)
                {
                    // Check that this animator is using Lilac's event one.
                    if (animator.runtimeAnimatorController.name == "Lilac")
                    {
                        // Kill the object for Lilac's hair.
                        if (obj.transform.GetChild(0).name == "tail")
                            obj.transform.GetChild(0).gameObject.SetActive(false);

                        // Swap to Sonic's event animator.
                        // TODO: Super Sonic is missing some poses so he reverts to normal in that case.
                        if (!FPPlayerPatcher.isSuper) animator.runtimeAnimatorController = Plugin.sonicAssetBundle.LoadAsset<RuntimeAnimatorController>("Event Sonic Animator");
                        else animator.runtimeAnimatorController = Plugin.sonicAssetBundle.LoadAsset<RuntimeAnimatorController>("Event Super Sonic Animator");
                    }
                }
            }
        }
    }
}
