using System;
using System.Collections.Generic;
using UnityEngine;

namespace NpcWeaponTrade
{
    // Registered explicitly by Entry: buyer scope and order-overload prefix.
    // No HarmonyPatch attribute: never auto-register this class through PatchAll.
    // Seller-cap proxy only, not actual purchase, repair or attachment cost.
    public static class OwnedStandardPricing
    {
        [ThreadStatic] private static int buyerDepth;
        public static void Enter(out bool __state)
        {
            __state = false;
            if (buyerDepth == int.MaxValue) return;
            buyerDepth++;
            __state = true;
        }
        public static Exception Exit(Exception __exception, bool __state)
        {
            if (__state && buyerDepth > 0) buyerDepth--;
            return __exception;
        }
        public static void Price(GunOrderData __0)
        {
            if (buyerDepth <= 0 || __0 == null || __0.isBestOrder || __0.isServiceOrder) return;
            try { Reprice(__0); } catch (Exception) { /* retain original order */ }
        }
        private static bool IsNaked(GameItem item)
        {
            // Visit every reachable storage node with cycle and size limits. Unknown,
            // malformed or oversized graphs fail closed; no item is ever created or destroyed.
            var seen = new HashSet<GraphNodeStorage>();
            var queue = new Queue<GraphNodeStorage>();
            queue.Enqueue(item);
            while (queue.Count != 0)
            {
                var node = queue.Dequeue();
                if (node == null) return false;
                if (!seen.Add(node)) return false; // cycle/shared node: cannot verify a tree safely
                if (seen.Count > 1024) return false;
                if (node != item && node is GameItem) return false;
                var children = node.children;
                if (children == null) return false;
                foreach (var child in children)
                {
                    if (child == null || queue.Count >= 1024) return false;
                    queue.Enqueue(child);
                }
            }
            return true;
        }
        // IsMet reads a cloned tag, but GetTagReadonly creates a missing entry.
        // Read the same integer without creating tags or checking their enabled flag.
        internal static bool TryCompare(GameItem item, GunOrderRequirement req, out int value, out bool met)
        {
            value = 0;
            met = false;
            if (item == null || req == null || string.IsNullOrEmpty(req.id) || item.modifiedState == null || item.modifiedState.dict == null) return false;
            if (item.modifiedState.dict.TryGetValue(req.id, out var tag))
            {
                if (tag == null) return false;
                value = tag.GetInt();
            }
            met = req.isCaliber ? item.IsTag("ADV_GUN_TAG") && value == req.value
                : req.isMustBeLargerThan ? value > req.value : value < req.value;
            return true;
        }
        private static void Reprice(GunOrderData order)
        {
            if (order.tier < 1 || order.tier > 3 || order.baseReward < 0 || order.mandatoryRequirements == null || order.mandatoryRequirements.Count == 0 || order.bonusRequirements == null || EmporiumEntry.Instance == null || PlayerStore.Instance == null) return;
            int factor = checked(100 + GunOrderData.GetMarketModifierPercent() + PlayerStore.Instance.retailMarkup);
            if (factor <= 0 || factor > 100000) return;
            // Any qualifying gun may be delivered, not just the selected reference.
            // Bound every bonus independently with vanilla's modifier and rounding.
            long allBonus = 0;
            foreach (var req in order.bonusRequirements)
            {
                if (req == null || string.IsNullOrEmpty(req.id) || req.reward < 0) return;
                allBonus = checked(allBonus + Adjust(req.reward, factor));
                if (allBonus > int.MaxValue) return;
            }
            long bestDistance = long.MaxValue, bestCost = long.MaxValue;
            int bestBonus = 0;
            string bestKey = null;
            foreach (var item in EmporiumEntry.Instance.GetAllOwnedItems())
            {
                if (item == null || !item.IsTag("ADV_GUN_TAG") || item.unitValue <= 0 || !Patches.TryResolveSpec(item.identifier, out var spec) || spec.Tier > order.tier || !IsNaked(item)) continue;
                long distance = 0;
                bool match = true;
                foreach (var req in order.mandatoryRequirements)
                {
                    if (!TryCompare(item, req, out int value, out bool met)) return;
                    if (!met) { match = false; break; }
                    if (!req.isCaliber)
                        distance = checked(distance + (req.isMustBeLargerThan ? (long)value - ((long)req.value + 1) : ((long)req.value - 1) - value));
                }
                if (!match) continue;
                int bonus = 0;
                foreach (var req in order.bonusRequirements)
                {
                    if (req == null || req.reward < 0 || !TryCompare(item, req, out _, out bool met)) return;
                    if (met) bonus = checked(bonus + Adjust(req.reward, factor));
                }
                // Fair condition 88%, seller -20%: nominal cap proxy only. Actual
                // seller offer may differ due to market, features, rounding and gear.
                if (!Economy011.RevenueCurve.TryGet(item.identifier, out var economicSpec) || economicSpec.Kind != "gun") continue;
                long cost = Economy011.RevenueCurve.ReferenceCost(item.identifier, 88);
                if (cost <= 0 || cost > int.MaxValue) continue;
                string key = item.identifier;
                if (distance < bestDistance || (distance == bestDistance && (cost < bestCost || (cost == bestCost && (bestKey == null || string.CompareOrdinal(key, bestKey) < 0)))))
                { bestDistance = distance; bestCost = cost; bestBonus = bonus; bestKey = key; }
            }
            if (bestKey == null) return;
            long target = Economy011.RevenueCurve.TargetRevenue(bestKey, bestCost);
            if (!TryBase(target, bestBonus, factor, out int candidate)) return;
            // Check the whole bonus set, not only the reference gun's bonuses.
            // Nonnegative contributions make every intermediate sum safe too.
            if ((long)Adjust(candidate, factor) + allBonus > int.MaxValue) return;
            // Commit only after all reads/calculations succeed. The bound is for
            // the current factor and bonus list, not future changes to either.
            if (candidate > order.baseReward) order.baseReward = candidate;
        }
        internal static int Adjust(int amount, int factor)
        {
            return Mathf.RoundToInt((float)checked(amount * factor) / 100f);
        }
        internal static bool TryBase(long target, int bonus, int factor, out int raw)
        {
            raw = 0;
            if (target <= 0 || target > int.MaxValue || bonus < 0 || factor <= 0) return false;
            if (bonus >= target) return true;
            int hi = 2147483000 / factor, lo = 0;
            if ((long)Adjust(hi, factor) + bonus < target) return false;
            while (lo < hi)
            {
                int mid = lo + (hi - lo) / 2;
                if ((long)Adjust(mid, factor) + bonus >= target) hi = mid; else lo = mid + 1;
            }
            if ((long)Adjust(lo, factor) + bonus > int.MaxValue) return false;
            raw = lo;
            return true;
        }
    }
}
