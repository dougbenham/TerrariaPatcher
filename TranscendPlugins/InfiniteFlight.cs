using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;

namespace ZeromaruPlugins
{
    [PluginDescription("Wings, rocket boots, and flying carpets never run out while enabled. Toggled in game with a hotkey.")]
    public class InfiniteFlight : PluginBase, IPluginUpdate
    {
        private static readonly HotkeySetting ToggleKey = new Hotkey { Key = Keys.I, Action = Toggle };

        private static bool flight;

        private static void Toggle()
        {
            flight = !flight;

            var green = Color.Green;
            Main.NewText("Infinite Flight " + (flight ? "Enabled" : "Disabled"), green.R, green.G, green.B);
        }

        public void OnUpdate()
        {
            if (!flight) return;

            Player.rocketTime = 1;
            Player.carpetTime = 1;
            Player.wingTime = 1f;
        }
    }
}
