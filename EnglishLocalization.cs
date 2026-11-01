using System;
using System.Collections.Generic;
using HarmonyLib;

namespace EnglishLocalization
{
    public static class L
    {
        public static bool English => string.Equals(LocHelper.GetCurrentLocaleCode(), "en", StringComparison.OrdinalIgnoreCase);
        // The base game serializes name and shortDescription. Rehydrate only known mod IDs
        // from their factory so old saves follow the selected language on load.
        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.DecodeNodes))]
        private static class LocalizeLoadedItems
        {
            private static void Postfix(List<SaveItemNode> savedItems)
            {
                if (savedItems == null) return;
                foreach (var node in savedItems)
                {
                    var item = node?.tempLink;
                    if (item == null) continue;
                    // The supplier's paper card is not necessarily registered in a ModItemDirectory.
                    if (node.identifier == "local.powdersupplierprototype.peace_margin_card")
                    {
                        item.name = ModHelper.GetLocalized(ColdWarModernExpansion.Entry.ModId, "card_name");
                        item.shortDescription = ModHelper.GetLocalized(ColdWarModernExpansion.Entry.ModId, "card_description");
                        continue;
                    }
                    if (!IsOurId(node.identifier) || !DirectoryMaster.Has<GameItem>(node.identifier)) continue;
                    GameItem template = null;
                    try {
                        // Use only the mod directory, never a potentially colliding vanilla/other-mod factory.
                        foreach (var dir in UnityEngine.Resources.FindObjectsOfTypeAll<ModItemDirectory>())
                        {
                            if (dir != null && dir.factoryDictionary != null && dir.factoryDictionary.TryGetValue(node.identifier, out var factory)) { template = factory(); break; }
                        }
                        if (template == null) continue;
                        item.name = template.name;
                        item.shortDescription = template.shortDescription;
                    } catch (Exception e) { UnityEngine.Debug.LogWarning("[GunCollection] Could not localize saved item " + node.identifier + ": " + e.Message); }
                    finally { if (template != null) template.Destroy(); }
                }
            }
        }
        // Exact catalog membership only: a shared prefix is not proof that an item belongs to us.
        private static bool IsOurId(string id) => id != null && KnownItemIds.Contains(id);
        private static readonly HashSet<string> KnownItemIds = CreateKnownItemIds();
        private static HashSet<string> CreateKnownItemIds()
        {
            var ids = new HashSet<string>(StringComparer.Ordinal) {
                "ak47prototype.rifle", "ak47prototype.magazine", "ak47prototype.ammo",
                "glock17prototype.pistol", "glock17prototype.magazine", "glock17prototype.ammo",
                "cncreserveretest.machine", "ammomachinetest.machine", "local.powdersupplierprototype.peace_margin_card"
            };
            AddIds(ids, "earlyfirearms.", "hand_cannon matchlock wheellock blunderbuss brown_bess charleville naval_pistol baker hall_1819 dreyse colt_1851 mississippi_1841 paper_cartridge");
            // Legacy save keys remain valid, but their factories now provide ordinary antique text.
            AddIds(ids, "earlyfirearms.", "matchlock.trade_test brown_bess.trade_test charleville.trade_test baker.trade_test hall_1819.trade_test dreyse.trade_test mississippi_1841.trade_test");
            AddIds(ids, "local.modernwars.", "colt_saa m1911 winchester1873 mosin1891 kar98k m1garand thompson1928 ppsh41 winchester1897 bar1918 mg42 stg44 part_m1911 part_thompson1928 part_ppsh41 part_bar1918 part_stg44 clip_mosin1891 clip_kar98k clip_m1garand belt_mg42 ammo_45colt ammo_45acp ammo_4440 ammo_76254r ammo_79257 ammo_3006 ammo_76225 ammo_79233");
            AddIds(ids, "coldwarmodern.", "m16a1 fnfal mp5 p90 sw629 rem870 m24 m82a1 m249 m60 m16a1.feed fnfal.feed mp5.feed p90.feed m82a1.feed m249.feed m60.feed");
            AddIds(ids, "coldwarmodern.ammo.", "55645 76251 5728 44mag 12g 50bmg");
            AddIds(ids, "rimfire22.", "markiv p22 buckmark 1022 markiv.magazine p22.magazine buckmark.magazine 1022.magazine");
            AddIds(ids, "local.deepspace010.", "winchester37 fp45 texas_defender tt33 svt40 springfield1903 beretta92fs sks mini14 mp40 m4a1 bfg9000");
            AddIds(ids, "local.deepspace010.attachment.", "red_dot holographic scope_1_4 scope_4_32 ballistic_sight light_stock telescopic_stock heavy_stock precision_stock light_grip tactical_grip angled_grip heavy_grip grenade_launcher underbarrel_shotgun tactical_light laser muzzle_brake advanced_brake flash_hider tactical_flash_hider makeshift_suppressor advanced_suppressor full_choke half_choke optimized_trigger advanced_trigger competition_trigger cleaning silver_plating gold_plating alofs m1911_stock glock_switch pedersen sks_bump_stock");
            AddIds(ids, "local.deepspace010.material.", "smokeless_powder casings projectiles");
            AddIds(ids, "storagecontainers.", "small_rack medium_rack large_rack accessory_box ammo_box");
            AddIds(ids, "gunsmithmachine.", "barrel_short barrel_medium barrel_long");
            return ids;
        }
        private static void AddIds(HashSet<string> ids, string prefix, string keys)
        {
            foreach (var key in keys.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)) ids.Add(prefix + key);
        }
        public static string Pick(string zh, string en) => English ? en : zh;
        public static string Gun(string key, string zh) => WeaponDescriptions.GunsTextLocalization.Description(key, English);
        public static string GunName(string key, string zh) => WeaponDescriptions.GunsTextLocalization.Name(key, English);
        private static readonly Dictionary<string,string> AmmoNames = new Dictionary<string,string> {
            ["ammo_45colt"]=".45 Colt Ammunition",["ammo_45acp"]=".45 ACP Ammunition",["ammo_4440"]=".44-40 WCF Ammunition",["ammo_76254r"]="7.62×54R Ammunition",["ammo_79257"]="7.92×57 Mauser Ammunition",["ammo_3006"]=".30-06 Springfield Ammunition",["ammo_76225"]="7.62×25 Tokarev Ammunition",["ammo_79233"]="7.92×33 Kurz Ammunition",
            ["55645"]="5.56×45 mm NATO Ammunition",["76251"]="7.62×51 mm NATO Ammunition",["5728"]="5.7×28 mm Ammunition",["44mag"]=".44 Magnum Ammunition",["12g"]="12-Gauge Shells (12/70)",["50bmg"]=".50 BMG Ammunition"
        };
        private static readonly Dictionary<string,string> Parts = new Dictionary<string,string> {
            ["part_m1911"]="M1911 Magazine",["part_thompson1928"]="Thompson M1928A1 Magazine",["part_ppsh41"]="PPSh-41 Drum Magazine",["part_bar1918"]="BAR M1918 Magazine",["part_stg44"]="StG 44 Magazine",
            ["clip_mosin1891"]="Mosin Five-Round Stripper Clip",["clip_kar98k"]="Mauser Five-Round Stripper Clip",["clip_m1garand"]="M1 Garand Eight-Round En Bloc Clip",["belt_mg42"]="MG 42 Ammunition Belt"
        };
        public static string AmmoName(string k,string zh) => English && AmmoNames.TryGetValue(k,out var n) ? n : zh;
        public static string PartName(string k,string zh) => English && Parts.TryGetValue(k,out var n) ? n : zh;
        public static string StorageName(string k,string zh) { if(!English)return zh; switch(k){case "small_rack":return "Small Gun Rack";case "medium_rack":return "Medium Gun Rack";case "large_rack":return "Large Gun Rack";case "accessory_box":return "Gun Parts Box";case "ammo_box":return "Ammunition Box";default:return zh;} }
        public static string StorageDesc(string k,string zh) { if(!English)return zh; switch(k){case "small_rack":return "A small rack for storing weapons. Limited space, but tidier than leaving guns on the floor.";case "medium_rack":return "A medium-sized rack designed to store weapons.";case "large_rack":return "A large gun rack with plenty of room for a collection.";case "accessory_box":return "A storage box designed for firearm modifications.";case "ammo_box":return "A storage box designed for ammunition.";default:return zh;} }
    }
}
