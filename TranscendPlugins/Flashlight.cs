using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;

namespace MrBlueSLPlugins
{
    [PluginDescription("Shines a light at your cursor while enabled, letting you see further than your held light source reaches.")]
    public class Flashlight : PluginBase, IPluginPlayerUpdate
    {
        private static readonly HotkeySetting ToggleKey = new Hotkey { Key = Keys.U, Action = Toggle };

        private static bool flashlight;

        private static void Toggle()
        {
            flashlight = !flashlight;
            Main.NewText("Flashlight " + (flashlight ? "Enabled" : "Disabled"), 150, 150, 150);
        }

        public void OnPlayerUpdate(Player player)
        {
            if (flashlight)
            {
                Lighting.AddLight((int)(Main.mouseX + Main.screenPosition.X + (double)(Player.defaultWidth / 2)) / 16, (int)(Main.mouseY + Main.screenPosition.Y + (double)(Player.defaultHeight / 2)) / 16, 1f, 1f, 1f);
            }
        }
    }
}
