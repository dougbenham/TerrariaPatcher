using System;
using GTRPlugins.UI;
using Microsoft.Xna.Framework;
using Terraria;

namespace GTRPlugins
{
    public class LoadoutSwap
    {
        private static Button btnLoadoutSwap;
        static LoadoutSwap()
        {
            btnLoadoutSwap = new Button("Swap Loadout", new Vector2(502f, 298f), BtnLoadoutSwapClick);
            btnLoadoutSwap.Scale = 0.9f;
        }
        public static void Draw(object sender, EventArgs e)
        {
            if (Main.player[Main.myPlayer].chest == -1 && Main.npcShop == 0)
            {
                btnLoadoutSwap.Draw();
            }
        }
        private static void BtnLoadoutSwapClick(object sender, EventArgs e)
        {
            Player player = Main.player[Main.myPlayer];
            for (int i = 0; i < 10; i++)
            {
                Item item = player.armor[i].Clone();
                player.armor[i] = player.armor[i + 10];
                player.armor[i + 10] = item;
            }
        }
    }
}
