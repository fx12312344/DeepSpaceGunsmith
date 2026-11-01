using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace NpcWeaponTrade
{
    public sealed class Entry
    {
        private Harmony harmony;
        private ModLog log;
        public void Init(ModManifest manifest) { log = new ModLog(manifest); }
        public void OnEnable()
        {
            if (harmony != null) return;
            try {
                harmony = new Harmony("local.npcweapontrade");
                foreach (var name in new[] { "CreateLLGunBuyer", "CreateMercGunBuyer", "CreateULBuyer" })
                {
                    var method = AccessTools.Method(typeof(StoreClientListGun), name, Type.EmptyTypes);
                    if (method == null) throw new MissingMethodException(name);
                    harmony.Patch(method, prefix: new HarmonyMethod(typeof(Patches), nameof(Patches.Enter)), finalizer: new HarmonyMethod(typeof(Patches), nameof(Patches.Exit)));
                    harmony.Patch(method, prefix: new HarmonyMethod(typeof(OwnedStandardPricing), nameof(OwnedStandardPricing.Enter)), finalizer: new HarmonyMethod(typeof(OwnedStandardPricing), nameof(OwnedStandardPricing.Exit)));
                    var orderMethod = AccessTools.Method(typeof(StoreClientListGun), name, new[] { typeof(GunOrderData) });
                    if (orderMethod == null) throw new MissingMethodException(name + "(GunOrderData)");
                    harmony.Patch(orderMethod, prefix: new HarmonyMethod(typeof(OwnedStandardPricing), nameof(OwnedStandardPricing.Price)));
                }
                foreach (var name in new[] { "GetRandomBasicOrder", "GetRandomMidOrder", "GetRandomHighOrder" })
                {
                    var method = AccessTools.Method(typeof(StoreClientListGun), name, Type.EmptyTypes);
                    if (method == null) throw new MissingMethodException(name);
                    harmony.Patch(method, transpiler: new HarmonyMethod(typeof(Patches), nameof(Patches.ReplaceBestCall)));
                }
                log.Warning("NPC weapon trade buyer caliber adaptation enabled; verify in game before use.");
            } catch (Exception ex) { harmony?.UnpatchAll(harmony.Id); harmony = null; log.Error("Patching aborted: " + ex); }
        }
        public void OnDisable() { harmony?.UnpatchAll(harmony.Id); harmony = null; }
    }

    public static class Patches
    {
        [ThreadStatic] private static int buyerDepth;
        public static void Enter() { buyerDepth++; }
        public static Exception Exit(Exception __exception) { if (buyerDepth > 0) buyerDepth--; return __exception; }
        public static IEnumerable<CodeInstruction> ReplaceBestCall(IEnumerable<CodeInstruction> instructions)
        {
            var source = instructions.ToList();
            var original = AccessTools.Method(typeof(GunOrderData), nameof(GunOrderData.CreateBestOrder), new[] { typeof(int), typeof(int) });
            var replacement = AccessTools.Method(typeof(Patches), nameof(CreateBestScoped));
            int count = source.Count(i => i.opcode == OpCodes.Call && Equals(i.operand, original));
            if (count != 1) throw new InvalidOperationException("Expected exactly one CreateBestOrder call; found " + count);
            foreach (var i in source) { if (i.opcode == OpCodes.Call && Equals(i.operand, original)) i.operand = replacement; yield return i; }
        }
        public static GunOrderData CreateBestScoped(int tier, int originalCaliber)
        {
            int chosen = originalCaliber;
            try
            {
                if (buyerDepth > 0 && EmporiumEntry.Instance != null)
                {
                    var candidates = new HashSet<int>();
                    foreach (var item in EmporiumEntry.Instance.GetAllOwnedItems())
                    {
                        if (item == null || !item.IsTag("ADV_GUN_TAG") || !item.IsTag("GUN_CALIBER_TAG")) continue;
                        if (!TryResolveSpec(item.identifier, out var spec)) continue;
                        // Antique paper-cartridge IDs are generic placeholders, not exact ammunition.
                        // Respect the order tier, but do not enforce licenses: vanilla handles sales.
                        if (spec.Caliber == 810001 || spec.Tier > tier) continue;
                        if (item.GetTagReadonly("GUN_CALIBER_TAG").GetInt() != spec.Caliber) continue;
                        candidates.Add(spec.Caliber);
                    }
                    // Preserve at least one quarter of vanilla best orders; random by distinct caliber.
                    if (candidates.Count > 0 && RNG.GetRandomIntExculsive(0, 4) != 0)
                    {
                        var list = candidates.OrderBy(x => x).ToList();
                        chosen = list[RNG.GetRandomIntExculsive(0, list.Count)];
                    }
                }
            }
            catch (Exception) { chosen = originalCaliber; }
            return GunOrderData.CreateBestOrder(tier, chosen);
        }
        public static bool TryResolveSpec(string id, out Spec spec)
        {
            if (id != null && Catalog.TryGetValue(id, out spec)) return true;
            if (NewGuns010.NewGunsFlow010.TryGetOrderSpec(id, out spec)) return true;
            if (id == Bfg010.Catalog.Id) { spec = new Spec(Bfg010.Catalog.FictionalEnergyCaliber, 2); return true; }
            const string suffix = ".trade_test";
            if (id != null && id.StartsWith("earlyfirearms.", StringComparison.Ordinal) && id.EndsWith(suffix, StringComparison.Ordinal))
                return Catalog.TryGetValue(id.Substring(0, id.Length - suffix.Length), out spec);
            spec = default(Spec);
            return false;
        }
        public readonly struct Spec { public readonly int Caliber, Tier; public Spec(int c, int t) { Caliber = c; Tier = t; } }
        public static readonly Dictionary<string, Spec> Catalog = new Dictionary<string, Spec> {
            ["earlyfirearms.hand_cannon"] = new Spec(810001,1), ["earlyfirearms.matchlock"] = new Spec(810001,1), ["earlyfirearms.wheellock"] = new Spec(810001,1), ["earlyfirearms.blunderbuss"] = new Spec(810001,1),
            ["earlyfirearms.brown_bess"] = new Spec(810001,1), ["earlyfirearms.charleville"] = new Spec(810001,1), ["earlyfirearms.naval_pistol"] = new Spec(810001,1), ["earlyfirearms.baker"] = new Spec(810001,1),
            ["earlyfirearms.hall_1819"] = new Spec(810001,1), ["earlyfirearms.dreyse"] = new Spec(810001,1), ["earlyfirearms.colt_1851"] = new Spec(810001,1), ["earlyfirearms.mississippi_1841"] = new Spec(810001,1),
            ["rimfire22.markiv"] = new Spec(22,1), ["rimfire22.p22"] = new Spec(22,1), ["rimfire22.buckmark"] = new Spec(22,1), ["rimfire22.1022"] = new Spec(22,1),
            ["glock17prototype.pistol"] = new Spec(919,2), ["ak47prototype.rifle"] = new Spec(76239,3),
            ["coldwarmodern.m16a1"] = new Spec(55645,3), ["coldwarmodern.fnfal"] = new Spec(76251,3), ["coldwarmodern.mp5"] = new Spec(919,3), ["coldwarmodern.p90"] = new Spec(5728,3), ["coldwarmodern.sw629"] = new Spec(441,2), ["coldwarmodern.rem870"] = new Spec(1270,2), ["coldwarmodern.m24"] = new Spec(76251,2), ["coldwarmodern.m82a1"] = new Spec(12799,2), ["coldwarmodern.m249"] = new Spec(55645,3), ["coldwarmodern.m60"] = new Spec(76251,3),
            ["local.modernwars.colt_saa"] = new Spec(204501,2), ["local.modernwars.m1911"] = new Spec(204502,2), ["local.modernwars.winchester1873"] = new Spec(204403,2), ["local.modernwars.mosin1891"] = new Spec(207654,2), ["local.modernwars.kar98k"] = new Spec(207957,2), ["local.modernwars.m1garand"] = new Spec(203006,2), ["local.modernwars.thompson1928"] = new Spec(204502,3), ["local.modernwars.ppsh41"] = new Spec(207625,3), ["local.modernwars.winchester1897"] = new Spec(1270,2), ["local.modernwars.bar1918"] = new Spec(203006,3), ["local.modernwars.mg42"] = new Spec(207957,3), ["local.modernwars.stg44"] = new Spec(207933,3)
        };
    }
}
