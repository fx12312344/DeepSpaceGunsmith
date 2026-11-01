using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using WeaponCore;

namespace Bfg010
{
    // Registration, sprite asset and any dedicated sorting integration belong to the caller.
    public static class Catalog
    {
        public const string Id = "local.deepspace010.bfg9000";
        public const string SpriteKey = "asset_bfg9000";
        // Fictional placeholder only: no ammunition item is supplied or associated with .50 BMG.
        public const int FictionalEnergyCaliber = 990001;

        public static IEnumerable<ItemDefinition> Definitions()
        {
            yield return new ItemDefinition(Id, "BFG 9000 gun", Create);
        }

        private static GameItem Create()
        {
            var (window, stock, rail, magazine, underbarrel, barrel, special1, special2, special3) = GunModHelper.CreateGunModWindow(true, true, 5);
            var item = ItemDirectory.CreateEmptyItem(null);
            foreach (var mod in ModLoader.Mods)
            {
                if (mod == null || mod.Manifest == null || mod.Manifest.ID != ColdWarModernExpansion.Entry.ModId) continue;
                Sprite original;
                if (mod.Sprites.TryGetValue(SpriteKey, out original) && original != null && original.pixelsPerUnit != 800f)
                {
                    var scaled = Sprite.Create(original.texture, original.rect, new Vector2(0.5f, 0.5f), 800f);
                    scaled.name = original.name;
                    mod.Sprites[SpriteKey] = scaled;
                }
                break;
            }
            item.SetSprite(ColdWarModernExpansion.Entry.ModId, SpriteKey);
            item.useSpriteFromMod = true;
            Grid50.Apply(item, 10, 3);
            item.SetContentWindow(window);
            item.name = EnglishLocalization.L.GunName("bfg9000", "毁灭战士BFG 9000能量狙击枪");
            item.shortDescription = WeaponDescriptions.Text.Gun("bfg9000");
            item.SetGameItemType("WEAPON");
            item.SetGameItemType("FIREARM");
            item.SetGameItemType("MIDLINE");
            // Tier 2 mirrors the existing M82 game classification, not a real-world claim.
            ContrabandHelper.InitContrabandItem(item, 2);
            AudioHelper.InitHeavyGun(item);
            item.unitValue = 1800;
            GunHelper.InitGun(item, 130, 85, 85, 95, 30, 100, FictionalEnergyCaliber, 40, true);
            ColdWarModernExpansion.ExistingGunSlots.Configure(stock, rail, magazine, underbarrel, barrel, special1, special2, special3, true, true, true, true);
            GunHelper.SetEquipmentCondition(item, (ItemFeatureList.EquipmentCondition)2);
            return item;
        }
    }
    // Give the fictional energy caliber a readable in-game name, without registering ammunition.
    [HarmonyPatch(typeof(GunHelper), nameof(GunHelper.GetCaliberName))]
    internal static class EnergyCaliberName
    {
        private static void Postfix(int caliber, ref string __result)
        {
            if (caliber == Catalog.FictionalEnergyCaliber)
                __result = EnglishLocalization.L.Pick("虚构能量", "Fictional energy");
        }
    }
}
