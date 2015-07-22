using System;
using GTRPlugins.Utils;
using Terraria;

namespace GTRPlugins
{
    public class CycleAmmo
    {
        public static void Update(object sender, EventArgs e)
        {
            if (Input.KeyPressed(Config.CharToXnaKey(Inventory_Enhancements.config.CAKey)) && Inventory_Enhancements.config.CAHotkeyEnabled)
            {
                Cycle();
            }
        }

        private static void Cycle()
        {
            Item[] tempItems = new Item[4];
            Player p = Main.player[Main.myPlayer];
            for (int i = 54; i < 58; i++)
            {
                tempItems[i - 54] = p.inventory[i].Clone();
            }
            p.inventory[54] = tempItems[3];
            p.inventory[55] = tempItems[0];
            p.inventory[56] = tempItems[1];
            p.inventory[57] = tempItems[2];
        }
    }
}
