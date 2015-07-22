using System;
using GTRPlugins.Utils;
using Microsoft.Xna.Framework.Input;
using Terraria;

namespace GTRPlugins
{
    internal class AutoTrash
    {
        public static void Update(object sender, EventArgs e)
        {
            if (Input.KeyPressed(Config.CharToXnaKey(Inventory_Enhancements.config.SortKey)) && Main.keyState.IsKeyDown(Keys.LeftShift))
            {
                if (Inventory_Enhancements.config.AutoTrash)
                {
                    Inventory_Enhancements.config.AutoTrash = false;
                    Main.NewText("Autotrash Disabled", 255, 50, 50);
                    Inventory_Enhancements.config.SaveConfig();
                }
                else
                {
                    Inventory_Enhancements.config.AutoTrash = true;
                    Main.NewText("Autotrash Enabled", 255, 50, 50);
                    Inventory_Enhancements.config.SaveConfig();
                }
            }
            if (Inventory_Enhancements.config.AutoTrash)
            {
                Trash();
            }
        }

        public static void Trash()
        {
            if (Main.netMode == 1)
            {
                Player p = Main.player[Main.myPlayer];
                for (int i = 10; i < 51; i++)
                {
                    if (Inventory_Enhancements.config.TrashList.Contains(p.inventory[i].type))
                    {
                        if (p.inventory[i].favorited) continue;
                        p.trashItem = p.inventory[i];
                        p.inventory[i] = new Item();
                    }
                }
            }
        }

        public static bool Trash(Player plr, Item newItem)
        {
            if (Main.myPlayer == plr.whoAmI && Main.netMode == 0)
            {
                if (Inventory_Enhancements.config.TrashList.Contains(newItem.type))
                { return true; }
            }
            return false;
        }
    }
}
