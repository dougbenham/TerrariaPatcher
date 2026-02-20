using System;
using PluginLoader;
using Terraria;

namespace TranscendPlugins
{
    public class Turrets : MarshalByRefObject, IPluginPlayerUpdateArmorSets, IPluginChatCommand
    {
        private int turrets;
        private bool enabled = true;

        public Turrets()
        {
            if (!int.TryParse(IniAPI.ReadIni("Turrets", "Max", "100", writeIt: true), out turrets))
                turrets = 100;
            bool stored;
            if (bool.TryParse(IniAPI.ReadIni("Turrets", "Enabled", "true", writeIt: true), out stored))
                enabled = stored;
        }
        public void OnPlayerUpdateArmorSets(Player player)
        {
            if (enabled && player.whoAmI == Main.myPlayer)
                player.maxTurrets = turrets;
        }

        public bool OnChatCommand(string command, string[] args)
        {
            if (command != "turrets") return false;

            if (args.Length == 0)
            {
                Main.NewText("Turrets " + (enabled ? "enabled" : "disabled") + " count=" + turrets);
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
                Main.NewText("Usage: /turrets <number|on|off>");
                return true;
            }
            else
            {
                int value;
                if (!int.TryParse(args[0], out value))
                {
                    Main.NewText("Usage: /turrets <number|on|off>");
                    return true;
                }
                if (value < 0) value = 0;
                turrets = value;
                IniAPI.WriteIni("Turrets", "Max", turrets.ToString());
            }

            IniAPI.WriteIni("Turrets", "Enabled", enabled.ToString());
            Main.NewText("Turrets " + (enabled ? "enabled" : "disabled") + " (max " + turrets + ").");
            return true;
        }
    }
}
