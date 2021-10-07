using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;

namespace ZeromaruPlugins
{
    public class InfiniteFlight : MarshalByRefObject, IPluginUpdate
    {
        private bool flight = false;
        private Keys flightKey;

        public InfiniteFlight()
        {
            if (!Keys.TryParse(IniAPI.ReadIni("InfiniteFlight", "FlightKey", "I", writeIt: true), out flightKey))
                flightKey = Keys.I;

            Color green = Color.Green;
            Loader.RegisterHotkey(() =>
            {
                flight = !flight;
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
    }
}