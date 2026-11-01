namespace UtilityItems
{
    // Display text only. Item values, shapes and inventory rules live in their factories.
    internal static class UtilityItemText
    {
        private static string Pick(string zh, string en) => EnglishLocalization.L.Pick(zh, en);

        internal static string CncDescription => Pick(
            "枪匠的夜班搭档。置于自有物品栏，装入并开启电源；将自有材料拆为单件，可逐件投入或扩选多份拖到机器图标，一起折为储量。每份金属锭按六档纯度由低到高兑换75/85/100/125/150/300，普通电子元件25、打印耗材25、核废料500；各类上限5000，超额不收。选择配方与数量（1–99）后点开始：通常1–2件1晚、3–6件2晚、7件以上3晚，弹壳堆与弹头堆任意批均1晚。每个有效工作夜耗8电，缺电暂停；输出满时余货保留待取。核废料仅作封存储备，不参与加工。",
            "A gunsmith's night-shift partner. Keep it in your own inventory, insert and switch on a power source, then deposit owned materials individually or group-select separate items and drag them onto the machine icon to build reserves. Split stacks first. Each ingot yields 75/85/100/125/150/300 metal, from lowest to highest purity; common electronics and printer material yield 25 each, nuclear waste 500. Each reserve caps at 5000; excess deposits are refused. Select a recipe and quantity (1–99), then Start: normally 1–2 items take 1 night, 3–6 take 2, and 7+ take 3; case and projectile batches always take 1 night. Each working night uses 8 power; insufficient power pauses work. A full output holds remaining goods for later collection. Nuclear waste is a sealed reserve, not a production input.");

        internal static string AmmoDescription => Pick(
            "让散装零件各归其位的弹药作坊。置于自有物品栏，装入并开启电源；将自有材料拆为单件，可逐件投入或扩选多份拖到机器图标一起加入。每份无烟火药和底火、弹壳堆、弹头堆各兑换对应储量100，核废料500；各类上限5000，超额不收。选择配方与数量（1–99）后点开始，任意批量均需1个有效工作夜、整批8电；缺电暂停，输出满时余货保留待取。核废料仅作封存储备，不参与加工。",
            "An ammunition workshop that puts loose components to work. Keep it in your own inventory, insert and switch on a power source, then deposit owned materials individually or group-select separate items and drag them onto the machine icon. Split stacks first. Each pack of smokeless powder and primers, pile of cases or pile of projectiles adds 100 to its matching reserve; nuclear waste adds 500. Each reserve caps at 5000; excess deposits are refused. Select a recipe and quantity (1–99), then Start. Any batch takes 1 working night and 8 power total; insufficient power pauses work. A full output holds remaining goods for later collection. Nuclear waste is a sealed reserve, not a production input.");

        internal static string BarrelDescription(int width)
        {
            switch (width)
            {
                case 2: return Pick("一截短小敦实的枪管坯件，切口与外壁已加工整齐。可收藏或交易，不能直接装到枪械上。", "A short, stout barrel blank with neatly machined ends and outer walls. A collectible or trade good; it cannot be fitted to a firearm.");
                case 4: return Pick("一根长度适中的枪管坯件，规整的金属轮廓颇有工坊气息。可收藏或交易，不能直接装到枪械上。", "A medium-length barrel blank with the clean metal profile of a workshop product. A collectible or trade good; it cannot be fitted to a firearm.");
                case 6: return Pick("一根修长的枪管坯件，摆在架上很难不引人注意。可收藏或交易，不能直接装到枪械上。", "A long, slender barrel blank that is hard to overlook on a shelf. A collectible or trade good; it cannot be fitted to a firearm.");
                default: throw new System.ArgumentOutOfRangeException(nameof(width));
            }
        }

        internal static string StorageName(string key, string fallback)
        {
            switch (key)
            {
                case "small_rack": return Pick("小型枪架", "Small Gun Rack");
                case "medium_rack": return Pick("中型枪架", "Medium Gun Rack");
                case "large_rack": return Pick("大型枪架", "Large Gun Rack");
                case "accessory_box": return Pick("配件箱", "Gun Parts Box");
                case "ammo_box": return Pick("弹药箱", "Ammunition Box");
                default: return fallback;
            }
        }

        internal static string StorageDescription(string key)
        {
            switch (key)
            {
                case "small_rack": return Pick("一座紧凑的小型枪架，让几件心头好不必再挤在地上。内有8×6格，仅收武器；格数不是可存枪数。", "A compact rack that keeps a few prized weapons off the floor. An 8×6-cell interior accepts weapons only; cells do not equal weapon capacity.");
                case "medium_rack": return Pick("一座横向展开的中型枪架，为渐成规模的收藏留出位置。内有20×6格，仅收武器；格数不是可存枪数。", "A broad rack with room for a growing collection. A 20×6-cell interior accepts weapons only; cells do not equal weapon capacity.");
                case "large_rack": return Pick("一座铺开便颇有阵势的大型枪架，像是给整面收藏墙打的底。内有30×6格，仅收武器；格数不是可存枪数。", "An expansive rack fit to anchor a wall of collected arms. A 30×6-cell interior accepts weapons only; cells do not equal weapon capacity.");
                case "accessory_box": return Pick("一只专收枪械改装件的箱子，免得小零件总在最需要时失踪。内有8×12格，仅收枪械配件，不收普通材料。", "A box for firearm modifications, so small parts stop disappearing when needed most. An 8×12-cell interior accepts gun modifications only, not ordinary materials.");
                case "ammo_box": return Pick("一只专收弹药的箱子，让柜台不再到处散着弹药包。内有8×12格，仅收成品弹药，不收火药、弹壳堆或弹头堆。", "A dedicated box that keeps ammunition packs from taking over the counter. An 8×12-cell interior accepts finished ammunition only, not powder, case piles or projectile piles.");
                default: throw new System.ArgumentException("Missing storage description: " + key, nameof(key));
            }
        }
    }
}
