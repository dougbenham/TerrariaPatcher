using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PluginLoader;

namespace TranscendPlugins
{
    [PluginDescription("Lights every block fully.")]
    public class FullBright : PluginBase, IPluginLightingGetColor
    {
        public FullBright() : base(enabledByDefault: false, toggleKey: Keys.Y)
        { }

        public bool OnLightingGetColor(int x, int y, out Color color)
        {
            color = Color.White;
            return true;
        }
    }
}
