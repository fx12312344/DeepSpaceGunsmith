using System;
using UnityEngine;

namespace StorageContainers
{
    // IDs and inventory graph structure are save-file keys: do not change casually.
    public sealed class Definition
    {
        public readonly string Key, Name, Type;
        public readonly int ItemWidth, ItemHeight, GridWidth, GridHeight;
        public readonly long Value;
        public Definition(string key, string name, string type, int iw, int ih, int gw, int gh, long value)
        { Key=key; Name=name; Type=type; ItemWidth=iw; ItemHeight=ih; GridWidth=gw; GridHeight=gh; Value=value; }
        public string Id => "storagecontainers." + Key;
    }
    public static class Config
    {
        // Width and height are grid cells, NOT item count; prices are also temporary.
        public static readonly Definition[] Items = {
            new Definition("small_rack", "小型枪架", "WEAPON", 4,3, 8,6, 100),
            new Definition("medium_rack", "中型枪架", "WEAPON", 6,3, 20,6, 200),
            new Definition("large_rack", "大型枪架", "WEAPON", 8,3, 30,6, 400),
            new Definition("accessory_box", "配件箱", "GUN_MOD", 4,3, 8,12, 150),
            new Definition("ammo_box", "弹药箱", "AMMUNITION", 4,3, 8,12, 150)
        };
    }
    public sealed class Entry
    {
        public const string ModId = "local.coldwarmodern";
        private const string DebugOwner = "local.coldwarmodern.storage";
        private ModLog log;
        private bool enabled;
        public void Init(ModManifest manifest) { log = new ModLog(manifest); }
        public void OnEnable()
        {
            if (enabled) return;
            ModHook.OnModItemDirectoryInit += Register;
            ModHook.OnPlaceInventorInventoryItemEarly += OfferAtJackson;
            foreach (var definition in Config.Items)
            {
                var d = definition;
                ModHelper.AddDebugFunction(DebugOwner, ModHelper.DEBUG_CATEGORY_SPAWN, d.Name, () => Spawn(d), false);
            }
            enabled = true;
            foreach (var dir in Resources.FindObjectsOfTypeAll<ModItemDirectory>())
                if (dir != null && dir.factoryDictionary != null) Register(dir);
        }
        public void OnDisable()
        {
            if (!enabled) return;
            ModHook.OnModItemDirectoryInit -= Register;
            ModHook.OnPlaceInventorInventoryItemEarly -= OfferAtJackson;
            ModHelper.RemoveDebugFunctions(DebugOwner);
            enabled = false;
        }
        // One independent roll per container, per inventor stock-generation event.
        // Temporary balance value; applies when visiting Jackson, not his visit to the player.
        private const int JacksonOfferPercent = 35;
        private void OfferAtJackson(System.Collections.Generic.List<GameItem> owned)
        {
            if (!PlayerStore.HasInstance || PlayerStore.Instance.playerLocation != PlayerStore.PlayerLocation.INVENTOR
                || EmporiumEntry.Instance == null || EmporiumEntry.Instance.frontInvinvElement == null) return;
            foreach (var d in Config.Items)
            {
                if (!DirectoryMaster.Has<GameItem>(d.Id) || !RNG.Roll(JacksonOfferPercent)) continue;
                if (!PlayerStore.Instance.TryAddDirectSellingItemToTable(d.Id, false, false, false, 0))
                    log.Warning("Jackson offer could not be placed: " + d.Id);
            }
        }
        private void Register(ModItemDirectory dir)
        {
            if (dir == null || dir.factoryDictionary == null) return;
            foreach (var definition in Config.Items)
            {
                var d = definition;
                if (DirectoryMaster.Has<GameItem>(d.Id) || dir.Has(d.Id)) { log.Warning("Existing ID skipped: " + d.Id); continue; }
                if (!dir.Add(d.Id, () => Create(d))) log.Error("Registration failed: " + d.Id);
            }
        }
        private static void SetHighResolutionSprite(GameItem item, string key)
        {
            foreach (var mod in ModLoader.Mods)
            {
                if (mod == null || mod.Manifest == null || mod.Manifest.ID != ModId) continue;
                Sprite original;
                if (!mod.Sprites.TryGetValue(key, out original) || original == null) break;
                if (original.pixelsPerUnit != 800f)
                {
                    var scaled = Sprite.Create(original.texture, original.rect, new Vector2(0.5f, 0.5f), 800f);
                    scaled.name = original.name;
                    mod.Sprites[key] = scaled;
                }
                break;
            }
            item.SetSprite(ModId, key);
            item.useSpriteFromMod = true;
        }
        private static GameItem Create(Definition d)
        {
            var item = ItemDirectory.CreateEmptyItem(null);
            item.identifier = d.Id; item.identifierName = d.Id;
            SetHighResolutionSprite(item, d.Key);
            // Every dedicated storage item uses rectangular hit testing, including
            // transparent space between rack supports. Keep both rendered backgrounds aligned.
            if (item is GameItemElement visual)
            {
                if (visual.background?.image != null) visual.background.image.alphaHitTestMinimumThreshold = 0f;
                if (visual.fakeBackground?.image != null) visual.fakeBackground.image.alphaHitTestMinimumThreshold = 0f;
            }
            item.SetShape(new GridShapeBuilder().SetDataFill(d.ItemWidth, d.ItemHeight, 1));
            item.SetName(UtilityItems.UtilityItemText.StorageName(d.Key, d.Name));
            item.shortDescription = UtilityItems.UtilityItemText.StorageDescription(d.Key);
            item.SetValue(d.Value); item.unitBaseValue = d.Value;
            item.SetGameItemType("STORAGE");
            var (window, inventory) = DirectoryUtils.CreateInventoryWindow(d.GridWidth, d.GridHeight, true);
            inventory.identifier = d.Id + ".inventory";
            item.SetContentWindow(window);
            ContainerHelper.InitContainerItem(inventory, item);
            // The engine OR-reduces multicast callbacks, so adding a filter with += is unsafe.
            // Preserve all existing predicates, then AND the requested item type in one callback.
            var original = inventory.mayInventoryAddItemFunc;
            inventory.mayInventoryAddItemFunc = (candidate, inv) => candidate != null && candidate.IsGameItemType(d.Type)
                && (original == null || original.InvokeAllReduce(candidate, inv, InvokeFuncExtensions.BoolReduceOR));
            // Hoppers otherwise bypass the inventory's filtering callback entirely.
            inventory.EnableTag("NO_HOPPER_INVENTORY_TAG");
            ContainerHelper.AddContainerTooltipTag(item);
            ContainerHelper.SetBasicContainerValueChangeRule(item, inventory);
            return item;
        }
        private void Spawn(Definition d)
        {
            if (!PlayerStore.HasInstance || EmporiumEntry.Instance == null || EmporiumEntry.Instance.invElement == null || !DirectoryMaster.Has<GameItem>(d.Id))
            { log.Warning("Load a store save before spawning " + d.Id); return; }
            var item = ModHelper.SpawnItem(d.Id);
            if (item == null) return;
            ModHelper.SetItemOwnerShip(item, true);
            GraphUtils.TryAcceptAll(EmporiumEntry.Instance.invElement, item, -1);
            if (item.parentInventory == null) { item.Destroy(); log.Warning("Inventory full: " + d.Id); }
        }
    }
}
