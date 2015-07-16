using System;
using System.Reflection;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;

namespace TranscendPlugins
{
    public class Weather : MarshalByRefObject, IPlugin
    {
        private Keys toggleKey;
        private Keys toggleSlimeKey;

        private MethodInfo startRain;

        private void StartRain()
        {
            startRain.Invoke(null, null);
        }

        private MethodInfo stopRain;

        private void StopRain()
        {
            stopRain.Invoke(null, null);
        }

        public Weather()
        {
            var main = Assembly.GetEntryAssembly().GetType("Terraria.Main");
            startRain = main.GetMethod("StartRain", BindingFlags.Static | BindingFlags.NonPublic);
            stopRain = main.GetMethod("StopRain", BindingFlags.Static | BindingFlags.NonPublic);

            if (!Keys.TryParse(IniAPI.ReadIni("Weather", "ToggleRain", "OemSemicolon", writeIt: true), out toggleKey))
                toggleKey = Keys.OemSemicolon;

            if (!Keys.TryParse(IniAPI.ReadIni("Weather", "ToggleSlimeRain", "OemQuotes", writeIt: true), out toggleSlimeKey))
                toggleSlimeKey = Keys.OemQuotes;

            Loader.RegisterHotkey(() =>
            {
                Main.NewText("Toggle rain");
                if (Main.raining)
                    StopRain();
                else
                    StartRain();
            }, toggleKey);

            Loader.RegisterHotkey(() =>
            {
                Main.NewText("Toggle slime rain");
                if (Main.slimeRain)
                    Main.StopSlimeRain();
                else
                    Main.StartSlimeRain();
            }, toggleSlimeKey);
        }
    }
}
