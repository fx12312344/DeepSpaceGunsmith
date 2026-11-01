using System;
using System.Runtime.CompilerServices;
using HarmonyLib;

namespace MachineMaterialDeposit
{
    // Multi-select uses a synchronous foreach over items. Drain the completed
    // material slot only after the entire vanilla TryAcceptAll call returns,
    // never inside UncheckedAccept's add/validation callbacks.
    internal static class BulkMaterialDeposit
    {
        private sealed class Receiver
        {
            internal Action Flush;
            internal Func<bool> Active;
            internal bool Running;
        }
        private static ConditionalWeakTable<GameItem, Receiver> receivers = new ConditionalWeakTable<GameItem, Receiver>();
        [ThreadStatic] private static int groupDepth;

        internal static void Register(GameItem machine, Func<bool> active, Action flush)
        {
            if (machine == null || active == null || flush == null) throw new ArgumentNullException();
            if (machine.identifier != null && machine.identifier.Length != 0 &&
                machine.identifier != "cncreserveretest.machine" && machine.identifier != "ammomachinetest.machine")
                throw new ArgumentException("Not a material workbench.");
            receivers.Remove(machine);
            receivers.Add(machine, new Receiver { Active=active, Flush=flush });
        }
        private static void EnterGroup(out int __state)
        {
            __state=groupDepth;
            if (groupDepth < int.MaxValue) groupDepth++;
        }
        private static Exception LeaveGroup(Exception __exception, int __state)
        {
            groupDepth=__state;
            return __exception;
        }
        private static void AfterAccept(GraphNodeStorage __0, GameItem __1, int __result)
        {
            if(groupDepth<=0 || __result!=0 || __1==null || __1.unitCount!=1 || !(__0 is GameItem machine) || !receivers.TryGetValue(machine,out var receiver) || receiver.Running)return;
            // Only drain the successfully inserted single material, not an unrelated
            // nested acceptance or a previously pending slot after a failed move.
            if(__1 is GameItemElement inputElement && inputElement.IsDestroyed())return;
            if(!(__1.parentInventory is GameSlotInventory input) || !ReferenceEquals(input.childItem,__1) || !ReferenceEquals(GraphUtils.FindParentType<GameItem>(input),machine))return;
            if(input.identifier==null || !(input.identifier.StartsWith("gunsmithmachine.reserve.DSG_CNC_RESERVE_",StringComparison.Ordinal) || input.identifier.StartsWith("ammomachine.reserve.DSG_AMMO_RESERVE_",StringComparison.Ordinal)))return;
            if(!ColdWarModernExpansion.Entry.IsActive || !receiver.Active() || !GeneralHelper.IsItemOwned(machine))return;
            if(machine is GameItemElement element && element.IsDestroyed())return;
            receiver.Running=true;
            try { receiver.Flush(); }
            catch(Exception ex) { UnityEngine.Debug.LogWarning("[DeepSpaceGunsmith] Group material conversion paused; unconverted items remain in their slots: "+ex.Message); }
            finally { receiver.Running=false; }
        }

        [HarmonyPatch(typeof(ItemMultiSelectHandler), "EndGroupDrag")]
        private static class GroupDragScope
        {
            private static void Prefix(out int __state) { EnterGroup(out __state); }
            private static Exception Finalizer(Exception __exception,int __state) { return LeaveGroup(__exception,__state); }
        }
        [HarmonyPatch(typeof(GraphUtils), nameof(GraphUtils.TryAcceptAll), new[] { typeof(GraphNodeStorage), typeof(GameItem), typeof(int) })]
        private static class AcceptCompletion
        {
            private static void Postfix(GraphNodeStorage __0, GameItem __1, int __result) { AfterAccept(__0, __1, __result); }
        }
        [HarmonyPatch(typeof(ColdWarModernExpansion.Entry), nameof(ColdWarModernExpansion.Entry.OnDisable))]
        private static class ResetReceivers
        {
            // Do not clear live receivers: they are needed if the same loaded
            // machine is re-enabled. ConditionalWeakTable releases dead machines.
            private static void Postfix() { groupDepth=0; }
        }
    }
}
