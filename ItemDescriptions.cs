// Player-facing descriptions; item identifiers and gameplay remain unchanged.
namespace WeaponDescriptions
{
    public static class Text
    {
        public static string Gun(string key) => GunsTextLocalization.Description(key, EnglishLocalization.L.English);
        public static string Storage(string key)
        {
            switch (key) {
                case "small_rack": return "一座小型枪架，可存放武器。空间有限，但比把枪堆在地上整齐。";
                case "medium_rack": return "一座中型枪架，专门用于存放武器。";
                case "large_rack": return "一座大型枪架，可为收藏提供更宽敞的存放空间。";
                case "accessory_box": return "一只配件箱，专门存放枪械改装件。";
                case "ammo_box": return "一只弹药箱，专门存放弹药。";
                default: throw new System.ArgumentException("Missing storage description: " + key);
            }
        }
        public static string Ammo(string name) => name.StartsWith("12号") ? "一批" + name + "。可供使用这一弹种的枪械使用。" : "一批" + name + "。可供使用相同弹种的枪械使用。";
        public static string Magazine(string name) => "适配" + name + "的可拆卸弹匣。";
    }
}
