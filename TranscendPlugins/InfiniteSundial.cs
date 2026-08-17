using PluginLoader;
using Terraria;

namespace TranscendPlugins
{
    [PluginDescription("Removes the Enchanted Sundial's and Enchanted Moondial's cooldowns, so you can skip to the next " +
                       "day or night as often as you like. Single player only.")]
    public class InfiniteSundial : PluginBase, IPluginUpdate
    {
        public void OnUpdate()
        {
            if (Main.netMode != 0) return;

            Main.sundialCooldown = 0;
            Main.moondialCooldown = 0;
        }
    }
}
