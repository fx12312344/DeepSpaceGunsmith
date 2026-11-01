using System;
using System.Collections.Generic;
using UnityEngine;

namespace Attachments010
{
    // Positions are unrotated UI coordinates, not grid cells. Mod-gun points
    // are estimates from artwork, not measured mechanical mounting points.
    // Stock and exclusive visuals need separate layers and are not invented.
    internal static class AttachmentVisuals010
    {
        private enum Kind { Rail, Under, Barrel }
        private sealed class Anchor
        {
            public readonly Vector2 Rail, Under, Muzzle;
            public Anchor(float rx, float ry, float ux, float uy, float mx, float my)
            { Rail=new Vector2(rx,ry); Under=new Vector2(ux,uy); Muzzle=new Vector2(mx,my); }
        }
        // Six original-gun rail and muzzle positions use the documented native
        // RDS/compensator examples. Underbarrel positions exist only for SMG/shotgun.
        private static readonly Dictionary<string,Anchor> Anchors = new Dictionary<string,Anchor> {
            {"printed_gun",new Anchor(16,4,float.NaN,0,63,-5)},
            {"handmade_pistol",new Anchor(16,5,float.NaN,0,61,-4)},
            {"heavy_handmade_pistol",new Anchor(16,4,float.NaN,0,62,-5)},
            {"glock_receiver",new Anchor(16,4,float.NaN,0,63,-5)},
            {"smg",new Anchor(32,5,68,-18,109,-6)},
            {"shotgun",new Anchor(32,4,45,-18,109,-5)},
            // Estimated from each current PNG's alpha bbox. X samples: 43% for
            // the sight and 72% for underbarrel; Y is the local visible contour.
            // Muzzle uses the right alpha edge and the midpoint at 97% of width.
            // UI: 16 units/cell; 800 PPU = 8 source pixels/UI unit,
            // 1250 PPU = 12.5 pixels/UI unit. These are not verified mounts.
            {"earlyfirearms.hand_cannon",new Anchor(55.2f,-20.1f,float.NaN,0,126.6f,-22.1f)},
            {"earlyfirearms.matchlock",new Anchor(55.2f,-9.9f,float.NaN,0,126.6f,-17.6f)},
            {"earlyfirearms.wheellock",new Anchor(27.7f,-13.4f,float.NaN,0,63.0f,-20.8f)},
            {"earlyfirearms.blunderbuss",new Anchor(56.6f,-8.1f,float.NaN,0,116.8f,-17.1f)},
            {"earlyfirearms.brown_bess",new Anchor(55.2f,-11.9f,float.NaN,0,126.6f,-19.9f)},
            {"earlyfirearms.charleville",new Anchor(55.2f,-17.1f,float.NaN,0,126.9f,-20.0f)},
            {"earlyfirearms.naval_pistol",new Anchor(27.7f,-17.4f,float.NaN,0,63.0f,-20.7f)},
            {"earlyfirearms.baker",new Anchor(55.2f,-14.5f,91.6f,-25.5f,126.6f,-20.2f)},
            {"earlyfirearms.hall_1819",new Anchor(55.3f,-14.2f,91.6f,-21.1f,126.8f,-17.3f)},
            {"earlyfirearms.dreyse",new Anchor(55.2f,-16.0f,91.6f,-22.4f,126.6f,-16.9f)},
            {"earlyfirearms.colt_1851",new Anchor(27.7f,-13.2f,float.NaN,0,63.0f,-14.9f)},
            {"earlyfirearms.mississippi_1841",new Anchor(55.2f,-15.8f,91.6f,-21.5f,126.6f,-18.6f)},
            {"local.modernwars.colt_saa",new Anchor(27.5f,-10.8f,float.NaN,0,64.0f,-13.6f)},
            {"local.modernwars.m1911",new Anchor(27.5f,-6.8f,float.NaN,0,64.0f,-11.2f)},
            {"local.modernwars.winchester1873",new Anchor(55.0f,-8.4f,92.2f,-16.5f,128.0f,-11.9f)},
            {"local.modernwars.mosin1891",new Anchor(55.0f,-15.6f,92.2f,-23.0f,128.0f,-17.2f)},
            {"local.modernwars.kar98k",new Anchor(55.0f,-13.8f,92.2f,-20.6f,128.0f,-14.2f)},
            {"local.modernwars.m1garand",new Anchor(55.0f,-14.1f,92.2f,-21.4f,128.0f,-15.9f)},
            {"local.modernwars.thompson1928",new Anchor(34.4f,-14.9f,float.NaN,0,80.0f,-16.5f)},
            {"local.modernwars.ppsh41",new Anchor(34.4f,-14.2f,float.NaN,0,80.0f,-17.2f)},
            {"local.modernwars.winchester1897",new Anchor(55.0f,-10.9f,92.2f,-22.4f,128.0f,-13.4f)},
            {"local.modernwars.bar1918",new Anchor(55.0f,-11.6f,92.2f,-19.6f,128.0f,-14.4f)},
            {"local.modernwars.mg42",new Anchor(68.8f,-4.1f,115.2f,-26.9f,160.0f,-14.5f)},
            {"local.modernwars.stg44",new Anchor(55.0f,-6.9f,92.2f,-16.1f,128.0f,-8.7f)},
            {"coldwarmodern.m16a1",new Anchor(55.7f,-8.0f,91.9f,-20.4f,126.8f,-17.1f)},
            {"coldwarmodern.fnfal",new Anchor(55.1f,-13.6f,91.4f,-21.2f,126.4f,-18.2f)},
            {"coldwarmodern.mp5",new Anchor(34.8f,-11.8f,float.NaN,0,78.5f,-16.5f)},
            {"coldwarmodern.p90",new Anchor(34.5f,-17.1f,float.NaN,0,79.1f,-24.5f)},
            {"coldwarmodern.sw629",new Anchor(27.8f,-10.5f,float.NaN,0,62.6f,-12.7f)},
            {"coldwarmodern.rem870",new Anchor(55.4f,-12.9f,90.9f,-22.2f,125.1f,-15.0f)},
            {"coldwarmodern.m24",new Anchor(69.4f,-17.0f,114.2f,-26.9f,157.5f,-19.0f)},
            {"coldwarmodern.m82a1",new Anchor(69.1f,-15.0f,113.3f,-44.9f,156.0f,-19.1f)},
            {"coldwarmodern.m249",new Anchor(71.1601633f,-8.3489213f,107.3793120f,-24.9001050f,142.3087580f,-14.6899592f)},
            {"coldwarmodern.m60",new Anchor(69.1759296f,-7.0679648f,112.2019472f,-20.0406334f,153.7144868f,-12.6894545f)},
            // Reworked .22 sprites face right; samples use the same bbox method.
            {"rimfire22.markiv",new Anchor(27.6f,-13.5f,float.NaN,0,63.0f,-15.3f)},
            {"rimfire22.p22",new Anchor(27.6f,-6.4f,float.NaN,0,63.0f,-12.0f)},
            {"rimfire22.buckmark",new Anchor(27.6f,-13.6f,float.NaN,0,63.0f,-15.6f)},
            {"rimfire22.1022",new Anchor(55.1f,-10.1f,91.8f,-20.1f,127.0f,-12.6f)},
            {"local.deepspace010.winchester37",new Anchor(55.6f,-11.4f,90.5f,-19.8f,124.2f,-14.0f)},
            {"local.deepspace010.fp45",new Anchor(27.7457726f,-5.4605248f,float.NaN,0,62.6752187f,-9.6027988f)},
            {"local.deepspace010.texas_defender",new Anchor(27.6945455f,-3.3338182f,float.NaN,0,62.7200000f,-13.3410909f)},
            {"local.deepspace010.tt33",new Anchor(27.7104654f,-7.1018336f,float.NaN,0,62.6983357f,-12.0846262f)},
            {"local.deepspace010.svt40",new Anchor(55.3746058f,-13.4625726f,91.0658921f,-20.2609129f,125.4824896f,-16.4368465f)},
            {"local.deepspace010.springfield1903",new Anchor(55.3967213f,-12.4590164f,91.0688525f,-20.8524590f,125.4819672f,-13.5081967f)},
            {"local.deepspace010.beretta92fs",new Anchor(27.6848908f,-5.8899563f,float.NaN,0,62.6752838f,-9.6908297f)},
            {"local.deepspace010.sks",new Anchor(55.6f,-13.9f,90.5f,-21.4f,124.2f,-15.2f)},
            {"local.deepspace010.mini14",new Anchor(55.8f,-15.3f,89.7f,-22.3f,122.4f,-18.0f)},
            {"local.deepspace010.mp40",new Anchor(34.6822157f,-12.7206997f,float.NaN,0,78.3440233f,-13.7002915f)},
            {"local.deepspace010.m4a1",new Anchor(55.8f,-9.5f,89.7f,-22.1f,122.5f,-18.3f)},
            // BFG anchor estimates require calibration in game.
            {"local.deepspace010.bfg9000",new Anchor(75f,-6f,95f,-40f,136f,-25f)},
            {"ak47prototype.rifle",new Anchor(55.4f,-7.1f,91.1f,-17.5f,125.6f,-13.9f)},
            {"glock17prototype.pistol",new Anchor(27.7f,-6.1f,float.NaN,0,62.4f,-11.1f)},
        };
        private static readonly HashSet<string> NativeGuns = new HashSet<string>("printed_gun handmade_pistol heavy_handmade_pistol glock_receiver smg shotgun".Split(' '));
        private static readonly HashSet<string> Rails = new HashSet<string>("red_dot holographic scope_1_4 scope_4_32 ballistic_sight".Split(' '));
        private static readonly HashSet<string> Unders = new HashSet<string>("light_grip tactical_grip angled_grip heavy_grip grenade_launcher underbarrel_shotgun tactical_light laser".Split(' '));
        private static readonly HashSet<string> Barrels = new HashSet<string>("muzzle_brake advanced_brake flash_hider tactical_flash_hider makeshift_suppressor advanced_suppressor full_choke half_choke".Split(' '));

