using PluginLoader;
using Terraria;
using Terraria.ID;

namespace TranscendPlugins
{
    [PluginDescription("Makes Coin Gun ammo worth using: each coin gets a configurable damage value, and can be made to " +
                       "home in on enemies.")]
    public class CoinGun : PluginBase, IPluginItemSetDefaults
    {
        private static readonly Setting<bool> CopperCoinEnemyTracking = true;
        private static readonly Setting<int> CopperCoinDamage = 200;
        private static readonly Setting<bool> SilverCoinEnemyTracking = true;
        private static readonly Setting<int> SilverCoinDamage = 200;
        private static readonly Setting<bool> GoldCoinEnemyTracking = true;
        private static readonly Setting<int> GoldCoinDamage = 200;
        private static readonly Setting<bool> PlatinumCoinEnemyTracking = true;
        private static readonly Setting<int> PlatinumCoinDamage = 200;

        public void OnItemSetDefaults(Item item)
        {
            switch (item.type)
            {
                case ItemID.CopperCoin:
                    if (CopperCoinEnemyTracking) item.shoot = ProjectileID.ChlorophyteBullet;
                    item.damage = CopperCoinDamage;
                    break;
                case ItemID.SilverCoin:
                    if (SilverCoinEnemyTracking) item.shoot = ProjectileID.ChlorophyteBullet;
                    item.damage = SilverCoinDamage;
                    break;
                case ItemID.GoldCoin:
                    if (GoldCoinEnemyTracking) item.shoot = ProjectileID.ChlorophyteBullet;
                    item.damage = GoldCoinDamage;
                    break;
                case ItemID.PlatinumCoin:
                    if (PlatinumCoinEnemyTracking) item.shoot = ProjectileID.ChlorophyteBullet;
                    item.damage = PlatinumCoinDamage;
                    break;
            }
        }
    }
}
