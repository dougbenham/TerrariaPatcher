using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;

namespace TranscendPlugins
{
    [PluginDescription("Lights the entire world evenly, so caves and night are as visible as daytime. Toggled in game with a hotkey.")]
    public class FullBright : PluginBase, IPluginLightingGetColor
    {
        private static readonly Setting<bool> Enabled = false;
        private static readonly HotkeySetting ToggleKey = new Hotkey { Key = Keys.Y, Action = Toggle };

        private static void Toggle()
        {
            Enabled.Value = !Enabled;

            var green = Color.Green;
            Main.NewText("Full Bright " + (Enabled ? "Enabled" : "Disabled"), green.R, green.G, green.B);
        }

        public bool OnLightingGetColor(int x, int y, out Color color)
        {
            color = Color.White;
            return Enabled;
        }
    }
}
