using System;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;

namespace MrBlueSLPlugins
{
    public class Flashlight : MarshalByRefObject, IPluginPlayerUpdate, IPluginChatCommand
    {
        private bool flashlight = false;
        private Keys flashlightKey;

        public Flashlight()
        {
            bool stored;
            if (bool.TryParse(IniAPI.ReadIni("Flashlight", "Enabled", "false", writeIt: true), out stored))
                flashlight = stored;

            if (!Keys.TryParse(IniAPI.ReadIni("Flashlight", "ToggleKey", "U", writeIt: true), out flashlightKey))
                flashlightKey = Keys.U;

            Loader.RegisterHotkey(() =>
            {
                flashlight = !flashlight;
                IniAPI.WriteIni("Flashlight", "Enabled", flashlight.ToString());
                Main.NewText("Flashlight " + (flashlight ? "Enabled" : "Disabled"), 150, 150, 150);
            }, flashlightKey);
        }

        public void OnPlayerUpdate(Player player)
        {
            if (flashlight)
            {
                Lighting.AddLight((int)(Main.mouseX + Main.screenPosition.X + (double)(Player.defaultWidth / 2)) / 16, (int)(Main.mouseY + Main.screenPosition.Y + (double)(Player.defaultHeight / 2)) / 16, 1f, 1f, 1f);
            }
        }

        public bool OnChatCommand(string command, string[] args)
        {
            if (command != "flashlight") return false;

            string arg = args.Length > 0 ? args[0].ToLower() : "toggle";
            switch (arg)
            {
                case "on":
                    flashlight = true;
                    break;
                case "off":
                    flashlight = false;
                    break;
                case "toggle":
                case "":
                    flashlight = !flashlight;
                    break;
                case "help":
                    Main.NewText("Usage: /flashlight [on|off|toggle]");
                    return true;
                default:
                    Main.NewText("Usage: /flashlight [on|off|toggle]");
                    return true;
            }

            IniAPI.WriteIni("Flashlight", "Enabled", flashlight.ToString());
            Main.NewText("Flashlight " + (flashlight ? "Enabled" : "Disabled"), 150, 150, 150);
            return true;
        }
    }
}
