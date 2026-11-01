using System;
using System.Collections.Generic;
using UnityEngine;
using WeaponCore;

// Standalone catalog: the host must add Definitions() to its registry and enable
// the existing gun slots. No hooks, global patches, new slots or ammunition.
namespace Attachments010
{
    public static class Catalog
    {
        private enum Slot { Rail, Stock, Under, Barrel, Special }
        private enum Scope { All, LongGun, Shotgun, Winchester37, M1911, Glock17, Springfield1903, Sks }
        private sealed class Spec
        {
            public string Key, Name; public Slot Slot; public Scope Scope; public int Value;
            public int[] Percent, Flat; public bool Contraband, Suppressor, Makeshift;
            public Spec(string key, string name, Slot slot, Scope scope, int value, int[] percent, int[] flat, bool illegal = false, bool suppressor = false, bool makeshift = false)
            { Key=key; Name=name; Slot=slot; Scope=scope; Value=value; Percent=percent; Flat=flat; Contraband=illegal; Suppressor=suppressor; Makeshift=makeshift; }
        }
        // Attribute order: firepower, style, reliability, recoil, ergonomics, accuracy.
        private static int[] S(int a=0,int b=0,int c=0,int d=0,int e=0,int f=0) => new[]{a,b,c,d,e,f};
        private static readonly Spec[] Specs = {
            new Spec("red_dot","红点瞄具",Slot.Rail,Scope.All,60,S(b:20,f:25),S(b:5,f:10)),
            new Spec("holographic","全息瞄具",Slot.Rail,Scope.All,85,S(b:25,e:-5,f:30),S(b:7,e:-2,f:10)),
            new Spec("scope_1_4","1–4 倍可变倍率瞄准镜",Slot.Rail,Scope.All,100,S(b:20,e:-10,f:35),S(b:5,e:-3,f:12)),
            new Spec("scope_4_32","4–32 倍可变倍率瞄具",Slot.Rail,Scope.All,140,S(b:25,e:-20,f:50),S(b:7,e:-6,f:15)),
            new Spec("ballistic_sight","弹道计算器瞄准具",Slot.Rail,Scope.All,190,S(b:40,e:-25,f:65),S(b:12,e:-8,f:20)),
            new Spec("light_stock","轻型合成枪托",Slot.Stock,Scope.LongGun,40,S(d:10,e:20),S(d:3,e:6)),
            new Spec("telescopic_stock","战术伸缩枪托",Slot.Stock,Scope.LongGun,65,S(b:20,d:20,e:15),S(b:6,d:6,e:5)),
            new Spec("heavy_stock","重型稳定枪托",Slot.Stock,Scope.LongGun,40,S(d:50,e:-30),S(d:15,e:-9)),
            new Spec("precision_stock","精密射击枪托",Slot.Stock,Scope.LongGun,90,S(d:25,e:-20,f:35),S(d:8,e:-6,f:10)),
            new Spec("light_grip","轻型垂直握把",Slot.Under,Scope.LongGun,25,S(e:15),S(e:5)),
            new Spec("tactical_grip","战术垂直握把",Slot.Under,Scope.LongGun,40,S(b:15,d:15,e:15),S(b:5,d:5,e:5)),
            new Spec("angled_grip","斜角握把",Slot.Under,Scope.LongGun,60,S(e:30,f:5),S(e:9,f:2)),
            new Spec("heavy_grip","重型稳定握把",Slot.Under,Scope.LongGun,50,S(d:40,e:-20),S(d:12,e:-6)),
            new Spec("grenade_launcher","通用下挂榴弹发射器",Slot.Under,Scope.LongGun,150,S(a:75,b:30,d:25,e:-75),S(a:20,b:9,d:8,e:-20)),
            new Spec("underbarrel_shotgun","通用下挂霰弹枪",Slot.Under,Scope.LongGun,130,S(a:50,b:20,e:-50),S(a:15,b:6,e:-15)),
            new Spec("tactical_light","战术灯",Slot.Under,Scope.LongGun,35,S(b:10,e:10),S(b:3,e:3)),
            new Spec("laser","镭射发射器",Slot.Under,Scope.LongGun,50,S(b:20,e:20),S(b:6,e:6)),
            new Spec("muzzle_brake","标准制退器",Slot.Barrel,Scope.All,50,S(b:10,d:30,e:-5),S(b:3,d:9,e:-2)),
            new Spec("advanced_brake","高效制退器",Slot.Barrel,Scope.All,80,S(b:15,d:45,e:-15),S(b:4,d:13,e:-5)),
            new Spec("flash_hider","标准消焰器",Slot.Barrel,Scope.All,35,S(b:10,e:5),S(b:3,e:1)),
            new Spec("tactical_flash_hider","战术消焰器",Slot.Barrel,Scope.All,60,S(b:20,e:10),S(b:6,e:3)),
            new Spec("makeshift_suppressor","简易消音器",Slot.Barrel,Scope.All,25,S(b:-30,c:-20,d:10,e:-20),S(b:-9,c:-6,d:3,e:-6),suppressor:true,makeshift:true),
            new Spec("advanced_suppressor","高级消音器",Slot.Barrel,Scope.All,90,S(b:40,d:25,e:-10),S(b:12,d:8,e:-3),suppressor:true),
            new Spec("full_choke","全喉缩",Slot.Barrel,Scope.Shotgun,50,S(e:-5,f:25),S(e:-2,f:8)),
            new Spec("half_choke","半喉缩",Slot.Barrel,Scope.Shotgun,35,S(f:15),S(f:5)),
            new Spec("optimized_trigger","优化扳机组",Slot.Special,Scope.All,25,S(c:12,e:10),S(c:5,e:3)),
            new Spec("advanced_trigger","高级扳机组",Slot.Special,Scope.All,45,S(c:20,e:20),S(c:7,e:6)),
            new Spec("competition_trigger","竞赛扳机组",Slot.Special,Scope.All,70,S(c:-5,e:25,f:20),S(c:-2,e:8,f:6)),
            new Spec("cleaning","洗剂清理",Slot.Special,Scope.All,20,S(b:10),S(b:3)),
            new Spec("silver_plating","镀银",Slot.Special,Scope.All,70,S(b:30),S(b:9)),
            new Spec("gold_plating","镀金",Slot.Special,Scope.All,130,S(b:50),S(b:15)),
            new Spec("alofs","Alofs 装置",Slot.Special,Scope.Winchester37,150,S(a:50,c:-10,e:-15),S(a:15,c:-3,e:-5),true),
            new Spec("m1911_stock","M1911 可拆卸枪托",Slot.Stock,Scope.M1911,90,S(d:30,e:-10,f:15),S(d:9,e:-3,f:5),true),
            new Spec("glock_switch","Glock 开关",Slot.Special,Scope.Glock17,80,S(a:90,c:-25,d:-90),S(a:25,c:-8,d:-25),true),
            new Spec("pedersen","佩德森转换装置",Slot.Special,Scope.Springfield1903,160,S(a:50,c:-20,f:-15),S(a:15,c:-6,f:-5),true),
            new Spec("sks_bump_stock","SKS 撞火专用枪托",Slot.Stock,Scope.Sks,120,S(a:50,c:-15,d:-50,f:-20),S(a:15,c:-5,d:-15,f:-6),true)
        };
        // Grid50 takes width (length) first, then height. Inventory sprites only;
        // gun-specific attach_010_ overlays retain their own scale and resources.
        private static readonly Dictionary<string, (int Width, int Height)> GridSizes = new Dictionary<string, (int, int)> {
            {"red_dot",(1,1)}, {"holographic",(1,1)}, {"scope_1_4",(2,1)},
            {"scope_4_32",(3,1)}, {"ballistic_sight",(3,1)},
            {"light_stock",(4,2)}, {"telescopic_stock",(4,2)}, {"heavy_stock",(4,2)},
            {"precision_stock",(4,2)}, {"m1911_stock",(4,2)}, {"sks_bump_stock",(4,2)},
            {"light_grip",(1,2)}, {"tactical_grip",(1,2)}, {"angled_grip",(1,2)}, {"heavy_grip",(1,2)},
            {"grenade_launcher",(3,2)}, {"underbarrel_shotgun",(3,2)}, {"tactical_light",(2,1)}, {"laser",(1,1)},
            {"muzzle_brake",(2,1)}, {"advanced_brake",(2,1)}, {"makeshift_suppressor",(2,1)}, {"advanced_suppressor",(2,1)},
            {"flash_hider",(1,1)}, {"tactical_flash_hider",(1,1)}, {"full_choke",(1,1)}, {"half_choke",(1,1)},
            {"optimized_trigger",(1,2)}, {"advanced_trigger",(1,2)}, {"competition_trigger",(1,2)},
            {"cleaning",(2,2)}, {"silver_plating",(2,2)}, {"gold_plating",(2,2)}, {"alofs",(2,2)}, {"pedersen",(2,2)},
            {"glock_switch",(1,1)}
        };
        private static readonly HashSet<string> Shotguns = new HashSet<string> {
            "shotgun", "local.modernwars.winchester1897", "coldwarmodern.rem870", "local.deepspace010.winchester37"
        };
        private static readonly HashSet<string> LongGuns = new HashSet<string> {
            "earlyfirearms.baker", "earlyfirearms.hall_1819", "earlyfirearms.dreyse", "earlyfirearms.mississippi_1841",
            "local.modernwars.winchester1873", "local.modernwars.mosin1891", "local.modernwars.kar98k", "local.modernwars.m1garand", "local.modernwars.bar1918", "local.modernwars.mg42", "local.modernwars.stg44",
            "ak47prototype.rifle", "coldwarmodern.m16a1", "coldwarmodern.fnfal", "coldwarmodern.m24", "coldwarmodern.m82a1", "coldwarmodern.m249", "coldwarmodern.m60", "rimfire22.1022",
            "local.deepspace010.svt40", "local.deepspace010.springfield1903", "local.deepspace010.sks", "local.deepspace010.mini14", "local.deepspace010.m4a1", "local.deepspace010.bfg9000"
        };
        private static readonly HashSet<string> Guns = new HashSet<string> {
            "printed_gun", "handmade_pistol", "heavy_handmade_pistol", "glock_receiver", "smg", "shotgun", "revolver",
            "earlyfirearms.hand_cannon", "earlyfirearms.matchlock", "earlyfirearms.wheellock", "earlyfirearms.blunderbuss", "earlyfirearms.brown_bess", "earlyfirearms.charleville", "earlyfirearms.naval_pistol", "earlyfirearms.colt_1851",
            "local.modernwars.colt_saa", "local.modernwars.m1911", "local.modernwars.thompson1928", "local.modernwars.ppsh41",
            "glock17prototype.pistol", "coldwarmodern.mp5", "coldwarmodern.p90", "coldwarmodern.sw629", "rimfire22.markiv", "rimfire22.p22", "rimfire22.buckmark",
            "local.deepspace010.tt33", "local.deepspace010.mp40", "local.deepspace010.beretta92fs", "local.deepspace010.fp45", "local.deepspace010.texas_defender"
        };
        public static List<ItemDefinition> Definitions()
        {
            var list = new List<ItemDefinition>();
            foreach (var spec in Specs) { var s=spec; list.Add(new ItemDefinition("local.deepspace010.attachment."+s.Key, s.Name, () => Create(s))); }
            return list;
        }
        private static bool Accepts(Scope scope, GameItem gun)
        {
            string id = gun.identifier;
            switch (scope)
            {
                case Scope.All: return Guns.Contains(id) || LongGuns.Contains(id) || Shotguns.Contains(id);
                case Scope.LongGun: return LongGuns.Contains(id) || Shotguns.Contains(id);
                case Scope.Shotgun: return Shotguns.Contains(id);
                case Scope.Winchester37: return gun.IsID("local.deepspace010.winchester37");
                case Scope.M1911: return gun.IsID("local.modernwars.m1911");
                case Scope.Glock17: return gun.IsID("glock17prototype.pistol");
                case Scope.Springfield1903: return gun.IsID("local.deepspace010.springfield1903");
                case Scope.Sks: return gun.IsID("local.deepspace010.sks");
                default: return false;
            }
        }
        private static GameItem Create(Spec s)
        {
            var item=ItemDirectory.CreateEmptyItem(null);
            string spriteKey = "item_010_"+s.Key;
            // At 2500 PPU a 400x400 inventory sprite fits one grid cell.
            // Future pre-fitted rectangular inventory sprites use this same PPU.
            foreach (var mod in ModLoader.Mods) {
                if (mod?.Manifest?.ID != ColdWarModernExpansion.Entry.ModId) continue;
                Sprite original;
                if (mod.Sprites.TryGetValue(spriteKey, out original) && original != null && original.pixelsPerUnit != 2500f) {
                    var scaled = Sprite.Create(original.texture, original.rect, new Vector2(0.5f,0.5f), 2500f);
                    scaled.name = original.name; mod.Sprites[spriteKey] = scaled;
                }
                break;
            }
            item.SetSprite(ColdWarModernExpansion.Entry.ModId, spriteKey);
            item.useSpriteFromMod=true;
            var size = GridSizes[s.Key];
            Grid50.Apply(item, size.Width, size.Height);
            item.name=AttachmentTextLocalization.Name(s.Key);
            item.shortDescription=AttachmentTextLocalization.Description(s.Key);
            item.unitValue=Economy011.RevenueCurve.NewBase("local.deepspace010.attachment."+s.Key);
            switch(s.Slot) {
                case Slot.Rail: GunModHelper.InitGunModRail(item); break;
                case Slot.Stock: GunModHelper.InitGunModStock(item); break;
                case Slot.Under: GunModHelper.InitGunModUnderRail(item); break;
                case Slot.Barrel: GunModHelper.InitGunModBarrel(item,s.Suppressor); break;
                case Slot.Special: GunModHelper.InitGunModSpecial(item); break;
            }
            var p=s.Percent; var f=s.Flat;
            GunModHelper.InitGunMod(item,p[0],p[1],p[2],p[3],p[4],p[5]);
            GunModHelper.InitGunModBaseStats(item,f[0],f[1],f[2],f[3],f[4],f[5]);
            if(s.Makeshift) item.SetGameItemType("MAKESHIFT");
            if(s.Contraband) ContrabandHelper.InitContrabandItem(item,3);
            // Ordinary inventory/container storage remains legal. Any actual gun parent
            // must match the explicit catalogue, including for the exclusive items.
            item.mayInventoryAddItemFunc = (mod, inv) => {
                var parent=inv.GetParentItem();
                if(parent==null || parent.IsTag("CONTAINER_TAG") || !parent.IsTag("ADV_GUN_TAG")) return true;
                return GunModHelper.MayModGun(parent) && Accepts(s.Scope,parent);
            };
            AttachmentVisuals010.Bind(item, s.Key);
            StockVisuals010.Bind(item, s.Key);
            StockVisualsV2.Bind(item, s.Key);
            PlatingVisuals010.Bind(item, s.Key);
            return item;
        }
        // Opt-in per-gun visual layer; overlay must be a separately authored, aligned
        // sprite in the parent gun's mod. Preserves an existing child callback.
        public static void Bind(GameItem part, string gunId, string overlay, Vector2 position)
        {
            if(part==null || string.IsNullOrEmpty(gunId) || string.IsNullOrEmpty(overlay)) throw new ArgumentException("Invalid visual binding");
            var previous=part.parentItemSpriteModifierForItemFunc;
            part.parentItemSpriteModifierForItemFunc = (child, inv, slot, shape, state, parent) => {
                var old=previous == null ? null : previous(child,inv,slot,shape,state,parent);
                var result=old == null ? new List<ItemSpriteModifier>() : new List<ItemSpriteModifier>(old);
                var element=parent as GameItemElement;
                if(element != null && parent.IsID(gunId)) {
                    var sprite=element.ResolveSpriteByName(overlay);
                    if(sprite != null && sprite != RenderHandler.unknownSprite) result.Add(new ItemSpriteModifier(sprite,position));
                }
                return result;
            };
        }
    }
}
