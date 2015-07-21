using Terraria;

namespace GTRPlugins
{
    public static class AutoClean
    {
        public static void Clean()
        {
            if (Main.player[Main.myPlayer].chest == -1)
            {
                AutoClean.CleanPlayer();
                return;
            }
            AutoClean.CleanChest();
        }
        private static void CleanPlayer()
        {
            Player player = Main.player[Main.myPlayer];
            for (int i = 10; i < 50; i++)
            {
                Item item = player.inventory[i];
                if (!item.favorited && item.type != 0 && item.stack < item.maxStack)
                {
                    for (int j = 10; j < 50; j++)
                    {
                        Item item2 = player.inventory[j];
                        if (!item2.favorited && j != i && item2.stack < item2.maxStack && item.type == item2.type)
                        {
                            int num = item.maxStack - item.stack;
                            item.stack += item2.stack;
                            if (item.stack >= item.maxStack)
                            {
                                player.inventory[i].stack = item.maxStack;
                            }
                            item2.stack -= num;
                            if (item2.stack <= 0)
                            {
                                player.inventory[j].SetDefaults(0, false);
                            }
                        }
                    }
                }
            }
        }
        private static void CleanChest()
        {
            Player player = Main.player[Main.myPlayer];
            Chest chest;
            if (player.chest == -2)
            {
                chest = player.bank;
            }
            else if (player.chest == -3)
            {
                chest = player.bank2;
            }
            else
            {
                chest = Main.chest[player.chest];
            }
            for (int i = 0; i < 40; i++)
            {
                Item item = chest.item[i];
                if (item.type != 0 && item.stack < item.maxStack)
                {
                    for (int j = 0; j < 40; j++)
                    {
                        Item item2 = chest.item[j];
                        if (j != i && item2.stack < item2.maxStack && item.type == item2.type)
                        {
                            int num = item.maxStack - item.stack;
                            item.stack += item2.stack;
                            if (item.stack >= item.maxStack)
                            {
                                chest.item[i].stack = item.maxStack;
                            }
                            item2.stack -= num;
                            if (item2.stack <= 0)
                            {
                                chest.item[j].SetDefaults(0, false);
                            }
                        }
                    }
                }
            }
        }
    }
}
