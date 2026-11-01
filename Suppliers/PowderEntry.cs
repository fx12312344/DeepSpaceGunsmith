using System;
using System.Collections.Generic;

namespace PowderSupplierIntegration
{
    public sealed class Entry
    {
        const string PhoneUpgrade = Planning.Source + ".phone", Share = Planning.Source + ".shareholder", DialogId = Planning.Source + ".dialog";
        const long Number = 75284;
        const string PhoneLocId = Planning.Source + ".contact";
        static string T(string key, params object[] args) => ModHelper.GetLocalized(Planning.ResourceModId, key, args);
        const string DividendDay = "dividend_day";
        ModLog log; bool enabled, servicesRegistered; int epoch;
        readonly Dictionary<string, Func<StoreClient>> factories = new Dictionary<string, Func<StoreClient>>();
        Func<StorePhoneClient, Dialogue> dialogFactory;
        public void Init(ModManifest manifest) { log = new ModLog(manifest); }
        public void OnEnable()
        {
            if (enabled) return;
            foreach (var mod in ModLoader.Mods) if (mod != null && mod.IsEnabled && mod.Manifest != null && mod.Manifest.ID == Planning.Source) { log.Warning("Standalone powder supplier already enabled; integration skipped."); return; }
            // Never overwrite another extension's identifiers.
            if (PhoneClientList.DialogFuncs.ContainsKey(DialogId) || StoreClientListDict.storeClientDict.ContainsKey(Planning.Intro) || StoreClientListDict.storeClientDict.ContainsKey(Planning.Small) || StoreClientListDict.storeClientDict.ContainsKey(Planning.Large) || StoreClientListDict.storeClientDict.ContainsKey(Planning.Dividend)) { log.Warning("Powder supplier identifier collision; integration disabled."); return; }
            // Definitions must never overwrite an existing owner's registrations.
            if (NetworkUpgrade.GetDefinition(PhoneUpgrade) != null || NetworkUpgrade.GetDefinition(Share) != null) { log.Warning("Powder upgrade identifier collision; disabled."); return; }
            try {
                PowderQuoteIsolation.QuoteScope.Install(); PowderWholesaleGuard.Guard.Install(); PowderPurchaseGuard.Guard.Install(); PhoneName.Install();
                enabled = true; unchecked { epoch++; }
                dialogFactory = MakeDialog; PhoneClientList.DialogFuncs.Add(DialogId, dialogFactory);
                RegisterFactory(Planning.Intro, OrderKind.Intro); RegisterFactory(Planning.Small, OrderKind.Small); RegisterFactory(Planning.Large, OrderKind.Large); RegisterFactory(Planning.Dividend, OrderKind.Dividend);
                ModHook.OnGameLoadedEarly += Loaded; ModHook.OnGameLoadedLate += EnsurePhone; ModHook.OnNewGameStarted += NewGameStarted; ModHook.OnHandlingNightlyServicesLate += Dividend;
                // Register only from game-load/new-game hooks: PlayerStore.Instance is unsafe in the main menu.
            } catch (Exception ex) { log.Warning("Powder initialization failed: " + ex); OnDisable(); PhoneName.Uninstall(); PowderPurchaseGuard.Guard.Uninstall(); PowderWholesaleGuard.Guard.Uninstall(); PowderQuoteIsolation.QuoteScope.Uninstall(); }
        }
        void NewGameStarted(string scenario) { EnsureServices(); EnsurePhone(); }
        void EnsureServices()
        {
            if (!enabled || servicesRegistered || !PlayerStore.HasInstance || PlayerStore.Instance.networkUpgrade == null) return;
            if (NetworkUpgrade.GetDefinition(PhoneUpgrade) != null || NetworkUpgrade.GetDefinition(Share) != null) { log.Warning("Powder upgrade identifier collision; services not registered."); return; }
            try { RegisterPaidServices(); servicesRegistered = true; }
            catch (Exception ex) { ModHelper.RemoveNetworkUpgrades(Planning.Source); log.Warning("Powder service registration failed: " + ex); }
        }
        void RegisterPaidServices()
        {
            const string icon = Planning.ResourceModId + ":smokeless_powder_010";
            ModHelper.AddNetworkUpgrade(Planning.Source, PhoneUpgrade, 400, "Introduction", () => T("phone_upgrade"), () => T("phone_upgrade_desc"), EnsurePhone, iconKey: icon);
            ModHelper.AddNetworkUpgrade(Planning.Source, Share, 3000, "Introduction", () => T("share"), () => T("share_desc"), null, iconKey: icon, prerequisites: new[] { PhoneUpgrade });
        }
        void RegisterFactory(string id, OrderKind kind) { Func<StoreClient> f = () => MakeCourier(kind); factories.Add(id, f); StoreClientListDict.storeClientDict.Add(id, f); }
        public void OnDisable()
        {
            if (!enabled) return; enabled = false; unchecked { epoch++; }
            ModHook.OnGameLoadedEarly -= Loaded; ModHook.OnGameLoadedLate -= EnsurePhone; ModHook.OnNewGameStarted -= NewGameStarted; ModHook.OnHandlingNightlyServicesLate -= Dividend;
            ModHelper.RemoveNetworkUpgrades(Planning.Source); servicesRegistered = false;
            if (PhoneClientList.DialogFuncs.TryGetValue(DialogId, out var d) && d == dialogFactory) PhoneClientList.DialogFuncs.Remove(DialogId);
            foreach (var p in factories) if (StoreClientListDict.storeClientDict.TryGetValue(p.Key, out var f) && f == p.Value) StoreClientListDict.storeClientDict.Remove(p.Key);
            factories.Clear();
            PhoneName.Uninstall(); PowderPurchaseGuard.Guard.Uninstall(); PowderWholesaleGuard.Guard.Uninstall(); PowderQuoteIsolation.QuoteScope.Uninstall();
            // Persisted orders are intentionally left intact: stripping them destroys purchases.
        }
        void Loaded() {
            unchecked { epoch++; }
            EnsureServices();
            // In-memory offer identities cannot survive a save reload; reject old active offers.
            PowderWholesaleGuard.Guard.Uninstall(); PowderQuoteIsolation.QuoteScope.Uninstall();
            try { PowderQuoteIsolation.QuoteScope.Install(); PowderWholesaleGuard.Guard.Install(); }
            catch (Exception ex) { log.Warning("Powder guards failed after load; integration disabled: " + ex); OnDisable(); }
        }
        const string CardBook = "peace_margin_card", CardGiven = "peace_card_given";
        void GiveCard()
        {
            if (ModHelper.GetModDataInt(Planning.Source, CardGiven, 0) != 0 || !PlayerStore.HasInstance || EmporiumEntry.Instance == null || EmporiumEntry.Instance.backInvinvElement == null) return;
            GameItem card = null;
            try {
                card = ModHelper.SpawnPaperItem(Planning.ResourceModId, CardBook);
                if (card == null) { log.Warning("Peace Margin card is not registered; will retry when phone service is checked."); return; }
                ModHelper.SetItemOwnerShip(card, true);
                GraphUtils.TryAcceptAllMid(EmporiumEntry.Instance.backInvinvElement, card, -1);
                if (card.parentInventory == null) { card.Destroy(); log.Warning("No room for Peace Margin card; will retry on next load."); return; }
                ModHelper.SetModDataInt(Planning.Source, CardGiven, 1);
            } catch (Exception ex) { if (card != null && card.parentInventory == null) card.Destroy(); log.Warning("Peace Margin card delivery failed: " + ex); }
        }
        void EnsurePhone()
        {
            if (!enabled || !PlayerStore.HasInstance || PlayerStore.Instance == null || PlayerStore.Instance.PhoneClientDict == null || !ModHelper.IsNetworkUpgradeUnlocked(PhoneUpgrade)) return;
            var phones = PlayerStore.Instance.PhoneClientDict;
            if (phones.TryGetValue(Number, out var existing)) { if (existing == null || existing.dialogFuncId != DialogId) log.Warning("Powder supplier phone number collision; existing contact preserved."); else { existing.locID = PhoneLocId; GiveCard(); } return; }
            phones.Add(Number, new StorePhoneClient { displayName = T("contact"), locID = PhoneLocId, dialogFuncId = DialogId, phoneClientType = StorePhoneClient.PhoneClientType.Supplier, phoneState = StorePhoneClient.PhoneState.Introduction, cooldownDuration = 4, alias = new List<string> { Planning.Intro, Planning.Small, Planning.Large } });
            GiveCard();
        }
        bool IsCurrent(StorePhoneClient client) => enabled && PlayerStore.HasInstance && PlayerStore.Instance != null && PlayerStore.Instance.PhoneClientDict != null && PlayerStore.Instance.PhoneClientDict.TryGetValue(Number, out var current) && ReferenceEquals(current, client);
        string QueueStatus()
        {
            if (!PlayerStore.HasInstance || PlayerStore.Instance == null) return T("queue_unknown");
            var ids = PlayerStore.Instance.futurStoreClientIdQueue;
            var days = PlayerStore.Instance.futurStoreClientDayQueue;
            if (!Planning.TrySummarizeQueue(ids, days, out int count, out int earliest)) return T("queue_unknown");
            return count == 0 ? T("queue_empty") : T("queue_pending", count, earliest);
        }
        bool Book(StorePhoneClient client, OrderKind kind)
        {
            if (!IsCurrent(client) || !DirectoryMaster.Has<GameItem>(Planning.ItemId)) return false;
            if (kind == OrderKind.Intro ? client.phoneState != StorePhoneClient.PhoneState.Introduction : client.phoneState != StorePhoneClient.PhoneState.Regular) return false;
            if (kind == OrderKind.Large && client.amountSpent < 400) return false;
            if (client.amountSpent < 0) return false;
            var plan = Planning.Make(kind, client.amountSpent);
            var store = PlayerStore.Instance;
            if (!Planning.TrySummarizeQueue(store.futurStoreClientIdQueue, store.futurStoreClientDayQueue, out int pending, out _) || pending != 0 || HasLiveOrder(store))
            { log.Warning("Powder delivery already pending or queue unavailable; no new order or cooldown."); return false; }
            if (!TryQueue(store, plan.ClientId, plan.Delay)) return false;
            client.UseService(); return true;
        }
        static bool IsOrder(string id) => id == Planning.Intro || id == Planning.Small || id == Planning.Large;
        static bool HasLiveOrder(PlayerStore store) =>
            (store.storeClientManager != null && store.storeClientManager.clientStack != null && store.storeClientManager.clientStack.Exists(c => c != null && IsOrder(c.identifier))) ||
            (store.currentClientInstance != null && store.currentClientInstance.GetClientBlueprint() != null && IsOrder(store.currentClientInstance.GetClientBlueprint().identifier));
        bool TryQueue(PlayerStore store, string id, int delay)
        {
            var ids = store.futurStoreClientIdQueue; var days = store.futurStoreClientDayQueue;
            if (ids == null || days == null || ids.Count != days.Count) return false;
            int before = ids.Count;
            try { store.QueueFuturClient(id, delay); }
            catch (Exception ex) { log.Warning("Powder queue call failed; checking whether it actually appended: " + ex); }
            // A late exception after a complete append must not permit a duplicate retry.
            if (ReferenceEquals(ids, store.futurStoreClientIdQueue) && ReferenceEquals(days, store.futurStoreClientDayQueue) && ids.Count == before + 1 && days.Count == before + 1 && ids[before] == id && days[before] >= 0) return true;
            // Roll back only the single suffix written by this synchronous call; never touch older orders.
            if (ReferenceEquals(ids, store.futurStoreClientIdQueue) && ReferenceEquals(days, store.futurStoreClientDayQueue) && ids.Count >= before && ids.Count <= before + 1 && days.Count >= before && days.Count <= before + 1 && (ids.Count == before || ids[before] == id)) {
                if (ids.Count > before) ids.RemoveAt(before);
                if (days.Count > before) days.RemoveAt(before);
            }
            log.Warning("Powder delivery was not queued; no cooldown or dividend day consumed."); return false;
        }
        Dialogue MakeDialog(StorePhoneClient client)
        {
            string name = T("contact");
            if (client.phoneState == StorePhoneClient.PhoneState.Introduction) {
                var first = new Dialogue().SetText(name, T("intro1")); var tail = first;
                for (int i = 2; i <= 6; i++) tail = tail.NextDialogue().SetText(name, T("intro" + i));
                tail.SetEndAction(() => { Book(client, OrderKind.Intro); PhoneUIManager.Instance.OnPhoneDialogEnd(); }); return first;
            }
            if (client.phoneState == StorePhoneClient.PhoneState.Cooldown) {
                var first = new Dialogue().SetText(name, T("greeting"));
                first.NextDialogue().SetText(name, T("cooldown1")).NextDialogue().SetText(name, T("cooldown2", Math.Max(0, client.currentCooldown))).SetEndAction(() => PhoneUIManager.Instance.OnPhoneDialogEnd()); return first;
            }
            if (client.phoneState != StorePhoneClient.PhoneState.Regular) {
                var first = new Dialogue().SetText(name, T("unavailable1"));
                first.NextDialogue().SetText(name, T("unavailable2")).SetEndAction(() => PhoneUIManager.Instance.OnPhoneDialogEnd()); return first;
            }
            var greeting = new Dialogue().SetText(name, T("greeting"));
            var menu = greeting.NextDialogue().SetText(name, T("paid_menu"));
            AddChoice(menu, client, OrderKind.Small, T("small"));
            if (client.amountSpent >= 400) AddChoice(menu, client, OrderKind.Large, T("large"));
            var quit = new Dialogue().SetText(name, T("goodbye")).SetEndAction(() => PhoneUIManager.Instance.OnPhoneDialogEnd());
            menu.AddChoice(DirectoryMaster.Item("$"), T("end_call"), "", quit);
            return greeting;
        }
        void AddChoice(Dialogue menu, StorePhoneClient client, OrderKind kind, string label)
        {
            string name = T("contact");
            var confirmation = new Dialogue().SetText(name, T(kind == OrderKind.Small ? "small_reply1" : "large_reply1"));
            var tail = confirmation.NextDialogue().SetText(name, T(kind == OrderKind.Small ? "small_reply2" : "large_reply2"));
            if (kind == OrderKind.Large) tail = tail.NextDialogue().SetText(name, T("large_reply3"));
            tail.SetEndAction(() => PhoneUIManager.Instance.OnPhoneDialogEnd());
            menu.AddChoice(DirectoryMaster.Item("$"), label, "", confirmation, () => { if (!Book(client, kind)) { confirmation.SetText(name, T("order_failed")); confirmation.SetNextDialogue(null); confirmation.SetEndAction(() => PhoneUIManager.Instance.OnPhoneDialogEnd()); } });
        }
        StoreClient MakeCourier(OrderKind kind)
        {
            var plan = Planning.Make(kind, kind == OrderKind.Large ? 400 : 0);
            int generation = epoch; bool started = false;
            bool free = kind == OrderKind.Intro || kind == OrderKind.Dividend;
            var items = new List<GameItem>();
            var courier = new StoreClient { identifier = plan.ClientId, displayName = T("courier"), spriteName = SpriteDict.GetCourrierSpriteName(), clientFaction = "FACTION_LOWER_LEVEL", clientIntent = StoreClient.ClientIntent.SELL, isBargainAvailable = false, isOfferWholesale = true, isRegularSellDisable = true, wholesaleDiscount = ShareholderHelper.WholesaleDiscount(Share, -20) };
            ModHelper.SetClientSource(courier, Planning.Source);
            string deliveryKey = kind == OrderKind.Intro ? "delivery_intro" : kind == OrderKind.Dividend ? "delivery_dividend" : "delivery_paid";
            courier.mainDialogue.SetText(courier.displayName, T(deliveryKey + "1"));
            courier.mainDialogue.NextDialogue().SetText(courier.displayName, T(deliveryKey + "2")).SetEndAction(() => {
                if (!enabled || generation != epoch || started || !PlayerStore.HasInstance || PlayerStore.Instance.currentClientInstance == null || !ReferenceEquals(PlayerStore.Instance.currentClientInstance.GetClientBlueprint(), courier)) return;
                int due = -1;
                if (kind == OrderKind.Dividend) {
                    due = ModHelper.GetModDataInt(Planning.Source, DividendDay, -1);
                    if (due < 0 || ModHelper.GetModDataInt(Planning.Source, "dividend_delivered_day", -1) >= due) { log.Warning("Dividend delivery has no unclaimed schedule; skipped."); DialogUIManager.Instance.CurrentClientLeave(); return; }
                }
                // Reject a known missing resource before consuming this courier's attempt.
                if (!DirectoryMaster.Has<GameItem>(Planning.ItemId)) { log.Warning("Missing powder; courier has not been consumed."); return; }
                started = true; int placed = 0, attempted = 0;
                try {
                    // Generate the complete batch before granting or displaying any of it.
                    for (int i = 0; i < plan.Quantity; i++) {
                        var item = DirectoryMaster.Item(Planning.ItemId);
                        if (item == null) throw new InvalidOperationException("Null powder");
                        items.Add(item);
                        if (!free) PowderQuoteIsolation.QuoteScope.Register(item, courier);
                    }
                    foreach (var item in items) {
                        attempted++;
                        if (!PlayerStore.Instance.TryAddDirectSellingItemToTable(item, free)) throw new InvalidOperationException("Table placement failed");
                        placed++;
                    }
                    if (!free) {
                        var saleStore = PlayerStore.Instance;
                        StorePhoneClient salePhone = null;
                        if (saleStore.PhoneClientDict != null) saleStore.PhoneClientDict.TryGetValue(Number, out salePhone);
                        PowderWholesaleGuard.Guard.Register(courier, items, (c, goods, price) => {
                            long sum = 0;
                            foreach (var item in goods) { if (PowderQuoteIsolation.QuoteScope.QuoteUnitValue(item) != plan.UnitPrice) return false; long v = item.GetNegociatedValue(); if (v < 0 || v > int.MaxValue || sum + v > int.MaxValue) return false; sum += v; }
                            return price == RNG.CalculatePercentageOff((int)sum, c.wholesaleDiscount);
                        }, paid => {
                            if (!enabled || generation != epoch || !PlayerStore.HasInstance || !ReferenceEquals(PlayerStore.Instance, saleStore) || saleStore.currentClientInstance == null || !ReferenceEquals(saleStore.currentClientInstance.GetClientBlueprint(), courier) || saleStore.PhoneClientDict == null || salePhone == null || salePhone.dialogFuncId != DialogId || !saleStore.PhoneClientDict.TryGetValue(Number, out var currentPhone) || !ReferenceEquals(currentPhone, salePhone))
                                throw new InvalidOperationException("Powder spend accounting context is no longer current");
                            salePhone.amountSpent = (int)Math.Min(int.MaxValue, (long)salePhone.amountSpent + paid);
                        });
                    }
                    if (kind == OrderKind.Dividend) ModHelper.SetModDataInt(Planning.Source, "dividend_delivered_day", due);
                } catch (Exception ex) {
                    PowderWholesaleGuard.Guard.Forget(courier);
                    foreach (var item in items) PowderQuoteIsolation.QuoteScope.Forget(item);
                    // A placement helper may throw after attaching its item; count that visible side effect.
                    if (attempted > placed && items[attempted - 1].parentInventory != null) placed = attempted;
                    // Nothing reached the table: do not burn the callback or the dividend claim.
                    // After a partial free grant, retain the claim to prevent duplicate free goods.
                    if (placed == 0) started = false;
                    else if (kind == OrderKind.Dividend) ModHelper.SetModDataInt(Planning.Source, "dividend_delivered_day", due);
                    for (int i = placed; i < items.Count; i++) try { if (items[i].parentInventory == null) items[i].Destroy(); } catch { }
                    if (placed == 0) items.Clear();
                    log.Warning("Powder delivery incomplete (placed " + placed + "/" + plan.Quantity + "); partial deliveries need manual recovery: " + ex);
                    if (placed > 0) DialogUIManager.Instance.CurrentClientLeave();
                    return;
                }
                if (free) DialogUIManager.Instance.CurrentClientLeave();
            });
            // Spend is credited by the settled wholesale callback, not rounded onItemSold costs.
            courier.allDoneDialogue = new Dialogue().SetText(courier.displayName, T("done")).SetEndAction(() => DialogUIManager.Instance.CurrentClientLeave());
            courier.CompleteClientCreation(); return courier;
        }
        void Dividend()
        {
            if (!enabled || !PlayerStore.HasInstance || PlayerStore.Instance == null || StoreStation.Instance == null || !ModHelper.IsNetworkUpgradeUnlocked(Share)) return;
            int day = StoreStation.Instance.dayCounter;
            if (day < 0 || day % 7 != 4 || ModHelper.GetModDataInt(Planning.Source, DividendDay, -1) >= day) return;
            var store = PlayerStore.Instance;
            var ids = store.futurStoreClientIdQueue; var days = store.futurStoreClientDayQueue;
            if (ids == null || days == null || ids.Count != days.Count || store.storeClientManager == null || store.storeClientManager.clientStack == null || !DirectoryMaster.Has<GameItem>(Planning.ItemId)) { log.Warning("Dividend not queued: incomplete queue or missing powder; no day recorded."); return; }
            if (ids.Contains(Planning.Dividend) || store.storeClientManager.clientStack.Exists(c => c != null && c.identifier == Planning.Dividend) || (store.currentClientInstance != null && store.currentClientInstance.GetClientBlueprint() != null && store.currentClientInstance.GetClientBlueprint().identifier == Planning.Dividend)) { log.Warning("Dividend not queued: previous dividend courier still pending."); return; }
            // Record only a verified append. Existing future/stack/current checks prevent repeats.
            if (TryQueue(store, Planning.Dividend, 1)) ModHelper.SetModDataInt(Planning.Source, DividendDay, day);
        }
    }
}
