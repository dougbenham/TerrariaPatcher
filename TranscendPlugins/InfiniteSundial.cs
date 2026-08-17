using System;
using PluginLoader;
using Terraria;

namespace TranscendPlugins
{
    [PluginDescription("Removes the Enchanted Sundial's cooldown, so you can skip to the next day as often as you like.")]
    public class InfiniteSundial : PluginBase, IPluginUpdate
    {
        public void OnUpdate()
        {
            Main.sundialCooldown = 0;
        }
    }
}
