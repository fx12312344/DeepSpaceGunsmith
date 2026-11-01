using System.Collections.Generic;
using UnityEngine;
using WeaponCore;

namespace Rimfire22
{
    public sealed class Spec
    {
        public readonly string Key, Name;
        public readonly bool Rifle;
        public readonly int Fire, Style, Reliability, Recoil, Ergonomics, Accuracy, Price;
        public Spec(string key, string name, bool rifle, int fire, int style, int reliability, int recoil, int ergonomics, int accuracy, int price)
        { Key=key; Name=name; Rifle=rifle; Fire=fire; Style=style; Reliability=reliability; Recoil=recoil; Ergonomics=ergonomics; Accuracy=accuracy; Price=price; }
        public string Id => "rimfire22." + Key;
    }
    public static class Catalog
    {
        // The vanilla small pistol ammunition already uses game caliber 22 and NORMAL_AMMO.
        // It is a generic category, not explicitly identified as .22 LR; avoid a duplicate item.
        public const int Caliber = 22;
        public static readonly Spec[] Guns = {
            new Spec("markiv", "Ruger Mark IV 标准型手枪", false, 18, 30, 45, 20, 45, 45, 160),
            new Spec("p22", "Walther P22 手枪", false, 17, 30, 40, 20, 50, 35, 140),
            new Spec("buckmark", "Browning Buck Mark 靶射手枪", false, 18, 35, 45, 20, 40, 50, 180),
            new Spec("1022", "Ruger 10/22 半自动步枪", true, 25, 30, 50, 18, 45, 50, 220)
        };
        public static IEnumerable<ItemDefinition> Definitions()
        {
            foreach (var spec in Guns)
            {
                var s=spec;
                yield return new ItemDefinition(s.Id, ".22 LR " + s.Key, () => CreateGun(s));
                yield return new ItemDefinition(s.Id + ".magazine", ".22 LR " + s.Key + " magazine", () => CreateMagazine(s));
            }
        }
        private static void Sprite(GameItem item, string key)
        {
            foreach (var mod in ModLoader.Mods)
            {
                if (mod == null || mod.Manifest == null || mod.Manifest.ID != ColdWarModernExpansion.Entry.ModId) continue;
                UnityEngine.Sprite original;
                if (!mod.Sprites.TryGetValue(key, out original) || original == null) break;
                if (original.pixelsPerUnit != 800f)
                {
                    var scaled = UnityEngine.Sprite.Create(original.texture, original.rect, new Vector2(0.5f, 0.5f), 800f);
                    scaled.name = original.name;
                    mod.Sprites[key] = scaled;
                }
                break;
            }
            item.SetSprite(ColdWarModernExpansion.Entry.ModId, key);
            item.useSpriteFromMod = true;
        }
        private static GameItem CreateGun(Spec s)
        {
            var (window, stock, rail, magazine, underbarrel, barrel, special1, special2, special3) = GunModHelper.CreateGunModWindow(true, true, 5);
            var item=ItemDirectory.CreateEmptyItem(null);
            Sprite(item, "rimfire_" + s.Key);
            Grid50.Apply(item, s.Rifle ? 8 : 4, 3);
            item.SetContentWindow(window);
            item.name=EnglishLocalization.L.GunName(s.Key, s.Name);
            item.shortDescription=WeaponDescriptions.Text.Gun(s.Key);
            item.unitValue=s.Price;
            item.SetGameItemType("WEAPON"); item.SetGameItemType("FIREARM");
            if (s.Rifle) item.SetGameItemType("MIDLINE");
            ContrabandHelper.InitContrabandItem(item, 1);
            if (s.Rifle) AudioHelper.InitHeavyGun(item); else AudioHelper.InitPistol(item);
            GunHelper.InitGun(item, s.Fire, s.Style, s.Reliability, s.Recoil, s.Ergonomics, s.Accuracy, Caliber, 55, true);
            ColdWarModernExpansion.ExistingGunSlots.Configure(stock, rail, magazine, underbarrel, barrel, special1, special2, special3,
                true, s.Rifle, s.Rifle, true);
            GunHelper.SetEquipmentCondition(item, (ItemFeatureList.EquipmentCondition)2);
            return item;
        }
        private static GameItem CreateMagazine(Spec s)
        {
            var item=ItemDirectory.CreateEmptyItem(null);
            Sprite(item, "rimfire_" + s.Key + "_mag"); Grid50.Apply(item, 1, 2);
            var gunName=EnglishLocalization.L.GunName(s.Key, s.Name);
            item.name=ItemCopy.CommodityText.Name(s.Id + ".magazine", EnglishLocalization.L.English);
            item.shortDescription=ItemCopy.CommodityText.Description(s.Id + ".magazine", EnglishLocalization.L.English);
            item.unitValue=35;
            GunModHelper.InitGunModMagazine(item);
            GunModHelper.InitGunMod(item, 10, 0, 0, 0, 0, 0);
            GunModHelper.InitGunModBaseStats(item, 3, 0, 0, 0, 0, 0);
            GunModHelper.AllowOnlyWeapon(new[] { s.Id }, item);
            return item;
        }
    }
}
