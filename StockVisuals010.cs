using System;
using System.Collections.Generic;
using UnityEngine;

namespace Attachments010
{
    // OPTIONAL CANDIDATE ONLY. Call Bind(part,key) at attachment registration;
    // this file alone neither registers parts nor changes the distributed mod.
    // All other guns fail closed. The SKS overlays touch only a conservative
    // interior of the baked-in rear wood; their outline remains the original.
    // M1911 has no baked-in shoulder stock: its candidate is drawn entirely
    // outside the original 512px canvas, not over the pistol or its grip.
    // PNGs are aligned to assets/asset_010_sks.png (1600x600, 1250 PPU) and
    // assets/m1911.png (512x384, 800 PPU). Replacements/reskins, flips,
    // scale, renderer layer ordering, other mods and arbitrary overlapping
    // attachments have NOT been verified; do not claim universal safety.
    // A full-size overlay does not erase the old outline, so SKS is a stylized
    // material change, not an accurate mechanical stock replacement.
    // Test in game with all accessories and both inventory/world views before
    // release; remove a candidate rather than cover an accessory. Asset files
    // must be packaged separately; this module never modifies existing sprites.
    internal static class StockVisuals010
    {
        private static readonly HashSet<string> SksKeys = new HashSet<string>(StringComparer.Ordinal)
        { "light_stock", "telescopic_stock", "heavy_stock", "precision_stock", "sks_bump_stock" };
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>(StringComparer.Ordinal);

        internal static void Bind(GameItem part, string key)
        {
            if (part == null || (key != "m1911_stock" && !SksKeys.Contains(key))) return;
            var previous = part.parentItemSpriteModifierForItemFunc;
            part.parentItemSpriteModifierForItemFunc = (child, inv, slot, shape, state, parent) =>
            {
                var old = previous == null ? null : previous(child, inv, slot, shape, state, parent);
                var result = old == null ? new List<ItemSpriteModifier>() : new List<ItemSpriteModifier>(old);
                if (child == null || inv == null || parent == null || inv.GetParentItem() != parent) return result;
                bool sks = parent.identifier == "local.deepspace010.sks" && SksKeys.Contains(key);
                bool pistol = parent.identifier == "local.modernwars.m1911" && key == "m1911_stock";
                if (!sks && !pistol) return result;
                var sprite = FindOverlay(sks ? "asset_010_sks" : "m1911",
                    sks ? "stock_010_sks_" + key : "stock_010_m1911_m1911_stock", sks);
                if (sprite != null)
                    result.Add(new ItemSpriteModifier(sprite, sks ? Vector2.zero : new Vector2(-32f, 0f)));
                return result;
            };
        }

        private static Sprite FindOverlay(string baseKey, string layerKey, bool sks)
        {
            foreach (var mod in ModLoader.Mods)
            {
                if (mod?.Manifest?.ID != ColdWarModernExpansion.Entry.ModId) continue;
                Sprite original, layer, cached;
                if (!mod.Sprites.TryGetValue(baseKey, out original) || original == null || original == RenderHandler.unknownSprite ||
                    !mod.Sprites.TryGetValue(layerKey, out layer) || layer == null || layer == RenderHandler.unknownSprite) return null;
                int bw = sks ? 1600 : 512, bh = sks ? 600 : 384;
                int lw = sks ? 1600 : 768;
                if (original.rect.width != bw || original.rect.height != bh || layer.rect.width != lw || layer.rect.height != bh) return null;
                if (Cache.TryGetValue(layerKey, out cached) && cached != null && cached.texture == layer.texture && cached.rect == layer.rect) return cached;
                try
                {
                    // Safety belt: reject non-readable assets and any pixels beyond
                    // conservative SKS butt region / M1911 outside-canvas region.
                    var px = layer.texture.GetPixels32();
                    int x0 = (int)layer.rect.x, y0 = (int)layer.rect.y, tw = layer.texture.width;
                    bool visible = false;
                    for (int y = 0; y < bh; y++) for (int x = 0; x < lw; x++)
                    {
                        if (px[(y0+y)*tw+x0+x].a == 0) continue;
                        visible = true;
                        // y is bottom-origin, unlike PNG's top-origin.
                        int topY = bh - 1 - y;
                        if (sks ? (x < 88 || x > 451 || topY < 264 || topY > 438) : x >= 256) return null;
                    }
                    if (!visible) return null;
                    var sprite = Sprite.Create(layer.texture, layer.rect, new Vector2(.5f,.5f), sks ? 1250f : 800f);
                    sprite.name = layer.name;
                    Cache[layerKey] = sprite;
                    return sprite;
                }
                catch (Exception) { return null; }
            }
            return null;
        }
    }
}
