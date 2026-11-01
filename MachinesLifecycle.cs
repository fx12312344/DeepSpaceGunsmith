using HarmonyLib;

namespace DeepSpaceGunsmithMachines
{
    // Host loader patches lifecycle attributes in the primary assembly.
    internal static class MachinesLifecycle
    {
        private static readonly CncReserveStandalone.Entry Cnc = new CncReserveStandalone.Entry();
        private static readonly AmmoMachineTest.Entry Ammo = new AmmoMachineTest.Entry();

        [HarmonyPatch(typeof(ColdWarModernExpansion.Entry), nameof(ColdWarModernExpansion.Entry.Init))]
        private static class InitPatch { private static void Postfix(ModManifest manifest) { Cnc.Init(manifest); Ammo.Init(manifest); } }
        [HarmonyPatch(typeof(ColdWarModernExpansion.Entry), nameof(ColdWarModernExpansion.Entry.OnEnable))]
        private static class EnablePatch { private static void Postfix() { Cnc.OnEnable(); Ammo.OnEnable(); } }
        [HarmonyPatch(typeof(ColdWarModernExpansion.Entry), nameof(ColdWarModernExpansion.Entry.OnDisable))]
        private static class DisablePatch { private static void Prefix() { Cnc.OnDisable(); Ammo.OnDisable(); } }
    }
}
