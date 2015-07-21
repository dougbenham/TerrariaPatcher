using System;
using PluginLoader;
using Terraria;

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
                    chest.item[7].SetDefaults("Healing Potion");
                }
                else if (player.statLifeMax >= 300 && player.statLifeMax <= 499)
                {
                    chest.item[7].SetDefaults("Greater Healing Potion");
                }
                else if (player.statLifeMax >= 500)
                {
                    chest.item[7].SetDefaults("Super Healing Potion");
                }

                if (player.statManaMax >= 160 && player.statManaMax <= 200)
                {
                    chest.item[8].SetDefaults("Mana Potion");
                }
                else if (player.statManaMax >= 201 && player.statManaMax <= 399)
                {
                    chest.item[8].SetDefaults("Greater Mana Potion");
                }
                else if (player.statManaMax >= 400)
                {
                    chest.item[8].SetDefaults("Super Mana Potion");
                }
            }
        }
    }
}
