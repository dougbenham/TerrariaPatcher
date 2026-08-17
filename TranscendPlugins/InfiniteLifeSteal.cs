using System;
using PluginLoader;
using Terraria;

namespace TranscendPlugins
{
    [PluginDescription("Removes the cap on life steal, so Vampire Knives and Spectre armour keep healing you without limit.")]
    public class InfiniteLifeSteal : PluginBase, IPluginPlayerUpdate
    {
        public void OnPlayerUpdate(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
                player.lifeSteal = 10000;
        }
    }
}
