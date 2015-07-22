using Terraria;

namespace GTRPlugins
{
    public class HotbarSwap
    {
        public static void Swap(bool cycle)
        {
            Player p = Main.player[Main.myPlayer];
            if (Main.gameMenu) return;
            Item[] temp = new Item[10];
            if (cycle)
            {
                for (int i = 0; i < 10; i++)
                {
                    temp[i] = p.inventory[i];
                    p.inventory[i] = p.inventory[i + 10];
                    p.inventory[i + 10] = p.inventory[i + 20];
                    p.inventory[i + 20] = p.inventory[i + 30];
                    p.inventory[i + 30] = p.inventory[i + 40];
                    p.inventory[i + 40] = temp[i];
                }
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    temp[i] = p.inventory[i];
                    p.inventory[i] = p.inventory[i + 40];
                    p.inventory[i + 40] = temp[i];
                }
            }
        }
    }
}
