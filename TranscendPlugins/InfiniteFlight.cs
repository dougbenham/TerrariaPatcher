using Microsoft.Xna.Framework.Input;
using PluginLoader;

namespace ZeromaruPlugins
{
    [PluginDescription("Wings, rocket boots, and flying carpets never run out while enabled.")]
    public class InfiniteFlight : PluginBase, IPluginUpdate
    {
        public InfiniteFlight() : base(toggleKey: Keys.I)
        { }

        public void OnUpdate()
        {
            Player.rocketTime = 1;
            Player.carpetTime = 1;
            Player.wingTime = 1f;
        }
    }
}
