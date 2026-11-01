using System;
using System.Collections.Generic;
using UnityEngine;
using WeaponCore;

namespace MixedAmmoMaterials010
{
    // Mixed-size component commodities; no ammunition loading or production logic.
    internal static class Catalog
    {
        private sealed class Spec
        {
            internal readonly string Key, Zh, En, DescriptionZh, DescriptionEn;
            internal Spec(string key, string zh, string en, string dzh, string den)
            { Key=key; Zh=zh; En=en; DescriptionZh=dzh; DescriptionEn=den; }
            internal string Id => "local.deepspace010.material." + Key;
            internal string SpriteKey => "material_" + Key + "_010";
        }
        private static readonly Spec[] Items = {
            new Spec("casings", "弹壳堆", "Pile of Cartridge Cases", "一堆规格混杂的空弹壳，金属边缘碰在一起叮当作响。自有单件投入弹药制造台可兑换100弹壳储量；只是加工材料，不是成品弹药。", "An assortment of empty cartridge cases whose metal rims clink together. Deposit one owned pile into the Ammunition Workbench for 100 case reserve. A production material, not finished ammunition."),
            new Spec("projectiles", "弹头堆", "Pile of Projectiles", "一堆规格混杂的弹头，小小的金属尖头挤成一团。自有单件投入弹药制造台可兑换100弹头储量；只是加工材料，不是成品弹药。", "An assortment of loose projectiles, their small metal tips crowded together. Deposit one owned pile into the Ammunition Workbench for 100 projectile reserve. A production material, not finished ammunition.")
        };
        internal static IEnumerable<ItemDefinition> Definitions()
        {
            foreach (var spec in Items) { var s=spec; yield return new ItemDefinition(s.Id, s.En + " material", () => Create(s)); }
        }
        private static GameItem Create(Spec s)
        {
            var item=ItemDirectory.CreateEmptyItem(null);
            foreach (var mod in ModLoader.Mods)
            {
                if (mod?.Manifest?.ID != ColdWarModernExpansion.Entry.ModId) continue;
                Sprite original;
                if (mod.Sprites.TryGetValue(s.SpriteKey, out original) && original != null && original.pixelsPerUnit != 800f)
                {
                    var scaled=Sprite.Create(original.texture,original.rect,new Vector2(.5f,.5f),800f);
                    scaled.name=original.name; mod.Sprites[s.SpriteKey]=scaled;
                }
                break;
            }
            item.SetSprite(ColdWarModernExpansion.Entry.ModId,s.SpriteKey);
            item.useSpriteFromMod=true;
            Grid50.Apply(item,2,1);
            item.name=EnglishLocalization.L.Pick(s.Zh,s.En);
            item.shortDescription=EnglishLocalization.L.Pick(s.DescriptionZh,s.DescriptionEn);
            item.SetGameItemType("MATERIAL");
            item.unitValue=s.Key == "casings" ? 10 : 15;
            return item;
        }
    }
}
