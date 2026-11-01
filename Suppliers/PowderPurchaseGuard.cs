using System;
using HarmonyLib;

namespace PowderPurchaseGuard
{
    // Experimental fail-closed preflight only. Never charges, unlocks, or registers services.
    public static class Guard
    {
        public const string Source = "local.powdersupplierprototype";
        public const string PhoneId = Source + ".phone", ShareId = Source + ".shareholder";
        const string DialogId = Source + ".dialog", Intro = Source + ".delivery.free", Small = Source + ".delivery.small", Large = Source + ".delivery.large";
        const long Number = 75284;
        static readonly Harmony Harmony = new Harmony("local.powder.purchase.guard.experimental");
        static bool installed;
        public static void Install()
        {
            if (installed) return;
            var method = AccessTools.Method(typeof(WildNetworkUI), "OnUseClicked", Type.EmptyTypes);
            var field = AccessTools.Field(typeof(WildNetworkUI), "selectedId");
            if (method == null || field == null || field.FieldType != typeof(string) || !field.IsStatic) throw new InvalidOperationException("Unsupported WildNetworkUI shape");
            Harmony.Patch(method, prefix: new HarmonyMethod(typeof(Guard), nameof(Prefix)));
            installed = true;
        }
        public static void Uninstall()
        {
            if (!installed) return;
            Harmony.UnpatchAll("local.powder.purchase.guard.experimental"); installed = false;
        }
        public static bool Prefix(string ___selectedId)
        {
            if (___selectedId != PhoneId && ___selectedId != ShareId) return true;
            // Preserve the standalone preflight; the game still owns payment and unlocking.
            try
            {
                if (Ready(out var why)) return true;
                UnityEngine.Debug.LogWarning("[PowderPurchaseGuard] Purchase blocked: " + why);
            }
            catch (Exception ex) { UnityEngine.Debug.LogWarning("[PowderPurchaseGuard] Purchase blocked: preflight exception " + ex.GetType().Name); }
            return false;
        }
        static bool Ready(out string reason)
        {
            reason = "run or registries unavailable";
            if (!PlayerStore.HasInstance || PlayerStore.Instance == null || PlayerStore.Instance.PhoneClientDict == null || PlayerStore.Instance.networkUpgrade == null || PhoneClientList.DialogFuncs == null || StoreClientListDict.storeClientDict == null) return false;
            reason = "upgrade registrations incomplete";
            var phone = NetworkUpgrade.GetDefinition(PhoneId); var share = NetworkUpgrade.GetDefinition(ShareId);
            if (phone == null || share == null || phone.id != PhoneId || share.id != ShareId || phone.source != Source || share.source != Source || !PlayerStore.Instance.networkUpgrade.TryGetValue(PhoneId, out var runPhone) || runPhone == null || !PlayerStore.Instance.networkUpgrade.TryGetValue(ShareId, out var runShare) || runShare == null) return false;
            reason = "dialog registration missing or unexpected";
            if (!PhoneClientList.DialogFuncs.TryGetValue(DialogId, out var dialog) || dialog == null || dialog.Target == null || dialog.Target.GetType().FullName != "PowderSupplierIntegration.Entry" || dialog.Method.Name != "MakeDialog") return false;
            reason = "phone unlock callback missing or different owner";
            if (NetworkUpgradeList.UnlockActions == null || !NetworkUpgradeList.UnlockActions.TryGetValue(PhoneId, out var callback) || callback == null || !ReferenceEquals(callback.Target, dialog.Target) || callback.Method.Name != "EnsurePhone") return false;
            reason = "powder item unavailable";
            if (!DirectoryMaster.Has<GameItem>("local.deepspace010.material.smokeless_powder")) return false;
            reason = "courier registrations incomplete or different owner";
            if (!OwnedFactory(Intro, dialog.Target) || !OwnedFactory(Small, dialog.Target) || !OwnedFactory(Large, dialog.Target)) return false;
            reason = "phone number occupied by another contact";
            if (PlayerStore.Instance.PhoneClientDict.TryGetValue(Number, out var existing) && (existing == null || existing.dialogFuncId != DialogId)) return false;
            reason = null; return true;
        }
        static bool OwnedFactory(string id, object owner)
        {
            if (!StoreClientListDict.storeClientDict.TryGetValue(id, out var factory) || factory == null || factory.Target == null || factory.Method.DeclaringType == null || factory.Method.DeclaringType.Assembly != owner.GetType().Assembly) return false;
            foreach (var field in factory.Target.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic))
                if (ReferenceEquals(field.GetValue(factory.Target), owner)) return true;
            return ReferenceEquals(factory.Target, owner);
        }
    }
}
