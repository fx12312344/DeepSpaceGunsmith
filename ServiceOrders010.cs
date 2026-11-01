// Pure, disconnected research model. No Harmony patches, game API, save access,
// registration, or production replacement. An empty whitelist is intentional.
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeepSpaceGunsmith.ServiceOrders010
{
    public static class ReplacementPolicy
    {
        // No gun/condition/tier combination has been validated against the game.
        public static bool IsWhitelisted(string gunId, int tier, string condition) => false;
    }

    // Test-only values, not representations of live GameItem instances.
    public sealed class Part
    {
        public readonly string Id;
        public readonly bool PlayerOwned;
        public Part(string id, bool playerOwned) { Id = id; PlayerOwned = playerOwned; }
    }
    public sealed class Order
    {
        public readonly string Id, CustomerGunId;
        public readonly bool CustomerGunOwnedByPlayer;
        public readonly int UsedRepairParts;
        public readonly Part[] Parts;
        public Order(string id, string gunId, bool playerOwned, int usedRepairParts, IEnumerable<Part> parts)
        {
            Id = id; CustomerGunId = gunId; CustomerGunOwnedByPlayer = playerOwned;
            UsedRepairParts = usedRepairParts; Parts = parts == null ? null : parts.ToArray();
        }
    }
    public sealed class Snapshot
    {
        public readonly int Capacity;
        public readonly string[] Inventory;
        public readonly Order[] Orders;
        public Snapshot(int capacity, IEnumerable<string> inventory, IEnumerable<Order> orders)
        {
            Capacity = capacity; Inventory = inventory == null ? null : inventory.ToArray();
            Orders = orders == null ? null : orders.ToArray();
        }
    }
    public static class CancellationModel
    {
        // Builds a new snapshot only. Failure never mutates the input. The caller must
        // supply explicit identities for restored repair parts; no factory is invoked.
        // This is NOT an atomic transaction over real game objects or a refund hook.
        public static bool TryCancel(Snapshot input, string orderId, IEnumerable<string> repairRefundIds, out Snapshot result)
        {
            result = null;
            if (input == null || input.Inventory == null || input.Orders == null ||
                input.Capacity < 0 || input.Inventory.Length > input.Capacity ||
                string.IsNullOrEmpty(orderId) || repairRefundIds == null) return false;
            var ids = new HashSet<string>(StringComparer.Ordinal);
            var orderIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in input.Inventory)
                if (string.IsNullOrEmpty(item) || !ids.Add(item)) return false;
            Order target = null;
            foreach (var order in input.Orders)
            {
                if (order == null || string.IsNullOrEmpty(order.Id) || !orderIds.Add(order.Id) ||
                    string.IsNullOrEmpty(order.CustomerGunId) || !ids.Add(order.CustomerGunId) ||
                    order.Parts == null || order.UsedRepairParts < 0) return false;
                foreach (var part in order.Parts)
                    if (part == null || string.IsNullOrEmpty(part.Id) || !ids.Add(part.Id)) return false;
                if (order.Id == orderId) target = order;
            }
            // Player-owned gun must never be destroyed as a customer's gun.
            if (target == null || target.CustomerGunOwnedByPlayer) return false;
            var refunds = repairRefundIds.ToArray();
            if (refunds.Length != target.UsedRepairParts) return false;
            foreach (var refund in refunds)
                if (string.IsNullOrEmpty(refund) || !ids.Add(refund)) return false;
            var playerParts = target.Parts.Where(p => p.PlayerOwned).Select(p => p.Id).ToArray();
            if ((long)input.Inventory.Length + playerParts.Length + refunds.Length > input.Capacity) return false;
            result = new Snapshot(input.Capacity, input.Inventory.Concat(playerParts).Concat(refunds),
                input.Orders.Where(o => o.Id != orderId));
            return true;
        }
    }
}
