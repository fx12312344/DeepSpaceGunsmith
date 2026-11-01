using System;
using UnityEngine;
using GunsmithMachineIntegration;

namespace CncReserveStandalone
{
    internal sealed class Entry
    {
        public const string ModId=ColdWarModernExpansion.Entry.ModId;
        private const string DebugId="local.dsg.cnc.machine";
        public const string MachineId="cncreserveretest.machine";
        private const string MachineSpriteKey="cnc_machine_merged";
        private const float MachinePixelsPerUnit=800f;
        private bool hooked;
        internal static bool IsActive { get; private set; }
        private ModLog log;
        public void Init(ModManifest manifest) { log=new ModLog(manifest); }
        public void OnEnable()
        {
            if(hooked)return;
            ModHook.OnModItemDirectoryInit+=Register;
            ModHook.OnGoingSleepEarly+=CncSafety.RestoreExisting;
            ModHelper.AddDebugFunction(DebugId,ModHelper.DEBUG_CATEGORY_SPAWN,"Gunsmith's Multipurpose Workbench",Spawn,false);
            hooked=true;
            IsActive=true;
            foreach(var dir in Resources.FindObjectsOfTypeAll<ModItemDirectory>()) if(dir!=null && dir.factoryDictionary!=null) Register(dir);
        }
        public void OnDisable()
        {
            if(!hooked)return;
            IsActive=false;
            ModHook.OnModItemDirectoryInit-=Register;
            ModHook.OnGoingSleepEarly-=CncSafety.RestoreExisting;
            ModHelper.RemoveDebugFunctions(DebugId);
            hooked=false;
        }
        private void Register(ModItemDirectory dir)
        {
            if(dir==null || dir.factoryDictionary==null || DirectoryMaster.Has<GameItem>(MachineId) || dir.Has(MachineId)) return;
            if(!dir.Add(MachineId,MakeMachine)) log.Error("Could not register CNC workbench.");
        }
        private void Spawn()
        {
            if(!PlayerStore.HasInstance || EmporiumEntry.Instance==null || EmporiumEntry.Instance.invElement==null || !DirectoryMaster.Has<GameItem>(MachineId)) {log.Warning("Load a store before spawning the CNC workbench.");return;}
            var item=ModHelper.SpawnItem(MachineId);
            if(item==null)return;
            ModHelper.SetItemOwnerShip(item,true);
            GraphUtils.TryAcceptAll(EmporiumEntry.Instance.invElement,item,-1);
            if(item.parentInventory==null){item.Destroy();log.Warning("Inventory full; CNC workbench was not spawned.");}
        }
        private static GameItem MakeMachine()
        {
            var machine=ItemDirectory.CreateEmptyItem(null);
            var battery=new GameSlotInventory(); var metal=new GameSlotInventory(); var electronics=new GameSlotInventory(); var plastic=new GameSlotInventory(); var nuclear=new GameSlotInventory();
            var output=new GameGridInventory(6,8); bool allowOutput=false; output.mayInventoryAddItemFunc=(item,inv)=>allowOutput;
            var reserves=CncReserveInputs.Create(machine,metal,electronics,plastic,nuclear);
            // Battery remains the first graph storage, as expected by vanilla helpers.
            var inputStack=MachineryStack.Vertical(MachineHelper.PowerPanel(battery),reserves.Metal,reserves.Electronics,reserves.Plastic,reserves.Nuclear);
            var metalValue=new MachineryText(); var electronicsValue=new MachineryText(); var plasticValue=new MachineryText(); var nuclearValue=new MachineryText();
            var metalBar=new MachineryProgressBar(0f,100); var electronicsBar=new MachineryProgressBar(0f,100); var plasticBar=new MachineryProgressBar(0f,100); var nuclearBar=new MachineryProgressBar(0f,100);
            var recipeName=new MachineryText(); var recipeCost=new MachineryText(); var recipeTime=new MachineryText(); var recipeStatus=new MachineryText(); var recipeNote=new MachineryText();
            int recipeIndex=0, quantity=1;
            Action refreshRecipe=null;
            var recipeList=new CncRecipeList(index=>{recipeIndex=index;refreshRecipe?.Invoke();});
            var minus=new MachineryButton("−",()=>{quantity=Math.Max(1,quantity-1);refreshRecipe?.Invoke();});
            var plus=new MachineryButton("+",()=>{quantity=Math.Min(99,quantity+1);refreshRecipe?.Invoke();});
            var start=new MachineryButton(EnglishLocalization.L.Pick("开始制造","Start"));
            refreshRecipe=()=> {
                var recipe=RecipeCatalog.All[recipeIndex];
                int left=CncJobs.Remaining(machine), nights=CncJobs.NightsLeft(machine);
                recipeName.SetText(recipe.Name+" × "+quantity);
                recipeNote.SetText(recipe.Id=="gun_part" ? EnglishLocalization.L.Pick("将金属储量加工为枪械零件。","Machine metal reserves into gun parts.") : "");
                recipeCost.SetText(EnglishLocalization.L.Pick("金/电/料: ","M/E/P: ")+RecipeCatalog.Cost(recipe.Metal,quantity)+" / "+RecipeCatalog.Cost(recipe.Electronics,quantity)+" / "+RecipeCatalog.Cost(recipe.Plastic,quantity));
                recipeTime.SetText(EnglishLocalization.L.Pick("所需晚数: ","Nights: ")+RecipeCatalog.Nights(recipe,quantity)+EnglishLocalization.L.Pick("  电/晚: 8","  Power/night: 8"));
                bool available=DirectoryMaster.Has<GameItem>(recipe.Id);
                string job=left>0 ? (nights>0 ? EnglishLocalization.L.Pick("加工中，剩余晚数: ","Working; nights left: ")+nights : EnglishLocalization.L.Pick("待取出，剩余: ","Awaiting output: ")+left) : (available ? EnglishLocalization.L.Pick("就绪","Ready") : EnglishLocalization.L.Pick("缺少主模组配件","Main mod item unavailable"));
                recipeStatus.SetText(job);
            };
            start.SetCallback(()=> { if(!IsActive) { recipeStatus.SetText(EnglishLocalization.L.Pick("模组已停用","Mod disabled")); return; } if(!CncJobs.TryStart(machine,RecipeCatalog.All[recipeIndex],quantity,out var error)) recipeStatus.SetText(EnglishLocalization.L.Pick("无法开工：","Cannot start: ")+EnglishLocalization.L.Pick(error=="materials"?"材料不足":error=="missing"?"物品未注册":error=="busy"?"机器忙碌":error=="placement"?"请移至自己的物品栏再开工":"参数错误",error)); else { reserves.Refresh(); refreshRecipe(); } });
            var recipeControls=MachineryStack.Vertical(recipeList,recipeName,MachineryStack.Horizontal(minus,plus),recipeCost,recipeTime,recipeNote,start,recipeStatus);
            var controlStack=MachineryStack.Vertical(metalValue,metalBar,electronicsValue,electronicsBar,plasticValue,plasticBar,nuclearValue,nuclearBar);
            reserves.Changed=()=> {
                string Format(int value)=>value<0 ? "?" : value.ToString();
                int m=CncReserveInputs.Metal(machine), e=CncReserveInputs.Electronics(machine), p=CncReserveInputs.Plastic(machine), n=CncReserveInputs.Nuclear(machine);
                string cap=" / "+ReservePolicy.Limit;
                metalValue.SetText(EnglishLocalization.L.Pick("金属：","Metal: ")+Format(m)+cap);
                electronicsValue.SetText(EnglishLocalization.L.Pick("电子元件：","Electronics: ")+Format(e)+cap);
                plasticValue.SetText(EnglishLocalization.L.Pick("打印耗材：","Printer material: ")+Format(p)+cap);
                nuclearValue.SetText(EnglishLocalization.L.Pick("核废料：","Nuclear waste: ")+Format(n)+cap);
                metalBar.SetValue(m < 0 ? 0 : Mathf.Clamp01((float)m / ReservePolicy.Limit));
                electronicsBar.SetValue(e < 0 ? 0 : Mathf.Clamp01((float)e / ReservePolicy.Limit));
                plasticBar.SetValue(p < 0 ? 0 : Mathf.Clamp01((float)p / ReservePolicy.Limit));
                nuclearBar.SetValue(n < 0 ? 0 : Mathf.Clamp01((float)n / ReservePolicy.Limit));
            };
            var inputPanel=new MachineryPanel(EnglishLocalization.L.Pick("投料口","INPUT"),inputStack,"inputs");
            var outputPanel=MachineHelper.Panel(EnglishLocalization.L.Pick("成品取出","OUTPUT"),output,"output");
            var controlPanel=new MachineryPanel(EnglishLocalization.L.Pick("材料储备","MATERIAL RESERVES"),controlStack,"control");
            var recipePanel=new MachineryPanel(EnglishLocalization.L.Pick("配方选择","RECIPE SELECTION"),recipeControls,"recipes");
            var window=MachineHelper.CreateMachineryWindow(true,inputPanel,outputPanel,controlPanel,recipePanel);
            machine.SetContentWindow(window);
            window.openCallback+=()=>{if(!IsActive)return; reserves.Refresh(); CncJobs.Deliver(machine,output,value=>allowOutput=value,refreshRecipe); refreshRecipe();};
            output.onSlotRemoveItemEndFunc+=(item,inv,marker)=>LetterboxManager.RunNextUpdate(()=>{if(IsActive) { CncJobs.Deliver(machine,output,value=>allowOutput=value,refreshRecipe); refreshRecipe(); }});
            machine.onCycleEndSlotItemFunc+=(item,inv,marker)=>{if(IsActive) CncJobs.OnNight(machine,battery,output,value=>allowOutput=value,refreshRecipe);};
            refreshRecipe();
            foreach(var mod in ModLoader.Mods)
            {
                if(mod?.Manifest?.ID!=ModId)continue;
                // This 6x4 canvas belongs only to the merged CNC; do not mutate
                // the separate gunsmith_machine sprite.
                if(mod.Sprites.TryGetValue(MachineSpriteKey,out var original) && original!=null && original.pixelsPerUnit!=MachinePixelsPerUnit)
                {
                    var scaled=Sprite.Create(original.texture,original.rect,new Vector2(0.5f,0.5f),MachinePixelsPerUnit);scaled.name=original.name;mod.Sprites[MachineSpriteKey]=scaled;
                }
                break;
            }
            machine.SetSpriteAndShapeFromMod(ModId,MachineSpriteKey);
            // Match the storage-box fix: hit the full image rectangle, including
            // transparent pixels. Runs for newly created and save-rebuilt items.
            if(machine is GameItemElement visual) {
                if(visual.background?.image!=null) visual.background.image.alphaHitTestMinimumThreshold=0f;
                if(visual.fakeBackground?.image!=null) visual.fakeBackground.image.alphaHitTestMinimumThreshold=0f;
            }
            // Latest size convention: length 6, height 4 (128 pixels per cell).
            machine.SetShape(new GridShapeBuilder().SetDataFill(6,4,1));
            machine.name=EnglishLocalization.L.Pick("枪匠万能机床","Gunsmith's Multipurpose Workbench");
            machine.shortDescription=UtilityItems.UtilityItemText.CncDescription;
            machine.unitValue=1500;machine.EnableTag("CONTAINER_TAG");MachineHelper.SetupBatterySlot(battery,machine);
            // Customer-facing counter is erased by vanilla at night.
            var originalPlacement=machine.mayInventoryAddItemFunc;
            machine.mayInventoryAddItemFunc=(candidate,inv)=>(!GeneralHelper.IsItemOwned(candidate) || !CncSafety.IsCustomerCounter(inv)) && (originalPlacement==null || originalPlacement.InvokeAllReduce(candidate,inv,InvokeFuncExtensions.BoolReduceOR));
            return machine;
        }
    }
}
