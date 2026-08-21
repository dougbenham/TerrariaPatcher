using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;

namespace TranscendPlugins
{
    [PluginDescription("Forces the Christmas or Halloween season on regardless of the date, for the seasonal drops and " +
                       "decorations. On a server only the decorations follow, because the season " +
                       "the server is in is what decides the drops and the seasonal enemies.")]
    public class Season : PluginBase, IPluginCheckSeason
    {
        private static readonly Setting<bool> Xmas = false;
        private static readonly Setting<bool> Halloween = false;

        public Season() : base(toggleKey: Keys.None)
        { }

        public bool OnCheckXmas()
        {
            Main.xMas = Xmas;
            return true;
        }

        public bool OnCheckHalloween()
        {
            Main.halloween = Halloween;
            return true;
        }
    }
}
