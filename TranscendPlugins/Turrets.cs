using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;

namespace TranscendPlugins
{
    [PluginDescription("Adds to the number of sentry turrets you can have placed at once, on top of what your gear " +
                       "already gives you.")]
    public class Turrets : PluginBase, IPluginPlayerUpdateArmorSets
    {
        private static readonly Setting<int> Bonus = 100;

        public Turrets() : base(toggleKey: Keys.None)
        { }

        public void OnPlayerUpdateArmorSets(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
                player.maxTurrets += Bonus;
        }
    }
}
