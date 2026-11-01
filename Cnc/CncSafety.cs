using System;
using System.Collections.Generic;
using UnityEngine;

namespace CncReserveStandalone
{
    // The vanilla night cycle deletes every top-level item on the customer counter.
    // Prevent machine placement anywhere beneath that counter; do not touch other items.
    public static class CncSafety
    {
        public static bool IsCustomerCounter(GraphNodeStorage node)
        {
            var counter=EmporiumEntry.Instance?.frontInvinvElement;
            if(node==null || counter==null)return false;
            var queue=new Queue<GraphNodeStorage>();var seen=new HashSet<GraphNodeStorage>();queue.Enqueue(node);
            while(queue.Count>0 && seen.Count<1024)
            {
                var current=queue.Dequeue(); if(current==null || !seen.Add(current))continue;
                if(ReferenceEquals(current,counter))return true;
                var parents=current.parents;if(parents==null)return true; // unknown: fail closed
                foreach(var parent in parents)if(parent!=null)queue.Enqueue(parent);
            }
            return queue.Count>0 || seen.Count>=1024; // malformed graph: do not risk placing it there
        }
        public static bool SafeForStart(GameItem machine) => machine!=null && machine.parentInventory!=null && !IsCustomerCounter(machine.parentInventory);
        public static void RestoreExisting()
        {
            if(!Entry.IsActive || !PlayerStore.HasInstance || EmporiumEntry.Instance?.frontInvinvElement==null)return;
            // For new test instances accidentally left on the counter before this
            // update, move only this machine (and its owned contents) to inventory.
            var counter=EmporiumEntry.Instance.frontInvinvElement;
            foreach(var machine in new List<GameItem>(counter.childItems))
            {
                if(machine==null || machine.identifier!=Entry.MachineId || !GeneralHelper.IsItemOwned(machine))continue;
                if(EmporiumEntry.Instance.invElement!=null && GraphUtils.CanAccept(EmporiumEntry.Instance.invElement,machine))
                {
                    GraphUtils.TryAcceptAll(EmporiumEntry.Instance.invElement,machine,-1);
                    if(!IsCustomerCounter(machine.parentInventory))continue;
                }
                Debug.LogWarning("[CncReserveTest] Machine remains on customer counter; move it to your inventory before sleeping. No deletion was forced by the mod.");
            }
        }
    }
}
