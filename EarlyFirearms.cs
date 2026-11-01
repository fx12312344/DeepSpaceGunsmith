using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using WeaponCore;

namespace EarlyFirearms
{
    public sealed class Entry
    {
        public const string ModId = "local.earlyfirearms";
        public const string AmmoId = "earlyfirearms.paper_cartridge";
        public const int SharedCaliber = 810001;
        private bool hooked;
        private ModLog log;
        public void Init(ModManifest manifest) { log = new ModLog(manifest); }
        public void OnEnable()
        {
            if (hooked) return;
            ModHook.OnModItemDirectoryInit += Register;
            foreach (var spec in Catalog.All)
            {
                var s = spec;
                ModHelper.AddDebugFunction(ModId, ModHelper.DEBUG_CATEGORY_SPAWN, s.Button, () => Registry.SpawnOwnedToInventory(s.Id, log), false);
            }
            ModHelper.AddDebugFunction(ModId, ModHelper.DEBUG_CATEGORY_SPAWN, "13 Paper cartridges", () => Registry.SpawnOwnedToInventory(AmmoId, log), false);
            hooked = true;
            foreach (var dir in Resources.FindObjectsOfTypeAll<ModItemDirectory>())
                if (dir != null && dir.factoryDictionary != null) Register(dir);
            log.Log("Early firearms registered.");
        }
        public void OnDisable()
        {
            if (!hooked) return;
            ModHook.OnModItemDirectoryInit -= Register;
            ModHelper.RemoveDebugFunctions(ModId);
            hooked = false;
        }
        private void Register(ModItemDirectory dir)
        {
            var definitions = new List<ItemDefinition>();
            foreach (var s in Catalog.All)
            {
                var spec = s;
                definitions.Add(new ItemDefinition(spec.Id, spec.Button, () => Catalog.Create(spec, false)));
                if (Catalog.NeedsTradeVariant(spec))
                    definitions.Add(new ItemDefinition(spec.Id + ".trade_test", spec.Button + " Trade edition", () => Catalog.Create(spec, true)));
            }
            definitions.Add(new ItemDefinition(AmmoId, "13 Paper cartridges", Catalog.CreateAmmo));
            Registry.Register(dir, definitions, log);
        }
    }
    public sealed class Spec
    {
        public string Id, Button, Name, Sprite, Era, CaliberLabel;
        public int Length, Height, Price, Caliber, Fire, Style, Reliability, Recoil, Ergonomics, Accuracy;
        public bool Pistol, DimensionsVerified;
        public Spec(string key, string button, string name, string era, int length, int height, int price, int caliber, string caliberLabel, bool pistol, bool dimensionsVerified, int fire, int style, int reliability, int recoil, int ergonomics, int accuracy)
        { Id="earlyfirearms."+key; Sprite=key; Button=button; Name=name; Era=era; Length=length; Height=height; Price=price; Caliber=caliber; CaliberLabel=caliberLabel; Pistol=pistol; DimensionsVerified=dimensionsVerified; Fire=fire; Style=style; Reliability=reliability; Recoil=recoil; Ergonomics=ergonomics; Accuracy=accuracy; }
    }
    public static class Catalog
    {
        // Approximate heights and most lengths are placeholders, not museum measurements.
        public static readonly Spec[] All = {
          new Spec("hand_cannon","01 Hand cannon","中世纪手铳（古董）","14—15世纪",600,100,800,810001,"纸包火药和弹丸",false,false,5,25,3,75,5,3),
          new Spec("matchlock","02 Matchlock","荷兰火绳枪（古董）","约1637—1688年",1530,250,900,810001,"纸包火药和弹丸",false,true,8,30,6,70,10,8),
          new Spec("wheellock","03 Wheellock pistol","荷兰簧轮手枪（古董）","约1630年",531,160,1100,810001,"纸包火药和弹丸",true,false,7,35,8,65,20,6),
          new Spec("blunderbuss","04 Blunderbuss","燧发短铳（古董）","约18世纪",760,200,950,810001,"纸包火药和弹丸",false,false,18,30,10,75,15,5),
          new Spec("brown_bess","05 Brown Bess","布朗·贝斯火枪（古董）","约18世纪",1500,220,850,810001,"纸包火药和弹丸",false,false,14,25,12,65,12,12),
          new Spec("charleville","06 Charleville 1766","夏勒维尔1766火枪（古董）","1766年",1520,220,950,810001,"纸包火药和弹丸",false,false,14,30,12,65,12,14),
          new Spec("naval_pistol","07 Naval pistol","海军燧发手枪（古董）","约1790年",500,180,950,810001,"纸包火药和弹丸",true,false,10,35,12,65,25,10),
          new Spec("baker","08 Baker rifle","贝克步枪（古董）","约1800年",1160,220,1200,810001,"纸包火药和弹丸",false,false,15,35,16,60,18,25),
          new Spec("hall_1819","09 Hall M1819","霍尔M1819步枪（古董）","1819年",1320,220,1250,810001,"纸包火药和弹丸",false,false,17,35,16,60,20,22),
          new Spec("dreyse","10 Dreyse","德莱塞针发枪（古董）","19世纪中期",1420,220,1400,810001,"纸包火药和弹丸",false,false,20,40,20,55,20,25),
          new Spec("colt_1851","11 Colt 1851 Navy","柯尔特1851海军型转轮枪（古董）","1851年",330,150,1600,810001,"纸包火药和弹丸",true,false,16,45,18,55,30,18),
          new Spec("mississippi_1841","12 Mississippi 1841","1841式密西西比步枪（古董）","1841年",1240,220,1150,810001,"纸包火药和弹丸",false,false,16,30,20,60,18,25),
        };
        private static void SetHighResolutionSprite(GameItem item, string key)
        {
            foreach (var mod in ModLoader.Mods)
            {
                if (mod == null || mod.Manifest == null || mod.Manifest.ID != ColdWarModernExpansion.Entry.ModId) continue;
                Sprite sprite;
                if (!mod.Sprites.TryGetValue(key, out sprite) || sprite == null) break;
                if (sprite.pixelsPerUnit != 800f)
                {
                    var highRes = Sprite.Create(sprite.texture, sprite.rect, new Vector2(0.5f, 0.5f), 800f);
                    highRes.name = sprite.name;
                    mod.Sprites[key] = highRes;
                }
                break;
            }
            item.SetSprite(ColdWarModernExpansion.Entry.ModId, key);
            item.useSpriteFromMod = true;
        }
        public static GameItem CreateAmmo()
        {
            var item = ItemDirectory.CreateEmptyItem(null);
            SetHighResolutionSprite(item, "paper_cartridge");
            Grid50.Apply(item, 1, 1);
            item.name = ItemCopy.CommodityText.Name(Entry.AmmoId, EnglishLocalization.L.English);
            item.shortDescription = ItemCopy.CommodityText.Description(Entry.AmmoId, EnglishLocalization.L.English);
            item.unitValue = 90;
            WeaponCore.Ammo.Mark(item, Entry.SharedCaliber);
            ContrabandHelper.InitContrabandItem(item, 1);
            return item;
        }
        // Keep registering old trade-test IDs so existing saves can still resolve them.
        public static bool NeedsTradeVariant(Spec s) => Grid50.Cells(s.Length) > 18 || Grid50.Cells(s.Height) > 4;
        public static GameItem Create(Spec s, bool tradeTest)
        {
            var item = ItemDirectory.CreateEmptyItem(null);
            SetHighResolutionSprite(item, s.Sprite);
            // Artwork faces horizontally: width is the long axis; rotation swaps the axes.
            Grid50.Apply(item, s.Pistol ? 4 : 8, 3);
            item.name = EnglishLocalization.L.GunName(s.Sprite, s.Name);
            item.shortDescription = WeaponDescriptions.Text.Gun(s.Sprite);
            item.unitValue = s.Price;
            item.SetGameItemType("WEAPON"); item.SetGameItemType("FIREARM"); item.SetGameItemType("LUXURY_ITEM");
            if (!s.Pistol) item.SetGameItemType("MIDLINE");
            // Game tier 1: antique-style firearms (a gameplay simplification).
            ContrabandHelper.InitContrabandItem(item, 1);
            AudioHelper.InitHeavyGun(item);
            GunHelper.InitGun(item, s.Fire, s.Style, s.Reliability, s.Recoil, s.Ergonomics, s.Accuracy, s.Caliber, 30, true);
            var (window, stock, rail, magazine, underbarrel, barrel, special1, special2, special3) = GunModHelper.CreateGunModWindow(true, true, 5);
            item.SetContentWindow(window);
            // Only explicitly identified rifles receive long-gun slots; five ambiguous antiques remain unclassified.
            bool rifle = s.Sprite == "baker" || s.Sprite == "hall_1819" || s.Sprite == "dreyse" || s.Sprite == "mississippi_1841";
            ColdWarModernExpansion.ExistingGunSlots.Configure(stock, rail, magazine, underbarrel, barrel, special1, special2, special3,
                false, rifle, rifle, false);
            GunHelper.SetEquipmentCondition(item, (ItemFeatureList.EquipmentCondition)2);
            return item;
        }
    }
    [HarmonyPatch(typeof(GunHelper), nameof(GunHelper.GetCaliberName))]
    public static class AntiqueCaliberPatch
    {
        public static void Postfix(int caliber, ref string __result)
        {
            foreach (var spec in Catalog.All) if (caliber == spec.Caliber) { __result = EnglishLocalization.L.Pick(spec.CaliberLabel, "Paper Cartridge and Shot"); return; }
        }
    }
}
