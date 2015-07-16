using System;
using PluginLoader;
using Terraria;
using Terraria.ID;

namespace TranscendPlugins
{
    public class CoinGun : MarshalByRefObject, IPluginItemSetDefaults
    {
        private bool copperCoinEnemyTracking, silverCoinEnemyTracking, goldCoinEnemyTracking, platinumCoinEnemyTracking;
        private int copperCoinDamage, silverCoinDamage, goldCoinDamage, platinumCoinDamage;

        public CoinGun()
        {
            copperCoinEnemyTracking = bool.Parse(IniAPI.ReadIni("CoinGunModifications", "CopperCoinEnemyTracking", "true", writeIt: true));
            copperCoinDamage = int.Parse(IniAPI.ReadIni("CoinGunModifications", "CopperCoinDamage", "200", writeIt: true));
            silverCoinEnemyTracking = bool.Parse(IniAPI.ReadIni("CoinGunModifications", "SilverCoinEnemyTracking", "true", writeIt: true));
            silverCoinDamage = int.Parse(IniAPI.ReadIni("CoinGunModifications", "SilverCoinDamage", "200", writeIt: true));
            goldCoinEnemyTracking = bool.Parse(IniAPI.ReadIni("CoinGunModifications", "GoldCoinEnemyTracking", "true", writeIt: true));
            goldCoinDamage = int.Parse(IniAPI.ReadIni("CoinGunModifications", "GoldCoinDamage", "200", writeIt: true));
            platinumCoinEnemyTracking = bool.Parse(IniAPI.ReadIni("CoinGunModifications", "PlatinumCoinEnemyTracking", "true", writeIt: true));
            platinumCoinDamage = int.Parse(IniAPI.ReadIni("CoinGunModifications", "PlatinumCoinDamage", "200", writeIt: true));
        }

        public void OnItemSetDefaults(Item item)
        {
            switch (item.type)
            {
                case ItemID.CopperCoin:
                    if (copperCoinEnemyTracking) item.shoot = ProjectileID.ChlorophyteBullet;
                    item.damage = copperCoinDamage;
                    break;
                case ItemID.SilverCoin:
                    if (silverCoinEnemyTracking) item.shoot = ProjectileID.ChlorophyteBullet;
                    item.damage = silverCoinDamage;
                    break;
                case ItemID.GoldCoin:
                    if (goldCoinEnemyTracking) item.shoot = ProjectileID.ChlorophyteBullet;
                    item.damage = goldCoinDamage;
                    break;
                case ItemID.PlatinumCoin:
                    if (platinumCoinEnemyTracking) item.shoot = ProjectileID.ChlorophyteBullet;
                    item.damage = platinumCoinDamage;
                    break;
            }
        }
    }
}
