using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;

namespace TranscendPlugins
{
    [PluginDescription("Adds to the number of minions you can summon at once, on top of what your summoner gear already " +
                       "gives you.")]
    public class Minions : PluginBase, IPluginPlayerUpdateArmorSets
    {
        private static readonly Setting<int> Bonus = 100;

        public Minions() : base(toggleKey: Keys.None)
        { }

        public void OnPlayerUpdateArmorSets(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
                player.maxMinions += Bonus;
        }
    }
}
