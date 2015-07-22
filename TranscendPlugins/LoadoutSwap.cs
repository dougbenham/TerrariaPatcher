using System;
using GTRPlugins.UI;
using Microsoft.Xna.Framework;
using PluginLoader;
using Terraria;

namespace GTRPlugins
{
    public class LoadoutSwap : IPluginDrawInventory
    {
        private Button btnLoadoutSwap;

        public LoadoutSwap()
        {
            btnLoadoutSwap = new Button("Swap Loadout", new Vector2(502f, 298f), BtnLoadoutSwapClick);
            btnLoadoutSwap.Scale = 0.9f;
        }

        private void BtnLoadoutSwapClick(object sender, EventArgs e)
        {
            Player player = Main.player[Main.myPlayer];
            for (int i = 0; i < 10; i++)
            {
                Item item = player.armor[i].Clone();
                player.armor[i] = player.armor[i + 10];
                player.armor[i + 10] = item;
            }
        }

        public void OnDrawInventory()
        {
            if (Main.player[Main.myPlayer].chest == -1 && Main.npcShop == 0)
            {
                btnLoadoutSwap.Draw();
            }
        }
    }
}
