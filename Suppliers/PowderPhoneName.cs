using System;
using HarmonyLib;

namespace PowderSupplierIntegration
{
    internal static class PhoneName
    {
        const string HarmonyId = "local.powdersupplierprototype.phone_name";
        const string DialogId = "local.powdersupplierprototype.dialog";
        const string LocId = "local.powdersupplierprototype.contact";
        static Harmony harmony;
        internal static void Install()
        {
            if (harmony != null) return;
            var h = new Harmony(HarmonyId);
            try {
                var target = AccessTools.Method(typeof(StorePhoneClient), nameof(StorePhoneClient.GetLocalizedDisplayName), Type.EmptyTypes);
                if (target == null || target.ReturnType != typeof(string)) throw new MissingMethodException("StorePhoneClient.GetLocalizedDisplayName");
                h.Patch(target, prefix: new HarmonyMethod(typeof(PhoneName), nameof(Prefix)));
                harmony = h;
            } catch { h.UnpatchAll(HarmonyId); throw; }
        }
        internal static void Uninstall() { if (harmony == null) return; harmony.UnpatchAll(HarmonyId); harmony = null; }
        internal static bool Prefix(StorePhoneClient __instance, ref string __result)
        {
            if (__instance == null || __instance.dialogFuncId != DialogId || __instance.locID != LocId ||
                !PlayerStore.HasInstance || PlayerStore.Instance.PhoneClientDict == null ||
                !PlayerStore.Instance.PhoneClientDict.TryGetValue(75284, out var current) || !ReferenceEquals(current, __instance)) return true;
            try {
                var text = ModHelper.GetLocalized(Planning.ResourceModId, "contact");
                __result = string.IsNullOrWhiteSpace(text) || text == "contact" ? (__instance.displayName ?? "Peace Margin Industries") : text;
                return false;
            } catch { __result = __instance.displayName ?? "Peace Margin Industries"; return false; }
        }
    }
}
