using System;
using UnityEngine;
using HarmonyLib;
using WeaponCore;

namespace ModernWars
{
    public sealed class GunDefinition
    {
        public readonly string Key, Name, Caliber;
        public readonly int LengthMm, HeightMm, CaliberCode;
        public readonly bool Estimated;
        public GunDefinition(string key, string name, string caliber, int length, int height, int code, bool estimated = true)
        { Key = key; Name = name; Caliber = caliber; LengthMm = length; HeightMm = height; CaliberCode = code; Estimated = estimated; }
        public string Id => Entry.ModId + "." + Key;
    }
    public sealed class MagazineDefinition
    {
        public readonly string Key, Name, GunKey;
        public readonly int Width, Height;
        public MagazineDefinition(string key, string name, string gunKey, int width, int height)
        { Key = key; Name = name; GunKey = gunKey; Width = width; Height = height; }
        public string Id => Entry.ModId + "." + Key;
        public string GunId => Entry.ModId + "." + GunKey;
    }
    public sealed class Entry
    {
        public const string ModId = "local.modernwars";
        private ModLog log;
        private bool hooked;
        private ModItemDirectory registeredDirectory;
        private readonly System.Collections.Generic.HashSet<string> registered = new System.Collections.Generic.HashSet<string>();
        public static readonly GunDefinition[] Guns = {
            new GunDefinition("colt_saa", "柯尔特单动军用转轮手枪", ".45 Colt", 318, 140, 204501),
            new GunDefinition("m1911", "M1911 半自动手枪", ".45 ACP", 216, 140, 204502),
            new GunDefinition("winchester1873", "温彻斯特 M1873 杠杆步枪", ".44-40 WCF", 1093, 166, 204403, false),
            new GunDefinition("mosin1891", "莫辛-纳甘 M1891 栓动步枪", "7.62×54R", 1306, 190, 207654),
            new GunDefinition("kar98k", "Kar98k 栓动卡宾枪", "7.92×57", 1111, 180, 207957),
            new GunDefinition("m1garand", "M1 加兰德半自动步枪", ".30-06", 1118, 190, 203006),
            new GunDefinition("thompson1928", "汤普森 M1928A1 冲锋枪", ".45 ACP", 882, 260, 204502),
            new GunDefinition("ppsh41", "PPSh-41 冲锋枪（弹鼓）", "7.62×25", 843, 250, 207625),
            new GunDefinition("winchester1897", "温彻斯特 M1897 战壕枪", "12 gauge", 980, 190, 1270),
            new GunDefinition("bar1918", "勃朗宁 M1918 自动步枪", ".30-06", 1215, 250, 203006),
            new GunDefinition("mg42", "MG 42 通用机枪", "7.92×57", 1220, 260, 207957),
            new GunDefinition("stg44", "StG 44 突击步枪", "7.92×33", 940, 250, 207933)
        };
        public static readonly MagazineDefinition[] Magazines = {
            new MagazineDefinition("part_m1911", "M1911 弹匣", "m1911", 1, 2),
            new MagazineDefinition("part_thompson1928", "汤普森 M1928A1 弹匣", "thompson1928", 1, 2),
            new MagazineDefinition("part_ppsh41", "PPSh-41 弹鼓", "ppsh41", 2, 2),
            new MagazineDefinition("part_bar1918", "BAR M1918 弹匣", "bar1918", 1, 2),
            new MagazineDefinition("part_stg44", "StG 44 弹匣", "stg44", 1, 2)
        };
        public static readonly string[][] ExtraParts = {
            new[] {"clip_mosin1891", "莫辛五发桥夹"}, new[] {"clip_kar98k", "毛瑟五发桥夹"},
            new[] {"clip_m1garand", "M1 加兰德八发整装弹夹"}, new[] {"belt_mg42", "MG 42 弹链"}
        };
        // 12/70 is supplied by the modern module, identifier coldwarmodern.ammo.12g.
        public static readonly string[][] Ammos = {
            new[]{"ammo_45colt", ".45 Colt 弹药", "204501"}, new[]{"ammo_45acp", ".45 ACP 弹药", "204502"},
            new[]{"ammo_4440", ".44-40 WCF 弹药", "204403"}, new[]{"ammo_76254r", "7.62×54R 弹药", "207654"},
            new[]{"ammo_79257", "7.92×57 毛瑟弹药", "207957"}, new[]{"ammo_3006", ".30-06 Springfield 弹药", "203006"},
            new[]{"ammo_76225", "7.62×25 托卡列夫弹药", "207625"}, new[]{"ammo_79233", "7.92×33 Kurz 弹药", "207933"}
        };
        private static void SetHighResolutionSprite(GameItem item, string key)
        {
            // Loader creates sprites at 100 pixels per unit. These canvases use 8x
            // texture pixels while keeping exactly the old logical UI dimensions.
            foreach (var mod in ModLoader.Mods)
            {
                if (mod == null || mod.Manifest == null || mod.Manifest.ID != ColdWarModernExpansion.Entry.ModId) continue;
                Sprite original;
                if (!mod.Sprites.TryGetValue(key, out original) || original == null) break;
                if (original.pixelsPerUnit != 800f)
                {
                    var highRes = Sprite.Create(original.texture, original.rect, new Vector2(0.5f, 0.5f), 800f);
                    highRes.name = original.name;
                    mod.Sprites[key] = highRes;
                }
                break;
            }
            // Set identifiers directly instead of deriving a huge shape from the HD texture.
            item.SetSprite(ColdWarModernExpansion.Entry.ModId, key);
            item.useSpriteFromMod = true;
        }
        public void Init(ModManifest manifest) { log = new ModLog(manifest); }
        public void OnEnable()
        {
            if (hooked) return;
            ModHook.OnModItemDirectoryInit += Register;
            foreach (var def in Guns) { var gun = def; ModHelper.AddDebugFunction(ModId, ModHelper.DEBUG_CATEGORY_SPAWN, "Modern Wars: " + gun.Key, () => Spawn(gun.Id), false); }
            foreach (var def in Magazines) { var part = def; ModHelper.AddDebugFunction(ModId, ModHelper.DEBUG_CATEGORY_SPAWN, "Modern Wars: " + part.Key, () => Spawn(part.Id), false); }
            foreach (var def in ExtraParts) { var d = def; ModHelper.AddDebugFunction(ModId, ModHelper.DEBUG_CATEGORY_SPAWN, "Modern Wars: " + d[0], () => Spawn(ModId + "." + d[0]), false); }
            foreach (var def in Ammos) { var d = def; ModHelper.AddDebugFunction(ModId, ModHelper.DEBUG_CATEGORY_SPAWN, "Ammo: " + d[1], () => Spawn(ModId + "." + d[0]), false); }
            foreach (var def in Guns) { var d = def; AddOffer(d.Id, d.Key); }
            foreach (var def in Magazines) { var d = def; AddOffer(d.Id, d.Key); }
            foreach (var def in ExtraParts) { var d = def; AddOffer(ModId + "." + d[0], d[0]); }
            foreach (var def in Ammos) { var d = def; AddOffer(ModId + "." + d[0], d[0]); }
            hooked = true;
            foreach (var directory in Resources.FindObjectsOfTypeAll<ModItemDirectory>())
                if (directory != null && directory.factoryDictionary != null) Register(directory);
        }
        public void OnDisable()
        {
            if (!hooked) return;
            ModHook.OnModItemDirectoryInit -= Register;
            ModHelper.RemoveDebugFunctions(ModId);
            hooked = false; registered.Clear(); registeredDirectory = null;
        }
        private void Register(ModItemDirectory dir)
        {
            if (dir == null || dir.factoryDictionary == null || registeredDirectory == dir) return;
            registered.Clear(); registeredDirectory = dir;
            foreach (var definition in Guns)
            {
                var gun = definition;
                if (DirectoryMaster.Has<GameItem>(gun.Id) || dir.Has(gun.Id)) { log.Error("ID collision; spawn disabled: " + gun.Id); continue; }
                if (dir.Add(gun.Id, () => MakeGun(gun))) { registered.Add(gun.Id); log.Log("Registered " + gun.Id); }
                else log.Error("Failed to register " + gun.Id);
            }
            foreach (var definition in Magazines)
            {
                var part = definition;
                if (DirectoryMaster.Has<GameItem>(part.Id) || dir.Has(part.Id)) { log.Error("ID collision; spawn disabled: " + part.Id); continue; }
                if (dir.Add(part.Id, () => MakeMagazine(part))) { registered.Add(part.Id); log.Log("Registered " + part.Id); }
                else log.Error("Failed to register " + part.Id);
            }
            foreach (var d in ExtraParts) { var part = d; RegisterExtra(dir, ModId + "." + part[0], () => MakeExtra(part, false)); }
            foreach (var d in Ammos) { var ammo = d; RegisterExtra(dir, ModId + "." + ammo[0], () => MakeExtra(ammo, true)); }
        }
        private void RegisterExtra(ModItemDirectory dir, string id, Func<GameItem> factory)
        {
            if (DirectoryMaster.Has<GameItem>(id) || dir.Has(id)) { log.Error("ID collision; spawn disabled: " + id); return; }
            if (dir.Add(id, factory)) registered.Add(id); else log.Error("Failed to register " + id);
        }
        private void AddOffer(string id, string key)
        {
            ModHelper.AddDebugFunction(ModId, ModHelper.DEBUG_CATEGORY_SPAWN, "BUY Modern Wars: " + key, () => Offer(id), false);
        }
        private void Offer(string id)
        {
            if (!registered.Contains(id) || registeredDirectory == null || !registeredDirectory.Has(id) ||
                !PlayerStore.HasInstance || EmporiumEntry.Instance == null || EmporiumEntry.Instance.frontInvinvElement == null || !DirectoryMaster.Has<GameItem>(id))
            { log.Warning("Load a store and check item registration before offering " + id); return; }
            if (PlayerStore.Instance.TryAddDirectSellingItemToTable(id, false, false, false, 0)) log.Log("Added unowned counter offer: " + id);
            else log.Warning("Could not add counter offer: " + id);
        }
        private void Spawn(string id)
        {
            if (!registered.Contains(id) || registeredDirectory == null || !registeredDirectory.Has(id))
            { log.Warning("This module has not registered the item; refusing debug spawn: " + id); return; }
            if (Registry.SpawnOwnedToInventory(id, log)) log.Log("Spawned " + id);
        }
        private static GameItem MakeGun(GunDefinition def)
        {
            var item = ItemDirectory.CreateEmptyItem(null);
            SetHighResolutionSprite(item, def.Key);
            // Gun sprites face horizontally: width is the long axis; rotation swaps axes.
            int width = def.Key == "colt_saa" || def.Key == "m1911" ? 4 :
                def.Key == "thompson1928" || def.Key == "ppsh41" ? 5 : def.Key == "mg42" ? 10 : 8;
            Grid50.Apply(item, width, 3);
            bool hasMagazineSlot = false;
            foreach (var part in Magazines)
                if (part.GunKey == def.Key) { hasMagazineSlot = true; break; }
            var (window, stock, rail, magazine, underbarrel, barrel, special1, special2, special3) = GunModHelper.CreateGunModWindow(true, true, 5);
            item.SetContentWindow(window);
            bool longGun = def.Key != "colt_saa" && def.Key != "m1911" && def.Key != "thompson1928" && def.Key != "ppsh41";
            ColdWarModernExpansion.ExistingGunSlots.Configure(stock, rail, magazine, underbarrel, barrel, special1, special2, special3,
                hasMagazineSlot, longGun || def.Key == "m1911", longGun, hasMagazineSlot);
            item.name = EnglishLocalization.L.GunName(def.Key, def.Name);
            item.shortDescription = WeaponDescriptions.Text.Gun(def.Key);
            item.SetGameItemType("WEAPON"); item.SetGameItemType("FIREARM"); item.SetGameItemType("MIDLINE");
            // Select-fire / automatic historical configurations use tier 3; ordinary firearms tier 2.
            bool automatic = def.Key == "thompson1928" || def.Key == "ppsh41" || def.Key == "bar1918" || def.Key == "mg42" || def.Key == "stg44";
            ContrabandHelper.InitContrabandItem(item, automatic ? 3 : 2);
            AudioHelper.InitHeavyGun(item);
            WeaponBalance.GunBalance.Apply(item, def.Key, def.CaliberCode, 35, true);
            GunHelper.SetEquipmentCondition(item, (ItemFeatureList.EquipmentCondition)2);
            return item;
        }
        private static GameItem MakeMagazine(MagazineDefinition def)
        {
            var item = ItemDirectory.CreateEmptyItem(null);
            SetHighResolutionSprite(item, def.Key);
            Grid50.Apply(item, def.Width, def.Height);
            item.name = ItemCopy.CommodityText.Name(def.Id, EnglishLocalization.L.English);
            item.shortDescription = ItemCopy.CommodityText.Description(def.Id, EnglishLocalization.L.English);
            item.unitValue = 40;
            GunModHelper.InitGunModMagazine(item);
            // Drums are balanced separately; no reload or capacity gameplay is added.
            bool drum = def.Key == "part_ppsh41";
            GunModHelper.InitGunMod(item, drum ? 40 : 10, 0, 0, 0, drum ? -15 : 0, 0);
            GunModHelper.InitGunModBaseStats(item, drum ? 12 : 3, 0, 0, 0, drum ? -5 : 0, 0);
            if (drum) item.unitValue = 80;
            item.unitValue = Economy011.RevenueCurve.NewBase(def.Id);
            GunModHelper.AllowOnlyWeapon(new[] { def.GunId }, item);
            return item;
        }
        private static GameItem MakeExtra(string[] def, bool ammo)
        {
            var item = ItemDirectory.CreateEmptyItem(null);
            SetHighResolutionSprite(item, def[0]);
            int size = def[0] == "belt_mg42" ? 2 : 1;
            Grid50.Apply(item, size, size);
            item.name = ammo ? EnglishLocalization.L.AmmoName(def[0], def[1]) : EnglishLocalization.L.PartName(def[0], def[1]);
            item.shortDescription = ammo ? EnglishLocalization.L.Pick(WeaponDescriptions.Text.Ammo(def[1]), "A batch of " + item.name + ", for firearms chambered in the same cartridge.") : (def[0] == "belt_mg42" ? EnglishLocalization.L.Pick("一条供MG 42使用的弹链，适合作为独立商品收藏或交易。", "An ammunition belt for the MG 42, sold as a collectible trade item.") : EnglishLocalization.L.Pick("用于整理和携带弹药的装弹夹，适合作为独立商品收藏或交易。", "An ammunition clip for organizing and carrying cartridges, sold as a collectible trade item."));
            item.name = ItemCopy.CommodityText.Name(ModId + "." + def[0], EnglishLocalization.L.English);
            item.shortDescription = ItemCopy.CommodityText.Description(ModId + "." + def[0], EnglishLocalization.L.English);
            item.unitValue = ammo ? 35 : Economy011.RevenueCurve.NewBase(ModId + "." + def[0]);
            if (ammo) { WeaponCore.Ammo.Mark(item, int.Parse(def[2])); ContrabandHelper.InitContrabandItem(item, 1); }
            return item;
        }
    }
    [HarmonyPatch(typeof(GunHelper), nameof(GunHelper.GetCaliberName))]
    public static class CaliberNames
    {
        public static void Postfix(int caliber, ref string __result)
        {
            switch (caliber) {
                case 204501: __result = ".45 Colt"; break; case 204502: __result = ".45 ACP"; break;
                case 204403: __result = ".44-40 WCF"; break; case 207654: __result = "7.62×54R"; break;
                case 207957: __result = "7.92×57 Mauser"; break; case 203006: __result = ".30-06 Springfield"; break;
                case 207625: __result = "7.62×25 Tokarev"; break; case 207933: __result = "7.92×33 Kurz"; break;
            }
        }
    }
}
