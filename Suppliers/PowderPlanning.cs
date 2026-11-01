using System;
using System.Collections.Generic;

namespace PowderSupplierIntegration
{
    // Pure planning only: no state mutation, queues, charges or item grants.
    public enum OrderKind { Intro, Small, Large, Dividend }
    public readonly struct OrderPlan
    {
        public readonly string ClientId;
        public readonly int Quantity, UnitPrice, Delay;
        public OrderPlan(string id, int quantity, int unitPrice, int delay)
        { ClientId = id; Quantity = quantity; UnitPrice = unitPrice; Delay = delay; }
        public int PreDiscountTotal => checked(Quantity * UnitPrice);
    }
    public static class Planning
    {
        public const string Source = "local.powdersupplierprototype";
        public const string ResourceModId = "local.coldwarmodern";
        public const string ItemId = "local.deepspace010.material.smokeless_powder";
        // Balance defaults: normal -20% wholesale costs about base value 30 or less per unit.
        public const int SmallUnitPrice = 37, LargeUnitPrice = 35;
        public const string Intro = Source + ".delivery.free", Small = Source + ".delivery.small", Large = Source + ".delivery.large", Dividend = Source + ".delivery.dividend";
        public static OrderPlan Make(OrderKind kind, int spent)
        {
            if (spent < 0) throw new ArgumentOutOfRangeException(nameof(spent));
            switch (kind)
            {
                case OrderKind.Intro: return new OrderPlan(Intro, 4, 0, 1);
                case OrderKind.Dividend: return new OrderPlan(Dividend, 4, 0, 1);
                case OrderKind.Small: return new OrderPlan(Small, 4, SmallUnitPrice, 1);
                case OrderKind.Large:
                    if (spent < 400) throw new InvalidOperationException("Large order requires actual spend of at least 400.");
                    return new OrderPlan(Large, 8, LargeUnitPrice, 1);
                default: throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }
        // Future queue only: this cannot see a customer already moved to the store queue.
        public static bool TrySummarizeQueue(IList<string> ids, IList<int> days, out int count, out int earliest)
        {
            count = 0; earliest = 0;
            if (ids == null || days == null || ids.Count != days.Count) return false;
            for (int i = 0; i < ids.Count; i++)
                if (ids[i] == Intro || ids[i] == Small || ids[i] == Large)
                {
                    int day = Math.Max(0, days[i]);
                    if (count == 0 || day < earliest) earliest = day;
                    count++;
                }
            return true;
        }
        // Each callback instance consumes its own token exactly once, including reentrancy.
        public static bool TryConsume(ref bool consumed)
        {
            if (consumed) return false;
            consumed = true;
            return true;
        }
    }
}
