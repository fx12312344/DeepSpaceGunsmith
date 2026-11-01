using System;
using UnityEngine;

namespace GunsmithMachineIntegration
{
    public static class CncReserveInputs
    {
        public sealed class Panels
        {
            public MachineryPanel Metal, Electronics, Plastic, Nuclear;
            public Action Refresh;
            public Action Changed;
        }
        private const string MetalTag="DSG_CNC_RESERVE_METAL";
        private const string ElectronicsTag="DSG_CNC_RESERVE_ELECTRONICS";
        private const string PlasticTag="DSG_CNC_RESERVE_PLASTIC";
        private const string NuclearTag="DSG_CNC_RESERVE_NUCLEAR_WASTE";
        private const string PurityTag="INGOT_PURITY_TAG";
        public static int Metal(GameItem machine) => Current(machine, MetalTag);
        public static int Electronics(GameItem machine) => Current(machine, ElectronicsTag);
        public static int Plastic(GameItem machine) => Current(machine, PlasticTag);
        public static int Nuclear(GameItem machine) => Current(machine, NuclearTag);
        private static int Current(GameItem machine, string key)
        {
            if (machine.state == null || machine.state.dict == null) return -1;
            if (!machine.state.dict.TryGetValue(key, out var state)) return 0;
            return state != null && state.IsEnabled() ? state.GetInt() : -1;
        }
        private static int PerItem(GameItem item, string material)
        {
            if (item == null || item.identifier != material || !GeneralHelper.IsItemOwned(item) || item.unitCount <= 0) return 0;
            if (material == "common_electronic") return ReservePolicy.ElectronicPerItem;
            if (material == "printer_plastic") return ReservePolicy.PlasticPerItem;
            if (material == "nuclear_waste") return ReservePolicy.NuclearWastePerItem;
            if (material != "metal_ingot" || item.state == null || item.state.dict == null || !item.state.dict.TryGetValue(PurityTag, out var purity) || purity == null || !purity.IsEnabled()) return 0;
            return ReservePolicy.MetalPerIngot(purity.GetString());
        }
        public static Panels Create(GameItem machine, GameSlotInventory metal, GameSlotInventory electronics, GameSlotInventory plastic, GameSlotInventory nuclear)
        {
            var result = new Panels();
            Action changed=()=>result.Changed?.Invoke();
            result.Metal = CreatePanel(machine, metal, "metal_ingot", MetalTag, "金属锭", "Metal ingot", changed);
            result.Electronics = CreatePanel(machine, electronics, "common_electronic", ElectronicsTag, "电子元件", "Electronics", changed);
            result.Plastic = CreatePanel(machine, plastic, "printer_plastic", PlasticTag, "打印耗材", "Printer material", changed);
            result.Nuclear = CreatePanel(machine, nuclear, "nuclear_waste", NuclearTag, "核废料", "Nuclear waste", changed);
            result.Refresh=()=> {
                result.Changed?.Invoke();
                if (metal.childItem != null) metal.onSlotChangeAmountItemFunc?.Invoke(metal.childItem,metal,null);
                if (electronics.childItem != null) electronics.onSlotChangeAmountItemFunc?.Invoke(electronics.childItem,electronics,null);
                if (plastic.childItem != null) plastic.onSlotChangeAmountItemFunc?.Invoke(plastic.childItem,plastic,null);
                if (nuclear.childItem != null) nuclear.onSlotChangeAmountItemFunc?.Invoke(nuclear.childItem,nuclear,null);
            };
            return result;
        }
        private static MachineryPanel CreatePanel(GameItem machine, GameSlotInventory slot, string material, string tag, string zh, string en, Action changed)
        {
            slot.identifier="gunsmithmachine.reserve."+tag;
            slot.EnableTag("NO_HOPPER_INVENTORY_TAG");
            MachineryPanel panel=null;
            Action refresh=()=>changed?.Invoke();
            // Never stack into a pending slot; the engine stack path bypasses
            // onSlotAddItemEndFunc. Also prevent hopper-style inventory injection.
            slot.maxSlotAddItemFunc=(item,inv,marker)=> slot.childItem == null ? 1 : 0;
            slot.mayInventoryAddItemFunc=(item,inv)=> {
                int rate=PerItem(item,material);
                // The vanilla split-stack path recreates an item and can discard per-item
                // purity. Reject stacks here; the player can split them outside first.
                return CncReserveStandalone.Entry.IsActive && ReferenceEquals(inv,slot) && item.unitCount==1 && rate>0 && ReservePolicy.TryDeposit(Current(machine,tag),rate,item.unitCount,out _);
            };
            Action<GameItem> schedule=item=> {
                // Destroying an item inside UncheckedAccept breaks the game's remaining
                // validation. Convert next frame, after the completed inventory transfer.
                try { LetterboxManager.RunNextUpdate(()=> {
                    try { Convert(machine,slot,item,material,tag); }
                    catch(Exception ex) { Debug.LogWarning("[DeepSpaceGunsmith] Reserve deposit failed; item remains in slot: "+ex.Message); }
                    finally { try { refresh(); } catch(Exception ex) { Debug.LogWarning("[DeepSpaceGunsmith] Reserve display refresh failed: "+ex.Message); } }
                }); } catch(Exception ex) { Debug.LogWarning("[DeepSpaceGunsmith] Reserve scheduling failed; item remains in slot: "+ex.Message); }
            };
            slot.onSlotAddItemEndFunc+=(item,inv,marker)=>schedule(item);
            // Save loading may restore an interrupted pending input through unchecked
            // insertion. Reopening gives it another safe conversion opportunity.
            slot.onSlotChangeAmountItemFunc+=(item,inv,marker)=>schedule(item);
            panel=MachineHelper.Panel(EnglishLocalization.L.Pick(zh,en),slot,"reserve_"+tag);
            refresh();
            return panel;
        }
        private static void Convert(GameItem machine, GameSlotInventory slot, GameItem item, string material, string key)
        {
            if (!CncReserveStandalone.Entry.IsActive || machine == null || slot == null || item == null || (machine is GameItemElement m && m.IsDestroyed()) || (item is GameItemElement i && i.IsDestroyed())) return;
            if (!ReferenceEquals(slot.childItem,item) || !ReferenceEquals(item.parentInventory,slot) || !ReferenceEquals(GraphUtils.FindParentType<GameItem>(slot),machine)) return;
            int rate=PerItem(item,material); int old=Current(machine,key);
            if (!ReservePolicy.TryDeposit(old,rate,item.unitCount,out var next)) return;
            // Prepare the tag while the material remains intact. Once Expel succeeds,
            // mutate the tag directly; then destroy the unparented item. No yielding or
            // validation callbacks between credit and removal.
            var tag=machine.state.GetTag(key);
            if (tag == null) return;
            if (!slot.Expel(item)) return;
            tag.Enable().SetInt(next);
            try { item.Destroy(); }
            finally { try { machine.ValidateShapeState(); } catch(Exception e) { Debug.LogWarning("[DeepSpaceGunsmith] Reserve refresh failed: "+e.Message); } }
        }
    }
}
