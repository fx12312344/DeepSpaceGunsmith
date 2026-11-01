using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using UnityEngine;

namespace WeaponSources
{
    public static class Sources
    {
        // Relative to one original loot roll, not per day.
        public const int DumpReplacementPercent = 4;
        private static readonly List<string> Ids = BuildIds();
        private static readonly HashSet<string> IdSet = new HashSet<string>(Ids);
        private sealed class SellerOffer
        {
            // Bind to the exact visit, not a blueprint that another visit may reuse.
            public StoreClientInstance Seller;
            public long Cap;
        }
        private static readonly ConditionalWeakTable<GameItem, SellerOffer> SellerOffers = new ConditionalWeakTable<GameItem, SellerOffer>();

        // Each invocation has its own synchronous, thread-local scope. Nested seller
        // callbacks restore their caller's scope even if the callback throws.
        private sealed class SellerScope
        {
            public int Tier;
            public StoreClient Seller;
            public bool Creating;
            public bool Consumed;
            public string MatchedGun;
            public GameItem MatchedItem;
            public bool GunPlaced;
            public bool AddingSupplies;
            public readonly HashSet<GameItem> NonSmgFromSmgSlot = new HashSet<GameItem>();
        }
        [ThreadStatic] private static SellerScope currentSeller;

        private static List<string> BuildIds()
        {
            var result = new List<string>();
            foreach (var s in EarlyFirearms.Catalog.All) result.Add(s.Id);
            foreach (var s in ModernWars.Entry.Guns) result.Add(s.Id);
            result.Add("ak47prototype.rifle"); result.Add("glock17prototype.pistol");
            foreach (var key in new[] { "m16a1", "fnfal", "mp5", "p90", "sw629", "rem870", "m24", "m82a1", "m249", "m60" }) result.Add("coldwarmodern." + key);
            foreach (var s in Rimfire22.Catalog.Guns) result.Add(s.Id);
            foreach (var id in NewGuns010.NewGunsFlow010.Candidates.Keys) result.Add(id);
            result.Add(Bfg010.Catalog.Id);
            return result;
        }
        private static string RandomRegisteredId()
        {
            var valid = new List<string>();
            foreach (var id in Ids) if (DirectoryMaster.Has<GameItem>(id)) valid.Add(id);
            return valid.Count == 0 ? null : valid[UnityEngine.Random.Range(0, valid.Count)];
        }
        public static void OnLootRolled(LootRollContext ctx)
        {
            if (ctx == null || ctx.groupId != "dumpingGroundTG" || string.IsNullOrEmpty(ctx.itemId) || !RNG.Roll(DumpReplacementPercent)) return;
            var id = RandomRegisteredId();
            if (id != null) ctx.itemId = id;
        }
        public static void SetDumpCondition(GameItem item)
        {
            if (item == null || !IdSet.Contains(item.identifier)) return;
            GunHelper.SetEquipmentCondition(item, RNG.Roll(50) ? ItemFeatureList.EquipmentCondition.Poor : ItemFeatureList.EquipmentCondition.Fair);
        }
        public static void Wrap(StoreClient client, int tier)
        {
            if (!ColdWarModernExpansion.Entry.IsActive || client?.mainDialogue == null) return;
            var previous = client.mainDialogue.endAction;
            client.mainDialogue.endAction = () =>
            {
                if (!ColdWarModernExpansion.Entry.IsActive) { previous?.Invoke(); return; }
                var parent = currentSeller;
                currentSeller = new SellerScope { Tier = tier, Seller = client };
                try { previous?.Invoke(); AddMatchingSupplies(); }
                finally { currentSeller = parent; }
            };
            Attachments010.Supply.Wrap(client, tier);
        }

