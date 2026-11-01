using System;
using UnityEngine;

namespace AmmoMachineTest {
 internal static class AmmoReserveInputs {
  internal const int Limit=5000;
  internal sealed class Spec {
   internal readonly string Id,Tag,Zh,En;
   internal readonly int Amount;
   internal Spec(string id,string tag,string zh,string en,int amount=100){Id=id;Tag=tag;Zh=zh;En=en;Amount=amount;}
  }
  internal static readonly Spec[] Specs={
   new Spec("local.deepspace010.material.smokeless_powder","DSG_AMMO_RESERVE_1","火药与底火","Powder and primers"),
   new Spec("local.deepspace010.material.casings","DSG_AMMO_RESERVE_2","弹壳堆","Cartridge cases"),
   new Spec("local.deepspace010.material.projectiles","DSG_AMMO_RESERVE_3","弹头堆","Projectiles"),
   new Spec("nuclear_waste","DSG_AMMO_RESERVE_4","核废料","Nuclear waste",500)
  };
  static string Pick(string zh,string en)=>string.Equals(LocHelper.GetCurrentLocaleCode(),"en",StringComparison.OrdinalIgnoreCase)?en:zh;
  internal static int Current(GameItem machine,string key) {
   if(machine?.state?.dict==null)return -1;
   if(!machine.state.dict.TryGetValue(key,out var state))return 0;
   return state!=null && state.IsEnabled()?state.GetInt():-1;
  }
  internal static bool CanDeposit(int value,int amount)=>amount>0 && amount<=Limit && value>=0 && value<=Limit-amount;
  internal static MachineryPanel Create(GameItem machine,GameSlotInventory slot,Spec spec,Action changed) {
   slot.identifier="ammomachine.reserve."+spec.Tag;
   slot.EnableTag("NO_HOPPER_INVENTORY_TAG");
   slot.maxSlotAddItemFunc=(item,inv,marker)=>slot.childItem==null?1:0;
   slot.mayInventoryAddItemFunc=(item,inv)=>Entry.IsActive && ReferenceEquals(inv,slot) && item!=null && item.identifier==spec.Id && GeneralHelper.IsItemOwned(item) && item.unitCount==1 && CanDeposit(Current(machine,spec.Tag),spec.Amount);
   Action<GameItem> schedule=item=> {
    try { LetterboxManager.RunNextUpdate(()=> {
     try { Convert(machine,slot,item,spec); }
     catch(Exception ex) { Debug.LogWarning("[AmmoMachine] Deposit failed: "+ex.Message); }
     finally { try{changed?.Invoke();}catch(Exception ex){Debug.LogWarning("[AmmoMachine] Refresh failed: "+ex.Message);} }
    }); } catch(Exception ex) { Debug.LogWarning("[AmmoMachine] Scheduling failed: "+ex.Message); }
   };
   slot.onSlotAddItemEndFunc+=(item,inv,marker)=>schedule(item);
   slot.onSlotChangeAmountItemFunc+=(item,inv,marker)=>schedule(item);
   return MachineHelper.Panel(Pick(spec.Zh,spec.En),slot,"reserve_"+spec.Tag);
  }
  internal static void RefreshPending(GameSlotInventory[] slots,Action changed) {
   changed?.Invoke();
   foreach(var slot in slots) if(slot.childItem!=null)slot.onSlotChangeAmountItemFunc?.Invoke(slot.childItem,slot,null);
  }
  static void Convert(GameItem machine,GameSlotInventory slot,GameItem item,Spec spec) {
   if(!Entry.IsActive || machine==null || slot==null || item==null || (machine is GameItemElement m && m.IsDestroyed()) || (item is GameItemElement i && i.IsDestroyed()))return;
   if(!ReferenceEquals(slot.childItem,item) || !ReferenceEquals(item.parentInventory,slot) || !ReferenceEquals(GraphUtils.FindParentType<GameItem>(slot),machine))return;
   if(item.identifier!=spec.Id || !GeneralHelper.IsItemOwned(item) || item.unitCount!=1)return;
   int old=Current(machine,spec.Tag); if(!CanDeposit(old,spec.Amount))return;
   var tag=machine.state.GetTag(spec.Tag); if(tag==null || !slot.Expel(item))return;
   tag.Enable().SetInt(old+spec.Amount);
   try{item.Destroy();}finally{try{machine.ValidateShapeState();}catch(Exception e){Debug.LogWarning("[AmmoMachine] Validate failed: "+e.Message);}}
  }
 }
}
