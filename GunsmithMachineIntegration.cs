using System;
using HarmonyLib;

namespace GunsmithMachineIntegration {
 internal static class Integration {
  const string Mod = "local.coldwarmodern";
  const string DebugId = "local.gunsmithmachine";
  static ModLog log;
  static bool hooked;
  [HarmonyPatch(typeof(ColdWarModernExpansion.Entry), nameof(ColdWarModernExpansion.Entry.Init))]
  internal static class InitPatch { static void Postfix(ModManifest manifest) { log = new ModLog(manifest); } }
  [HarmonyPatch(typeof(ColdWarModernExpansion.Entry), nameof(ColdWarModernExpansion.Entry.OnEnable))]
  internal static class EnablePatch { static void Postfix() { Enable(); } }
  [HarmonyPatch(typeof(ColdWarModernExpansion.Entry), nameof(ColdWarModernExpansion.Entry.OnDisable))]
  internal static class DisablePatch { static void Prefix() { Disable(); } }
  static void Enable() {
   if (hooked) return; hooked=true;
   ModHook.OnModItemDirectoryInit += Register;
   ModHook.OnPlaceInventorInventoryItemEarly += OfferAtJackson;
   foreach (var dir in UnityEngine.Resources.FindObjectsOfTypeAll<ModItemDirectory>()) if (dir != null && dir.factoryDictionary != null) Register(dir);
   AddSpawn("gunsmithmachine.barrel_short", "02 Short barrel");
   AddSpawn("gunsmithmachine.barrel_medium", "03 Medium barrel");
   AddSpawn("gunsmithmachine.barrel_long", "04 Long barrel");
  }
  static void Disable() { if (!hooked) return; ModHook.OnModItemDirectoryInit -= Register; ModHook.OnPlaceInventorInventoryItemEarly -= OfferAtJackson; ModHelper.RemoveDebugFunctions(DebugId); hooked=false; }
  static void OfferAtJackson(System.Collections.Generic.List<GameItem> owned) {
   if (!PlayerStore.HasInstance || PlayerStore.Instance.playerLocation != PlayerStore.PlayerLocation.INVENTOR || !DirectoryMaster.Has<GameItem>("cncreserveretest.machine")) return;
   if (!RNG.Roll(35)) return; // 新机床沿用旧机床博士进货概率。
   if (!PlayerStore.Instance.TryAddDirectSellingItemToTable("cncreserveretest.machine")) log.Warning("博士货架空间不足，无法摆放枪匠万能机床。");
  }
  static void AddSpawn(string id,string label) { ModHelper.AddDebugFunction(DebugId,ModHelper.DEBUG_CATEGORY_SPAWN,label,()=>Spawn(id),false); }
  static bool Spawn(string id) {
   if (!PlayerStore.HasInstance || EmporiumEntry.Instance == null || EmporiumEntry.Instance.invElement == null || !DirectoryMaster.Has<GameItem>(id)) return false;
   var item=ModHelper.SpawnItem(id); if (item == null) return false;
   ModHelper.SetItemOwnerShip(item,true); GraphUtils.TryAcceptAll(EmporiumEntry.Instance.invElement,item,-1);
   if (item.parentInventory == null) { item.Destroy(); return false; } return true;
  }
  static void Register(ModItemDirectory dir) {
   Add(dir,"gunsmithmachine.barrel_short",()=>Barrel("短枪管",2));
   Add(dir,"gunsmithmachine.barrel_medium",()=>Barrel("中枪管",4));
   Add(dir,"gunsmithmachine.barrel_long",()=>Barrel("长枪管",6));
  }
  static void Add(ModItemDirectory dir,string id,Func<GameItem> factory) { if (dir != null && dir.factoryDictionary != null && !DirectoryMaster.Has<GameItem>(id) && !dir.Has(id)) dir.Add(id,factory); }
  static GameItem Barrel(string name,int width) {
   var item=ItemDirectory.CreateEmptyItem(null); item.SetSpriteAndShapeFromMod(Mod,"gunsmith_barrel"); item.SetShape(new GridShapeBuilder().SetDataFill(width,1,1));
   item.name=EnglishLocalization.L.Pick(name, width==2 ? "Short Barrel Blank" : width==4 ? "Medium Barrel Blank" : "Long Barrel Blank"); item.shortDescription=UtilityItems.UtilityItemText.BarrelDescription(width); item.unitValue=width*40; item.SetGameItemType("MATERIAL"); return item;
  }
 }
}