        // Called before the original factory constructs anything. A failed choice,
        // lookup, construction, or conditioning leaves the original factory in charge.
        public static bool TryReplace(string factory, ItemFeatureList.EquipmentCondition condition, ref GameItem result)
        {
            var scope = currentSeller;
            if (!ColdWarModernExpansion.Entry.IsActive || scope == null || scope.Creating || scope.Consumed) return false;
            bool expected = scope.Tier == 1 ? factory == nameof(PreBuiltItemHelper.CreateHandmadeGun) :
                scope.Tier == 2 ? factory == nameof(PreBuiltItemHelper.CreateRevolver) || factory == nameof(PreBuiltItemHelper.CreateHeavyHandmadeGun) :
                scope.Tier == 3 && (factory == nameof(PreBuiltItemHelper.CreatePoliceHandgun) || factory == nameof(PreBuiltItemHelper.CreatePoliceSMG));
            if (!expected) return false;
            // Vanilla makes exactly one of the tier's expected calls. Even a failed
            // draw consumes this slot, so later unrelated calls remain untouched.
            scope.Consumed = true;
            scope.Creating = true;
            try
            {
                var candidate = SellerWeaponPool.Choose(SellerWeaponPool.Candidates, scope.Tier,
                    id => DirectoryMaster.Has<GameItem>(id), UnityEngine.Random.value);
                if (candidate == null) return false;
                string original = factory == nameof(PreBuiltItemHelper.CreateHandmadeGun) ? "handmade_pistol" :
                    factory == nameof(PreBuiltItemHelper.CreateHeavyHandmadeGun) ? "heavy_handmade_pistol" :
                    factory == nameof(PreBuiltItemHelper.CreateRevolver) ? "revolver" :
                    factory == nameof(PreBuiltItemHelper.CreatePoliceHandgun) ? "glock_receiver" : "smg";
                if (candidate.Id == original) return false;
                var gun = DirectoryMaster.Item(candidate.Id);
                if (gun == null) return false;
                GunHelper.SetEquipmentCondition(gun, condition);
                if (factory == nameof(PreBuiltItemHelper.CreatePoliceSMG) && candidate.Id != "smg")
                    scope.NonSmgFromSmgSlot.Add(gun);
                // Only our own guns, and only after construction and conditioning succeed.
                // Vanilla seller callbacks use Poor/Fair; Good is included for safety.
                int percent = condition == ItemFeatureList.EquipmentCondition.Poor ? 75 :
                    condition == ItemFeatureList.EquipmentCondition.Fair ? 88 :
                    condition == ItemFeatureList.EquipmentCondition.Good ? 100 : 0;
                if (percent != 0 && Economy011.RevenueCurve.TryGet(candidate.Id, out var spec) && PlayerStore.HasInstance)
                {
                    var seller = PlayerStore.Instance.currentClientInstance;
                    if (seller != null && ReferenceEquals(seller.GetClientBlueprint(), scope.Seller))
                    {
                        long cap = (long)((decimal)Math.Max(0, spec.Baseline) * percent * 80 / 10000);
                        SaveOffer(gun, seller, cap);
                    }
                }
                if (IdSet.Contains(candidate.Id)) { scope.MatchedGun = candidate.Id; scope.MatchedItem = gun; }
                result = gun;
                return true;
            }
            catch (Exception)
            {
                // The base factory remains the fallback; do not break the dialogue.
                return false;
            }
            finally { scope.Creating = false; }
        }
        public static bool FilterOriginalSupply(ref string id)
        {
            var scope = currentSeller;
            if (!ColdWarModernExpansion.Entry.IsActive || scope == null || scope.AddingSupplies || scope.MatchedGun == null || !SellerLoadouts.IsVanillaAmmoOrMagazine(id)) return true;
            // Suppress only unrelated ammo/magazines from this original seller callback.
            // Matching goods are supplied separately after successful gun placement.
            return false;
        }
        public static void RecordGunPlacement(GameItem item, bool placed)
        {
            var scope = currentSeller;
            if (scope != null && item != null && ReferenceEquals(item, scope.MatchedItem) && placed) scope.GunPlaced = true;
        }
        private static void AddMatchingSupplies()
        {
            var scope = currentSeller;
            if (scope == null || !scope.GunPlaced || !ColdWarModernExpansion.Entry.IsActive || !PlayerStore.HasInstance || !SellerLoadouts.Items.TryGetValue(scope.MatchedGun, out var loadout)) return;
            scope.AddingSupplies = true;
            try {
                // These are separate offers, not loaded ammunition or installed parts.
                foreach (var id in new[] { loadout.Ammo, loadout.Feed }) {
                    if (string.IsNullOrEmpty(id) || !DirectoryMaster.Has<GameItem>(id)) continue;
                    PlayerStore.Instance.TryAddDirectSellingItemToTable(id, false, false, false, 0);
                }
            } catch (Exception e) { UnityEngine.Debug.LogWarning("[DeepSpaceGunsmith] Seller supplies could not be offered: " + e.Message); }
            finally { scope.AddingSupplies = false; }
        }
        // Read the same real feature as vanilla GunFrameHelper.GetCondition. There
        // is no EQUIPMENT_CONDITION_TAG; do not infer condition from a marked-up quote.
        private static bool TryGetOfferConditionPercent(GameItem item, out int percent)
        {
            percent = 100;
            if (!item.IsGameItemType("FIREARM")) return true;
            switch (item.FindItemFeatureByCategory("CATEGORY_EQUIPMENT_CONDITION")?.realCondition?.identifier)
            {
                case "equipment_condition_poor": percent = 75; return true;
                case "equipment_condition_fair": percent = 88; return true;
                case "equipment_condition_good": percent = 100; return true;
                case "equipment_condition_excellent": percent = 112; return true;
                case "equipment_condition_new": percent = 125; return true;
                default: return false; // An unrecognised gun condition leaves vanilla in charge.
            }
        }
        private static void SaveOffer(GameItem item, StoreClientInstance seller, long cap)
        {
            if (SellerOffers.TryGetValue(item, out var existingOffer) && ReferenceEquals(existingOffer.Seller, seller))
                cap = Math.Min(cap, existingOffer.Cap);
            // TryReplace may already have registered this same item before placement.
            SellerOffers.Remove(item);
            SellerOffers.Add(item, new SellerOffer { Seller = seller, Cap = cap });
        }
        public static void RegisterOffer(GameItem item, bool placed)
        {
            if (!ColdWarModernExpansion.Entry.IsActive || !placed || item == null) return;
            try
            {
                if (GeneralHelper.IsItemOwned(item) || item.IsTag(GunHelper.SERVICE_GUN_TAG) ||
                    !PlayerStore.HasInstance || EmporiumEntry.Instance == null ||
                    EmporiumEntry.Instance.frontInvinvElement == null ||
                    !ReferenceEquals(item.parentInventory, EmporiumEntry.Instance.frontInvinvElement) ||
                    !Economy011.RevenueCurve.TryGet(item.identifier, out var spec)) return;
                var seller = PlayerStore.Instance.currentClientInstance;
                var blueprint = seller?.GetClientBlueprint();
                if (blueprint == null || (blueprint.clientIntent != StoreClient.ClientIntent.SELL &&
                    blueprint.clientIntent != StoreClient.ClientIntent.SELLNBUY) ||
                    !TryGetOfferConditionPercent(item, out int percent)) return;
                long cap = (long)((decimal)Math.Max(0, spec.Baseline) * percent * 80 / 10000);
                SaveOffer(item, seller, cap);
            }
            catch (Exception)
            {
                // Quote bookkeeping must not turn a successful placement into a failure.
            }
        }
        public static void ForgetOwned(GameItem gun, bool owned)
        {
            if (owned && gun != null) SellerOffers.Remove(gun);
        }
        public static void CapSellerQuote(GameItem gun, ref long value)
        {
            if (gun == null || !SellerOffers.TryGetValue(gun, out var offer)) return;
            if (GeneralHelper.IsItemOwned(gun)) { SellerOffers.Remove(gun); return; }
            if (!ColdWarModernExpansion.Entry.IsActive || !PlayerStore.HasInstance ||
                PlayerStore.Instance.currentClientInstance == null ||
                !ReferenceEquals(PlayerStore.Instance.currentClientInstance, offer.Seller)) return;
            var seller = offer.Seller.GetClientBlueprint();
            if (seller == null || (seller.clientIntent != StoreClient.ClientIntent.SELL &&
                seller.clientIntent != StoreClient.ClientIntent.SELLNBUY) || gun.IsTag(GunHelper.SERVICE_GUN_TAG)) return;
            if (value > offer.Cap) value = offer.Cap;
        }
        public static bool SkipSmgSlotMod(GameItem gun, string modId)
        {
            return currentSeller != null && gun != null && currentSeller.NonSmgFromSmgSlot.Contains(gun) &&
                (modId == "bipod" || modId == "mag_10mm_smg_box");
        }
    }

