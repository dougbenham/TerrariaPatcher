using System;
using PluginLoader;
using Terraria;
using Terraria.ID;

namespace EraselsPlugins
{
    public class ShopSellsScalingPotions : MarshalByRefObject, IPluginChestSetupShop
    {
        public void OnChestSetupShop(Chest chest, int type)
        {
            if (type == 1)
            {
                var player = Main.player[Main.myPlayer];

                if (player.statLifeMax >= 200 && player.statLifeMax <= 299)
                {
                    chest.item[7].SetDefaults(ItemID.HealingPotion);
                }
                else if (player.statLifeMax >= 300 && player.statLifeMax <= 499)
                {
                    chest.item[7].SetDefaults(ItemID.GreaterHealingPotion);
                }
                else if (player.statLifeMax >= 500)
                {
                    chest.item[7].SetDefaults(ItemID.SuperHealingPotion);
                }

                if (player.statManaMax >= 160 && player.statManaMax <= 200)
                {
                    chest.item[8].SetDefaults(ItemID.ManaPotion);
                }
                else if (player.statManaMax >= 201 && player.statManaMax <= 399)
                {
                    chest.item[8].SetDefaults(ItemID.GreaterManaPotion);
                }
                else if (player.statManaMax >= 400)
                {
                    chest.item[8].SetDefaults(ItemID.SuperManaPotion);
                }
            }
        }
    }
}
