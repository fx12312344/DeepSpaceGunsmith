using System;
using System.Collections.Generic;
using UnityEngine;

namespace Attachments010
{
    // Candidate masks are not guaranteed to identify metal. Placement and draw
    // order need in-game verification. Missing, unreadable or empty masks are ignored.
    internal static class PlatingVisuals010
    {
        private const string GoldId = "local.deepspace010.attachment.gold_plating";
        private static readonly Dictionary<string, string> Assets = BuildAssets();
        private sealed class CacheEntry
        {
            internal Texture2D OriginalTexture, LayerTexture;
            internal Sprite Original, Layer, Result;
            internal Rect OriginalRect, LayerRect;
            internal float Ppu;
        }
        private static readonly List<CacheEntry> Cache = new List<CacheEntry>();

        // ModLoader discovers lifecycle patches even if the module is not queried
        // while disabled. Clear references only; sprites may still be in use.
        [HarmonyLib.HarmonyPatch(typeof(ColdWarModernExpansion.Entry), nameof(ColdWarModernExpansion.Entry.OnEnable))]
        private static class EnableCacheReset { private static void Prefix() { Cache.Clear(); } }
        [HarmonyLib.HarmonyPatch(typeof(ColdWarModernExpansion.Entry), nameof(ColdWarModernExpansion.Entry.OnDisable))]
        private static class DisableCacheReset { private static void Prefix() { Cache.Clear(); } }

        private static Dictionary<string, string> BuildAssets()
        {
            var map = new Dictionary<string, string>(StringComparer.Ordinal);
            Add(map, "earlyfirearms.", "", "hand_cannon matchlock wheellock blunderbuss brown_bess charleville naval_pistol baker hall_1819 dreyse colt_1851 mississippi_1841");
            Add(map, "local.modernwars.", "", "colt_saa m1911 winchester1873 mosin1891 kar98k m1garand thompson1928 ppsh41 winchester1897 bar1918 mg42 stg44");
            Add(map, "coldwarmodern.", "", "m16a1 fnfal mp5 p90 sw629 rem870 m24 m82a1 m249 m60");
            Add(map, "rimfire22.", "rimfire_", "markiv p22 buckmark 1022");
            Add(map, "local.deepspace010.", "asset_010_", "winchester37 fp45 texas_defender tt33 svt40 springfield1903 beretta92fs sks mini14 mp40 m4a1");
            map.Add("ak47prototype.rifle", "ak47_new");
            map.Add("glock17prototype.pistol", "glock17");
            return map;
        }
        private static void Add(Dictionary<string, string> map, string prefix, string assetPrefix, string keys)
        {
            foreach (string key in keys.Split(' ')) map.Add(prefix + key, assetPrefix + key);
        }

        internal static void Bind(GameItem part, string key)
        {
            bool gold = key == "gold_plating";
            if (part == null || (!gold && key != "silver_plating")) return;
            var previous = part.parentItemSpriteModifierForItemFunc;
            part.parentItemSpriteModifierForItemFunc = (child, inv, slot, shape, state, parent) =>
            {
                var old = previous == null ? null : previous(child, inv, slot, shape, state, parent);
                var result = old == null ? new List<ItemSpriteModifier>() : new List<ItemSpriteModifier>(old);
                string asset;
                if (parent == null || child == null || inv == null || inv.GetParentItem() != parent ||
                    !Assets.TryGetValue(parent.identifier, out asset) || !parent.useSpriteFromMod ||
                    parent.spriteAtlasPath != ColdWarModernExpansion.Entry.ModId || parent.spritePath != asset) return result;
                float ppu = parent.identifier.StartsWith("local.deepspace010.", StringComparison.Ordinal) ? 1250f : 800f;
                // Gold overrides silver only when its overlay exists and is usable.
                if (!gold)
                    foreach (GameItem other in GraphUtils.FindAllChildrenType<GameItem>(parent))
                        if (other != null && other.IsID(GoldId) && other.parentInventory != null &&
                            other.parentInventory.GetParentItem() == parent && FindOverlay(asset, true, ppu) != null) return result;
                Sprite overlay = FindOverlay(asset, gold, ppu);
                if (overlay != null) result.Add(new ItemSpriteModifier(overlay, Vector2.zero));
                return result;
            };
        }

        private static Sprite FindOverlay(string asset, bool gold, float ppu)
        {
            string layerKey = "plating_" + (gold ? "gold_" : "silver_") + asset;
            foreach (var mod in ModLoader.Mods)
            {
                if (mod?.Manifest?.ID != ColdWarModernExpansion.Entry.ModId) continue;
                Sprite original, layer;
                if (!mod.Sprites.TryGetValue(asset, out original) || original == null || original == RenderHandler.unknownSprite ||
                    !mod.Sprites.TryGetValue(layerKey, out layer) || layer == null || layer == RenderHandler.unknownSprite) return null;
                try
                {
                    if (layer.rect.width != original.rect.width || layer.rect.height != original.rect.height) return null;
                    var originalTexture = original.texture;
                    var layerTexture = layer.texture;
                    if (originalTexture == null || layerTexture == null || !layerTexture.isReadable) return null;
                    Rect originalRect = original.rect, layerRect = layer.rect;
                    foreach (var entry in Cache)
                        if (ReferenceEquals(entry.Original, original) && ReferenceEquals(entry.Layer, layer) &&
                            ReferenceEquals(entry.OriginalTexture, originalTexture) && ReferenceEquals(entry.LayerTexture, layerTexture) &&
                            entry.OriginalRect == originalRect && entry.LayerRect == layerRect && entry.Ppu == ppu &&
                            (ReferenceEquals(entry.Result, null) || entry.Result != null)) return entry.Result;
                    // GetPixels32 may fail on non-readable textures; fail closed rather
                    // than pretending an unverified candidate is usable.
                    Color32[] pixels = layer.texture.GetPixels32();
                    int width = layer.texture.width;
                    int x0 = (int)layer.rect.x, y0 = (int)layer.rect.y;
                    int x1 = x0 + (int)layer.rect.width, y1 = y0 + (int)layer.rect.height;
                    bool visible = false;
                    for (int y = y0; y < y1 && !visible; y++)
                        for (int x = x0; x < x1; x++)
                            if (pixels[y * width + x].a != 0) { visible = true; break; }
                    Sprite sprite = null;
                    if (visible)
                    {
                        sprite = Sprite.Create(layerTexture, layerRect, new Vector2(.5f, .5f), ppu);
                        if (sprite == null) return null;
                        sprite.name = layer.name;
                    }
                    Cache.Add(new CacheEntry { Original = original, Layer = layer, OriginalTexture = originalTexture,
                        LayerTexture = layerTexture, OriginalRect = originalRect, LayerRect = layerRect, Ppu = ppu, Result = sprite });
                    return sprite;
                }
                catch (Exception) { return null; }
            }
            return null;
        }
    }
}
