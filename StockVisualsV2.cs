using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace Attachments010
{
    // OPTIONAL, not registered. Package stock-v2 PNGs as sprites under their file
    // basenames; call Bind once per stock item, AFTER any other callbacks.
    // This is a conservative inset recolour, NOT a cut-out/replacement of the
    // original outline. Only two exact source images are supported. No global
    // sprite dictionary mutation, no persisted changes, fail closed on mismatch.
    internal static class StockVisualsV2
    {
        private sealed class Gun
        {
            internal string Id, SpriteKey, PixelHash; internal int W, H, X0, Y0, X1, Y1; internal float Ppu;
            internal Gun(string id, string key, string hash, int w, int h, int x0, int y0, int x1, int y1, float ppu)
            { Id=id; SpriteKey=key; PixelHash=hash; W=w; H=h; X0=x0; Y0=y0; X1=x1; Y1=y1; Ppu=ppu; }
        }
        private static readonly Gun[] Guns = {
            new Gun("coldwarmodern.rem870", "rem870", "e42b7d25532affd349a89a5e94c036aa35d3a283835489a00f0c1bb541aae2f4", 1024,384,55,163,245,220,800f),
            new Gun("local.deepspace010.mini14", "asset_010_mini14", "d37487da3697e2486be64bd9b5a42f5741b4876654fbbfb21706f0450c34f64b", 1600,600,115,274,350,351,1250f)
        };
        private static readonly HashSet<string> Keys = new HashSet<string>(StringComparer.Ordinal) {
            "light_stock", "telescopic_stock", "heavy_stock", "precision_stock"
        };
        private sealed class HashEntry
        {
            internal Texture2D Texture; internal Rect Rect; internal string Hash;
        }
        private sealed class CacheEntry
        {
            internal Gun Gun; internal Sprite Source, Layer, Result;
            internal Texture2D SourceTexture, LayerTexture;
            internal Rect SourceRect, LayerRect; internal float SourcePpu;
        }
        private static readonly List<HashEntry> HashCache = new List<HashEntry>();
        private static readonly List<CacheEntry> Cache = new List<CacheEntry>();
        private static void ClearCaches() { Cache.Clear(); HashCache.Clear(); }
        [HarmonyLib.HarmonyPatch(typeof(ColdWarModernExpansion.Entry), nameof(ColdWarModernExpansion.Entry.OnEnable))]
        private static class EnableCacheReset { private static void Prefix() { ClearCaches(); } }
        [HarmonyLib.HarmonyPatch(typeof(ColdWarModernExpansion.Entry), nameof(ColdWarModernExpansion.Entry.OnDisable))]
        private static class DisableCacheReset { private static void Prefix() { ClearCaches(); } }
        internal static void Bind(GameItem part, string key)
        {
            if (part == null || !Keys.Contains(key)) return;
            var previous = part.parentItemSpriteModifierForItemFunc;
            part.parentItemSpriteModifierForItemFunc = (child, inv, slot, shape, state, parent) => {
                var old = previous == null ? null : previous(child, inv, slot, shape, state, parent);
                var result = old == null ? new List<ItemSpriteModifier>() : new List<ItemSpriteModifier>(old);
                if (child == null || inv == null || parent == null || !(parent is GameItemElement) || inv.GetParentItem() != parent) return result;
                foreach (var gun in Guns) {
                    if (parent.identifier != gun.Id || !parent.useSpriteFromMod || parent.spriteAtlasPath != ColdWarModernExpansion.Entry.ModId || parent.spritePath != gun.SpriteKey) continue;
                    var sprite = Find(gun, key);
                    if (sprite != null) result.Add(new ItemSpriteModifier(sprite, Vector2.zero));
                    break;
                }
                return result;
            };
        }
        private static Sprite Find(Gun gun, string key)
        {
            try {
                foreach (var mod in ModLoader.Mods) {
                    if (mod?.Manifest?.ID != ColdWarModernExpansion.Entry.ModId) continue;
                    string layerKey = "stock_v2_" + gun.SpriteKey + "_" + key;
                    Sprite source, layer;
                    if (!mod.Sprites.TryGetValue(gun.SpriteKey, out source) || !mod.Sprites.TryGetValue(layerKey, out layer) || source == null || layer == null || source == RenderHandler.unknownSprite || layer == RenderHandler.unknownSprite) return null;
                    if (source.rect.width != gun.W || source.rect.height != gun.H || source.pixelsPerUnit != gun.Ppu || layer.rect.width != gun.W || layer.rect.height != gun.H) return null;
                    var sourceTexture = source.texture; var layerTexture = layer.texture;
                    if (sourceTexture == null || layerTexture == null || !sourceTexture.isReadable || !layerTexture.isReadable) return null;
                    Rect sourceRect = source.rect, layerRect = layer.rect;
                    string hash = null;
                    foreach (var entry in HashCache)
                        if (ReferenceEquals(entry.Texture, sourceTexture) && entry.Rect == sourceRect) { hash = entry.Hash; break; }
                    if (hash == null) {
                        hash = HashPixels(source, gun.W, gun.H);
                        HashCache.Add(new HashEntry { Texture = sourceTexture, Rect = sourceRect, Hash = hash });
                    }
                    if (hash != gun.PixelHash) return null;
                    foreach (var entry in Cache)
                        if (ReferenceEquals(entry.Gun, gun) && ReferenceEquals(entry.Source, source) && ReferenceEquals(entry.Layer, layer) &&
                            ReferenceEquals(entry.SourceTexture, sourceTexture) && ReferenceEquals(entry.LayerTexture, layerTexture) &&
                            entry.SourceRect == sourceRect && entry.LayerRect == layerRect && entry.SourcePpu == source.pixelsPerUnit &&
                            (ReferenceEquals(entry.Result, null) || entry.Result != null)) return entry.Result;
                    var pixels = layerTexture.GetPixels32(); int ox=(int)layerRect.x, oy=(int)layerRect.y, tw=layerTexture.width; bool visible=false, invalid=false;
                    for (int y=0; y<gun.H && !invalid; y++) for (int x=0; x<gun.W; x++) {
                        if (pixels[(oy+y)*tw+ox+x].a == 0) continue;
                        int top = gun.H-1-y; visible=true;
                        if (x<gun.X0 || x>=gun.X1 || top<gun.Y0 || top>=gun.Y1) { invalid=true; break; }
                    }
                    Sprite result = null;
                    if (visible && !invalid) {
                        result=Sprite.Create(layerTexture, layerRect, new Vector2(.5f,.5f), gun.Ppu);
                        if (result == null) return null;
                        result.name=layer.name;
                    }
                    Cache.Add(new CacheEntry { Gun = gun, Source = source, Layer = layer, SourceTexture = sourceTexture,
                        LayerTexture = layerTexture, SourceRect = sourceRect, LayerRect = layerRect, SourcePpu = source.pixelsPerUnit, Result = result });
                    return result;
                }
            } catch (Exception) { /* unreadable/compressed/mismatching assets: omit */ }
            return null;
        }
        private static string HashPixels(Sprite sprite, int w, int h)
        {
            var pixels=sprite.texture.GetPixels32(); int ox=(int)sprite.rect.x, oy=(int)sprite.rect.y, tw=sprite.texture.width;
            var bytes=new byte[w*h*4]; int i=0;
            for (int y=h-1; y>=0; y--) for (int x=0; x<w; x++) {
                var c=pixels[(oy+y)*tw+ox+x]; bytes[i++]=c.r; bytes[i++]=c.g; bytes[i++]=c.b; bytes[i++]=c.a;
            }
            using (var sha=SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
        }
    }
}