        internal static void Bind(GameItem part, string key)
        {
            Kind kind;
            if (Rails.Contains(key)) kind=Kind.Rail;
            else if (Unders.Contains(key)) kind=Kind.Under;
            else if (Barrels.Contains(key)) kind=Kind.Barrel;
            else return; // Internal treatments, unverified devices and integral stocks: no invented layer.
            var previous=part.parentItemSpriteModifierForItemFunc;
            part.parentItemSpriteModifierForItemFunc=(child, inv, slot, shape, state, parent) => {
                var old=previous == null ? null : previous(child,inv,slot,shape,state,parent);
                var result=old == null ? new List<ItemSpriteModifier>() : new List<ItemSpriteModifier>(old);
                Anchor anchor;
                if (parent == null || !(parent is GameItemElement) || !Anchors.TryGetValue(parent.identifier,out anchor)) return result;
                var point=kind==Kind.Rail ? anchor.Rail : kind==Kind.Under ? anchor.Under : anchor.Muzzle;
                if (float.IsNaN(point.x)) return result;
                Sprite sprite=null;
                foreach (var mod in ModLoader.Mods) {
                    if (mod?.Manifest?.ID != ColdWarModernExpansion.Entry.ModId) continue;
                    if (mod.Sprites.TryGetValue("attach_010_"+key,out sprite) && sprite != null && sprite.pixelsPerUnit != 800f) {
                        var adjusted=Sprite.Create(sprite.texture,sprite.rect,new Vector2(.5f,.5f),800f);
                        adjusted.name=sprite.name; mod.Sprites["attach_010_"+key]=adjusted; sprite=adjusted;
                    }
                    break;
                }
                if (sprite == null || sprite == RenderHandler.unknownSprite) return result;
                float width=sprite.rect.width/8f, height=sprite.rect.height/8f;
                // Rail's native coordinate is the view top-left; for estimates it
                // is a top mounting location, placing the part above the weapon.
                Vector2 position = kind==Kind.Rail ? new Vector2(point.x,point.y+height)
                    : kind==Kind.Under ? new Vector2(point.x-width/2f,point.y)
                    : new Vector2(point.x,point.y+height/2f);
                // Native original rail positions already refer to top-left, unlike estimates.
                if (kind==Kind.Rail && NativeGuns.Contains(parent.identifier)) position=point;
                result.Add(new ItemSpriteModifier(sprite, position, 0, false));
                return result;
            };
        }
    }
}
