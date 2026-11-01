using System;
using System.Collections.Generic;

namespace WeaponSources
{
    // Pure policy: weights are relative, not probabilities. Registry availability is
    // checked at draw time; a zero/invalid total fails closed to the original offer.
    public static class SellerWeaponPool
    {
        public sealed class Candidate
        {
            public readonly string Id;
            public readonly int Performance; // relative game balance, 1..5
            public readonly bool Antique;
            public readonly int Level;
            public Candidate(string id, int performance, bool antique = false, int level = 2)
            { Id = id; Performance = performance; Antique = antique; Level = level; }
        }
        public static readonly Candidate[] Candidates = Build();
        private static Candidate[] Build()
        {
            var all = new List<Candidate>();
            Action<string,int,int> vanilla = (id, p, level) => all.Add(new Candidate(id, p, false, level));
            vanilla("printed_gun",1,1); vanilla("handmade_pistol",1,1); vanilla("heavy_handmade_pistol",2,2);
            vanilla("revolver",2,2);
            vanilla("glock_receiver",3,2); vanilla("smg",4,3); vanilla("shotgun",3,3);
            foreach (var gun in EarlyFirearms.Catalog.All) all.Add(new Candidate(gun.Id,1,true));
            var modernScores = new Dictionary<string,int> {
                ["colt_saa"]=2, ["m1911"]=3, ["winchester1873"]=2, ["mosin1891"]=3,
                ["kar98k"]=3, ["m1garand"]=4, ["thompson1928"]=4, ["ppsh41"]=4,
                ["winchester1897"]=3, ["bar1918"]=4, ["mg42"]=5, ["stg44"]=4
            };
            foreach (var gun in ModernWars.Entry.Guns)
            {
                if (!modernScores.TryGetValue(gun.Key, out var score))
                    throw new InvalidOperationException("Missing seller score: " + gun.Key);
                bool automatic = gun.Key == "thompson1928" || gun.Key == "ppsh41" || gun.Key == "bar1918" || gun.Key == "mg42" || gun.Key == "stg44";
                all.Add(new Candidate(gun.Id, score, false, automatic ? 3 : 2));
            }
            all.Add(new Candidate("ak47prototype.rifle",4,false,3)); all.Add(new Candidate("glock17prototype.pistol",3,false,2));
            var keys = new[] { "m16a1","fnfal","mp5","p90","sw629","rem870","m24","m82a1","m249","m60" };
            var scores = new[] { 4,4,4,4,3,3,4,5,5,5 };
            for (int i = 0; i < keys.Length; i++) {
                bool automatic = keys[i] == "m16a1" || keys[i] == "fnfal" || keys[i] == "mp5" || keys[i] == "p90" || keys[i] == "m249" || keys[i] == "m60";
                all.Add(new Candidate("coldwarmodern." + keys[i], scores[i], false, automatic ? 3 : 2));
            }
            foreach (var gun in Rimfire22.Catalog.Guns) all.Add(new Candidate(gun.Id,2,false,1));
            foreach (var candidate in NewGuns010.NewGunsFlow010.Candidates.Values) all.Add(candidate);
            all.Add(new Candidate(Bfg010.Catalog.Id, 5, false, 2));
            return all.ToArray();
        }
        // All sellers draw from the same pool: lower-performance guns are modestly
        // more common. Antique weight is independent of their low performance.
        public static int Weight(Candidate candidate, int sellerTier)
        {
            if (candidate == null || sellerTier < 1 || sellerTier > 3 || candidate.Performance < 1 || candidate.Performance > 5 || candidate.Level < 1 || candidate.Level > 3) return 0;
            if (candidate.Antique) return 1;
            return 20 * (7 - candidate.Performance) + 10 * (3 - candidate.Level);
        }
        // randomUnit must be in [0,1). A null result means caller retains the original.
        public static Candidate Choose(IEnumerable<Candidate> candidates, int sellerTier, Func<string,bool> available, double randomUnit)
        {
            if (candidates == null || available == null || double.IsNaN(randomUnit) || randomUnit < 0 || randomUnit >= 1) return null;
            var eligible = new List<Candidate>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            long total = 0;
            foreach (var candidate in candidates)
            {
                if (candidate == null || string.IsNullOrEmpty(candidate.Id) || !seen.Add(candidate.Id)) continue;
                int weight = Weight(candidate, sellerTier);
                if (weight == 0 || !available(candidate.Id)) continue;
                eligible.Add(candidate); total += weight;
            }
            if (total == 0) return null;
            double ticket = randomUnit * total;
            foreach (var candidate in eligible)
            {
                ticket -= Weight(candidate, sellerTier);
                if (ticket < 0) return candidate;
            }
            return null;
        }
    }
}
