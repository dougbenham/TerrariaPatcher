using System;
using PluginLoader;
using Terraria;

namespace TranscendPlugins
{
    public class Season : MarshalByRefObject, IPluginCheckSeason, IPluginChatCommand
    {
        private bool xmas, halloween;

        public Season()
        {
            if (!bool.TryParse(IniAPI.ReadIni("Season", "Xmas", "false", writeIt: true), out xmas))
                xmas = false;
            if (!bool.TryParse(IniAPI.ReadIni("Season", "Halloween", "false", writeIt: true), out halloween))
                halloween = false;
        }

        public bool OnCheckXmas()
        {
            Main.xMas = xmas;
            return true;
        }

        public bool OnCheckHalloween()
        {
            Main.halloween = halloween;
            return true;
        }

        public bool OnChatCommand(string command, string[] args)
        {
            if (command != "season") return false;

            Action usage = () =>
            {
                Main.NewText("Usage:");
                Main.NewText("  /season none");
                Main.NewText("  /season xmas");
                Main.NewText("  /season halloween");
                Main.NewText("  /season help");
            };

            if (args.Length < 1 || args.Length > 1 || args[0] == "help")
            {
                usage();
                return true;
            }

            switch (args[0])
            {
                case "none":
                    IniAPI.WriteIni("Season", "Xmas", (xmas = false).ToString());
                    IniAPI.WriteIni("Season", "Halloween", (halloween = false).ToString());
                    Main.NewText("Christmas & Halloween disabled!");
                    return true;
                case "xmas":
                    IniAPI.WriteIni("Season", "Xmas", (xmas = !xmas).ToString());
                    IniAPI.WriteIni("Season", "Halloween", (halloween = false).ToString());
                    Main.NewText("Christmas " + (xmas ? "enabled" : "disabled") + "!");
                    return true;
                case "halloween":
                    IniAPI.WriteIni("Season", "Xmas", (xmas = false).ToString());
                    IniAPI.WriteIni("Season", "Halloween", (halloween = !halloween).ToString());
                    Main.NewText("Halloween " + (halloween ? "enabled" : "disabled") + "!");
                    return true;
                default:
                    usage();
                    return true;
            }
        }
    }
}
