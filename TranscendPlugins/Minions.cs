using System;
using PluginLoader;
using Terraria;

namespace TranscendPlugins
{
    public class Minions : MarshalByRefObject, IPluginPlayerUpdateArmorSets, IPluginChatCommand
    {
        private int minions;
        private bool enabled = true;

        public Minions()
        {
            if (!int.TryParse(IniAPI.ReadIni("Minions", "Max", "100", writeIt: true), out minions))
                minions = 100;
            bool stored;
            if (bool.TryParse(IniAPI.ReadIni("Minions", "Enabled", "true", writeIt: true), out stored))
                enabled = stored;
        }
        public void OnPlayerUpdateArmorSets(Player player)
        {
            if (enabled && player.whoAmI == Main.myPlayer)
                player.maxMinions = minions;
        }

        public bool OnChatCommand(string command, string[] args)
        {
            if (command != "minions") return false;

            if (args.Length == 0)
            {
                Main.NewText("Minions: " + (enabled ? "enabled" : "disabled") + " count=" + minions);
                return true;
            }

            var arg = args[0].ToLower();
            if (arg == "off" || arg == "disable")
            {
                enabled = false;
            }
            else if (arg == "on" || arg == "enable")
            {
                enabled = true;
            }
            else if (arg == "help")
            {
                Main.NewText("Usage: /minions <number|on|off>");
                return true;
            }
            else
            {
                int value;
                if (!int.TryParse(args[0], out value))
                {
                    Main.NewText("Usage: /minions <number|on|off>");
                    return true;
                }
                if (value < 0) value = 0;
                minions = value;
                enabled = value > 0 ? enabled : enabled; // keep toggle state
                IniAPI.WriteIni("Minions", "Max", minions.ToString());
            }

            IniAPI.WriteIni("Minions", "Enabled", enabled.ToString());
            Main.NewText("Minions " + (enabled ? "enabled" : "disabled") + " (max " + minions + ").");
            return true;
        }
    }
}
