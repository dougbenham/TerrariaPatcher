using System;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;

namespace TranscendPlugins
{
    public class Weather : MarshalByRefObject, IPlugin, IPluginChatCommand
    {
        private Keys toggleKey;
        private Keys toggleSlimeKey;

        public Weather()
        {
            if (!Keys.TryParse(IniAPI.ReadIni("Weather", "ToggleRain", "OemSemicolon", writeIt: true), out toggleKey))
                toggleKey = Keys.OemSemicolon;

            if (!Keys.TryParse(IniAPI.ReadIni("Weather", "ToggleSlimeRain", "OemQuotes", writeIt: true), out toggleSlimeKey))
                toggleSlimeKey = Keys.OemQuotes;

            Loader.RegisterHotkey(() =>
            {
                if (Main.raining)
                {
                    Main.StopRain();
                    Main.NewText("Rain stopped.");
                }
                else
                {
                    Main.StartRain();
                    Main.NewText("Rain started.");
                }
            }, toggleKey);

            Loader.RegisterHotkey(() =>
            {
                if (Main.slimeRain)
                {
                    Main.StopSlimeRain();
                    Main.NewText("Slime rain stopped.");
                }
                else
                {
                    Main.StartSlimeRain();
                    Main.NewText("Slime rain started.");
                }
            }, toggleSlimeKey);
        }

        public bool OnChatCommand(string command, string[] args)
        {
            if (command != "weather") return false;

            Action usage = () =>
            {
                Main.NewText("Usage:");
                Main.NewText("  /weather rain [on|off|toggle]");
                Main.NewText("  /weather slime [on|off|toggle]");
            };

            if (args.Length == 0 || args[0] == "help")
            {
                usage();
                return true;
            }

            string target = args[0].ToLower();
            string state = args.Length > 1 ? args[1].ToLower() : "toggle";

            switch (target)
            {
                case "rain":
                    ToggleRain(state);
                    return true;
                case "slime":
                    ToggleSlime(state);
                    return true;
                default:
                    usage();
                    return true;
            }
        }

        private void ToggleRain(string arg)
        {
            bool? desired = ParseState(arg);
            if (desired == null)
                desired = !Main.raining;

            if (desired.Value)
            {
                Main.StartRain();
                Main.NewText("Rain started.");
            }
            else
            {
                Main.StopRain();
                Main.NewText("Rain stopped.");
            }
        }

        private void ToggleSlime(string arg)
        {
            bool? desired = ParseState(arg);
            if (desired == null)
                desired = !Main.slimeRain;

            if (desired.Value)
            {
                Main.StartSlimeRain();
                Main.NewText("Slime rain started.");
            }
            else
            {
                Main.StopSlimeRain();
                Main.NewText("Slime rain stopped.");
            }
        }

        private bool? ParseState(string value)
        {
            if (value.Equals("on", StringComparison.OrdinalIgnoreCase)) return true;
            if (value.Equals("off", StringComparison.OrdinalIgnoreCase)) return false;
            if (value.Equals("toggle", StringComparison.OrdinalIgnoreCase)) return null;
            return null;
        }
    }
}
