using System;
using GTRPlugins.UI;
using Microsoft.Xna.Framework;
using Terraria;

namespace GTRPlugins
{
    public class Inventory_Enhancements_UI
    {
        private static Button btnChestSort;
        static Inventory_Enhancements_UI()
        {
            btnChestSort = new Button("Sort", new Vector2(405f, 258f), BtnChestSortClick);
        }
        private static void BtnChestSortClick(object sender, EventArgs e)
        {
            AutoTrash.Trash();
            Inventory_Enhancements.Clean();
            Inventory_Enhancements.Sort();
        }
        public static void Update(object sender, EventArgs e)
        {
            if (Main.npcShop == 0)
            {
                if (Main.player[Main.myPlayer].chest == -1)
                    btnChestSort.Position = new Vector2(405f, 258f);
                else
                    btnChestSort.Position = new Vector2(413f, 428f);
            }
        }
        public static void DrawInventory(object sender, EventArgs e)
        {
            if (Main.npcShop == 0 && Inventory_Enhancements.config.SortChests)
            {
                btnChestSort.Draw();
            }
        }
    }
}
