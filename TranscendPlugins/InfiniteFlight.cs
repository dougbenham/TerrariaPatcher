using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;

namespace ZeromaruPlugins
{
    public class InfiniteFlight : MarshalByRefObject, IPluginUpdate, IPluginChatCommand
    {
        private bool flight = false;
        private Keys flightKey;

        public InfiniteFlight()
        {
            bool stored;
            if (bool.TryParse(IniAPI.ReadIni("InfiniteFlight", "Enabled", "false", writeIt: true), out stored))
                flight = stored;

            if (!Keys.TryParse(IniAPI.ReadIni("InfiniteFlight", "FlightKey", "I", writeIt: true), out flightKey))
                flightKey = Keys.I;

            Color green = Color.Green;
            Loader.RegisterHotkey(() =>
            {
                flight = !flight;
                IniAPI.WriteIni("InfiniteFlight", "Enabled", flight.ToString());
                Main.NewText("Infinite Flight " + (flight ? "Enabled" : "Disabled"), green.R, green.G, green.B);
            }, flightKey);
        }

        public void OnUpdate()
        {
            if (flight)
            {
                var player = Main.player[Main.myPlayer];
                player.rocketTime = 1;
                player.carpetTime = 1;
                player.wingTime = 1f;
            }
        }

        public bool OnChatCommand(string command, string[] args)
        {
            if (command != "flight") return false;

            string arg = args.Length > 0 ? args[0].ToLower() : "toggle";
            switch (arg)
            {
                case "on":
                    flight = true;
                    break;
                case "off":
                    flight = false;
                    break;
                case "toggle":
                case "":
                    flight = !flight;
                    break;
                case "help":
                    Main.NewText("Usage: /flight [on|off|toggle]");
                    return true;
                default:
                    Main.NewText("Usage: /flight [on|off|toggle]");
                    return true;
            }

            IniAPI.WriteIni("InfiniteFlight", "Enabled", flight.ToString());
            Main.NewText("Infinite Flight " + (flight ? "Enabled" : "Disabled"), Color.Green.R, Color.Green.G, Color.Green.B);
            return true;
        }
    }
}
