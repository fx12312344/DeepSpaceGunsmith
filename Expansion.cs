using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using WeaponCore;

namespace ColdWarModernExpansion
{
    public sealed class Entry : IMod
    {
        public const string ModId = "local.coldwarmodern";
        private bool hooked;
        internal static bool IsActive { get; private set; }
        private ModLog log;
        private readonly ModernWars.Entry modernWars = new ModernWars.Entry();
        private readonly EarlyFirearms.Entry earlyFirearms = new EarlyFirearms.Entry();
        private readonly StorageContainers.Entry storage = new StorageContainers.Entry();
        private readonly NpcWeaponTrade.Entry npcTrade = new NpcWeaponTrade.Entry();
        private readonly GunSupplierPrototype.Entry gunSupplier = new GunSupplierPrototype.Entry();
        private readonly PowderSupplierIntegration.Entry powderSupplier = new PowderSupplierIntegration.Entry();
        private readonly GunsmithLicenseVisit.Entry licenseVisit = new GunsmithLicenseVisit.Entry();
        public void Init(ModManifest manifest) { log = new ModLog(manifest); modernWars.Init(manifest); earlyFirearms.Init(manifest); storage.Init(manifest); npcTrade.Init(manifest); gunSupplier.Init(manifest); powderSupplier.Init(manifest); licenseVisit.Init(manifest); }
        public void OnEnable()
        {
            if (hooked) return;
            modernWars.OnEnable();
            earlyFirearms.OnEnable();
            storage.OnEnable();
            npcTrade.OnEnable();
            gunSupplier.OnEnable();
            powderSupplier.OnEnable();
            licenseVisit.OnEnable();
            ModHook.OnLootRolled += WeaponSources.Sources.OnLootRolled;
            ModHook.OnModItemDirectoryInit += Register;
            foreach (var definition in Items.All)
            {
                var def = definition;
                ModHelper.AddDebugFunction(ModId, ModHelper.DEBUG_CATEGORY_SPAWN, def.Button,
                    () => Registry.SpawnOwnedToInventory(def.Id, log), false);
                ModHelper.AddDebugFunction(ModId, ModHelper.DEBUG_CATEGORY_SPAWN, "BUY " + def.Button,
                    () => Offer(def.Id), false);
            }
            hooked = true;
            IsActive = true;
            foreach (var directory in Resources.FindObjectsOfTypeAll<ModItemDirectory>())
                if (directory != null && directory.factoryDictionary != null) Register(directory);
        }
        public void OnDisable()
        {
            if (!hooked) return;
            IsActive = false;
            modernWars.OnDisable();
            earlyFirearms.OnDisable();
            storage.OnDisable();
            npcTrade.OnDisable();
            powderSupplier.OnDisable();
            gunSupplier.OnDisable();
            licenseVisit.OnDisable();
            ModHook.OnLootRolled -= WeaponSources.Sources.OnLootRolled;
            ModHook.OnModItemDirectoryInit -= Register;
            ModHelper.RemoveDebugFunctions(ModId);
            hooked = false;
        }
        private void Register(ModItemDirectory directory) => Registry.Register(directory, Items.All, log);
        private void Offer(string id)
        {
            if (!PlayerStore.HasInstance || EmporiumEntry.Instance == null || EmporiumEntry.Instance.frontInvinvElement == null || !DirectoryMaster.Has<GameItem>(id))
            { log.Warning("Load a store and check item registration before offering " + id); return; }
            if (PlayerStore.Instance.TryAddDirectSellingItemToTable(id, false, false, false, 0)) log.Log("Added unowned counter offer: " + id);
            else log.Warning("Could not add counter offer: " + id);
        }
    }
    // Configure only after SetContentWindow: vanilla callbacks capture each slot's parent.
    internal static class ExistingGunSlots
    {
        internal static void Configure(GameSlotInventory stock, GameSlotInventory rail, GameSlotInventory magazine,
            GameSlotInventory underbarrel, GameSlotInventory barrel, GameSlotInventory special1,
            GameSlotInventory special2, GameSlotInventory special3, bool railEnabled, bool stockEnabled,
            bool underbarrelEnabled, bool magazineEnabled)
        {
            GunModHelper.EnableBarrelSlot(barrel);
            GunModHelper.EnableSpecialSlot(special1);
            GunModHelper.EnableSpecialSlot(special2);
            GunModHelper.EnableSpecialSlot(special3);
            if (railEnabled) GunModHelper.EnableRailSlot(rail); else Block(rail);
            if (stockEnabled) GunModHelper.EnableStockSlot(stock); else Block(stock);
            if (underbarrelEnabled) GunModHelper.EnableUnderbarrelSlot(underbarrel); else Block(underbarrel);
            if (magazineEnabled) GunModHelper.EnableMagazineSlot(magazine, "smg_reload"); else Block(magazine);
        }
        private static void Block(GameSlotInventory slot)
        {
            slot.mayInventoryAddItemFunc = (mod, inv) => false;
            slot.maxSlotAddItemFunc = (mod, inv, marker) => 0;
        }
    }
    public sealed class GunSpec
    {
        public string Key, Name, Feed, Sprite;
        public int Length, Height, Caliber;
        // GunBalance is the sole authority for firearm base values.
        public GunSpec(string key, string name, string feed, int length, int height, int caliber)
        { Key=key; Name=name; Feed=feed; Sprite=key; Length=length; Height=height; Caliber=caliber; }
        public string Id => "coldwarmodern." + Key;
    }
    public sealed class AmmoSpec
    {
        public string Key, Name; public int Caliber; public long Value;
        public AmmoSpec(string key, string name, int caliber, long value) { Key=key; Name=name; Caliber=caliber; Value=value; }
    }
    public static class Items
    {
        // Heights are estimates for the depicted configuration, not published manufacturer measurements.
        private static readonly GunSpec[] Guns = {
            new GunSpec("m16a1","M16A1","mag",986,250,55645),
            new GunSpec("fnfal","FN FAL","mag",1090,250,76251),
            new GunSpec("mp5","HK MP5A2","mag",680,250,919),
            new GunSpec("p90","FN P90","mag",500,210,5728),
            new GunSpec("sw629","S&W Model 629（6英寸）","fixed",292,155,441),
            new GunSpec("rem870","Remington 870（短枪管型）","fixed",978,200,1270),
            new GunSpec("m24","M24 狙击步枪","fixed",1092,250,76251),
            new GunSpec("m82a1","Barrett M82A1","mag",1448,280,12799),
            new GunSpec("m249","M249 班用机枪","belt",1040,280,55645),
            new GunSpec("m60","M60 通用机枪","belt",1105,280,76251)
        };
        // 7.62x39 and 9x19 keep their legacy item IDs below.
        private static readonly AmmoSpec[] Ammos = {
            new AmmoSpec("55645","5.56×45 mm NATO 弹药",55645,45),
            new AmmoSpec("76251","7.62×51 mm NATO 弹药",76251,55),
            new AmmoSpec("5728","5.7×28 mm 弹药",5728,50),
            new AmmoSpec("44mag",".44 Magnum 弹药",441,55),
            new AmmoSpec("12g","12号霰弹（12/70）",1270,35),
            new AmmoSpec("50bmg",".50 BMG 弹药",12799,110)
        };
        public static readonly List<ItemDefinition> All = MakeDefinitions();
        private static List<ItemDefinition> MakeDefinitions()
        {
            var list = new List<ItemDefinition> {
                new ItemDefinition("ak47prototype.rifle", "AK47 rifle", AkRifle),
                new ItemDefinition("ak47prototype.magazine", "AK47 magazine", AkMagazine),
                new ItemDefinition("ak47prototype.ammo", "Ammo 7.62x39", Ammo76239),
                new ItemDefinition("glock17prototype.pistol", "G17 pistol", GlockPistol),
                new ItemDefinition("glock17prototype.magazine", "G17 magazine", GlockMagazine),
                new ItemDefinition("glock17prototype.ammo", "Ammo 9x19", Ammo9x19)
            };
            foreach (var gun in Guns)
            {
                var g = gun;
                list.Add(new ItemDefinition(g.Id, g.Key + " gun", () => CreateGun(g)));
                if (g.Feed != "fixed") list.Add(new ItemDefinition(g.Id + ".feed", g.Key + " feed", () => CreateFeed(g)));
            }
            list.AddRange(Rimfire22.Catalog.Definitions());
            list.AddRange(NewGuns010.Catalog.Definitions());
            list.AddRange(Attachments010.Catalog.Definitions());
            list.AddRange(SmokelessPowder010.Catalog.Definitions());
            list.AddRange(MixedAmmoMaterials010.Catalog.Definitions());
            list.AddRange(Bfg010.Catalog.Definitions());
            foreach (var ammo in Ammos)
            {
                var a = ammo;
                list.Add(new ItemDefinition("coldwarmodern.ammo." + a.Key, "Ammo " + a.Key, () => CreateAmmo(a)));
            }
            return list;
        }
        private static void SetHighResolutionSprite(GameItem item, string key)
        {
            foreach (var mod in ModLoader.Mods)
            {
                if (mod == null || mod.Manifest == null || mod.Manifest.ID != Entry.ModId) continue;
                Sprite original;
                if (!mod.Sprites.TryGetValue(key, out original) || original == null) break;
                if (original.pixelsPerUnit != 800f)
                {
                    var scaled = Sprite.Create(original.texture, original.rect, new Vector2(0.5f, 0.5f), 800f);
                    scaled.name = original.name;
                    mod.Sprites[key] = scaled;
                }
                break;
            }
            item.SetSprite(Entry.ModId, key);
            item.useSpriteFromMod = true;
        }
        private static GameItem AkRifle()
        {
            var (window, stock, rail, magazine, underbarrel, barrel, special1, special2, special3) = GunModHelper.CreateGunModWindow(true, true, 5);
            var item = ItemDirectory.CreateEmptyItem(null);
            SetHighResolutionSprite(item, "ak47_new");
            Grid50.Apply(item, 8, 3);
            item.SetContentWindow(window);
            item.name = EnglishLocalization.L.GunName("ak47", "AK-47突击步枪");
            item.shortDescription = WeaponDescriptions.Text.Gun("ak47");
            item.SetGameItemType("WEAPON"); item.SetGameItemType("FIREARM"); item.SetGameItemType("MIDLINE");
            ContrabandHelper.InitContrabandItem(item, 3);
            AudioHelper.InitHeavyGun(item);
            WeaponBalance.GunBalance.Apply(item, "ak47", 76239, 45, true);
            ExistingGunSlots.Configure(stock, rail, magazine, underbarrel, barrel, special1, special2, special3,
                true, true, true, true);
            GunHelper.SetEquipmentCondition(item, (ItemFeatureList.EquipmentCondition)2);
            return item;
        }
        private static GameItem AkMagazine()
        {
            var item = ItemDirectory.CreateEmptyItem(null);
            SetHighResolutionSprite(item, "ak47_magazine_new");
            Grid50.Apply(item, 1, 2);
            item.name = ItemCopy.CommodityText.Name("ak47prototype.magazine", EnglishLocalization.L.English);
            item.shortDescription = ItemCopy.CommodityText.Description("ak47prototype.magazine", EnglishLocalization.L.English);
            item.unitValue = 55;
            GunModHelper.InitGunModMagazine(item);
            GunModHelper.InitGunMod(item, 10, 0, 0, 0, 0, 0);
            GunModHelper.InitGunModBaseStats(item, 3, 0, 0, 0, 0, 0);
            GunModHelper.AllowOnlyWeapon(new[] { "ak47prototype.rifle" }, item);
            return item;
        }
        private static GameItem Ammo76239()
        {
            var item = ItemDirectory.CreateEmptyItem(null);
            SetHighResolutionSprite(item, "ammo_76239");
            Grid50.Apply(item, 1, 1);
            item.name = ItemCopy.CommodityText.Name("ak47prototype.ammo", EnglishLocalization.L.English);
            item.shortDescription = ItemCopy.CommodityText.Description("ak47prototype.ammo", EnglishLocalization.L.English);
            item.unitValue = 35;
            WeaponCore.Ammo.Mark(item, 76239);
            ContrabandHelper.InitContrabandItem(item, 1);
            return item;
        }
        private static GameItem GlockPistol()
        {
            var (window, stock, rail, magazine, underbarrel, barrel, special1, special2, special3) = GunModHelper.CreateGunModWindow(true, true, 5);
            var item = ItemDirectory.CreateEmptyItem(null);
            SetHighResolutionSprite(item, "glock17");
            Grid50.Apply(item, 4, 3);
            item.SetContentWindow(window);
            item.name = EnglishLocalization.L.GunName("glock17", "格洛克GLOCK 17半自动手枪");
            item.shortDescription = WeaponDescriptions.Text.Gun("glock17");
            item.SetGameItemType("WEAPON"); item.SetGameItemType("FIREARM");
            ContrabandHelper.InitContrabandItem(item, 2);
            AudioHelper.InitPistol(item);
            WeaponBalance.GunBalance.Apply(item, "glock17", 919, 30, true);
            ExistingGunSlots.Configure(stock, rail, magazine, underbarrel, barrel, special1, special2, special3,
                true, false, false, true);
            GunHelper.SetEquipmentCondition(item, (ItemFeatureList.EquipmentCondition)2);
            return item;
        }
        private static GameItem GlockMagazine()
        {
            var item = ItemDirectory.CreateEmptyItem(null);
            SetHighResolutionSprite(item, "glock17_magazine");
            Grid50.Apply(item, 1, 2);
            item.name = ItemCopy.CommodityText.Name("glock17prototype.magazine", EnglishLocalization.L.English);
            item.shortDescription = ItemCopy.CommodityText.Description("glock17prototype.magazine", EnglishLocalization.L.English);
            item.unitValue = 40;
            GunModHelper.InitGunModMagazine(item);
            GunModHelper.InitGunMod(item, 10, 0, 0, 0, 0, 0);
            GunModHelper.InitGunModBaseStats(item, 3, 0, 0, 0, 0, 0);
            GunModHelper.AllowOnlyWeapon(new[] { "glock17prototype.pistol" }, item);
            return item;
        }
        private static GameItem Ammo9x19()
        {
            var item = ItemDirectory.CreateEmptyItem(null);
            SetHighResolutionSprite(item, "glock17_ammo");
            Grid50.Apply(item, 1, 1);
            item.name = ItemCopy.CommodityText.Name("glock17prototype.ammo", EnglishLocalization.L.English);
            item.shortDescription = ItemCopy.CommodityText.Description("glock17prototype.ammo", EnglishLocalization.L.English);
            item.unitValue = 30;
            WeaponCore.Ammo.Mark(item, 919);
            ContrabandHelper.InitContrabandItem(item, 1);
            return item;
        }
        private static GameItem CreateGun(GunSpec g)
        {
            var (window, stock, rail, magazine, underbarrel, barrel, special1, special2, special3) = GunModHelper.CreateGunModWindow(true, true, 5);
            var item = ItemDirectory.CreateEmptyItem(null);
            SetHighResolutionSprite(item, g.Sprite);
            // Horizontal display: long side first; rotation swaps axes.
            int width = g.Key == "sw629" ? 4 : (g.Key == "mp5" || g.Key == "p90") ? 5 : (g.Key == "m24" || g.Key == "m82a1" || g.Feed == "belt") ? 10 : 8;
            Grid50.Apply(item, width, 3);
            item.SetContentWindow(window);
            item.name = EnglishLocalization.L.GunName(g.Key, g.Name);
            item.shortDescription = WeaponDescriptions.Text.Gun(g.Key);
            item.SetGameItemType("WEAPON"); item.SetGameItemType("FIREARM"); item.SetGameItemType("MIDLINE");
            // Classify the depicted military select-fire configurations as tier 3.
            bool automatic = g.Key == "m16a1" || g.Key == "fnfal" || g.Key == "mp5" || g.Key == "p90" || g.Key == "m249" || g.Key == "m60";
            ContrabandHelper.InitContrabandItem(item, automatic ? 3 : 2);
            if (g.Key == "sw629") AudioHelper.InitPistol(item); else AudioHelper.InitHeavyGun(item);
            WeaponBalance.GunBalance.Apply(item, g.Key, g.Caliber, 40, true);
            bool longGun = g.Key != "sw629" && g.Key != "mp5" && g.Key != "p90";
            ExistingGunSlots.Configure(stock, rail, magazine, underbarrel, barrel, special1, special2, special3,
                true, longGun, longGun, g.Feed == "mag");
            GunHelper.SetEquipmentCondition(item, (ItemFeatureList.EquipmentCondition)2);
            return item;
        }
        private static GameItem CreateFeed(GunSpec g)
        {
            var item = ItemDirectory.CreateEmptyItem(null);
            SetHighResolutionSprite(item, g.Key + "_feed");
            Grid50.Apply(item, g.Feed == "belt" ? 2 : 1, 2);
            item.name = ItemCopy.CommodityText.Name(g.Id + ".feed", EnglishLocalization.L.English);
            item.shortDescription = ItemCopy.CommodityText.Description(g.Id + ".feed", EnglishLocalization.L.English);
            item.unitValue = g.Feed == "belt" ? 100 : 55;
            if (g.Feed == "mag")
            {
                GunModHelper.InitGunModMagazine(item);
                GunModHelper.InitGunMod(item, 10, 0, 0, 0, 0, 0);
                GunModHelper.InitGunModBaseStats(item, 3, 0, 0, 0, 0, 0);
                GunModHelper.AllowOnlyWeapon(new[] { g.Id }, item);
            }
            return item;
        }
        private static GameItem CreateAmmo(AmmoSpec a)
        {
            var item = ItemDirectory.CreateEmptyItem(null);
            SetHighResolutionSprite(item, "ammo_" + a.Key);
            Grid50.Apply(item, 1, 1);
            item.name = ItemCopy.CommodityText.Name("coldwarmodern.ammo." + a.Key, EnglishLocalization.L.English);
            item.shortDescription = ItemCopy.CommodityText.Description("coldwarmodern.ammo." + a.Key, EnglishLocalization.L.English);
            item.unitValue = a.Value;
            WeaponCore.Ammo.Mark(item, a.Caliber);
            ContrabandHelper.InitContrabandItem(item, 1);
            return item;
        }
    }
    [HarmonyPatch(typeof(GunHelper), nameof(GunHelper.GetCaliberName))]
    public static class CaliberNames
    {
        public static void Postfix(int caliber, ref string __result)
        {
            switch (caliber) {
                case 76239: __result="7.62×39 mm"; break;
                case 919: __result="9×19 mm"; break;
                case 55645: __result="5.56×45 mm NATO"; break;
                case 76251: __result="7.62×51 mm NATO"; break;
                case 5728: __result="5.7×28 mm"; break;
                case 441: __result=".44 Magnum"; break;
                case 1270: __result="12/70"; break;
                case 12799: __result=".50 BMG"; break;
            }
        }
    }
}
