using System;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;

namespace TranscendPlugins
{
    public class Weather : MarshalByRefObject, IPlugin
    {
        private Keys toggleKey;
        private Keys toggleSlimeKey;

        public Weather()
        {
            if (!Keys.TryParse(IniAPI.ReadIni("Weather", "ToggleRain", "OemSemicolon", writeIt: true), out toggleKey))
                toggleKey = Keys.OemSemicolon;

            if (!Keys.TryParse(IniAPI.ReadIni("Weather", "ToggleSlimeRain", "OemQuotes", writeIt: true), out toggleSlimeKey))
                toggleSlimeKey = Keys.OemQuotes;

            Loader.RegisterHotkey(() =>
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
            }, toggleKey);

            Loader.RegisterHotkey(() =>
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
            }, toggleSlimeKey);
        }
    }
}
