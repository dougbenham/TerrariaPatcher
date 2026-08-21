using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;

namespace TranscendPlugins
{
    [PluginDescription("Sets the time of day from a hotkey or /time. The Night and Day hotkeys jump to midnight and noon, " +
                       "or to dusk and dawn while holding Control. Single player only: the server owns the clock and there " +
                       "is no packet to ask it for a change.")]
    public class Time : PluginBase, IPluginChatCommand
    {
        private static readonly HotkeySetting Night = new Hotkey
        {
            Key = Keys.OemComma,
            IgnoreModifierKeys = true,
            Action = () => ChangeTime(Loader.IsControlModifierKeyDown() ? "dusk" : "midnight")
        };

        private static readonly HotkeySetting Day = new Hotkey
        {
            Key = Keys.OemPeriod,
            IgnoreModifierKeys = true,
            Action = () => ChangeTime(Loader.IsControlModifierKeyDown() ? "dawn" : "noon")
        };

        private static void ChangeTime(string time)
        {
            // The server owns the clock and there is no packet to ask it for a change.
            if (Main.netMode != 0)
            {
                Main.NewText("The server controls the time.");
                return;
            }

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

            if (args.Length != 1 || args[0].ToLower() == "help")
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
