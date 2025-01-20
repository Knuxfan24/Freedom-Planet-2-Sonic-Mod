namespace FP2_Sonic_Mod.Patchers
{
    internal class AcrabellePieTrapPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(AcrabellePieTrap), "Update")]
        private static void SwapPieIfSuper(ref Sprite[] ___characterBase, ref Sprite[] ___characterStruggle)
        {
            // Swap to the Super Sonic sprites if the player is super.
            if (FPPlayerPatcher.isSuper)
            {
                for (int i = 0; i < ___characterBase.Length; i++)
                    if (___characterBase[i] == Plugin.sonicPieNormal)
                        ___characterBase[i] = Plugin.superPieNormal;

                for (int i = 0; i < ___characterStruggle.Length; i++)
                    if (___characterStruggle[i] == Plugin.sonicPieStruggle)
                        ___characterStruggle[i] = Plugin.superPieStruggle;
            }

            // Swap back to the normal sprites if the player isn't super.
            else
            {
                for (int i = 0; i < ___characterBase.Length; i++)
                    if (___characterBase[i] == Plugin.superPieNormal)
                        ___characterBase[i] = Plugin.sonicPieNormal;

                for (int i = 0; i < ___characterStruggle.Length; i++)
                    if (___characterStruggle[i] == Plugin.superPieStruggle)
                        ___characterStruggle[i] = Plugin.sonicPieStruggle;
            }
        }
    }
}
