using System;

namespace GunsmithMachineIntegration
{
    // Pure arithmetic: unknown purity and overflow reject the entire deposit.
    public static class ReservePolicy
    {
        public const int Limit = 5000;
        public const int ElectronicPerItem = 25;
        public const int PlasticPerItem = 25;
        public const int NuclearWastePerItem = 500;
        public static int MetalPerIngot(string purity)
        {
            switch (purity) {
                case "INGOT_PURITY_VERY_LOW": return 75;
                case "INGOT_PURITY_LOW": return 85;
                case "INGOT_PURITY_FAIR": return 100;
                case "INGOT_PURITY_HIGH": return 125;
                case "INGOT_PURITY_VERY_HIGH": return 150;
                case "INGOT_PURITY_PERFECT": return 300;
                default: return 0;
            }
        }
        public static bool TryDeposit(int current, int perItem, int quantity, out int next)
        {
            next=current;
            if (current < 0 || current > Limit || perItem <= 0 || quantity <= 0) return false;
            long total=(long)current+(long)perItem*quantity;
            if(total>Limit) return false;
            next=(int)total; return true;
        }
    }
}
