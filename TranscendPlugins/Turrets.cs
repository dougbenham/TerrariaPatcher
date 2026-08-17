using PluginLoader;
using Terraria;

namespace TranscendPlugins
{
    [PluginDescription("Raises the number of sentry turrets you can have placed at once.")]
    public class Turrets : PluginBase, IPluginPlayerUpdateArmorSets
    {
        private static readonly Setting<int> Max = 100;

        public void OnPlayerUpdateArmorSets(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
                player.maxTurrets = Max;
        }
    }
}
