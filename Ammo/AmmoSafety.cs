using System;
using System.Collections.Generic;
using UnityEngine;
namespace AmmoMachineTest {
 internal static class AmmoSafety {
  internal static bool IsCustomerCounter(GraphNodeStorage node) {
   var counter=EmporiumEntry.Instance?.frontInvinvElement;
   if(node==null || counter==null)return true;
   var queue=new Queue<GraphNodeStorage>();var seen=new HashSet<GraphNodeStorage>();queue.Enqueue(node);
   while(queue.Count>0 && seen.Count<1024) {var n=queue.Dequeue();if(n==null || !seen.Add(n))continue;if(ReferenceEquals(n,counter))return true;var parents=n.parents;if(parents==null)return true;foreach(var p in parents)if(p!=null)queue.Enqueue(p);}
   return queue.Count>0 || seen.Count>=1024;
  }
  internal static bool SafeForStart(GameItem item)=>item!=null && item.parentInventory!=null && !IsCustomerCounter(item.parentInventory);
  internal static void RestoreExisting() {
   if(!Entry.IsActive || !PlayerStore.HasInstance || EmporiumEntry.Instance?.frontInvinvElement==null)return;
   var counter=EmporiumEntry.Instance.frontInvinvElement;
   foreach(var m in new List<GameItem>(counter.childItems)) {
    if(m==null || m.identifier!=Entry.MachineId || !GeneralHelper.IsItemOwned(m))continue;
    if(EmporiumEntry.Instance.invElement!=null && GraphUtils.CanAccept(EmporiumEntry.Instance.invElement,m)) {GraphUtils.TryAcceptAll(EmporiumEntry.Instance.invElement,m,-1); if(!IsCustomerCounter(m.parentInventory))continue;}
    Debug.LogWarning("[AmmoMachine] Machine on customer counter may be removed at night; move it to your inventory.");
   }
  }
 }
}
