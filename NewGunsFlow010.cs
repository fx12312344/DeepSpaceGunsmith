using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WeaponSources;
using NpcWeaponTrade;

namespace NewGuns010
{
    // Integration data only: no registration, patches, inventory changes, or random draws.
    // Append Candidates to the existing pool, add Loadouts to SellerLoadouts.Items,
    // and consult Orders after the existing NPC catalog. Never replace the old entries.
    public static class NewGunsFlow010
    {
        private sealed class Row
        {
            public readonly string Key, Ammo;
            public readonly int Level, Performance, Caliber;
            public Row(string key, int level, int performance, int caliber, string ammo)
            { Key = key; Level = level; Performance = performance; Caliber = caliber; Ammo = ammo; }
            public string Id { get { return "local.deepspace010." + Key; } }
        }

        // Level mirrors the gun's contraband tier; performance is a relative seller
        // balance estimate (1..5), not the gun's firepower stat or a new weight rule.
        private static readonly Row[] Rows = {
            new Row("winchester37", 1, 2, 1270, "coldwarmodern.ammo.12g"),
            new Row("fp45", 1, 1, 204502, "local.modernwars.ammo_45acp"),
            new Row("texas_defender", 1, 1, 204501, "local.modernwars.ammo_45colt"),
            new Row("tt33", 2, 2, 207625, "local.modernwars.ammo_76225"),
            new Row("svt40", 2, 3, 207654, "local.modernwars.ammo_76254r"),
            new Row("springfield1903", 2, 3, 203006, "local.modernwars.ammo_3006"),
            new Row("beretta92fs", 2, 3, 919, "glock17prototype.ammo"),
            new Row("sks", 2, 3, 76239, "ak47prototype.ammo"),
            new Row("mini14", 2, 3, 55645, "coldwarmodern.ammo.55645"),
            new Row("mp40", 3, 4, 919, "glock17prototype.ammo"),
            new Row("m4a1", 3, 4, 55645, "coldwarmodern.ammo.55645")
        };

        public static readonly IReadOnlyDictionary<string, SellerWeaponPool.Candidate> Candidates;
        public static readonly IReadOnlyDictionary<string, SellerLoadouts.Loadout> Loadouts;
        public static readonly IReadOnlyDictionary<string, Patches.Spec> Orders;

        static NewGunsFlow010()
        {
            var candidates = new Dictionary<string, SellerWeaponPool.Candidate>(StringComparer.Ordinal);
            var loadouts = new Dictionary<string, SellerLoadouts.Loadout>(StringComparer.Ordinal);
            var orders = new Dictionary<string, Patches.Spec>(StringComparer.Ordinal);
            foreach (var row in Rows)
            {
                candidates.Add(row.Id, new SellerWeaponPool.Candidate(row.Id, row.Performance, false, row.Level));
                // No feed item is defined for any of these guns, even if a real gun
                // uses a detachable magazine or has an integral feeding mechanism.
                loadouts.Add(row.Id, new SellerLoadouts.Loadout(row.Ammo));
                orders.Add(row.Id, new Patches.Spec(row.Caliber, row.Level));
            }
            Candidates = new ReadOnlyDictionary<string, SellerWeaponPool.Candidate>(candidates);
            Loadouts = new ReadOnlyDictionary<string, SellerLoadouts.Loadout>(loadouts);
            Orders = new ReadOnlyDictionary<string, Patches.Spec>(orders);
        }

        public static bool TryGetCandidate(string id, out SellerWeaponPool.Candidate candidate)
        {
            candidate = null;
            return id != null && Candidates.TryGetValue(id, out candidate);
        }
        public static bool TryGetLoadout(string id, out SellerLoadouts.Loadout loadout)
        {
            loadout = null;
            return id != null && Loadouts.TryGetValue(id, out loadout);
        }
        public static bool TryGetOrderSpec(string id, out Patches.Spec spec)
        {
            spec = default(Patches.Spec);
            return id != null && Orders.TryGetValue(id, out spec);
        }

        // Pure model validation; does not instantiate items or probe the game registry.
        // It checks definition IDs and internal mapping consistency, not runtime tags,
        // item availability, or the private caliber/tier fields in Catalog's Specs.
        public static IReadOnlyList<string> ValidateModel()
        {
            var errors = new List<string>();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var definition in Catalog.Definitions())
                if (definition == null || !ids.Add(definition.Id)) errors.Add("Null or duplicate gun definition");
            if (Rows.Length != 11 || ids.Count != 11 || Candidates.Count != 11 || Loadouts.Count != 11 || Orders.Count != 11)
                errors.Add("Expected exactly eleven definitions and mappings");
            foreach (var row in Rows)
            {
                if (!ids.Remove(row.Id)) errors.Add("Missing or duplicate gun definition: " + row.Id);
                Category category;
                if (!Catalog.TryGetCategory(row.Id, out category)) errors.Add("Missing category: " + row.Id);
                SellerWeaponPool.Candidate candidate;
                if (!TryGetCandidate(row.Id, out candidate) || candidate.Id != row.Id || candidate.Antique ||
                    candidate.Level != row.Level || candidate.Performance != row.Performance ||
                    SellerWeaponPool.Weight(candidate, 1) <= 0 || SellerWeaponPool.Weight(candidate, 2) <= 0 || SellerWeaponPool.Weight(candidate, 3) <= 0)
                    errors.Add("Invalid seller candidate: " + row.Id);
                SellerLoadouts.Loadout loadout;
                if (!TryGetLoadout(row.Id, out loadout) || loadout.Ammo != row.Ammo || string.IsNullOrEmpty(loadout.Ammo) || loadout.Feed != null)
                    errors.Add("Invalid ammunition/feed mapping: " + row.Id);
                Patches.Spec spec;
                if (!TryGetOrderSpec(row.Id, out spec) || spec.Caliber != row.Caliber || spec.Tier != row.Level || spec.Caliber <= 0 || spec.Tier < 1 || spec.Tier > 3)
                    errors.Add("Invalid order spec: " + row.Id);
            }
            foreach (var id in ids) errors.Add("Unmapped gun definition: " + id);
            return errors.AsReadOnly();
        }
    }
}
