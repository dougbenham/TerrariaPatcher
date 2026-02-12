using System;
using PluginLoader;
using Terraria;

namespace Ruffi123456789Plugins
{
    public class Respawn : MarshalByRefObject, IPluginUpdate, IPluginChatCommand
    {
        private int maxTime;
        private bool enabled = true;

        private int RespawnTimerInSeconds
        {
            get
            {
                if (Main.frameRate == 0) return 0;
                return Main.player[Main.myPlayer].respawnTimer / Main.frameRate;
            }
            set { Main.player[Main.myPlayer].respawnTimer = value * Main.frameRate; }
        }

        public Respawn()
        {
            if (!int.TryParse(IniAPI.ReadIni("Respawn", "Time", "0", writeIt: true), out maxTime)) maxTime = 0;
            bool stored;
            if (bool.TryParse(IniAPI.ReadIni("Respawn", "Enabled", "true", writeIt: true), out stored))
                enabled = stored;
        }

        public void OnUpdate()
        {
            if (!enabled) return;

            if (RespawnTimerInSeconds > maxTime)
                RespawnTimerInSeconds = maxTime;
        }

        public bool OnChatCommand(string command, string[] args)
        {
            if (command != "respawn") return false;

            if (args.Length == 0)
            {
                Main.NewText("Respawn " + (enabled ? "enabled" : "disabled") + " time=" + maxTime + "s");
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
                Main.NewText("Usage: /respawn <seconds|on|off>");
                return true;
            }
            else
            {
                int value;
                if (!int.TryParse(args[0], out value))
                {
                    Main.NewText("Usage: /respawn <seconds|on|off>");
                    return true;
                }
                if (value < 0) value = 0;
                maxTime = value;
                IniAPI.WriteIni("Respawn", "Time", maxTime.ToString());
            }

            IniAPI.WriteIni("Respawn", "Enabled", enabled.ToString());
            Main.NewText("Respawn " + (enabled ? "enabled" : "disabled") + " time=" + maxTime + "s.");
            return true;
        }
    }
}
