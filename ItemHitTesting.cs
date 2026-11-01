using HarmonyLib;

namespace WeaponCore
{
    // Applied only to this mod's sprites; never change native or other mods' items.
    // The engine normally rejects transparent sprite pixels, leaving large parts
    // of inventory cells unclickable. Rectangle raycasts avoid that alpha test.
    public static class ItemHitTesting
    {
        public static void Apply(GameItem item)
        {
            if (item == null || item.spriteAtlasPath != ColdWarModernExpansion.Entry.ModId) return;
            var visual = item as GameItemElement;
            if (visual == null) return;
            if (visual.background != null && visual.background.image != null)
                visual.background.image.alphaHitTestMinimumThreshold = 0f;
            if (visual.fakeBackground != null && visual.fakeBackground.image != null)
                visual.fakeBackground.image.alphaHitTestMinimumThreshold = 0f;
        }
    }
    // The two sprite setters use different paths in the game. Both are patched so
    // existing saved items and the machine/barrel factories receive the same fix.
    [HarmonyPatch(typeof(GameItem), nameof(GameItem.SetSprite), new[] { typeof(string), typeof(string) })]
    public static class ModItemSpriteHitTestPatch
    {
        public static void Postfix(GameItem __instance) => ItemHitTesting.Apply(__instance);
    }
    [HarmonyPatch(typeof(GameItem), nameof(GameItem.SetSpriteAndShapeFromMod))]
    public static class ModShapeSpriteHitTestPatch
    {
        public static void Postfix(GameItem __instance) => ItemHitTesting.Apply(__instance);
    }
}
