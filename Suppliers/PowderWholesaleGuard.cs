using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;

namespace PowderWholesaleGuard
{
    // Explicitly opt-in; does not create offers, set prices or perform payment.
    public static class Guard
    {
        public const string SmallId = "local.powdersupplierprototype.delivery.small";
        public const string LargeId = "local.powdersupplierprototype.delivery.large";
        public const string PowderId = "local.deepspace010.material.smokeless_powder";
        sealed class Offer { public string Id; public HashSet<GameItem> Items; public Func<StoreClient, IReadOnlyList<GameItem>, int, bool> CheckPrice; public Action<int> OnPaid; public bool Used; public bool Checking; }
        sealed class Attempt { public StoreClient Client; public Offer Offer; public PlayerStore Store; public int Cash; }
        sealed class RefComparer<T> : IEqualityComparer<T> where T : class { public bool Equals(T x, T y) => ReferenceEquals(x, y); public int GetHashCode(T x) => RuntimeHelpers.GetHashCode(x); }
        static readonly object Gate = new object();
        static readonly Dictionary<StoreClient, Offer> Offers = new Dictionary<StoreClient, Offer>(new RefComparer<StoreClient>());
        static Harmony harmony;

        public static void Install()
        {
            lock (Gate) {
                if (harmony != null) return;
                var method = AccessTools.Method(typeof(NegociationUIManager), "OnWholesaleClick", Type.EmptyTypes);
                if (method == null || method.IsStatic || method.ReturnType != typeof(void)) throw new InvalidOperationException("Unsupported OnWholesaleClick");
                var h = new Harmony("local.powder.wholesale.guard.experimental");
                try { h.Patch(method, prefix: new HarmonyMethod(typeof(Guard), nameof(Prefix)), finalizer: new HarmonyMethod(typeof(Guard), nameof(Finalizer))); harmony = h; }
                catch { h.UnpatchAll("local.powder.wholesale.guard.experimental"); throw; }
            }
        }
        public static void Uninstall() { lock (Gate) { harmony?.UnpatchAll("local.powder.wholesale.guard.experimental"); harmony = null; Offers.Clear(); } }

