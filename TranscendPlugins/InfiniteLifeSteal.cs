using System;
using PluginLoader;
using Terraria;

namespace TranscendPlugins
{
    public class InfiniteLifeSteal : MarshalByRefObject, IPluginPlayerUpdate, IPluginChatCommand
    {
        private bool enabled;

        public InfiniteLifeSteal()
        {
            if (!bool.TryParse(IniAPI.ReadIni("InfiniteLifeSteal", "Enabled", "true", writeIt: true), out enabled))
                enabled = true;
        }

        public void OnPlayerUpdate(Player player)
        {
            if (!enabled) return;
            if (player.whoAmI == Main.myPlayer)
                player.lifeSteal = 10000;
        }

        public bool OnChatCommand(string command, string[] args)
        {
            if (command != "lifesteal") return false;

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
                    Main.NewText("Usage: /lifesteal [on|off|toggle]");
                    return true;
                default:
                    Main.NewText("Usage: /lifesteal [on|off|toggle]");
                    return true;
            }

            IniAPI.WriteIni("InfiniteLifeSteal", "Enabled", enabled.ToString());
            Main.NewText("Infinite Life Steal " + (enabled ? "enabled" : "disabled") + ".");
            return true;
        }
    }
}
