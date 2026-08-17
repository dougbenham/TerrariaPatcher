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

        /// <summary>
        /// The only weather change a client can ask the server for, through message 61. Rain, and stopping either
        /// kind, have no request packet.
        /// </summary>
        private const int RequestSlimeRain = -19;

        private static void ToggleRaining()
        {
            if (Main.netMode != 0)
            {
                Main.NewText("The server controls the weather.");
                return;
            }

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
            if (Main.netMode != 0)
            {
                if (Main.slimeRain)
                {
                    Main.NewText("Only the server can stop slime rain.");
                    return;
                }

                NetMessage.SendData(61, -1, -1, null, Main.myPlayer, RequestSlimeRain, 0f, 0f, 0, 0, 0);
                return;
            }

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
