using System;
using PluginLoader;
using Terraria;

namespace TranscendPlugins
{
    public class InfiniteLifeSteal : MarshalByRefObject, IPluginPlayerUpdate
    {
        public void OnPlayerUpdate(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
                player.lifeSteal = 10000;
        }
    }
}
