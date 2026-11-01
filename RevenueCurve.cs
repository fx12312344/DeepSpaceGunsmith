using System;
using System.Collections.Generic;

namespace Economy011
{
    // Frozen pre-adjustment base values. No game APIs, RNG, current locale or save mutation.
    public sealed class QuoteSpec
    {
        public readonly string Id, Kind;
        public readonly int Baseline, NewBase, MarkupBps;
        public double Markup => MarkupBps / 10000.0;
        internal QuoteSpec(string id, string kind, int baseline)
        {
            Id=id; Kind=kind; Baseline=baseline;
            MarkupBps=RevenueCurve.MarkupBasisPoints(baseline, kind == "gun");
            NewBase=checked((int)(((long)baseline * (10000 + MarkupBps) + 9999) / 10000));
        }
    }
    public static class RevenueCurve
    {
        public const int MinMarkupBps = 3000, MaxMarkupBps = 7500;
        public const int MinGunBase = 60, MaxGunBase = 1800, MinPartBase = 20, MaxPartBase = 190;
        public static int MarkupBasisPoints(int baseline, bool gun)
        {
            if (baseline <= 0) throw new ArgumentOutOfRangeException(nameof(baseline));
            int min=gun?MinGunBase:MinPartBase, max=gun?MaxGunBase:MaxPartBase;
            double t=Math.Max(0.0, Math.Min(1.0, Math.Log((double)baseline/min) / Math.Log((double)max/min)));
            double smooth=t*t*(3.0-2.0*t);
            return checked((int)Math.Round(MinMarkupBps+(MaxMarkupBps-MinMarkupBps)*smooth, MidpointRounding.AwayFromZero));
        }
        private static readonly Dictionary<string, QuoteSpec> Data = Build();
        public static IReadOnlyDictionary<string, QuoteSpec> Entries => Data;
        private static Dictionary<string, QuoteSpec> Build()
        {
            var d=new Dictionary<string, QuoteSpec>(StringComparer.Ordinal);
            Add(d,"ak47prototype.rifle","gun",350);
            Add(d,"coldwarmodern.fnfal","gun",420);
            Add(d,"coldwarmodern.m16a1","gun",380);
            Add(d,"coldwarmodern.m24","gun",600);
            Add(d,"coldwarmodern.m249","gun",650);
            Add(d,"coldwarmodern.m60","gun",700);
            Add(d,"coldwarmodern.m82a1","gun",1000);
            Add(d,"coldwarmodern.mp5","gun",380);
            Add(d,"coldwarmodern.p90","gun",500);
            Add(d,"coldwarmodern.rem870","gun",320);
            Add(d,"coldwarmodern.sw629","gun",400);
            Add(d,"earlyfirearms.baker","gun",1200);
            Add(d,"earlyfirearms.blunderbuss","gun",950);
            Add(d,"earlyfirearms.brown_bess","gun",850);
            Add(d,"earlyfirearms.charleville","gun",950);
            Add(d,"earlyfirearms.colt_1851","gun",1600);
            Add(d,"earlyfirearms.dreyse","gun",1400);
            Add(d,"earlyfirearms.hall_1819","gun",1250);
            Add(d,"earlyfirearms.hand_cannon","gun",800);
            Add(d,"earlyfirearms.matchlock","gun",900);
            Add(d,"earlyfirearms.mississippi_1841","gun",1150);
            Add(d,"earlyfirearms.naval_pistol","gun",950);
            Add(d,"earlyfirearms.wheellock","gun",1100);
            Add(d,"glock17prototype.pistol","gun",230);
            Add(d,"local.deepspace010.beretta92fs","gun",260);
            Add(d,"local.deepspace010.bfg9000","gun",1800);
            Add(d,"local.deepspace010.fp45","gun",60);
            Add(d,"local.deepspace010.m4a1","gun",500);
            Add(d,"local.deepspace010.mini14","gun",340);
            Add(d,"local.deepspace010.mp40","gun",320);
            Add(d,"local.deepspace010.sks","gun",300);
            Add(d,"local.deepspace010.springfield1903","gun",320);
            Add(d,"local.deepspace010.svt40","gun",360);
            Add(d,"local.deepspace010.texas_defender","gun",160);
            Add(d,"local.deepspace010.tt33","gun",180);
            Add(d,"local.deepspace010.winchester37","gun",120);
            Add(d,"local.modernwars.bar1918","gun",500);
            Add(d,"local.modernwars.colt_saa","gun",240);
            Add(d,"local.modernwars.kar98k","gun",320);
            Add(d,"local.modernwars.m1911","gun",220);
            Add(d,"local.modernwars.m1garand","gun",380);
            Add(d,"local.modernwars.mg42","gun",700);
            Add(d,"local.modernwars.mosin1891","gun",280);
            Add(d,"local.modernwars.ppsh41","gun",350);
            Add(d,"local.modernwars.stg44","gun",500);
            Add(d,"local.modernwars.thompson1928","gun",400);
            Add(d,"local.modernwars.winchester1873","gun",350);
            Add(d,"local.modernwars.winchester1897","gun",320);
            Add(d,"rimfire22.1022","gun",220);
            Add(d,"rimfire22.buckmark","gun",180);
            Add(d,"rimfire22.markiv","gun",160);
            Add(d,"rimfire22.p22","gun",140);
            Add(d,"local.deepspace010.attachment.advanced_brake","attachment",80);
            Add(d,"local.deepspace010.attachment.advanced_suppressor","attachment",90);
            Add(d,"local.deepspace010.attachment.advanced_trigger","attachment",45);
            Add(d,"local.deepspace010.attachment.alofs","attachment",150);
            Add(d,"local.deepspace010.attachment.angled_grip","attachment",60);
            Add(d,"local.deepspace010.attachment.ballistic_sight","attachment",190);
            Add(d,"local.deepspace010.attachment.cleaning","attachment",20);
            Add(d,"local.deepspace010.attachment.competition_trigger","attachment",70);
            Add(d,"local.deepspace010.attachment.flash_hider","attachment",35);
            Add(d,"local.deepspace010.attachment.full_choke","attachment",50);
            Add(d,"local.deepspace010.attachment.glock_switch","attachment",80);
            Add(d,"local.deepspace010.attachment.gold_plating","attachment",130);
            Add(d,"local.deepspace010.attachment.grenade_launcher","attachment",150);
            Add(d,"local.deepspace010.attachment.half_choke","attachment",35);
            Add(d,"local.deepspace010.attachment.heavy_grip","attachment",50);
            Add(d,"local.deepspace010.attachment.heavy_stock","attachment",40);
            Add(d,"local.deepspace010.attachment.holographic","attachment",85);
            Add(d,"local.deepspace010.attachment.laser","attachment",50);
            Add(d,"local.deepspace010.attachment.light_grip","attachment",25);
            Add(d,"local.deepspace010.attachment.light_stock","attachment",40);
            Add(d,"local.deepspace010.attachment.m1911_stock","attachment",90);
            Add(d,"local.deepspace010.attachment.makeshift_suppressor","attachment",25);
            Add(d,"local.deepspace010.attachment.muzzle_brake","attachment",50);
            Add(d,"local.deepspace010.attachment.optimized_trigger","attachment",25);
            Add(d,"local.deepspace010.attachment.pedersen","attachment",160);
            Add(d,"local.deepspace010.attachment.precision_stock","attachment",90);
            Add(d,"local.deepspace010.attachment.red_dot","attachment",60);
            Add(d,"local.deepspace010.attachment.scope_1_4","attachment",100);
            Add(d,"local.deepspace010.attachment.scope_4_32","attachment",140);
            Add(d,"local.deepspace010.attachment.silver_plating","attachment",70);
            Add(d,"local.deepspace010.attachment.sks_bump_stock","attachment",120);
            Add(d,"local.deepspace010.attachment.tactical_flash_hider","attachment",60);
            Add(d,"local.deepspace010.attachment.tactical_grip","attachment",40);
            Add(d,"local.deepspace010.attachment.tactical_light","attachment",35);
            Add(d,"local.deepspace010.attachment.telescopic_stock","attachment",65);
            Add(d,"local.deepspace010.attachment.underbarrel_shotgun","attachment",130);
            Add(d,"ak47prototype.magazine","feed",55);
            Add(d,"coldwarmodern.fnfal.feed","feed",55);
            Add(d,"coldwarmodern.m16a1.feed","feed",55);
            Add(d,"coldwarmodern.m249.feed","feed",100);
            Add(d,"coldwarmodern.m60.feed","feed",100);
            Add(d,"coldwarmodern.m82a1.feed","feed",55);
            Add(d,"coldwarmodern.mp5.feed","feed",55);
            Add(d,"coldwarmodern.p90.feed","feed",55);
            Add(d,"glock17prototype.magazine","feed",40);
            Add(d,"local.modernwars.belt_mg42","feed",40);
            Add(d,"local.modernwars.clip_kar98k","feed",40);
            Add(d,"local.modernwars.clip_m1garand","feed",40);
            Add(d,"local.modernwars.clip_mosin1891","feed",40);
            Add(d,"local.modernwars.part_bar1918","feed",40);
            Add(d,"local.modernwars.part_m1911","feed",40);
            Add(d,"local.modernwars.part_ppsh41","feed",80);
            Add(d,"local.modernwars.part_stg44","feed",40);
            Add(d,"local.modernwars.part_thompson1928","feed",40);
            Add(d,"rimfire22.1022.magazine","feed",35);
            Add(d,"rimfire22.buckmark.magazine","feed",35);
            Add(d,"rimfire22.markiv.magazine","feed",35);
            Add(d,"rimfire22.p22.magazine","feed",35);
            return d;
        }
        private static void Add(Dictionary<string,QuoteSpec> d,string id,string kind,int baseline) { d.Add(id,new QuoteSpec(id,kind,baseline)); }
        public static bool TryGet(string id,out QuoteSpec spec)
        {
            if (id != null && Data.TryGetValue(id,out spec)) return true;
            // Only registered antique compatibility aliases resolve to the same economic entry.
            if (id != null && id.EndsWith(".trade_test",StringComparison.Ordinal))
            {
                switch(id)
                {
                    case "earlyfirearms.matchlock.trade_test": return Data.TryGetValue("earlyfirearms.matchlock",out spec);
                    case "earlyfirearms.brown_bess.trade_test": return Data.TryGetValue("earlyfirearms.brown_bess",out spec);
                    case "earlyfirearms.charleville.trade_test": return Data.TryGetValue("earlyfirearms.charleville",out spec);
                    case "earlyfirearms.baker.trade_test": return Data.TryGetValue("earlyfirearms.baker",out spec);
                    case "earlyfirearms.hall_1819.trade_test": return Data.TryGetValue("earlyfirearms.hall_1819",out spec);
                    case "earlyfirearms.dreyse.trade_test": return Data.TryGetValue("earlyfirearms.dreyse",out spec);
                    case "earlyfirearms.mississippi_1841.trade_test": return Data.TryGetValue("earlyfirearms.mississippi_1841",out spec);
                }
            }
            spec=null;return false;
        }
        public static int NewBase(string id)
        {
            if (!TryGet(id,out var s)) throw new ArgumentException("Missing curve entry: "+id);
            return s.NewBase;
        }
        public static int NewBaseForGun(string key,int expectedBaseline)
        {
            foreach(var entry in Data.Values)
            {
                if(entry.Kind != "gun") continue;
                string k=entry.Id=="ak47prototype.rifle"?"ak47":entry.Id=="glock17prototype.pistol"?"glock17":entry.Id.Substring(entry.Id.LastIndexOf('.')+1);
                if(k!=key) continue;
                if(entry.Baseline != expectedBaseline) throw new InvalidOperationException("Gun baseline drift: "+key);
                return entry.NewBase;
            }
            throw new ArgumentException("Missing gun curve entry: "+key);
        }
        public static int ReferenceCost(string id,int conditionPercent=100)
        {
            if(conditionPercent<=0 || conditionPercent>1000)throw new ArgumentOutOfRangeException(nameof(conditionPercent));
            if(!TryGet(id,out var s))throw new ArgumentException("Missing cost entry: "+id);
            return checked((int)((long)s.Baseline * conditionPercent * 80 / 10000));
        }
        public static long TargetRevenue(string id,long cost)
        {
            if(cost<0 || cost>int.MaxValue)throw new ArgumentOutOfRangeException(nameof(cost));
            if(!TryGet(id,out var s))throw new ArgumentException("Missing target entry: "+id);
            return checked((cost * (10000+s.MarkupBps) + 9999)/10000);
        }
    }
}
