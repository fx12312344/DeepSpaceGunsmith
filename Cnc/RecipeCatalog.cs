using System;
using System.Collections.Generic;

namespace GunsmithMachineIntegration
{
    public sealed class CncRecipe
    {
        public readonly string Id, Zh, En;
        public readonly int Metal, Electronics, Plastic;
        public readonly bool SingleNight;
        public CncRecipe(string id,string zh,string en,int metal,int electronics,int plastic,bool singleNight=false)
        { Id=id; Zh=zh; En=en; Metal=metal; Electronics=electronics; Plastic=plastic; SingleNight=singleNight; }
        public string Name=>EnglishLocalization.L.Pick(Zh,En);
    }
    public static class RecipeCatalog
    {
        // Explicit balanced single-item reserves; offline 0.8V / 1.2 - 8 budget.
        // Zero inputs stay zero; positive inputs stay >= 1. See ../平衡调整/CNC调整依据.md.
        private const string A="local.deepspace010.attachment.", M="local.modernwars.", C="coldwarmodern.", R="rimfire22.";
        public static readonly CncRecipe[] All = {
            new CncRecipe(A+"red_dot","红点瞄具","Red Dot Sight",19,5,5),
            new CncRecipe(A+"holographic","全息瞄具","Holographic Sight",25,8,8),
            new CncRecipe(A+"scope_1_4","1–4倍瞄准镜","1–4x Scope",29,9,9),
            new CncRecipe(A+"scope_4_32","4–32倍瞄准镜","4–32x Scope",45,13,13),
            new CncRecipe(A+"ballistic_sight","弹道计算器瞄具","Ballistic Sight",62,19,20),
            new CncRecipe(A+"light_stock","轻型合成枪托","Light Stock",11,2,4),
            new CncRecipe(A+"telescopic_stock","战术伸缩枪托","Telescopic Stock",17,5,5),
            new CncRecipe(A+"heavy_stock","重型稳定枪托","Heavy Stock",11,2,4),
            new CncRecipe(A+"precision_stock","精密射击枪托","Precision Stock",26,8,8),
            new CncRecipe(A+"light_grip","轻型垂直握把","Light Grip",5,1,2),
            new CncRecipe(A+"tactical_grip","战术垂直握把","Tactical Grip",11,2,4),
            new CncRecipe(A+"angled_grip","斜角握把","Angled Grip",19,5,5),
            new CncRecipe(A+"heavy_grip","重型稳定握把","Heavy Grip",14,4,4),
            new CncRecipe(A+"grenade_launcher","通用下挂榴弹发射器","Underbarrel Launcher",49,14,16),
            new CncRecipe(A+"underbarrel_shotgun","通用下挂霰弹枪","Underbarrel Shotgun",42,12,12),
            new CncRecipe(A+"tactical_light","战术灯","Tactical Light",8,2,2),
            new CncRecipe(A+"laser","镭射发射器","Laser",14,4,4),
            new CncRecipe(A+"muzzle_brake","标准制退器","Muzzle Brake",14,4,4),
            new CncRecipe(A+"advanced_brake","高效制退器","Advanced Brake",23,7,7),
            new CncRecipe(A+"flash_hider","标准消焰器","Flash Hider",8,2,2),
            new CncRecipe(A+"tactical_flash_hider","战术消焰器","Tactical Flash Hider",17,5,5),
            new CncRecipe(A+"makeshift_suppressor","简易消音器","Makeshift Suppressor",5,1,2),
            new CncRecipe(A+"advanced_suppressor","高级消音器","Advanced Suppressor",26,8,8),
            new CncRecipe(A+"full_choke","全喉缩","Full Choke",14,4,4),
            new CncRecipe(A+"half_choke","半喉缩","Half Choke",8,2,2),
            new CncRecipe(A+"optimized_trigger","优化扳机组","Optimized Trigger",5,1,2),
            new CncRecipe(A+"advanced_trigger","高级扳机组","Advanced Trigger",13,3,4),
            new CncRecipe(A+"competition_trigger","竞赛扳机组","Competition Trigger",20,6,6),
            new CncRecipe(A+"cleaning","洗剂清理","Cleaning Treatment",1,1,1),
            new CncRecipe(A+"silver_plating","镀银","Silver Plating",20,6,6),
            new CncRecipe(A+"gold_plating","镀金","Gold Plating",42,12,12),
            new CncRecipe(A+"alofs","Alofs装置","Alofs Device",49,14,16),
            new CncRecipe(A+"m1911_stock","M1911可拆枪托","M1911 Stock",26,8,8),
            new CncRecipe(A+"glock_switch","Glock开关","Glock Switch",23,7,7),
            new CncRecipe(A+"pedersen","佩德森转换装置","Pedersen Device",53,15,17),
            new CncRecipe(A+"sks_bump_stock","SKS撞火枪托","SKS Bump Stock",37,11,11),
            new CncRecipe(M+"part_m1911","M1911弹匣","M1911 Magazine",11,2,4),
            new CncRecipe(M+"part_thompson1928","汤普森弹匣","Thompson Magazine",11,2,4),
            new CncRecipe(M+"part_ppsh41","PPSh-41弹鼓","PPSh-41 Drum",23,7,7),
            new CncRecipe(M+"part_bar1918","BAR弹匣","BAR Magazine",11,2,4),
            new CncRecipe(M+"part_stg44","StG44弹匣","StG44 Magazine",11,2,4),
            new CncRecipe(M+"clip_mosin1891","莫辛五发桥夹","Mosin Clip",11,2,4),
            new CncRecipe(M+"clip_kar98k","毛瑟五发桥夹","Mauser Clip",11,2,4),
            new CncRecipe(M+"clip_m1garand","M1整装弹夹","M1 En Bloc Clip",11,2,4),
            new CncRecipe(M+"belt_mg42","MG42弹链","MG42 Belt",11,2,4),
            new CncRecipe("ak47prototype.magazine","AK弹匣","AK Magazine",16,4,4),
            new CncRecipe("glock17prototype.magazine","G17弹匣","G17 Magazine",11,2,4),
            new CncRecipe(C+"m16a1.feed","M16A1弹匣","M16A1 Magazine",16,4,4),
            new CncRecipe(C+"fnfal.feed","FN FAL弹匣","FN FAL Magazine",16,4,4),
            new CncRecipe(C+"mp5.feed","MP5弹匣","MP5 Magazine",16,4,4),
            new CncRecipe(C+"p90.feed","P90弹匣","P90 Magazine",16,4,4),
            new CncRecipe(C+"m82a1.feed","M82A1弹匣","M82A1 Magazine",16,4,4),
            new CncRecipe(C+"m249.feed","M249弹链组件","M249 Belt Assembly",29,9,9),
            new CncRecipe(C+"m60.feed","M60弹链组件","M60 Belt Assembly",29,9,9),
            new CncRecipe(R+"markiv.magazine","Mark IV弹匣","Mark IV Magazine",8,2,2),
            new CncRecipe(R+"p22.magazine","P22弹匣","P22 Magazine",8,2,2),
            new CncRecipe(R+"buckmark.magazine","Buck Mark弹匣","Buck Mark Magazine",8,2,2),
            new CncRecipe(R+"1022.magazine","10/22弹匣","10/22 Magazine",8,2,2),
            // Barrel reserves are budget-adjusted, not the old 1/2/3-ingot equivalence.
            new CncRecipe("gunsmithmachine.barrel_short","短枪管","Short Barrel",90,0,0),
            new CncRecipe("gunsmithmachine.barrel_medium","中枪管","Medium Barrel",197,0,0),
            new CncRecipe("gunsmithmachine.barrel_long","长枪管","Long Barrel",300,0,0),
            // Budget-adjusted gun-part reserve; not a physical scrap conversion.
            // Material commodities below intentionally retain 10/16 metal and one-night processing.
            new CncRecipe("gun_part","枪械零件","Gun Part",17,0,0),
            new CncRecipe("local.deepspace010.material.casings","弹壳堆","Pile of Cartridge Cases",10,0,0,true),
            new CncRecipe("local.deepspace010.material.projectiles","弹头堆","Pile of Projectiles",16,0,0,true)
        };
        public static CncRecipe Find(string id) { foreach(var recipe in All) if(recipe.Id==id)return recipe; return null; }
        public static int Cost(int single,int quantity) { if(single<0 || quantity<1 || quantity>99) throw new ArgumentOutOfRangeException(); return checked((int)(((long)single*(10L+7L*(quantity-1))+9)/10)); }
        public static int Nights(int quantity) { if(quantity<1 || quantity>99) throw new ArgumentOutOfRangeException(); return quantity<=2 ? 1 : quantity<=6 ? 2 : 3; }
        public static int Nights(CncRecipe recipe,int quantity) { if(recipe==null) throw new ArgumentNullException(nameof(recipe)); int normal=Nights(quantity); return recipe.SingleNight ? 1 : normal; }
    }
}
