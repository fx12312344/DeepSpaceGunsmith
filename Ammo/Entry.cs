using System;
using UnityEngine;

namespace AmmoMachineTest {
 internal sealed class Entry {
  const string ModId=ColdWarModernExpansion.Entry.ModId;
  const string DebugId="local.dsg.ammo.machine";
  internal const string MachineId="ammomachinetest.machine";
  internal static bool IsActive {get;private set;}
  ModLog log;
  bool hooked;
  static string Pick(string zh,string en)=>string.Equals(LocHelper.GetCurrentLocaleCode(),"en",StringComparison.OrdinalIgnoreCase)?en:zh;
  public void Init(ModManifest manifest) { log=new ModLog(manifest); }
  public void OnEnable() {
   if(hooked)return;
   ModHook.OnModItemDirectoryInit+=Register;
   ModHook.OnGoingSleepEarly+=AmmoSafety.RestoreExisting;
   ModHook.OnPlaceInventorInventoryItemEarly+=OfferAtJackson;
   IsActive=true;
   ModHelper.AddDebugFunction(DebugId,ModHelper.DEBUG_CATEGORY_SPAWN,"Ammo workbench",Spawn,false);
   hooked=true;
   foreach(var dir in Resources.FindObjectsOfTypeAll<ModItemDirectory>()) if(dir!=null && dir.factoryDictionary!=null) Register(dir);
  }
  public void OnDisable() { if(!hooked)return; IsActive=false; ModHook.OnModItemDirectoryInit-=Register; ModHook.OnGoingSleepEarly-=AmmoSafety.RestoreExisting; ModHook.OnPlaceInventorInventoryItemEarly-=OfferAtJackson; ModHelper.RemoveDebugFunctions(DebugId); hooked=false; }
  void OfferAtJackson(System.Collections.Generic.List<GameItem> owned) {
   if(!PlayerStore.HasInstance || PlayerStore.Instance.playerLocation!=PlayerStore.PlayerLocation.INVENTOR || !DirectoryMaster.Has<GameItem>(MachineId) || !RNG.Roll(35))return;
   if(!PlayerStore.Instance.TryAddDirectSellingItemToTable(MachineId))log.Warning("博士柜台空间不足，无法摆放弹药制造台。");
  }
  void Register(ModItemDirectory dir) {
   if(dir==null || dir.factoryDictionary==null || DirectoryMaster.Has<GameItem>(MachineId) || dir.Has(MachineId))return;
   if(!dir.Add(MachineId,MakeMachine))log.Error("Could not register ammo workbench.");
  }
  void Spawn() {
   if(!PlayerStore.HasInstance || EmporiumEntry.Instance==null || EmporiumEntry.Instance.invElement==null || !DirectoryMaster.Has<GameItem>(MachineId)) {log.Warning("Load a store before spawning the ammo machine.");return;}
   var item=ModHelper.SpawnItem(MachineId); if(item==null)return;
   ModHelper.SetItemOwnerShip(item,true);
   GraphUtils.TryAcceptAll(EmporiumEntry.Instance.invElement,item,-1);
   if(item.parentInventory==null){item.Destroy();log.Warning("Inventory full; ammo machine was not spawned.");}
  }
  static void RefreshReserve(GameItem machine,string key,string zh,string en,MachineryText text,MachineryProgressBar bar) {
   const int cap=AmmoReserveInputs.Limit;
   int value=AmmoReserveInputs.Current(machine,key);
   text.SetText(Pick(zh+"：",en+": ")+(value<0?"?":value.ToString())+" / "+cap);
   bar.SetValue(value<0?0:Mathf.Clamp01((float)value/cap));
  }
  static GameItem MakeMachine() {
   var machine=ItemDirectory.CreateEmptyItem(null);
   var battery=new GameSlotInventory();
   var input1=new GameSlotInventory(); var input2=new GameSlotInventory(); var input3=new GameSlotInventory(); var input4=new GameSlotInventory();
   var slots=new[]{input1,input2,input3,input4};
   var output=new GameGridInventory(6,8); bool allowOutput=false; output.mayInventoryAddItemFunc=(item,inv)=>allowOutput;
   var reserve1=new MachineryText(); var reserve2=new MachineryText(); var reserve3=new MachineryText(); var reserve4=new MachineryText();
   var bar1=new MachineryProgressBar(0f,100); var bar2=new MachineryProgressBar(0f,100); var bar3=new MachineryProgressBar(0f,100); var bar4=new MachineryProgressBar(0f,100);
   var reserves=MachineryStack.Vertical(reserve1,bar1,reserve2,bar2,reserve3,bar3,reserve4,bar4);
   Action refresh=()=> {
    RefreshReserve(machine,AmmoReserveInputs.Specs[0].Tag,"火药与底火","Powder and primers",reserve1,bar1);
    RefreshReserve(machine,AmmoReserveInputs.Specs[1].Tag,"弹壳堆","Cartridge cases",reserve2,bar2);
    RefreshReserve(machine,AmmoReserveInputs.Specs[2].Tag,"弹头堆","Projectiles",reserve3,bar3);
    RefreshReserve(machine,AmmoReserveInputs.Specs[3].Tag,"核废料","Nuclear waste",reserve4,bar4);
   };
   var inputStack=MachineryStack.Vertical(MachineHelper.PowerPanel(battery),
    AmmoReserveInputs.Create(machine,input1,AmmoReserveInputs.Specs[0],refresh),
    AmmoReserveInputs.Create(machine,input2,AmmoReserveInputs.Specs[1],refresh),
    AmmoReserveInputs.Create(machine,input3,AmmoReserveInputs.Specs[2],refresh),
    AmmoReserveInputs.Create(machine,input4,AmmoReserveInputs.Specs[3],refresh));
   var name=new MachineryText(); var cost=new MachineryText(); var time=new MachineryText(); var status=new MachineryText(); int index=0,quantity=1;
   Action updateRecipe=null;
   var list=new AmmoRecipeList(i=>{index=i;updateRecipe?.Invoke();});
   var minus=new MachineryButton("−",()=>{quantity=Math.Max(1,quantity-1);updateRecipe?.Invoke();});
   var plus=new MachineryButton("+",()=>{quantity=Math.Min(99,quantity+1);updateRecipe?.Invoke();});
   updateRecipe=()=>{var r=AmmoRecipes.All[index];int left=AmmoJobs.Remaining(machine),nights=AmmoJobs.NightsLeft(machine);name.SetText(Pick(r.Zh,r.En)+" × "+quantity);cost.SetText(Pick("火药/壳/头: ","Powder/case/projectile: ")+(r.Powder*quantity)+" / "+(r.Cases*quantity)+" / "+(r.Projectiles*quantity));time.SetText(Pick("1 晚 / 整批 8 电","1 night / 8 power per batch"));status.SetText(left>0?(nights>0?Pick("加工中，待夜间供电","Waiting for powered night"):Pick("待取出：","Awaiting output: ")+left):(DirectoryMaster.Has<GameItem>(r.Id)?Pick("就绪","Ready"):Pick("物品未注册","Item unavailable")));};
   var start=new MachineryButton(Pick("开始制造","Start"),()=>{if(!IsActive){status.SetText(Pick("模组已停用","Mod disabled"));return;}if(!AmmoJobs.TryStart(machine,AmmoRecipes.All[index],quantity,out var error))status.SetText(Pick("无法开工：","Cannot start: ")+Pick(error=="materials"?"材料不足":error=="missing"?"物品未注册":error=="busy"?"机器忙碌":error=="placement"?"请移至自己的物品栏":"参数错误",error));else{refresh();updateRecipe();}});
   var recipes=MachineryStack.Vertical(list,name,MachineryStack.Horizontal(minus,plus),cost,time,start,status);
   updateRecipe();
   var window=MachineHelper.CreateMachineryWindow(true,
    new MachineryPanel(Pick("投料口","INPUT"),inputStack,"inputs"),
    MachineHelper.Panel(Pick("成品取出","OUTPUT"),output,"output"),
    new MachineryPanel(Pick("材料储备","MATERIAL RESERVES"),reserves,"control"),
    new MachineryPanel(Pick("配方选择","RECIPE SELECTION"),recipes,"recipes"));
   machine.SetContentWindow(window);
   window.openCallback+=()=>{if(!IsActive)return;AmmoReserveInputs.RefreshPending(slots,refresh); AmmoJobs.Deliver(machine,output,value=>allowOutput=value,updateRecipe); updateRecipe();};
   output.onSlotRemoveItemEndFunc+=(item,inv,marker)=>LetterboxManager.RunNextUpdate(()=>{if(IsActive){AmmoJobs.Deliver(machine,output,value=>allowOutput=value,updateRecipe);updateRecipe();}});
   machine.onCycleEndSlotItemFunc+=(item,inv,marker)=>{if(IsActive)AmmoJobs.OnNight(machine,battery,output,value=>allowOutput=value,updateRecipe);};
   refresh();
   // 1024x1536 source: fit 3x5 cells without reducing the source image.
   foreach(var mod in ModLoader.Mods) {
    if(mod?.Manifest?.ID!=ModId)continue;
    if(mod.Sprites.TryGetValue("ammo_machine",out var original) && original!=null) {
     float ppu=Mathf.Max(original.rect.width/48f,original.rect.height/80f)*100f;
     if(Math.Abs(original.pixelsPerUnit-ppu)>0.01f) {var scaled=Sprite.Create(original.texture,original.rect,new Vector2(.5f,.5f),ppu); scaled.name=original.name; mod.Sprites["ammo_machine"]=scaled;}
    }
    break;
   }
   machine.SetSpriteAndShapeFromMod(ModId,"ammo_machine");
   if(machine is GameItemElement visual) {if(visual.background?.image!=null) visual.background.image.alphaHitTestMinimumThreshold=0f; if(visual.fakeBackground?.image!=null) visual.fakeBackground.image.alphaHitTestMinimumThreshold=0f;}
   machine.SetShape(new GridShapeBuilder().SetDataFill(3,5,1));
   machine.name=Pick("弹药制造台","Ammunition Workbench");
   machine.shortDescription=UtilityItems.UtilityItemText.AmmoDescription;
   machine.unitValue=1000; machine.EnableTag("CONTAINER_TAG"); MachineHelper.SetupBatterySlot(battery,machine);
   var previousPlacement=machine.mayInventoryAddItemFunc;
   machine.mayInventoryAddItemFunc=(item,inv)=>(!GeneralHelper.IsItemOwned(item) || !AmmoSafety.IsCustomerCounter(inv)) && (previousPlacement==null || previousPlacement.InvokeAllReduce(item,inv,InvokeFuncExtensions.BoolReduceOR));
   return machine;
  }
 }
}
