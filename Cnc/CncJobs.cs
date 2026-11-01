using System;
using UnityEngine;

namespace GunsmithMachineIntegration
{
    // Job state is persisted as enabled GameItem tags; output is created one item
    // at a time, retaining the remaining count when blocked or unavailable.
    public static class CncJobs
    {
        private const string MT="DSG_CNC_RESERVE_METAL", ET="DSG_CNC_RESERVE_ELECTRONICS", PT="DSG_CNC_RESERVE_PLASTIC";
        private const string ID="DSG_CNC_JOB_ID", COUNT="DSG_CNC_JOB_REMAINING", NIGHTS="DSG_CNC_JOB_NIGHTS", DAY="DSG_CNC_JOB_LAST_DAY";
        private const int PowerPerNight=8;
        private static readonly System.Collections.Generic.HashSet<GameItem> Delivering=new System.Collections.Generic.HashSet<GameItem>();
        private static readonly System.Collections.Generic.HashSet<GameItem> NightRunning=new System.Collections.Generic.HashSet<GameItem>();
        private static int Read(GameItem m,string key) { if(m?.state?.dict==null)return -1; return !m.state.dict.TryGetValue(key,out var tag)?0:tag!=null && tag.IsEnabled()?tag.GetInt():-1; }
        private static string ReadId(GameItem m) { if(m?.state?.dict==null || !m.state.dict.TryGetValue(ID,out var tag) || tag==null || !tag.IsEnabled())return ""; return tag.GetString() ?? ""; }
        public static int Remaining(GameItem m)=>Read(m,COUNT);
        public static int NightsLeft(GameItem m)=>Read(m,NIGHTS);
        public static string CurrentId(GameItem m)=>ReadId(m);
        public static bool TryStart(GameItem machine,CncRecipe recipe,int quantity,out string error)
        {
            error="";
            if(machine==null || !GeneralHelper.IsItemOwned(machine) || recipe==null || RecipeCatalog.Find(recipe.Id)!=recipe || quantity<1 || quantity>99) { error="invalid"; return false; }
            if(!CncReserveStandalone.CncSafety.SafeForStart(machine)) {error="placement";return false;}
            if(Remaining(machine)!=0 || NightsLeft(machine)!=0 || !string.IsNullOrEmpty(CurrentId(machine))) { error="busy"; return false; }
            if(!DirectoryMaster.Has<GameItem>(recipe.Id)) { error="missing"; return false; }
            int mc=RecipeCatalog.Cost(recipe.Metal,quantity), ec=RecipeCatalog.Cost(recipe.Electronics,quantity), pc=RecipeCatalog.Cost(recipe.Plastic,quantity);
            int metal=Read(machine,MT), elec=Read(machine,ET), plastic=Read(machine,PT);
            if(metal<mc || elec<ec || plastic<pc || metal>ReservePolicy.Limit || elec>ReservePolicy.Limit || plastic>ReservePolicy.Limit) { error="materials"; return false; }
            // Material + metadata values are pre-created before mutation; then one
            // synchronous, no-yield state update. Nuclear reserve is intentionally unused.
            var tM=machine.state.GetTag(MT);var tE=machine.state.GetTag(ET);var tP=machine.state.GetTag(PT);
            var tI=machine.state.GetTag(ID);var tC=machine.state.GetTag(COUNT);var tN=machine.state.GetTag(NIGHTS);
            tM.Enable().SetInt(metal-mc);tE.Enable().SetInt(elec-ec);tP.Enable().SetInt(plastic-pc);
            tI.Enable().SetString(recipe.Id);tC.Enable().SetInt(quantity);tN.Enable().SetInt(RecipeCatalog.Nights(recipe,quantity));
            Refresh(machine); return true;
        }
        public static void OnNight(GameItem machine,GameSlotInventory battery,GameGridInventory output,Action<bool> allowOutput,Action changed)
        {
            if(machine==null || !GeneralHelper.IsItemOwned(machine) || Remaining(machine)<=0)return;
            if(!NightRunning.Add(machine))return;
            try {
                // A missing/disabled item definition must never consume energy.
                if(RecipeCatalog.Find(CurrentId(machine))==null || !DirectoryMaster.Has<GameItem>(CurrentId(machine)))return;
                int nights=NightsLeft(machine);
                if(nights<0 || string.IsNullOrEmpty(CurrentId(machine)))return;
                if(nights>0)
                {
                    if(StoreStation.Instance==null)return;
                    int day=StoreStation.GetDayCounter();
                    if(machine.state.dict.TryGetValue(DAY,out var d) && d!=null && d.IsEnabled() && d.GetInt()==day)return;
                    if(battery?.childItem==null || !PowerHelper.CanDrawPowerSource(battery.childItem,PowerPerNight))return;
                    if(!PowerHelper.DrawPowerSource(battery.childItem,PowerPerNight))return;
                    // These writes follow the draw: a throwing power API or process crash
                    // cannot be made atomic with the power source's own mutation.
                    machine.state.GetTag(DAY).Enable().SetInt(day);
                    machine.state.GetTag(NIGHTS).Enable().SetInt(nights-1);
                    Refresh(machine); changed?.Invoke();
                }
                if(NightsLeft(machine)==0) Deliver(machine,output,allowOutput,changed);
            } finally {NightRunning.Remove(machine);}
        }
        public static void Deliver(GameItem machine,GameGridInventory output,Action<bool> allowOutput,Action changed)
        {
            if(machine==null || !GeneralHelper.IsItemOwned(machine) || output==null || allowOutput==null || NightsLeft(machine)!=0 || Remaining(machine)<=0)return;
            string id=CurrentId(machine);
            if(RecipeCatalog.Find(id)==null || !DirectoryMaster.Has<GameItem>(id))return;
            if(!Delivering.Add(machine))return;
            try {
                while(Remaining(machine)>0)
                {
                    GameItem product=null, target=null; int before=0; bool committed=false;
                    try {
                        product=DirectoryMaster.Item(id);
                        if(product==null || product.identifier!=id || product.unitCount!=1) {product?.Destroy();product=null;return;}
                        ModHelper.SetItemOwnerShip(product,true);
                        allowOutput(true);
                        var slot=output.TryFindOneValidInventorySlot(product);
                        if(slot==null || slot.numTransfer<1) {product.Destroy();product=null;return;}
                        target=slot.targetItem; before=target?.unitCount ?? 0;
                        int accepted=slot.TryAcceptOnce(1);
                        bool inserted=product.parentInventory==output || (target!=null && target.unitCount==before+1);
                        if(!inserted) {
                            if(product.parentInventory==null)product.Destroy();
                            product=null;
                            if(accepted>0)Debug.LogWarning("[CncReserveTest] Transfer reported success without observable output.");
                            return;
                        }
                        // Observable placement wins even if an inconsistent API returned zero.
                        int left=Remaining(machine)-1;
                        machine.state.GetTag(COUNT).Enable().SetInt(left); committed=true;
                        if(left==0) {machine.state.GetTag(ID).Enable().SetString("");machine.state.GetTag(NIGHTS).Enable().SetInt(0);}
                        if(product.parentInventory==null)product.Destroy();
                        product=null;
                    } catch(Exception ex) {
                        bool inserted=product!=null && (product.parentInventory==output || (target!=null && target.unitCount==before+1));
                        if(inserted && !committed) {int left=Math.Max(0,Remaining(machine)-1);machine.state.GetTag(COUNT).Enable().SetInt(left);if(left==0) {machine.state.GetTag(ID).Enable().SetString("");machine.state.GetTag(NIGHTS).Enable().SetInt(0);}}
                        if(product!=null && product.parentInventory==null)product.Destroy();
                        Debug.LogWarning("[CncReserveTest] Output paused: "+ex.Message);return;
                    } finally {allowOutput(false);}
                }
                Refresh(machine);changed?.Invoke();
            } finally {try {allowOutput(false);} finally {Delivering.Remove(machine);}}
        }
        private static void Refresh(GameItem machine) { try {machine.ValidateShapeState();}catch(Exception ex){Debug.LogWarning("[CncReserveTest] UI refresh: "+ex.Message);} }
    }
}
