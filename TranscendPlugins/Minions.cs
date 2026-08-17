using PluginLoader;
using Terraria;

namespace TranscendPlugins
{
    [PluginDescription("Raises the number of minions you can summon at once, regardless of your summoner gear.")]
    public class Minions : PluginBase, IPluginPlayerUpdateArmorSets
    {
        private static readonly Setting<int> Max = 100;

        public void OnPlayerUpdateArmorSets(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
                player.maxMinions = Max;
        }
    }
}
