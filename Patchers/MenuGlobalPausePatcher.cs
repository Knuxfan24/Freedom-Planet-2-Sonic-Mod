using BepInEx.Bootstrap;

namespace FP2_Sonic_Mod.Patchers
{
    internal class MenuGlobalPausePatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MenuGlobalPause), "Start")]
        static void SonicModEmeraldTracker(MenuGlobalPause __instance)
        {
            // Only bother doing this if the Archipelago mod is installed.
            if (!Chainloader.PluginInfos.ContainsKey("K24_FP2_Archipelago"))
                return;

            // Find the Inventory window.
            var inventoryWindow = __instance.windows.transform.GetChild(0);

            // Load the gameplay emerald sprites.
            var chaosEmeraldSprites = Plugin.sonicAssetBundle.LoadAssetWithSubAssets<Sprite>("chaos_emeralds_gameplay");

            // Create the various emeralds.
            CreateEmerald("Red Chaos Emerald", 5, 283);
            CreateEmerald("Blue Chaos Emerald", 0, 305);
            CreateEmerald("Yellow Chaos Emerald", 6, 327);
            CreateEmerald("Green Chaos Emerald", 2, 349);
            CreateEmerald("White Chaos Emerald", 3, 371);
            CreateEmerald("Cyan Chaos Emerald", 1, 393);
            CreateEmerald("Purple Chaos Emerald", 4, 415);

            void CreateEmerald(string name, int spriteIndex, int xPosition)
            {
                // Create the game object for this emerald.
                GameObject emerald = new(name);

                // Add a Sprite Renderer to this emerald.
                SpriteRenderer emeraldSprite = emerald.AddComponent<SpriteRenderer>();
                emeraldSprite.sprite = chaosEmeraldSprites[spriteIndex];
                emeraldSprite.sortingOrder = 25;
                emeraldSprite.color = new(1, 1, 1, 0.25f);

                // Set this emerald to the UI layer.
                emerald.layer = inventoryWindow.gameObject.layer;

                // Set the position of this emerald.
                emerald.transform.parent = inventoryWindow.transform;
                emerald.transform.position = inventoryWindow.transform.position;
                emerald.transform.localPosition = new(xPosition, -24, 0);
            }
        }
    }
}
