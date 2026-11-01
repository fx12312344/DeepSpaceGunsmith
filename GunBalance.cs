using System;
using System.Collections.Generic;

// Firepower, style, reliability, recoil, ergonomics, accuracy, base value.
// Values transcribed from the latest agreed plan, not live item values.
namespace WeaponBalance
{
    public static class GunBalance
    {
        private static readonly Dictionary<string,int[]> Data = new Dictionary<string,int[]> {
            ["colt_saa"]=new[]{30,35,34,30,40,40,240},
            ["m1911"]=new[]{32,35,45,35,45,45,220},
            ["winchester1873"]=new[]{35,45,40,35,40,45,350},
            ["mosin1891"]=new[]{50,35,55,60,25,60,280},
            ["kar98k"]=new[]{52,40,55,55,35,65,320},
            ["m1garand"]=new[]{60,45,60,45,45,65,380},
            ["thompson1928"]=new[]{60,55,55,45,40,45,400},
            ["ppsh41"]=new[]{60,45,60,40,35,40,350},
            ["winchester1897"]=new[]{75,45,55,60,35,30,320},
            ["bar1918"]=new[]{75,50,55,60,25,55,500},
            ["mg42"]=new[]{95,60,60,70,15,50,700},
            ["stg44"]=new[]{65,60,55,45,40,55,500},
            ["ak47"]=new[]{70,55,75,45,50,55,350},
            ["glock17"]=new[]{35,40,65,30,60,50,230},
            ["m16a1"]=new[]{68,55,60,30,60,70,380},
            ["fnfal"]=new[]{75,55,70,60,40,65,420},
            ["mp5"]=new[]{60,60,75,25,70,60,380},
            ["p90"]=new[]{65,70,70,20,80,60,500},
            ["sw629"]=new[]{50,65,65,65,40,55,400},
            ["rem870"]=new[]{80,50,75,60,40,35,320},
            ["m24"]=new[]{80,55,75,55,35,90,600},
            ["m82a1"]=new[]{110,70,70,85,15,90,1000},
            ["m249"]=new[]{90,60,70,55,30,55,650},
            ["m60"]=new[]{95,60,60,70,20,50,700}
        };
        public static void Apply(GameItem item, string key, int caliber, int repairability, bool moddable)
        {
            if (!Data.TryGetValue(key, out var a)) throw new ArgumentException("Missing weapon balance: " + key);
            item.unitValue=a[6];
            GunHelper.InitGun(item,a[0],a[1],a[2],a[3],a[4],a[5],caliber,repairability,moddable);
        }
    }
}
