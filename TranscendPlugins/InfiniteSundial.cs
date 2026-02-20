using System;
using PluginLoader;
using Terraria;

namespace TranscendPlugins
{
    public class InfiniteSundial : MarshalByRefObject, IPluginUpdate, IPluginChatCommand
    {
        private bool enabled;

        public InfiniteSundial()
        {
            if (!bool.TryParse(IniAPI.ReadIni("InfiniteSundial", "Enabled", "true", writeIt: true), out enabled))
                enabled = true;
        }

        public void OnUpdate()
        {
            if (enabled)
                Main.sundialCooldown = 0;
        }

        public bool OnChatCommand(string command, string[] args)
        {
            if (command != "sundial") return false;

            string arg = args.Length > 0 ? args[0].ToLower() : "toggle";
            switch (arg)
            {
                case "on":
                    enabled = true;
                    break;
                case "off":
                    enabled = false;
                    break;
                case "toggle":
                case "":
                    enabled = !enabled;
                    break;
                case "help":
                    Main.NewText("Usage: /sundial [on|off|toggle]");
                    return true;
                default:
                    Main.NewText("Usage: /sundial [on|off|toggle]");
                    return true;
            }

            IniAPI.WriteIni("InfiniteSundial", "Enabled", enabled.ToString());
            Main.NewText("Infinite Sundial " + (enabled ? "enabled" : "disabled") + ".");
            return true;
        }
    }
}
