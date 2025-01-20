namespace FP2_Sonic_Mod.Patchers
{
    internal class SagaPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Saga), "State_Default")]
        private static void SwapIfSuper(ref Animator ___animator)
        {
            // If we're not Sonic, then don't make the edits.
            if (FPSaveManager.character != Plugin.sonicCharacterID)
                return;

            if (FPPlayerPatcher.isSuper)
            {
                if (___animator.runtimeAnimatorController.name == "Saga Sonic")
                    ___animator.runtimeAnimatorController = Plugin.sonicAssetBundle.LoadAsset<RuntimeAnimatorController>("Saga Super Sonic");
                if (___animator.runtimeAnimatorController.name == "Syntax Saga Sonic")
                    ___animator.runtimeAnimatorController = Plugin.sonicAssetBundle.LoadAsset<RuntimeAnimatorController>("Syntax Saga Super Sonic");
            }

            else
            {
                if (___animator.runtimeAnimatorController.name == "Saga Super Sonic")
                    ___animator.runtimeAnimatorController = Plugin.sonicAssetBundle.LoadAsset<RuntimeAnimatorController>("Saga Sonic");
                if (___animator.runtimeAnimatorController.name == "Syntax Saga Super Sonic")
                    ___animator.runtimeAnimatorController = Plugin.sonicAssetBundle.LoadAsset<RuntimeAnimatorController>("Syntax Saga Sonic");
            }
        }
    }
}
