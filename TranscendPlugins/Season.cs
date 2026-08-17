using System;
using PluginLoader;
using Terraria;

namespace TranscendPlugins
{
    [PluginDescription("Forces the Christmas or Halloween season on regardless of the date, for the seasonal drops and " +
                       "decorations. Switch with /season.")]
    public class Season : PluginBase, IPluginCheckSeason, IPluginChatCommand
    {
        private static readonly Setting<bool> Xmas = false;
        private static readonly Setting<bool> Halloween = false;

        public bool OnCheckXmas()
        {
            Main.xMas = Xmas;
            return true;
        }

        public bool OnCheckHalloween()
        {
            Main.halloween = Halloween;
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

            var option = args.Length > 0 ? args[0].ToLower() : "";

            if (args.Length != 1 || option == "help")
            {
                usage();
                return true;
            }

            switch (option)
            {
                case "none":
                    Xmas.Value = false;
                    Halloween.Value = false;
                    Main.NewText("Christmas & Halloween disabled!");
                    return true;
                case "xmas":
                    Xmas.Value = !Xmas;
                    Halloween.Value = false;
                    Main.NewText("Christmas " + (Xmas ? "enabled" : "disabled") + "!");
                    return true;
                case "halloween":
                    Xmas.Value = false;
                    Halloween.Value = !Halloween;
                    Main.NewText("Halloween " + (Halloween ? "enabled" : "disabled") + "!");
                    return true;
                default:
                    usage();
                    return true;
            }
        }
    }
}
