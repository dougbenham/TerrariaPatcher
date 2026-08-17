using System;
using PluginLoader;
using Terraria;

namespace TranscendPlugins
{
    [PluginDescription("Removes the Enchanted Sundial's and Enchanted Moondial's cooldowns, so you can skip to the next " +
                       "day or night as often as you like.")]
    public class InfiniteSundial : PluginBase, IPluginUpdate
    {
        public void OnUpdate()
        {
            Main.sundialCooldown = 0;
            Main.moondialCooldown = 0;
        }
    }
}
