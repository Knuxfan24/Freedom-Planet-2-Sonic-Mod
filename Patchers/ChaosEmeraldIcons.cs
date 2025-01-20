namespace FP2_Sonic_Mod.Patchers
{
    internal class ChaosEmeraldIcons
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPPauseMenu), "Start")]
        private static void PauseMenu(ref FPHudDigit[] ___itemIcon)
        {
            foreach (FPHudDigit item in ___itemIcon)
                ReplaceIcon(item);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPResultsMenu), "Start")]
        private static void ResultsMenu(ref FPHudDigit[] ___hudItemIcon)
        {
            foreach (FPHudDigit itemIcon in ___hudItemIcon)
                ReplaceIcon(itemIcon);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MenuFile), "Start")]
        private static void FileMenu(ref MenuFilePanel[] ___files)
        {
            foreach (MenuFilePanel file in ___files)
                foreach (FPHudDigit itemIcon in file.itemIcon)
                    ReplaceIcon(itemIcon);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuGlobalPause), "Start")]
        private static void GlobalPause(ref FPHudDigit ___powerupIcon) => ReplaceIcon(___powerupIcon);

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuItemSelect), "Start")]
        private static void ItemSelect(ref FPHudDigit ___pfPowerupIcon) => ReplaceIcon(___pfPowerupIcon);

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuItemSet), "Start")]
        private static void ItemSet(ref FPHudDigit ___pfPowerupIcon) => ReplaceIcon(___pfPowerupIcon);

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MenuWorldMapConfirm), "Start")]
        private static void WorldMap(ref FPHudDigit[] ___itemIcon)
        {
            foreach (FPHudDigit itemIcon in ___itemIcon)
                ReplaceIcon(itemIcon);
        }

        private static void ReplaceIcon(FPHudDigit itemIcon)
        {
            if (itemIcon == null)
                return;

            itemIcon.GetComponent<FPHudDigit>().digitFrames[31] = Plugin.sonicChaosEmeralds;

            // As the sprite is already set, we need to check the digit value and reset it to update the sprite.
            if (itemIcon.digitValue == 31)
                itemIcon.SetDigitValue(31);
        }
    }
}