        // Call only after generating all exact item instances and before displaying the offer.
        // The caller's predicate must independently validate the total against its pricing policy.
        // Optional accounting observes the settled cash net decrease once, never performs payment.
        public static void Register(StoreClient client, IEnumerable<GameItem> items, Func<StoreClient, IReadOnlyList<GameItem>, int, bool> checkPrice, Action<int> onPaid = null)
        {
            if (client == null || items == null || checkPrice == null) throw new ArgumentNullException();
            int count = Count(client.identifier);
            if (count == 0) throw new ArgumentException("Not a paid powder courier");
            var set = new HashSet<GameItem>(new RefComparer<GameItem>());
            foreach (var item in items) if (item == null || item.identifier != PowderId || GeneralHelper.IsItemOwned(item) || !set.Add(item)) throw new ArgumentException("Invalid or duplicate item");
            if (set.Count != count) throw new ArgumentException("Expected exactly 4 or 8 items");
            lock (Gate) {
                if (harmony == null) throw new InvalidOperationException("Guard not installed");
                if (Offers.ContainsKey(client)) throw new InvalidOperationException("Courier already registered; forget explicitly");
                foreach (var registered in Offers.Values)
                    if (registered.Items.Overlaps(set)) throw new InvalidOperationException("Item already registered to another courier");
                Offers.Add(client, new Offer { Id = client.identifier, Items = set, CheckPrice = checkPrice, OnPaid = onPaid });
            }
        }
        public static void Forget(StoreClient client) { if (client != null) lock (Gate) Offers.Remove(client); }
        static int Count(string id) => id == SmallId ? 4 : id == LargeId ? 8 : 0;
        static bool SameItems(List<GameItem> items, Offer offer)
        {
            if (items == null || items.Count != offer.Items.Count) return false;
            var seen = new HashSet<GameItem>(new RefComparer<GameItem>());
            foreach (var item in items)
                if (item == null || item.identifier != PowderId || GeneralHelper.IsItemOwned(item) || !offer.Items.Contains(item) || !seen.Add(item)) return false;
            return seen.Count == offer.Items.Count;
        }
        static bool Prefix(out Attempt __state)
        {
            __state = null;
            StoreClient client;
            // No current client: preserve the original early return.
            try { if (!PlayerStore.HasInstance || PlayerStore.Instance == null || PlayerStore.Instance.currentClientInstance == null) return true; client = PlayerStore.Instance.currentClientInstance.GetClientBlueprint(); }
            catch { return false; }
            if (client == null) return true;
            if (Count(client.identifier) == 0) return true;
            try {
                lock (Gate) {
                    if (!Offers.TryGetValue(client, out var offer) || offer.Used || offer.Checking || client.identifier != offer.Id || !client.isOfferWholesale || EmporiumEntry.Instance == null || NegociationUIManager.IsTradeLocked()) return false;
                    var items = EmporiumEntry.Instance.GetAllNonOwnedItem();
                    if (!SameItems(items, offer)) return false;
                    int price = PlayerStore.Instance.GetWholesalePrice();
                    if (price < 0 || PlayerStore.Instance.playerCash < price) return false;
                    bool accepted;
                    offer.Checking = true;
                    try { accepted = offer.CheckPrice(client, items.AsReadOnly(), price); }
                    finally { offer.Checking = false; }
                    if (!accepted) return false;
                    // The callback is untrusted integration code: recheck state and snapshot afterwards.
                    if (!Offers.TryGetValue(client, out var still) || !ReferenceEquals(still, offer) || offer.Used || PlayerStore.Instance.currentClientInstance == null || !ReferenceEquals(PlayerStore.Instance.currentClientInstance.GetClientBlueprint(), client) || client.identifier != offer.Id || !client.isOfferWholesale || !SameItems(EmporiumEntry.Instance.GetAllNonOwnedItem(), offer) || PlayerStore.Instance.GetWholesalePrice() != price || PlayerStore.Instance.playerCash < price) return false;
                    // Hold only a reentrancy lock before the original game transaction.
                    // Consume after observing side effects, not merely after successful preflight.
                    __state = new Attempt { Client = client, Offer = offer, Store = PlayerStore.Instance, Cash = PlayerStore.Instance.playerCash };
                    offer.Checking = true;
                    return true;
                }
            } catch { return false; }
        }
        static Exception Finalizer(Exception __exception, Attempt __state)
        {
            if (__state == null) return __exception;
            lock (Gate) {
                var offer = __state.Offer;
                try {
                    bool wasUsed = offer.Used;
                    bool changed = __state.Store.playerCash != __state.Cash || !__state.Client.isOfferWholesale;
                    bool allOwned = true;
                    foreach (var item in offer.Items) {
                        if (GeneralHelper.IsItemOwned(item)) changed = true;
                        else allOwned = false;
                    }
                    offer.Used = wasUsed || changed;
                    if (__exception != null && changed) UnityEngine.Debug.LogWarning("[PowderWholesaleGuard] Original trade failed after side effects; retry blocked for this offer. No synthetic refund attempted.");
                    // Observe the original transaction once; never sum rounded per-item sale callbacks.
                    // Used is set before invoking integration code, including when that code throws.
                    if (!wasUsed && __exception == null && allOwned && offer.OnPaid != null) {
                        try {
                            if (!PlayerStore.HasInstance || !ReferenceEquals(PlayerStore.Instance, __state.Store) || __state.Store.currentClientInstance == null || !ReferenceEquals(__state.Store.currentClientInstance.GetClientBlueprint(), __state.Client))
                                throw new InvalidOperationException("Store or current courier changed before accounting");
                            long paid = (long)__state.Cash - __state.Store.playerCash;
                            if (paid < 0) throw new InvalidOperationException("Successful offer has a negative cash debit");
                            offer.OnPaid((int)Math.Min(int.MaxValue, paid));
                        } catch (Exception ex) {
                            UnityEngine.Debug.LogWarning("[PowderWholesaleGuard] Trade settled but spend accounting failed; offer remains consumed, no payment retry or synthetic debit: " + ex);
                        }
                    }
                } catch {
                    // Only this attempted offer is held if its post-state cannot be inspected.
                    offer.Used = true;
                    UnityEngine.Debug.LogWarning("[PowderWholesaleGuard] Could not inspect attempted trade; manual recovery may be needed.");
                } finally { offer.Checking = false; }
            }
            return __exception; // Do not swallow game exceptions or replace original payment semantics.
        }
    }
}
