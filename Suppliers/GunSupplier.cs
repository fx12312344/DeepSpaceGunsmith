using System;
using System.Collections.Generic;

namespace GunSupplierPrototype
{
    public sealed class Entry
    {
        const string Id = "local.gunsupplierprototype.seller";
        const string ModId = "local.gunsupplierprototype";
        const string DueKey = "gunsmith_supplier_due_day";
        bool enabled;
        int epoch;
        ModLog log;
        public void Init(ModManifest manifest) { log = new ModLog(manifest); }
        public void OnEnable() { if (enabled) return; foreach (var mod in ModLoader.Mods) if (mod != null && mod.IsEnabled && mod.Manifest != null && mod.Manifest.ID == ModId) { log.Warning("Standalone firearms supplier already enabled; integrated supplier skipped."); return; } unchecked { epoch++; } enabled = true; ModHook.OnGenerateCustomerVeryLate += Generate; ModHook.OnNetworkUpgradeUnlocked += Unlocked; ModHook.OnGameLoadedEarly += Loaded; ModHook.OnGameLoadedLate += Migrate; }
        void Migrate()
        {
            if (!enabled || !PlayerStore.HasInstance || PlayerStore.Instance == null || StoreStation.Instance == null) return;
            try {
                if (!ModHelper.HasModData(ModId, DueKey)) {
                    int day = StoreStation.Instance.dayCounter;
                    // One-time legacy compensation is not a purchase notification.
                    ModHelper.SetModDataInt(ModId, DueKey, HasRetiredGunsmithService() && day < int.MaxValue ? day + 1 : 0);
                }
            } catch (Exception e) { log.Warning("Supplier migration skipped: " + e); }
        }
        // Invalidate callbacks belonging to the previous loaded run; data itself stays per-run.
        void Loaded() { unchecked { epoch++; } }
        void Unlocked(string id)
        {
            if (!enabled || id != NetworkUpgrade.RETIRED_GUNSMITH || !PlayerStore.HasInstance || PlayerStore.Instance == null || StoreStation.Instance == null) return;
            try {
                // Duplicate/reentrant unlock notifications must not postpone an outstanding visit.
                if ((!ModHelper.HasModData(ModId, DueKey) || ModHelper.GetModDataInt(ModId, DueKey, -2) == 0) && StoreStation.Instance.dayCounter < int.MaxValue)
                    ModHelper.SetModDataInt(ModId, DueKey, StoreStation.Instance.dayCounter + 1);
            } catch (Exception e) { log.Warning("Supplier appointment not recorded: " + e); }
        }
        public void OnDisable()
        {
            if (!enabled) return;
            enabled = false;
            unchecked { epoch++; }
            ModHook.OnGenerateCustomerVeryLate -= Generate;
            ModHook.OnNetworkUpgradeUnlocked -= Unlocked;
            ModHook.OnGameLoadedEarly -= Loaded;
            ModHook.OnGameLoadedLate -= Migrate;
            if (PlayerStore.HasInstance && PlayerStore.Instance.storeClientManager?.clientStack != null)
                PlayerStore.Instance.storeClientManager.clientStack.RemoveAll(c => c != null && c.identifier == Id);
        }
        void Generate(StoreClientManager manager)
        {
            if (!enabled || manager == null || manager.clientStack == null || manager.clientTracker == null || !PlayerStore.HasInstance || PlayerStore.Instance.storeClientManager != manager) return;
            try
            {
                // Supports enabling the mod after a run has already loaded.
                // Generate runs only with a live store manager; missing data can
                // therefore receive the same one-time legacy migration safely.
                if (!ModHelper.HasModData(ModId, DueKey)) Migrate();
                if (!ModHelper.HasModData(ModId, DueKey)) return;
                int due = ModHelper.GetModDataInt(ModId, DueKey, 0);
                bool appointment = due > 0 && StoreStation.Instance != null && StoreStation.Instance.dayCounter >= due;
                // A booked first visit takes precedence over normal random arrivals.
                if (due > 0 && !appointment) return;
                foreach (var c in manager.clientStack) if (c != null && c.identifier == Id) return;
                var current = PlayerStore.Instance.currentClientInstance;
                if (current != null && current.GetClientBlueprint() != null && current.GetClientBlueprint().identifier == Id) return;
                if (!HasRetiredGunsmithService() || !HasGun() || (!appointment && (StoreClientManager.IsClientOnCooldown(Id) || !RNG.Roll(20)))) return;
                bool english = string.Equals(LocHelper.GetCurrentLocaleCode(), "en", StringComparison.OrdinalIgnoreCase);
                var seller = new StoreClient { identifier = Id, displayName = english ? "Firearms Wholesaler" : "枪械批发商", spriteName = SpriteDict.GetRandomScavSpriteName(), clientIntent = StoreClient.ClientIntent.SELL, isOfferWholesale = true, wholesaleDiscount = -15, cooldown = 7 };
                ModHelper.SetClientSource(seller, ModId);
                StoreClientFactionSetup.InitScav(seller);
                int generationEpoch = epoch;
                bool stockStarted = false;
                seller.OnArrival += () => {
                    if (!enabled || generationEpoch != epoch || !PlayerStore.HasInstance || PlayerStore.Instance == null || StoreStation.Instance == null) return;
                    var active = PlayerStore.Instance.currentClientInstance;
                    if (active == null || !ReferenceEquals(active.GetClientBlueprint(), seller)) return;
                    try {
                        int pending = ModHelper.GetModDataInt(ModId, DueKey, 0);
                        if (pending > 0 && StoreStation.Instance.dayCounter >= pending) ModHelper.SetModDataInt(ModId, DueKey, -1);
                    } catch (Exception e) { log.Warning("Supplier appointment arrival update failed: " + e); }
                };
                seller.mainDialogue.SetText(seller.displayName, english ? "I've got a shipment of firearms and supplies. Take a look." : "有一批枪械和用品，看看有没有你需要的。").SetEndAction(() => {
                    if (!enabled || generationEpoch != epoch || !PlayerStore.HasInstance) return;
                    var active = PlayerStore.Instance.currentClientInstance;
                    if (active == null || !ReferenceEquals(active.GetClientBlueprint(), seller) || stockStarted) return;
                    stockStarted = true;
                    Stock();
                    if (enabled && generationEpoch == epoch && PlayerStore.HasInstance &&
                        PlayerStore.Instance.currentClientInstance != null &&
                        ReferenceEquals(PlayerStore.Instance.currentClientInstance.GetClientBlueprint(), seller)) seller.OnIntroduced();
                });
                seller.AddBasicDialog();
                seller.CompleteClientCreation();
                if (appointment) {
                    // AddNextClient bypasses cooldown; preserve a longer existing cooldown.
                    int oldCooldown = manager.clientTracker.TryGetValue(Id, out var tracked) ? tracked : 0;
                    manager.AddNextClient(seller);
                    if (oldCooldown > seller.cooldown && manager.clientTracker.ContainsKey(Id)) manager.clientTracker[Id] = oldCooldown;
                } else manager.TryAddClient(seller);
            }
            catch (Exception e) { log.Warning("Supplier generation skipped: " + e); }
        }
        static bool HasRetiredGunsmithService()
        {
            if (!PlayerStore.HasInstance || PlayerStore.Instance == null || PlayerStore.Instance.networkUpgrade == null) return false;
            return PlayerStore.Instance.networkUpgrade.TryGetValue(NetworkUpgrade.RETIRED_GUNSMITH, out var upgrade) && upgrade != null && upgrade.IsUnlocked();
        }
        static bool HasGun() { foreach (var id in Catalog.Guns) if (DirectoryMaster.Has<GameItem>(id)) return true; return false; }
        void Stock()
        {
            if (!enabled || !PlayerStore.HasInstance) return;
            try
            {
                var chosen = new HashSet<string>();
                int guns = RNG.GetRandomInt(1, 2);
                int parts = RNG.GetRandomInt(3, 4);
                int ammo = RNG.GetRandomInt(8, 14);
                for (int i = 0; i < guns; i++) Offer(Catalog.Guns, chosen, true);
                for (int i = 0; i < parts; i++) Offer(Catalog.Parts, chosen, false);
                // Each attempt spawns one default ammo commodity, not a quantity label
                // nor an assumed number of rounds. Types may repeat.
                for (int i = 0; i < ammo; i++) Offer(Catalog.Ammo, null, false);
            }
            catch (Exception e) { log.Warning("Supplier stock incomplete: " + e); }
        }
        void Offer(string[] pool, HashSet<string> chosen, bool weightedGuns)
        {
            var available = new List<string>();
            int total = 0;
            foreach (string id in pool) if ((chosen == null || !chosen.Contains(id)) && DirectoryMaster.Has<GameItem>(id))
            { available.Add(id); total += weightedGuns && id.StartsWith("earlyfirearms.", StringComparison.Ordinal) ? 1 : 20; }
            if (total == 0) return;
            int ticket = RNG.GetRandomInt(0, total - 1);
            string selected = null;
            foreach (string id in available) {
                ticket -= weightedGuns && id.StartsWith("earlyfirearms.", StringComparison.Ordinal) ? 1 : 20;
                if (ticket < 0) { selected = id; break; }
            }
            if (selected == null) return;
            if (chosen != null) chosen.Add(selected);
            if (!PlayerStore.Instance.TryAddDirectSellingItemToTable(selected, false, false, false, 0))
                log.Warning("Supplier offer not placed: " + selected);
        }
    }
    internal static class Catalog
    {
        public static readonly string[] Guns = { "ak47prototype.rifle", "glock17prototype.pistol", "coldwarmodern.m16a1", "coldwarmodern.fnfal", "coldwarmodern.mp5", "coldwarmodern.p90", "coldwarmodern.sw629", "coldwarmodern.rem870", "coldwarmodern.m24", "coldwarmodern.m82a1", "local.deepspace010.bfg9000", "coldwarmodern.m249", "coldwarmodern.m60", "rimfire22.markiv", "rimfire22.p22", "rimfire22.buckmark", "rimfire22.1022", "local.modernwars.colt_saa", "local.modernwars.m1911", "local.modernwars.winchester1873", "local.modernwars.mosin1891", "local.modernwars.kar98k", "local.modernwars.m1garand", "local.modernwars.thompson1928", "local.modernwars.ppsh41", "local.modernwars.winchester1897", "local.modernwars.bar1918", "local.modernwars.mg42", "local.modernwars.stg44", "local.deepspace010.winchester37", "local.deepspace010.fp45", "local.deepspace010.texas_defender", "local.deepspace010.tt33", "local.deepspace010.svt40", "local.deepspace010.springfield1903", "local.deepspace010.beretta92fs", "local.deepspace010.sks", "local.deepspace010.mini14", "local.deepspace010.mp40", "local.deepspace010.m4a1", "earlyfirearms.hand_cannon", "earlyfirearms.matchlock", "earlyfirearms.wheellock", "earlyfirearms.blunderbuss", "earlyfirearms.brown_bess", "earlyfirearms.charleville", "earlyfirearms.naval_pistol", "earlyfirearms.baker", "earlyfirearms.hall_1819", "earlyfirearms.dreyse", "earlyfirearms.colt_1851", "earlyfirearms.mississippi_1841" };
        public static readonly string[] Ammo = { "ak47prototype.ammo", "glock17prototype.ammo", "coldwarmodern.ammo.55645", "coldwarmodern.ammo.76251", "coldwarmodern.ammo.5728", "coldwarmodern.ammo.44mag", "coldwarmodern.ammo.12g", "coldwarmodern.ammo.50bmg", "small_pistol_ammo", "earlyfirearms.paper_cartridge", "local.modernwars.ammo_45colt", "local.modernwars.ammo_45acp", "local.modernwars.ammo_4440", "local.modernwars.ammo_76254r", "local.modernwars.ammo_79257", "local.modernwars.ammo_3006", "local.modernwars.ammo_76225", "local.modernwars.ammo_79233" };
        public static readonly string[] Parts = { "ak47prototype.magazine", "glock17prototype.magazine", "coldwarmodern.m16a1.feed", "coldwarmodern.fnfal.feed", "coldwarmodern.mp5.feed", "coldwarmodern.p90.feed", "coldwarmodern.m82a1.feed", "coldwarmodern.m249.feed", "coldwarmodern.m60.feed", "rimfire22.markiv.magazine", "rimfire22.p22.magazine", "rimfire22.buckmark.magazine", "rimfire22.1022.magazine", "local.modernwars.part_m1911", "local.modernwars.part_thompson1928", "local.modernwars.part_ppsh41", "local.modernwars.part_bar1918", "local.modernwars.part_stg44", "local.modernwars.clip_mosin1891", "local.modernwars.clip_kar98k", "local.modernwars.clip_m1garand", "local.modernwars.belt_mg42" };
    }
}
