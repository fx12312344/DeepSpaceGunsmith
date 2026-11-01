using System;
using System.Collections.Generic;
using UnityEngine;

// Source-integrated shared primitives. No separate runtime DLL or global patch.
// 0.46D's scanner refuses unlisted cross-mod assembly references.
namespace WeaponCore
{
    public static class Grid50
    {
        public const int MillimetersPerCell = 50;
        public static int Cells(int millimeters)
        {
            if (millimeters <= 0) throw new ArgumentOutOfRangeException(nameof(millimeters));
            return checked((int)(((long)millimeters + MillimetersPerCell - 1) / MillimetersPerCell));
        }
        public static void Apply(GameItem item, int width, int height)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (width <= 0 || height <= 0 || width > 256 || height > 256) throw new ArgumentOutOfRangeException("grid dimensions");
            item.SetShape(new GridShapeBuilder().SetDataFill(width, height, 1));
        }
    }
    public sealed class ItemDefinition
    {
        public readonly string Id, Button;
        public readonly Func<GameItem> Factory;
        public ItemDefinition(string id, string button, Func<GameItem> factory)
        { if(string.IsNullOrEmpty(id) || factory == null) throw new ArgumentException("Invalid item definition"); Id=id; Button=button; Factory=factory; }
    }
    public static class Registry
    {
        public static void Register(ModItemDirectory dir, IEnumerable<ItemDefinition> definitions, ModLog log)
        {
            if (dir == null || dir.factoryDictionary == null) return;
            foreach(var def in definitions)
            {
                if (DirectoryMaster.Has<GameItem>(def.Id) || dir.Has(def.Id)) { log.Warning("Skipped existing ID: " + def.Id); continue; }
                if (dir.Add(def.Id, def.Factory)) log.Log("Registered " + def.Id);
                else log.Error("Could not register " + def.Id);
            }
        }
        public static bool SpawnOwnedToInventory(string id, ModLog log)
        {
            if (!PlayerStore.HasInstance || EmporiumEntry.Instance == null || EmporiumEntry.Instance.invElement == null || !DirectoryMaster.Has<GameItem>(id))
            { log.Warning("Load a store save and check registration before spawning " + id); return false; }
            var item = ModHelper.SpawnItem(id);
            if (item == null) return false;
            ModHelper.SetItemOwnerShip(item, true);
            GraphUtils.TryAcceptAll(EmporiumEntry.Instance.invElement, item, -1);
            if (item.parentInventory == null) { item.Destroy(); log.Warning("Inventory is full: " + id); return false; }
            return true;
        }
    }
    public static class Ammo
    {
        public const string Normal = "NORMAL_AMMO";
        public static void Mark(GameItem item, int caliber, string ammoType = Normal)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (caliber <= 0 || string.IsNullOrEmpty(ammoType)) throw new ArgumentException("Invalid caliber or ammunition type");
            item.SetGameItemType("AMMUNITION");
            item.ModifyTag("GUN_CALIBER_TAG", state => state.Enable().SetInt(caliber), false);
            item.ModifyTag("AMMO_TYPE_TAG", state => state.Enable().SetString(ammoType), false);
        }
    }
    public static class Visuals
    {
        public static void Attach(GameItem part, string gunId, string overlay)
        {
            if (part == null || string.IsNullOrEmpty(gunId) || string.IsNullOrEmpty(overlay)) throw new ArgumentException("Invalid visual binding");
            part.parentItemSpriteModifierForItemFunc = (child, inv, slot, shape, state, parent) =>
            {
                var result = new List<ItemSpriteModifier>();
                if (parent == null || !parent.IsID(gunId)) return result;
                var element = parent as GameItemElement;
                if (element == null) return result;
                var sprite = element.ResolveSpriteByName(overlay);
                if (sprite != null && sprite != RenderHandler.unknownSprite) result.Add(new ItemSpriteModifier(sprite, Vector2.zero));
                return result;
            };
        }
    }
}
