using System;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;

namespace TranscendPlugins
{
    public class Time : MarshalByRefObject, IPluginChatCommand
    {
        private Keys nightKey, dayKey;
        public Time()
        {
            if (!Keys.TryParse(IniAPI.ReadIni("Time", "Night", "OemComma", writeIt: true), out nightKey))
                nightKey = Keys.OemComma;
            if (!Keys.TryParse(IniAPI.ReadIni("Time", "Day", "OemPeriod", writeIt: true), out dayKey))
                dayKey = Keys.OemPeriod;

            Loader.RegisterHotkey(() => ChangeTime(Loader.IsControlModifierKeyDown() ? "dusk" : "midnight"), nightKey, ignoreModifierKeys: true);
            Loader.RegisterHotkey(() => ChangeTime(Loader.IsControlModifierKeyDown() ? "dawn" : "noon"), dayKey, ignoreModifierKeys: true);
        }

        private void ChangeTime(string time)
        {
            switch (time.ToLower())
            {
                case "dusk":
                    Main.dayTime = true;
                    Main.time = 54001.0; // 7:30 PM (dusk), triggers all night time events
                    Main.NewText("Time changed to dusk.");
                    break;
                case "midnight":
                    Main.dayTime = false;
                    Main.time = 16200.0; // 12:00 AM (midnight)
                    Main.NewText("Time changed to midnight.");
                    break;
                case "dawn":
                    Main.dayTime = false;
                    Main.time = 32401.0; // 4:30 AM (dawn), triggers all day time events
                    Main.NewText("Time changed to dawn.");
                    break;
                case "noon":
                    Main.dayTime = true;
                    Main.time = 27000.0; // 12:00 PM (noon)
                    Main.NewText("Time changed to noon.");
                    break;
            }
        }

        public bool OnChatCommand(string command, string[] args)
        {
            if (command != "time") return false;
            
            if (args.Length < 1 || args.Length > 1 || args[0] == "help")
            {
                Main.NewText("Usage:");
                Main.NewText("   /time dawn");
                Main.NewText("   /time noon");
                Main.NewText("   /time midnight");
                Main.NewText("   /time dusk");
                Main.NewText("   /time help");
                return true;
            }

            ChangeTime(args[0]);
            return true;
        }
    }
}
