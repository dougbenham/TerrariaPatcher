using System;
using System.Linq;
using PluginLoader;
using Terraria;

namespace MrBlueSLPlugins
{
    [PluginDescription("Binds any chat command to a key with /bind, so /bind Control,T /time dusk makes Ctrl+T change the time. Binds are kept in Plugins.ini.")]
    public class Bind : PluginBase, IPluginChatCommand
    {
        public bool OnChatCommand(string command, string[] args)
        {
            if (command != "bind" && command != "unbind" && command != "listbinds") return false;

            var option = args.Length > 0 ? args[0].ToLower() : "";

            if ((command == "bind" && (args.Length <= 1 || option == "help")) ||
                (command == "unbind" && (args.Length <= 0 || option == "help")) ||
                (command == "listbinds" && args.Length > 0 && option == "help"))
            {
                Main.NewText("Usage:");
                Main.NewText("  /bind modifiers,hotkey command");
                Main.NewText("  /unbind modifiers,hotkey");
                Main.NewText("  /listbinds");
                Main.NewText("Example:");
                Main.NewText("  /bind Control,T /time dusk");
                Main.NewText("  /unbind Control,T");
                Main.NewText("  /bind Control,Shift,K /usetime");
                return true;
            }
            
            if (command == "bind")
                BindHotkey(args[0], string.Join(" ", args.Skip(1)));
            else if (command == "unbind")
                UnbindHotkey(args[0]);
            else if (command == "listbinds")
            {
                foreach (var hotkey in Loader.GetHotkeys().Where(hotkey => !string.IsNullOrEmpty(hotkey.Tag)))
                    Main.NewText(hotkey.ToString());
            }
            return true;
        }

        private void BindHotkey(string hotkey, string cmd)
        {
            var key = Loader.ParseHotkey(hotkey);

            if (string.IsNullOrEmpty(cmd) || !cmd.StartsWith("/") || key == null)
                Main.NewText("Invalid hotkey binding");
            else
            {
                IniAPI.WriteIni("HotkeyBinds", hotkey, cmd);
                Loader.RegisterHotkey(cmd, key);
                Main.NewText(hotkey + " set to " + cmd);
            }
        }

        private void UnbindHotkey(string hotkey)
        {
            var key = Loader.ParseHotkey(hotkey);

            if (key == null)
                Main.NewText("Invalid hotkey binding");
            else
            {
                IniAPI.WriteIni("HotkeyBinds", hotkey, null);
                Loader.UnregisterHotkey(key);
                Main.NewText("Unbound " + hotkey);
            }
        }
    }
}