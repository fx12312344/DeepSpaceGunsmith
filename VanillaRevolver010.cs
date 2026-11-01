using System;
using System.Reflection;
using HarmonyLib;

namespace ColdWarModernExpansion
{
    internal static class VanillaRevolver010
    {
        private const string HarmonyId = "local.coldwarmodern.vanillarevolver010";
        private static Harmony harmony;
        private static readonly MethodInfo Target = AccessTools.Method(typeof(DirectoryMaster), nameof(DirectoryMaster.Item), new[] { typeof(string), typeof(bool) });

        // ModLoader.PatchAll discovers these lifecycle hooks; register the target
        // explicitly, after Entry.OnEnable has made IsActive true.
        [HarmonyPatch(typeof(Entry), nameof(Entry.OnEnable))]
        private static class EnablePatch
        {
            private static void Postfix()
            {
                if (harmony != null) return;
                if (Target == null) throw new MissingMethodException("DirectoryMaster.Item(string, bool)");
                var instance = new Harmony(HarmonyId);
                instance.Patch(Target, postfix: new HarmonyMethod(typeof(VanillaRevolver010), nameof(Postfix)));
                harmony = instance;
            }
        }

        [HarmonyPatch(typeof(Entry), nameof(Entry.OnDisable))]
        private static class DisablePatch
        {
            private static void Prefix()
            {
                if (harmony == null) return;
                harmony.Unpatch(Target, HarmonyPatchType.Postfix, HarmonyId);
                harmony = null;
            }
        }

        private static void Postfix(string identifier, ref GameItem __result)
        {
            if (!Entry.IsActive || identifier != "revolver" || __result == null ||
                __result.identifier != "revolver" || !__result.IsTag("ADV_GUN_TAG") ||
                __result.contentWindow != null)
                return;

            var (window, stock, rail, magazine, underbarrel, barrel, special1, special2, special3) =
                GunModHelper.CreateGunModWindow(true, true, 5);
            // Enable* captures the parent now, not when the player inserts a mod.
            __result.SetContentWindow(window);
            ExistingGunSlots.Configure(stock, rail, magazine, underbarrel, barrel,
                special1, special2, special3, true, false, false, false);
            if (!__result.IsGameItemType("MODDABLE"))
                __result.SetGameItemType("MODDABLE");
        }
    }
}
