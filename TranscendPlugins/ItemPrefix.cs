using System;
using PluginLoader;
using Terraria;

namespace TranscendPlugins
{
    public class ItemPrefix : MarshalByRefObject, IPluginChatCommand
    {
        public bool OnChatCommand(string command, string[] args)
        {
            if (command != "prefix") return false;

            if (args.Length < 1 || args.Length > 1 || args[0] == "help")
            {
                Main.NewText("Usage:");
                Main.NewText("   /prefix name");
                Main.NewText("   /prefix id");
                Main.NewText("   /prefix help");
                Main.NewText("Example:");
                Main.NewText("   /prefix mythical");
                return true;
            }

            // get item on cursor, if nothing there, get hotbar item
            var item = Main.mouseItem;
            if (item.type == 0)
            {
                var player = Main.player[Main.myPlayer];
                item = player.inventory[player.selectedItem];
                if (item.type == 0)
                {
                    Main.NewText("No item selected.");
                    return true;
                }
            }

            int prefixId;
            if (!int.TryParse(args[0], out prefixId))
            {
                for (int i = 0; i < Lang.prefix.Length; i++)
                {
                    if (Lang.prefix[i].ToLower() == args[0].ToLower())
                    {
                        prefixId = i;
                        break;
                    }
                }
                if (prefixId == 0)
                {
                    Main.NewText("Invalid prefix ID.");
                    return true;
                }
            }

            if (prefixId == 0)
            {
                var stack = item.stack;
                item.netDefaults(item.type);
                item.stack = stack;
            }
            else if (!item.Prefix(prefixId))
                Main.NewText("Invalid prefix ID for this item type.");
            return true;
        }
    }
}
