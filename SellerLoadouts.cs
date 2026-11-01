using System;
using System.Collections.Generic;

namespace WeaponSources
{
    // Item IDs are the existing shared ammunition and matching feed components.
    public static class SellerLoadouts
    {
        public sealed class Loadout
        {
            public readonly string Ammo, Feed;
            public Loadout(string ammo, string feed = null) { Ammo = ammo; Feed = feed; }
        }
        public static readonly Dictionary<string,Loadout> Items = Build();
        private static Dictionary<string,Loadout> Build()
        {
            var d = new Dictionary<string,Loadout>(StringComparer.Ordinal);
            foreach (var g in EarlyFirearms.Catalog.All) d.Add(g.Id, new Loadout("earlyfirearms.paper_cartridge"));
            void Add(string gun, string ammo, string feed = null) => d.Add(gun, new Loadout(ammo,feed));
            const string m="local.modernwars."; const string c="coldwarmodern.";
            Add(m+"colt_saa",m+"ammo_45colt"); Add(m+"m1911",m+"ammo_45acp",m+"part_m1911");
            Add(m+"winchester1873",m+"ammo_4440"); Add(m+"mosin1891",m+"ammo_76254r",m+"clip_mosin1891");
            Add(m+"kar98k",m+"ammo_79257",m+"clip_kar98k"); Add(m+"m1garand",m+"ammo_3006",m+"clip_m1garand");
            Add(m+"thompson1928",m+"ammo_45acp",m+"part_thompson1928"); Add(m+"ppsh41",m+"ammo_76225",m+"part_ppsh41");
            Add(m+"winchester1897",c+"ammo.12g"); Add(m+"bar1918",m+"ammo_3006",m+"part_bar1918");
            Add(m+"mg42",m+"ammo_79257",m+"belt_mg42"); Add(m+"stg44",m+"ammo_79233",m+"part_stg44");
            Add("ak47prototype.rifle","ak47prototype.ammo","ak47prototype.magazine"); Add("glock17prototype.pistol","glock17prototype.ammo","glock17prototype.magazine");
            Add(c+"m16a1",c+"ammo.55645",c+"m16a1.feed"); Add(c+"fnfal",c+"ammo.76251",c+"fnfal.feed");
            Add(c+"mp5","glock17prototype.ammo",c+"mp5.feed"); Add(c+"p90",c+"ammo.5728",c+"p90.feed");
            Add(c+"sw629",c+"ammo.44mag"); Add(c+"rem870",c+"ammo.12g"); Add(c+"m24",c+"ammo.76251");
            Add(c+"m82a1",c+"ammo.50bmg",c+"m82a1.feed"); Add(c+"m249",c+"ammo.55645",c+"m249.feed"); Add(c+"m60",c+"ammo.76251",c+"m60.feed");
            foreach (var g in Rimfire22.Catalog.Guns) Add(g.Id,"small_pistol_ammo",g.Id+".magazine");
            foreach (var pair in NewGuns010.NewGunsFlow010.Loadouts) d.Add(pair.Key, pair.Value);
            return d;
        }
        public static bool IsVanillaAmmoOrMagazine(string id) => id != null && (id.StartsWith("small_pistol_ammo",StringComparison.Ordinal) || id.StartsWith("heavy_pistol_ammo",StringComparison.Ordinal) || id == "mag_22" || id == "mag_22_high" || id == "mag_22_ext" || id == "mag_10mm" || id == "mag_10mm_high" || id == "mag_10mm_ext" || id == "mag_10mm_smg" || id == "mag_10mm_smg_high");
    }
}
