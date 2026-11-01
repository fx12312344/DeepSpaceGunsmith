using System;
using UnityEngine;
namespace AmmoMachineTest {
 internal static class AmmoJobs {
  const string ID="DSG_AMMO_JOB_ID", COUNT="DSG_AMMO_JOB_REMAINING", NIGHTS="DSG_AMMO_JOB_NIGHTS", DAY="DSG_AMMO_JOB_LAST_DAY";
  const int Power=8;
  static readonly System.Collections.Generic.HashSet<GameItem> Delivering=new System.Collections.Generic.HashSet<GameItem>();
  static int Read(GameItem m,string key) { if(m?.state?.dict==null)return -1; return !m.state.dict.TryGetValue(key,out var t)?0:t!=null&&t.IsEnabled()?t.GetInt():-1; }
  internal static int Remaining(GameItem m)=>Read(m,COUNT);
  internal static int NightsLeft(GameItem m)=>Read(m,NIGHTS);
  internal static string CurrentId(GameItem m)=>m?.state?.dict!=null && m.state.dict.TryGetValue(ID,out var t) && t!=null && t.IsEnabled()?t.GetString()??"":"";
  internal static bool TryStart(GameItem m,AmmoRecipe r,int q,out string error) {
   error=""; if(m==null||!GeneralHelper.IsItemOwned(m)||r==null||AmmoRecipes.Find(r.Id)!=r||q<1||q>99){error="invalid";return false;}
   if(!AmmoSafety.SafeForStart(m)){error="placement";return false;}
   if(Remaining(m)!=0||NightsLeft(m)!=0||CurrentId(m)!=""){error="busy";return false;}
   if(!DirectoryMaster.Has<GameItem>(r.Id)){error="missing";return false;}
   int a=AmmoReserveInputs.Current(m,AmmoReserveInputs.Specs[0].Tag),b=AmmoReserveInputs.Current(m,AmmoReserveInputs.Specs[1].Tag),c=AmmoReserveInputs.Current(m,AmmoReserveInputs.Specs[2].Tag);
   if(a<r.Powder*q||b<r.Cases*q||c<r.Projectiles*q||a>5000||b>5000||c>5000){error="materials";return false;}
   var ta=m.state.GetTag(AmmoReserveInputs.Specs[0].Tag);var tb=m.state.GetTag(AmmoReserveInputs.Specs[1].Tag);var tc=m.state.GetTag(AmmoReserveInputs.Specs[2].Tag);
   var ti=m.state.GetTag(ID);var tq=m.state.GetTag(COUNT);var tn=m.state.GetTag(NIGHTS);
   ta.Enable().SetInt(a-r.Powder*q);tb.Enable().SetInt(b-r.Cases*q);tc.Enable().SetInt(c-r.Projectiles*q);
   ti.Enable().SetString(r.Id);tq.Enable().SetInt(q);tn.Enable().SetInt(1);
   Refresh(m);return true;
  }
  internal static void OnNight(GameItem m,GameSlotInventory battery,GameGridInventory output,Action<bool> allow,Action changed) {
   if(m==null||!GeneralHelper.IsItemOwned(m)||!AmmoSafety.SafeForStart(m)||Remaining(m)<=0||AmmoRecipes.Find(CurrentId(m))==null||!DirectoryMaster.Has<GameItem>(CurrentId(m)))return;
   int n=NightsLeft(m);if(n<0)return;
   if(n>0) {
    if(StoreStation.Instance==null)return;
    int day=StoreStation.GetDayCounter();
    if(m.state.dict.TryGetValue(DAY,out var d) && d!=null && d.IsEnabled() && d.GetInt()==day)return;
    if(battery?.childItem==null||!PowerHelper.CanDrawPowerSource(battery.childItem,Power)||!PowerHelper.DrawPowerSource(battery.childItem,Power))return;
    m.state.GetTag(DAY).Enable().SetInt(day);m.state.GetTag(NIGHTS).Enable().SetInt(n-1);Refresh(m);changed?.Invoke();
   }
   if(NightsLeft(m)==0)Deliver(m,output,allow,changed);
  }
  internal static void Deliver(GameItem m,GameGridInventory output,Action<bool> allow,Action changed) {
   if(m==null||!GeneralHelper.IsItemOwned(m)||output==null||allow==null||NightsLeft(m)!=0||Remaining(m)<=0)return;
   string id=CurrentId(m);if(AmmoRecipes.Find(id)==null||!DirectoryMaster.Has<GameItem>(id))return;
   if(!Delivering.Add(m))return;
   try {
    while(Remaining(m)>0) {
     GameItem product=null, target=null; int before=0; bool committed=false;
     try {
      product=DirectoryMaster.Item(id); if(product==null||product.identifier!=id||product.unitCount!=1){product?.Destroy();return;}
      ModHelper.SetItemOwnerShip(product,true); allow(true);
      var slot=output.TryFindOneValidInventorySlot(product);
      if(slot==null || slot.numTransfer<1){product.Destroy();product=null;return;}
      target=slot.targetItem; before=target?.unitCount ?? 0;
      slot.TryAcceptOnce(1);
      bool inserted=product.parentInventory==output || (target!=null && target.unitCount==before+1);
      if(!inserted){if(product.parentInventory==null)product.Destroy();product=null;return;}
      int left=Remaining(m)-1;m.state.GetTag(COUNT).Enable().SetInt(left);committed=true;
      if(left==0){m.state.GetTag(ID).Enable().SetString("");m.state.GetTag(NIGHTS).Enable().SetInt(0);}
      if(product.parentInventory==null)product.Destroy(); product=null;
     } catch(Exception ex) {
      bool inserted=product!=null && (product.parentInventory==output || (target!=null && target.unitCount==before+1));
      if(inserted && !committed){int left=Math.Max(0,Remaining(m)-1);m.state.GetTag(COUNT).Enable().SetInt(left);if(left==0){m.state.GetTag(ID).Enable().SetString("");m.state.GetTag(NIGHTS).Enable().SetInt(0);}}
      if(product!=null && product.parentInventory==null)product.Destroy();
      Debug.LogWarning("[AmmoMachine] Output paused: "+ex.Message); return;
     } finally {allow(false);}
    }
    Refresh(m);changed?.Invoke();
   } finally {try{allow(false);}finally{Delivering.Remove(m);}}
  }
  static void Refresh(GameItem m){try{m.ValidateShapeState();}catch(Exception e){Debug.LogWarning("[AmmoMachine] UI refresh: "+e.Message);}}
 }
}
