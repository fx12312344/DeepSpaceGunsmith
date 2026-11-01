using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace ColdWarModernExpansion
{
    internal static class ServiceGunIntegration010
    {
        private const string HarmonyId = "local.coldwarmodern.servicegun010";
        private const string Gold = "local.deepspace010.attachment.gold_plating";
        private const string Trigger = "local.deepspace010.attachment.advanced_trigger";
        private const string Style = "GUN_CURRENT_STYLE_TAG";
        private const string Reliability = "GUN_CURRENT_RELIABILITY_TAG";
        private static Harmony harmony;
        // Keep the creation guard installed after disable: a previously selected
        // customer may greet later; do not destroy their gun or change its ID.
        private static Harmony creationGuard;
        private static ModLog log;
        [ThreadStatic] private static int buyerDepth;
        private static readonly FieldInfo GunId = AccessTools.Field(typeof(GunOrderData), "serviceGunId");
        private static readonly FieldInfo GunObject = AccessTools.Field(typeof(GunOrderData), "serviceGun");

        [HarmonyPatch(typeof(Entry), nameof(Entry.Init))]
        private static class InitPatch { private static void Postfix(ModManifest manifest) { log = new ModLog(manifest); } }
        [HarmonyPatch(typeof(Entry), nameof(Entry.OnEnable))]
        private static class EnablePatch { private static void Postfix() { Install(); } }
        [HarmonyPatch(typeof(Entry), nameof(Entry.OnDisable))]
        private static class DisablePatch { private static void Prefix() { Remove(); } }

        private static void Install()
        {
            if (harmony != null || !Entry.IsActive) return;
            var h = new Harmony(HarmonyId);
            try
            {
                if (GunId == null || GunObject == null) throw new MissingFieldException("GunOrderData service gun fields");
                foreach (string name in new[] { "CreateLLGunBuyer", "CreateMercGunBuyer", "CreateULBuyer" })
                {
                    var noArgs = AccessTools.Method(typeof(StoreClientListGun), name, Type.EmptyTypes);
                    var withOrder = AccessTools.Method(typeof(StoreClientListGun), name, new[] { typeof(GunOrderData) });
                    if (noArgs == null || withOrder == null) throw new MissingMethodException(name);
                    h.Patch(noArgs, prefix: new HarmonyMethod(typeof(ServiceGunIntegration010), nameof(Enter)),
                        finalizer: new HarmonyMethod(typeof(ServiceGunIntegration010), nameof(Exit)));
                    h.Patch(withOrder, prefix: new HarmonyMethod(typeof(ServiceGunIntegration010), nameof(Select)));
                }
                var create = AccessTools.Method(typeof(GunOrderData), nameof(GunOrderData.CreateServiceGun), Type.EmptyTypes);
                if (create == null) throw new MissingMethodException("CreateServiceGun");
                if (creationGuard == null)
                {
                    var guard = new Harmony(HarmonyId + ".creation");
                    try { guard.Patch(create, finalizer: new HarmonyMethod(typeof(ServiceGunIntegration010), nameof(FinishServiceGun))); creationGuard = guard; }
                    catch { guard.UnpatchAll(guard.Id); throw; }
                }
                harmony = h;
            }
            catch (Exception ex) { h.UnpatchAll(HarmonyId); log?.Error("Service gun integration not installed: " + ex); }
        }
        private static void Remove() { harmony?.UnpatchAll(HarmonyId); harmony = null; buyerDepth = 0; }
        private static void Enter() { buyerDepth++; }
        private static Exception Exit(Exception __exception) { if (buyerDepth > 0) buyerDepth--; return __exception; }

        // The trade catalogue is 40 original IDs; the 010 flow adds the other 11.
        // Neither catalogue enumeration includes the antique .trade_test variants.
        internal static List<string> BuildCandidates(int tier)
        {
            var result = new List<string>();
            if (tier < 1 || tier > 3) return result;
            foreach (var pair in NpcWeaponTrade.Patches.Catalog)
                if (pair.Value.Tier == tier && DirectoryMaster.Has<GameItem>(pair.Key)) result.Add(pair.Key);
            foreach (var pair in NewGuns010.NewGunsFlow010.Orders)
                if (pair.Value.Tier == tier && DirectoryMaster.Has<GameItem>(pair.Key)) result.Add(pair.Key);
            if (tier == 2 && DirectoryMaster.Has<GameItem>(Bfg010.Catalog.Id)) result.Add(Bfg010.Catalog.Id);
            result.Sort(StringComparer.Ordinal);
            return result;
        }
        private static void Select(GunOrderData order)
        {
            if (!Entry.IsActive || buyerDepth <= 0 || order == null || !order.isServiceOrder || order.tier < 1 || order.tier > 3) return;
            try
            {
                if (GunObject.GetValue(order) != null || order.mandatoryRequirements == null || order.bonusRequirements == null ||
                    order.mandatoryRequirements.Count != 0 || order.bonusRequirements.Count != 0 || string.IsNullOrEmpty((string)GunId.GetValue(order))) return;
                if (!DirectoryMaster.Has<GameItem>(Gold) || !DirectoryMaster.Has<GameItem>(Trigger)) return;
                var available = BuildCandidates(order.tier);
                if (available.Count == 0 || RNG.GetRandomIntExculsive(0, 4) == 0) return;
                var chosen = available[RNG.GetRandomIntExculsive(0, available.Count)];
                string originalId = (string)GunId.GetValue(order), originalText = order.dialogText;
                string text = LocHelper.GetLocalizedOrDefault("Dialogue", "dialog_gun_order_req_service",
                    "Can you work on my {0}? I want it to do better. I pay for the parts you put on it.", GeneralHelper.GetDisplayNameFromID(chosen));
                try { GunId.SetValue(order, chosen); order.dialogText = text; }
                catch { GunId.SetValue(order, originalId); order.dialogText = originalText; throw; }
            }
            catch (Exception ex) { log?.Warning("Service gun selection skipped: " + ex.Message); }
        }

        // Factory-created guns have three special slots. The two distinct Scope.All
        // parts do not conflict: gold gives style +50%/+15, trigger reliability
        // +20%/+7. The game's integer formula uses base * totalPercent / 100 + flat.
        // Floor(base * partPercent / 100) + flat is a conservative gain even when
        // the condition multiplier has an integer rounding remainder.
        private static bool RewriteRequirements(GunOrderData order, GameItem gun)
        {
            if (gun == null || !gun.IsTag("ADV_GUN_TAG") || order.mandatoryRequirements == null || order.bonusRequirements == null ||
                order.mandatoryRequirements.Count != (order.tier == 1 ? 1 : 2) ||
                !DirectoryMaster.Has<GameItem>(Gold) || !DirectoryMaster.Has<GameItem>(Trigger)) return false;
            foreach (GameItem child in GraphUtils.FindAllChildrenType<GameItem>(gun))
                if (child != null && child.IsTag("WEAPON_MOD_TAG")) return false;
            int styleBase = gun.GetTagReadonly("GUN_STYLE_TAG").GetInt();
            int reliabilityBase = gun.GetTagReadonly("GUN_RELIABILITY_TAG").GetInt();
            if (styleBase < 0 || reliabilityBase < 0 || styleBase > 1000000 || reliabilityBase > 1000000) return false;
            int step = order.tier == 1 ? 8 : order.tier == 2 ? 10 : 12;
            int styleGain = styleBase * 50 / 100 + 15;
            int reliabilityGain = reliabilityBase * 20 / 100 + 7;
            int styleTarget = checked(gun.GetTagReadonly(Style).GetInt() + Math.Min(step, styleGain) - 1);
            int reliabilityTarget = checked(gun.GetTagReadonly(Reliability).GetInt() + Math.Min(step, reliabilityGain) - 1);
            foreach (var req in order.mandatoryRequirements) if (req == null) return false;
            // Keep every original requirement that the same verified two-part
            // configuration proves attainable. Unknown routes are not guessed.
            // Bonus requirements (including IDs, thresholds and rewards) are never changed.
            int styleMax = checked(gun.GetTagReadonly(Style).GetInt() + styleGain);
            int reliabilityMax = checked(gun.GetTagReadonly(Reliability).GetInt() + reliabilityGain);
            bool styleUsed = false, reliabilityUsed = false;
            var preserve = new bool[order.mandatoryRequirements.Count];
            for (int i = 0; i < preserve.Length; i++)
            {
                var req = order.mandatoryRequirements[i];
                if (req.isCaliber || !req.isMustBeLargerThan) continue;
                if (req.id == Style && req.value < styleMax) { preserve[i] = true; styleUsed = true; }
                else if (req.id == Reliability && req.value < reliabilityMax) { preserve[i] = true; reliabilityUsed = true; }
            }
            for (int i = 0; i < preserve.Length; i++)
            {
                if (preserve[i]) continue;
                if (!styleUsed) { SetRequirement(order.mandatoryRequirements[i], Style, styleTarget); styleUsed = true; }
                else if (!reliabilityUsed) { SetRequirement(order.mandatoryRequirements[i], Reliability, reliabilityTarget); reliabilityUsed = true; }
                else SetRequirement(order.mandatoryRequirements[i], Style, styleTarget);
            }
            return true;
        }
        private static void SetRequirement(GunOrderRequirement req, string id, int target)
        {
            req.id = id; req.value = target; req.isCaliber = false; req.isMustBeLargerThan = true;
        }
        private static bool IsModGun(string id, int tier)
        {
            NpcWeaponTrade.Patches.Spec spec;
            return id != null && ((NpcWeaponTrade.Patches.Catalog.TryGetValue(id, out spec) && spec.Tier == tier) ||
                (NewGuns010.NewGunsFlow010.Orders.TryGetValue(id, out spec) && spec.Tier == tier) ||
                (id == Bfg010.Catalog.Id && tier == 2));
        }
        private static Exception FinishServiceGun(GunOrderData __instance, ref GameItem __result, Exception __exception)
        {
            if (__instance == null || !__instance.isServiceOrder) return __exception;
            try
            {
                string id = (string)GunId.GetValue(__instance);
                if (!IsModGun(id, __instance.tier)) return __exception;
                if (__exception != null) return __exception;
                // Vanilla returns null when called again after a gun already exists;
                // that is not an error and must never mutate the existing object.
                if (__result == null && GunObject.GetValue(__instance) != null) return null;
                if (__result == null || !ReferenceEquals(GunObject.GetValue(__instance), __result) ||
                    !String.Equals(__result.identifier, id, StringComparison.Ordinal) || !RewriteRequirements(__instance, __result))
                    return new InvalidOperationException("Cannot validate customer service gun " + id);
                return null;
            }
            catch (Exception ex) { log?.Error("Service gun validation failed: " + ex); return __exception ?? ex; }
        }
    }
}
