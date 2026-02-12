using System;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;
using Terraria.Testing;

namespace TranscendPlugins
{
    public class Reveal : MarshalByRefObject, IPlugin, IPluginChatCommand
    {
        private Keys revealKey;

        public Reveal()
        {
            if (!Keys.TryParse(IniAPI.ReadIni("Reveal", "RevealKey", "L", writeIt: true), out revealKey))
                revealKey = Keys.L;

            Loader.RegisterHotkey(() =>
            {
	            if (Main.mapFullscreen && Main.Map != null)
	            {
		            Main.clearMap = true;
		            DebugOptions.unlockMap = 1;
		            Main.refreshMap = true;
	            }
            }, revealKey);
        }

        public bool OnChatCommand(string command, string[] args)
        {
            if (command != "reveal")
                return false;

            if (!Main.mapFullscreen || Main.Map == null)
            {
                Main.NewText("Open the map and use /reveal to uncover everything.", 0, 200, 255);
                return true;
            }

            Main.clearMap = true;
            DebugOptions.unlockMap = 1;
            Main.refreshMap = true;
            return true;
        }
    }
}
