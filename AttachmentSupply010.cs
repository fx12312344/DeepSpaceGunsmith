using System;
using System.Runtime.CompilerServices;

namespace Attachments010
{
    // Install after other seller wrappers, so the existing callback and its goods
    // complete first. Percentages are per-dialogue independent attempts, not
    // guaranteed placements or per-day probabilities.
    public static class Supply
    {
        private sealed class Offer
        {
            public readonly string Id;
            private readonly int lower, mercenary, upper;
            public Offer(string key, int lower, int mercenary, int upper)
            { Id = "local.deepspace010.attachment." + key; this.lower = lower; this.mercenary = mercenary; this.upper = upper; }
            public int Chance(int tier) => tier == 1 ? lower : tier == 2 ? mercenary : tier == 3 ? upper : 0;
        }
        private static readonly Offer[] Offers = {
            new Offer("red_dot",20,30,30), new Offer("holographic",15,20,25),
            new Offer("scope_1_4",12,15,20), new Offer("scope_4_32",0,12,15), new Offer("ballistic_sight",0,0,12),
            new Offer("light_stock",15,20,20), new Offer("telescopic_stock",0,15,20), new Offer("heavy_stock",0,15,20), new Offer("precision_stock",0,12,15),
            new Offer("light_grip",20,25,25), new Offer("tactical_grip",15,20,20), new Offer("angled_grip",0,15,20), new Offer("heavy_grip",0,15,20),
            new Offer("grenade_launcher",0,0,12), new Offer("underbarrel_shotgun",0,12,15), new Offer("tactical_light",20,25,25), new Offer("laser",20,30,30),
            new Offer("muzzle_brake",0,15,15), new Offer("advanced_brake",0,12,15), new Offer("flash_hider",15,20,20), new Offer("tactical_flash_hider",0,15,20),
            new Offer("makeshift_suppressor",30,0,0), new Offer("advanced_suppressor",0,15,15), new Offer("full_choke",0,15,15), new Offer("half_choke",15,20,20),
            new Offer("optimized_trigger",25,30,30), new Offer("advanced_trigger",12,15,15), new Offer("competition_trigger",0,15,15), new Offer("cleaning",20,20,20),
            new Offer("silver_plating",0,15,15), new Offer("gold_plating",0,0,12),
            new Offer("alofs",0,0,12), new Offer("m1911_stock",0,0,12), new Offer("glock_switch",0,0,12), new Offer("pedersen",0,0,12), new Offer("sks_bump_stock",0,0,12)
        };
        private sealed class Installed { }
        private static readonly ConditionalWeakTable<StoreClient, Installed> Wrapped = new ConditionalWeakTable<StoreClient, Installed>();

        // Call once per newly created seller; repeated calls do not add wrappers.
        // If another mod later replaces (rather than wraps) the callback, this
        // cannot safely recover it without risking duplicate rolls.
        public static void Wrap(StoreClient client, int tier) => Wrap(client, tier, null);
        public static void Wrap(StoreClient client, int tier, ModLog log)
        {
            if (client == null || client.mainDialogue == null || tier < 1 || tier > 3) return;
            if (Wrapped.TryGetValue(client, out _)) return;
            var dialogue = client.mainDialogue;
            var previous = dialogue.endAction;
            dialogue.endAction = () => {
                previous?.Invoke(); // Preserve the original exception behavior.
                if (ColdWarModernExpansion.Entry.IsActive) AddOffers(tier, log);
            };
            Wrapped.Add(client, new Installed());
        }

        private static void Warn(ModLog log, string text)
        {
            try { if (log != null) log.Warning(text); else UnityEngine.Debug.LogWarning("[DeepSpaceGunsmith] " + text); }
            catch (Exception) { /* Logging must not interrupt the seller. */ }
        }
        private static void AddOffers(int tier, ModLog log)
        {
            if (!PlayerStore.HasInstance || EmporiumEntry.Instance == null || EmporiumEntry.Instance.frontInvinvElement == null) return;
            foreach (var offer in Offers)
            {
                int chance = offer.Chance(tier);
                if (chance == 0) continue;
                try
                {
                    if (!DirectoryMaster.Has<GameItem>(offer.Id)) continue;
                    if (!RNG.Roll(chance)) continue;
                    // Keep the game's contraband/faction/perk checks and placement
                    // failures; do not bypass or replace existing seller goods.
                    if (!PlayerStore.Instance.TryAddDirectSellingItemToTable(offer.Id, false, false, false, 0))
                        Warn(log, "Attachment seller offer not placed: " + offer.Id);
                }
                catch (Exception e) { Warn(log, "Attachment seller offer failed: " + offer.Id + ": " + e.Message); }
            }
        }
    }
}
