using System;
using System.Collections.Generic;
using UnityEngine;
using WeaponCore;

namespace SmokelessPowder010
{
    // A standalone material commodity; no real-world preparation or production recipe.
    internal static class Catalog
    {
        internal const string Id = "local.deepspace010.material.smokeless_powder";
        internal const string SpriteKey = "smokeless_powder_010";
        internal static IEnumerable<ItemDefinition> Definitions()
        {
            yield return new ItemDefinition(Id, "Smokeless powder and primers material", Create);
        }
        private static GameItem Create()
        {
            var item = ItemDirectory.CreateEmptyItem(null);
            foreach (var mod in ModLoader.Mods)
            {
                if (mod?.Manifest?.ID != ColdWarModernExpansion.Entry.ModId) continue;
                Sprite original;
                if (mod.Sprites.TryGetValue(SpriteKey, out original) && original != null && original.pixelsPerUnit != 800f)
                {
                    var scaled = Sprite.Create(original.texture, original.rect, new Vector2(.5f, .5f), 800f);
                    scaled.name = original.name;
                    mod.Sprites[SpriteKey] = scaled;
                }
                break;
            }
            item.SetSprite(ColdWarModernExpansion.Entry.ModId, SpriteKey);
            item.useSpriteFromMod = true;
            Grid50.Apply(item, 1, 2);
            item.name = EnglishLocalization.L.Pick("无烟火药和底火", "Smokeless Powder and Primers");
            item.shortDescription = EnglishLocalization.L.Pick("一瓶封装的无烟火药，配着一盒整齐收好的底火。自有单件投入弹药制造台可兑换100火药与底火储量；是加工材料，不能直接当弹药使用。", "A sealed bottle of smokeless powder paired with a neatly packed box of primers. Deposit one owned item into the Ammunition Workbench for 100 powder-and-primer reserve. A production material, not usable ammunition.");
            item.SetGameItemType("MATERIAL");
            item.unitValue = 30;
            return item;
        }
    }
}