    [HarmonyPatch(typeof(PlayerStore), nameof(PlayerStore.AddDirectSellingItemToTable), new[] { typeof(string), typeof(bool), typeof(bool), typeof(bool), typeof(int) })]
    public static class SellerSupplyFilterPatch
    {
        public static bool Prefix(ref string __0) => Sources.FilterOriginalSupply(ref __0);
    }
    [HarmonyPatch(typeof(PlayerStore), nameof(PlayerStore.TryAddDirectSellingItemToTable), new[] { typeof(GameItem), typeof(bool), typeof(bool), typeof(bool), typeof(int) })]
    public static class SellerGunPlacementPatch
    {
        public static void Postfix(GameItem __0, bool __1, bool __result)
        {
            Sources.RecordGunPlacement(__0, __result);
            if (!__1) Sources.RegisterOffer(__0, __result);
        }
    }
    [HarmonyPatch(typeof(GameItem), nameof(GameItem.GetNegociatedValue))]
    public static class SellerOfferPricePatch
    {
        public static void Postfix(GameItem __instance, ref long __result) => Sources.CapSellerQuote(__instance, ref __result);
    }
    [HarmonyPatch(typeof(GeneralHelper), nameof(GeneralHelper.SetItemOwned))]
    public static class SellerOfferOwnershipPatch
    {
        public static void Postfix(GameItem __0, bool __1) => Sources.ForgetOwned(__0, __1);
    }
    [HarmonyPatch]
    public static class SellerGunFactoryPatch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            var signature = new[] { typeof(ItemFeatureList.EquipmentCondition) };
            foreach (var name in new[] { nameof(PreBuiltItemHelper.CreateHandmadeGun), nameof(PreBuiltItemHelper.CreateHeavyHandmadeGun),
                nameof(PreBuiltItemHelper.CreateRevolver), nameof(PreBuiltItemHelper.CreatePoliceHandgun), nameof(PreBuiltItemHelper.CreatePoliceSMG) })
                yield return AccessTools.Method(typeof(PreBuiltItemHelper), name, signature);
        }
        public static bool Prefix(MethodBase __originalMethod, ItemFeatureList.EquipmentCondition __0, ref GameItem __result)
            => !Sources.TryReplace(__originalMethod.Name, __0, ref __result);
    }
    [HarmonyPatch(typeof(PreBuiltItemHelper), nameof(PreBuiltItemHelper.FitGunMod))]
    public static class SellerSmgModPatch
    {
        public static bool Prefix(GameItem gun, string modId, ref GameItem __result)
        {
            if (!Sources.SkipSmgSlotMod(gun, modId)) return true;
            __result = gun;
            return false;
        }
    }
    [HarmonyPatch(typeof(ScavHelper), nameof(ScavHelper.SpawnDumpingGroundLoot))]
    public static class DumpConditionPatch
    {
        public static void Postfix(GameItem __result) => Sources.SetDumpCondition(__result);
    }
    [HarmonyPatch(typeof(StoreClientListGun), nameof(StoreClientListGun.CreateLLGunSeller))]
    public static class LowerSellerPatch { public static void Postfix(StoreClient __result) => Sources.Wrap(__result, 1); }
    [HarmonyPatch(typeof(StoreClientListGun), nameof(StoreClientListGun.CreateMercGunSeller))]
    public static class MercSellerPatch { public static void Postfix(StoreClient __result) => Sources.Wrap(__result, 2); }
    [HarmonyPatch(typeof(StoreClientListGun), nameof(StoreClientListGun.CreateULGunSeller))]
    public static class UpperSellerPatch { public static void Postfix(StoreClient __result) => Sources.Wrap(__result, 3); }
}
