using Terraria;

namespace GTRPlugins
{
    public static class AutoClean
    {
        public static void Clean()
        {
            if (Main.player[Main.myPlayer].chest == -1) CleanPlayer();
            else CleanChest();
        }

        static void CleanPlayer()
        {
            Player player = Main.player[Main.myPlayer];
            for (int curSlot = 10; curSlot < 50; curSlot++)
            {
                Item item = player.inventory[curSlot];
                if (item.favorited) continue;
                if (item.type != 0 && item.stack < item.maxStack)
                {
                    for (int i = 10; i < 50; i++)
                    {
                        Item item2 = player.inventory[i];
                        if (item2.favorited) continue;
                        if (i != curSlot && item2.stack < item2.maxStack)
                        {
                            if (item.type == item2.type)
                            {
                                int spaceRemaining = item.maxStack - item.stack;

                                item.stack += item2.stack;
                                if (item.stack >= item.maxStack)
                                {
                                    player.inventory[curSlot].stack = item.maxStack;
                                }

                                item2.stack -= spaceRemaining;
                                if (item2.stack <= 0)
                                {
                                    player.inventory[i].SetDefaults(0, false);
                                }
                            }
                        }
                    }
                }
            }
        }

        static void CleanChest()
        {
            Player player = Main.player[Main.myPlayer];
            Chest chest;
            if (player.chest == -2) chest = player.bank;
            else if (player.chest == -3) chest = player.bank2;
            else chest = Main.chest[player.chest];
            for (int curSlot = 0; curSlot < 40; curSlot++)
            {
                Item item = chest.item[curSlot];
                if (item.type != 0 && item.stack < item.maxStack)
                {
                    for (int i = 0; i < 40; i++)
                    {
                        Item item2 = chest.item[i];
                        if (i != curSlot && item2.stack < item2.maxStack)
                        {
                            if (item.type == item2.type)
                            {
                                int spaceRemaining = item.maxStack - item.stack;

                                item.stack += item2.stack;
                                if (item.stack >= item.maxStack)
                                {
                                    chest.item[curSlot].stack = item.maxStack;
                                }

                                item2.stack -= spaceRemaining;
                                if (item2.stack <= 0)
                                {
                                    chest.item[i].SetDefaults(0, false);
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}
