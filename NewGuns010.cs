using System;
using System.Collections.Generic;
using UnityEngine;
using WeaponCore;

// The caller registers these definitions and supplies the corresponding sprites.
namespace NewGuns010
{
    public enum Category { Pistol, Smg, Rifle, Shotgun }

    public static class Catalog
    {
        private sealed class Spec
        {
            public readonly string Key, Name, Description;
            public readonly Category Category;
            public readonly int Width, Caliber, Tier, Firepower, Style, Reliability, Recoil, Ergonomics, Accuracy, Price;
            public string Id => "local.deepspace010." + Key;
            public Spec(string key, string name, string description, Category category, int width, int caliber, int tier,
                int firepower, int style, int reliability, int recoil, int ergonomics, int accuracy, int price)
            {
                Key=key; Name=name; Description=description; Category=category; Width=width; Caliber=caliber; Tier=tier;
                Firepower=firepower; Style=style; Reliability=reliability; Recoil=recoil; Ergonomics=ergonomics; Accuracy=accuracy; Price=price;
            }
        }

        private static readonly Spec[] Guns = {
            new Spec("winchester37", "Winchester Model 37", "单管单发霰弹枪，使用12/70霰弹。", Category.Shotgun, 8, 1270, 1, 55,20,45,25,35,25,120),
            new Spec("fp45", "FP-45 Liberator", "单发手枪，使用.45 ACP弹药。", Category.Pistol, 4, 204502, 1, 20,10,15,20,20,12,60),
            new Spec("texas_defender", "Bond Arms Texas Defender", "双管德林杰手枪，使用.45 Colt弹药。", Category.Pistol, 4, 204501, 1, 28,35,50,20,35,25,160),
            new Spec("tt33", "TT-33 Tokarev", "半自动手枪，使用7.62×25毫米托卡列夫弹药。", Category.Pistol, 4, 207625, 2, 30,30,50,30,45,40,180),
            new Spec("svt40", "SVT-40", "半自动步枪，使用7.62×54R弹药。", Category.Rifle, 8, 207654, 2, 60,45,55,35,45,60,360),
            new Spec("springfield1903", "Springfield M1903", "栓动步枪，使用.30-06 Springfield弹药。", Category.Rifle, 8, 203006, 2, 55,45,65,30,35,70,320),
            new Spec("beretta92fs", "Beretta 92FS", "半自动手枪，使用9×19毫米弹药。", Category.Pistol, 4, 919, 2, 35,45,70,45,65,55,260),
            new Spec("sks", "SKS Carbine", "半自动卡宾枪，使用7.62×39毫米弹药。", Category.Rifle, 8, 76239, 2, 55,40,70,45,50,55,300),
            new Spec("mini14", "Ruger Mini-14 Ranch Rifle", "半自动步枪，使用5.56×45毫米NATO弹药。", Category.Rifle, 8, 55645, 2, 55,45,65,55,65,60,340),
            new Spec("mp40", "MP 40", "冲锋枪，使用9×19毫米弹药。", Category.Smg, 5, 919, 3, 55,45,65,45,50,45,320),
            new Spec("m4a1", "M4A1", "选择射击卡宾枪，使用5.56×45毫米NATO弹药。", Category.Rifle, 8, 55645, 3, 70,65,75,55,70,75,500)
        };

        public static IEnumerable<ItemDefinition> Definitions()
        {
            foreach (var spec in Guns)
            {
                var gun = spec;
                yield return new ItemDefinition(gun.Id, gun.Key + " gun", () => Create(gun));
            }
        }

        public static bool TryGetCategory(string id, out Category category)
        {
            foreach (var gun in Guns)
                if (String.Equals(gun.Id, id, StringComparison.Ordinal)) { category=gun.Category; return true; }
            category=default(Category);
            return false;
        }

        private static GameItem Create(Spec g)
        {
            var item = ItemDirectory.CreateEmptyItem(null);
            string key = "asset_010_" + g.Key;
            // Match legacy 128 px/cell at 800 PPU using 200 px/cell at 1250 PPU; grid size stays independent.
            foreach (var mod in ModLoader.Mods)
            {
                if (mod == null || mod.Manifest == null || mod.Manifest.ID != ColdWarModernExpansion.Entry.ModId) continue;
                Sprite original;
                if (mod.Sprites.TryGetValue(key, out original) && original != null && original.pixelsPerUnit != 1250f)
                {
                    var scaled = Sprite.Create(original.texture, original.rect, new Vector2(0.5f, 0.5f), 1250f);
                    scaled.name = original.name;
                    mod.Sprites[key] = scaled;
                }
                break;
            }
            item.SetSprite(ColdWarModernExpansion.Entry.ModId, key);
            item.useSpriteFromMod = true;
            Grid50.Apply(item, g.Width, 3);
            item.name = EnglishLocalization.L.GunName(g.Key, g.Name);
            item.shortDescription = WeaponDescriptions.Text.Gun(g.Key);
            item.SetGameItemType("WEAPON");
            item.SetGameItemType("FIREARM");
            if (g.Category != Category.Pistol) item.SetGameItemType("MIDLINE");
            ContrabandHelper.InitContrabandItem(item, g.Tier);
            if (g.Category == Category.Pistol) AudioHelper.InitPistol(item); else AudioHelper.InitHeavyGun(item);
            item.unitValue = g.Price;
            GunHelper.InitGun(item, g.Firepower, g.Style, g.Reliability, g.Recoil, g.Ergonomics, g.Accuracy, g.Caliber, 40, true);
            var (window, stock, rail, magazine, underbarrel, barrel, special1, special2, special3) = GunModHelper.CreateGunModWindow(true, true, 5);
            item.SetContentWindow(window);
            // Enable after attaching the window: the original helpers capture the slot's parent.
            GunModHelper.EnableRailSlot(rail);
            GunModHelper.EnableBarrelSlot(barrel);
            GunModHelper.EnableSpecialSlot(special1);
            GunModHelper.EnableSpecialSlot(special2);
            GunModHelper.EnableSpecialSlot(special3);
            if (g.Category == Category.Rifle || g.Category == Category.Shotgun)
            {
                GunModHelper.EnableStockSlot(stock);
                GunModHelper.EnableUnderbarrelSlot(underbarrel);
            }
            // Null slot callbacks otherwise default to accepting arbitrary items.
            // Block the slots this prototype intentionally does not expose.
            magazine.mayInventoryAddItemFunc = (mod, inv) => false;
            magazine.maxSlotAddItemFunc = (mod, inv, marker) => 0;
            if (g.Category == Category.Pistol || g.Category == Category.Smg) {
                stock.mayInventoryAddItemFunc = (mod, inv) => false;
                stock.maxSlotAddItemFunc = (mod, inv, marker) => 0;
                underbarrel.mayInventoryAddItemFunc = (mod, inv) => false;
                underbarrel.maxSlotAddItemFunc = (mod, inv, marker) => 0;
            }
            GunHelper.SetEquipmentCondition(item, (ItemFeatureList.EquipmentCondition)2);
            return item;
        }
    }
}
