using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;

namespace TranscendPlugins
{
    [PluginDescription("Starts and stops rain and slime rain on demand with a hotkey.")]
    public class Weather : PluginBase
    {
        private static readonly HotkeySetting ToggleRain = new Hotkey { Key = Keys.OemSemicolon, Action = ToggleRaining };
        private static readonly HotkeySetting ToggleSlimeRain = new Hotkey { Key = Keys.OemQuotes, Action = ToggleSlimeRaining };

        private static void ToggleRaining()
        {
            if (Main.raining)
            {
                Main.StopRain();
                Main.NewText("Rain stopped.");
            }
            else
            {
                Main.StartRain();
                Main.NewText("Rain started.");
            }
        }

        private static void ToggleSlimeRaining()
        {
            if (Main.slimeRain)
            {
                Main.StopSlimeRain();
                Main.NewText("Slime rain stopped.");
            }
            else
            {
                Main.StartSlimeRain();
                Main.NewText("Slime rain started.");
            }
        }
    }
}
