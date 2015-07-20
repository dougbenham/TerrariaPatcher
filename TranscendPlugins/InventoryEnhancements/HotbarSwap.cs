using Terraria;

namespace GTRPlugins
{
    public class HotbarSwap
    {
        public static void Swap(bool cycle)
        {
            Player player = Main.player[Main.myPlayer];
            if (!Main.gameMenu)
            {
                Item[] array = new Item[10];
                if (cycle)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        array[i] = player.inventory[i];
                        player.inventory[i] = player.inventory[i + 10];
                        player.inventory[i + 10] = player.inventory[i + 20];
                        player.inventory[i + 20] = player.inventory[i + 30];
                        player.inventory[i + 30] = player.inventory[i + 40];
                        player.inventory[i + 40] = array[i];
                    }
                }
                else
                {
                    for (int i = 0; i < 10; i++)
                    {
                        array[i] = player.inventory[i];
                        player.inventory[i] = player.inventory[i + 40];
                        player.inventory[i + 40] = array[i];
                    }
                }
            }
        }
    }
}
