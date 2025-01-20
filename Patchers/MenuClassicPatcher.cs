namespace FP2_Sonic_Mod.Patchers
{
    internal class MenuClassicPatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuClassic), "Start")]
        private static void ResetRocketWispCheck() => FPPlayerPatcher.UsedRocketWisp = false;
    }
}
