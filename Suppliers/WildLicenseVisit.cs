using System;
using System.Linq;

namespace GunsmithLicenseVisit
{
    // The appointment is stored per save, independently of the transient daily customer stack.
    internal sealed class Entry
    {
        private const string ModId = "local.coldwarmodern";
        private const string ClientId = "local.coldwarmodern.wild_license_intro";
        private const string DueKey = "wild_license_intro_due"; // >0 pending; -1 complete
        private bool enabled;
        private ModLog log;
        private static string T(string key) => ModHelper.GetLocalized(ModId, key);
        internal void Init(ModManifest manifest) { log = new ModLog(manifest); }
        internal void OnEnable()
        {
            if (enabled) return;
            enabled = true;
            ModHook.OnNetworkUpgradeUnlocked += OnUnlock;
            ModHook.OnGenerateCustomerVeryLate += OnGenerate;
        }
        internal void OnDisable()
        {
            if (!enabled) return;
            enabled = false;
            ModHook.OnNetworkUpgradeUnlocked -= OnUnlock;
            ModHook.OnGenerateCustomerVeryLate -= OnGenerate;
            if (PlayerStore.HasInstance && PlayerStore.Instance?.storeClientManager?.clientStack != null)
                PlayerStore.Instance.storeClientManager.clientStack.RemoveAll(c => c != null && c.identifier == ClientId);
        }
        private void OnUnlock(string id)
        {
            if (!enabled || id != "GUN_PERMIT_I" || !PlayerStore.HasInstance || StoreStation.Instance == null) return;
            try
            {
                if (ModHelper.HasModData(ModId, DueKey)) return;
                int day = StoreStation.Instance.dayCounter;
                if (day < int.MaxValue) ModHelper.SetModDataInt(ModId, DueKey, day + 1);
            }
            catch (Exception ex) { log.Warning("Unable to schedule Wild's firearms introduction: " + ex); }
        }
        private void OnGenerate(StoreClientManager manager)
        {
            if (!enabled || manager?.clientStack == null || !PlayerStore.HasInstance || StoreStation.Instance == null || PlayerStore.Instance.storeClientManager != manager) return;
            try
            {
                int due = ModHelper.GetModDataInt(ModId, DueKey, 0);
                if (due <= 0 || StoreStation.Instance.dayCounter < due) return;
                if (manager.clientStack.Any(c => c != null && c.identifier == ClientId)) return;
                var current = PlayerStore.Instance.currentClientInstance;
                if (current != null && current.GetClientBlueprint()?.identifier == ClientId) return;
                var visitor = CreateVisitor();
                manager.AddClient(visitor);
            }
            catch (Exception ex) { log.Warning("Unable to queue Wild's firearms introduction: " + ex); }
        }
        private StoreClient CreateVisitor()
        {
            var visitor = new StoreClient {
                identifier = ClientId,
                displayName = LocHelper.GetLocalizedClientName("name_joe_wild"),
                spriteName = "joe", bubbleColor = "pink", nonShootable = true,
                clientIntent = StoreClient.ClientIntent.DIALOGUE, dismissable = false
            };
            ModHelper.SetClientSource(visitor, ModId);
            Dialogue node = visitor.mainDialogue.SetText(visitor.displayName, T("wild_license_intro_1"));
            for (int i = 2; i <= 6; i++) node = node.NextDialogue().SetText(visitor.displayName, T("wild_license_intro_" + i));
            node.SetEndAction(() => {
                if (!enabled || !PlayerStore.HasInstance) return;
                var active = PlayerStore.Instance.currentClientInstance;
                if (active == null || !ReferenceEquals(active.GetClientBlueprint(), visitor)) return;
                ModHelper.SetModDataInt(ModId, DueKey, -1);
                PlayerStore.Instance.DismissCurrentClient(false);
            });
            return visitor;
        }
    }
}
