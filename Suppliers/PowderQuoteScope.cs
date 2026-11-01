using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;

namespace PowderQuoteIsolation
{
    // Opt-in API: caller owns the lifecycle and registers only newly generated offer instances.
    public static class QuoteScope
    {
        const string PowderId = "local.deepspace010.material.smokeless_powder";
        const string SmallId = "local.powdersupplierprototype.delivery.small";
        const string LargeId = "local.powdersupplierprototype.delivery.large";
        sealed class Quote { public StoreClient Client; public string ClientId; public long Value; }
        static ConditionalWeakTable<GameItem, Quote> Quotes = new ConditionalWeakTable<GameItem, Quote>();
        static readonly object Gate = new object();
        static Harmony harmony;

        public static void Install()
        {
            lock (Gate)
            {
                if (harmony != null) return;
                var h = new Harmony("local.powderquoteisolation");
                try {
                    h.Patch(AccessTools.Method(typeof(GameItem), nameof(GameItem.GetNegociatedValue)), transpiler: new HarmonyMethod(typeof(QuoteScope), nameof(Transpiler)));
                    harmony = h;
                } catch { h.UnpatchAll("local.powderquoteisolation"); throw; }
            }
        }
        public static void Uninstall() { lock (Gate) { harmony?.UnpatchAll("local.powderquoteisolation"); harmony = null; Quotes = new ConditionalWeakTable<GameItem, Quote>(); } }
        public static void Register(GameItem item, StoreClient client)
        {
            if (item == null || client == null) throw new ArgumentNullException();
            long value = client.identifier == SmallId ? PowderSupplierIntegration.Planning.SmallUnitPrice : client.identifier == LargeId ? PowderSupplierIntegration.Planning.LargeUnitPrice : throw new ArgumentException("Not a paid powder courier");
            if (item.identifier != PowderId || GeneralHelper.IsItemOwned(item)) throw new ArgumentException("Not an unowned powder offer");
            lock (Gate) { if (harmony == null) throw new InvalidOperationException("Quote patch is not installed"); Quotes.Remove(item); Quotes.Add(item, new Quote { Client = client, ClientId = client.identifier, Value = value }); }
        }
        public static void Forget(GameItem item) { if (item != null) lock (Gate) Quotes.Remove(item); }
        public static long QuoteUnitValue(GameItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            lock (Gate)
            {
                if (harmony != null && Quotes.TryGetValue(item, out var q) && item.identifier == PowderId && !GeneralHelper.IsItemOwned(item)
                    && PlayerStore.HasInstance && PlayerStore.Instance != null && PlayerStore.Instance.currentClientInstance != null
                    && ReferenceEquals(PlayerStore.Instance.currentClientInstance.GetClientBlueprint(), q.Client)
                    && q.Client.identifier == q.ClientId) return q.Value;
            }
            return item.unitValue;
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);
            MethodInfo getter = AccessTools.PropertyGetter(typeof(GameItem), nameof(GameItem.unitValue));
            MethodInfo replacement = AccessTools.Method(typeof(QuoteScope), nameof(QuoteUnitValue));
            int count = 0;
            foreach (var instruction in code)
                if ((instruction.opcode == OpCodes.Call || instruction.opcode == OpCodes.Callvirt) && Equals(instruction.operand, getter))
                { instruction.opcode = OpCodes.Call; instruction.operand = replacement; count++; }
            if (count != 1) throw new InvalidOperationException("Unsupported GetNegociatedValue IL: expected one unitValue getter, found " + count);
            return code;
        }
    }
}
