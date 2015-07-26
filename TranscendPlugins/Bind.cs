using System;
using System.Linq;
using PluginLoader;
using Terraria;

namespace MrBlueSLPlugins
{
    public class Bind : MarshalByRefObject, IPluginChatCommand
    {
        public bool OnChatCommand(string command, string[] args)
        {
            if (command != "bind" && command != "unbind" && command != "listbinds") return false;
            
            if ((command == "bind" && (args.Length <= 1 || args[0] == "help")) ||
                (command == "unbind" && (args.Length <= 0 || args[0] == "help")) ||
                (command == "listbinds" && args.Length > 0 && args[0] == "help"))
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