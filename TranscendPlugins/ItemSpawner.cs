using System;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;
using Terraria.ID;

namespace Ruffi123456789Plugins
{
    public class ItemSpawner : MarshalByRefObject, IPluginUpdate, IPluginChatCommand
    {
        private string toSpawn = "";

        public ItemSpawner()
        {
            for (int i = 0; i < 10; i++)
            {
                int j = i;
                Keys key;
                if (Keys.TryParse("NumPad" + j, out key))
                {
                    Loader.RegisterHotkey(() =>
                    {
                        toSpawn = toSpawn + j;
                    }, key, control: true);
                }
            }
        }

        public void OnUpdate()
        {
            if (toSpawn != "" && !Loader.IsControlModifierKeyDown())
            {
                int id;
                if (int.TryParse(toSpawn, out id))
                    Main.player[Main.myPlayer].QuickSpawnItem(id);
                toSpawn = "";
            }
        }

        public bool OnChatCommand(string command, string[] args)
        {
            if (command != "item") return false;

            if (args.Length < 1 || args.Length > 2 || args[0] == "help")
            {
                Main.NewText("Usage:");
                Main.NewText("  /item id [count]");
                Main.NewText("  /item name [count]");
                Main.NewText("Example:");
                Main.NewText("  /item 123");
                Main.NewText("  /item 85 5");
                Main.NewText("  /item ChlorophyteBullet 200");
                return true;
            }

            int itemId;
            if (!int.TryParse(args[0], out itemId))
            {
                var field = typeof(ItemID).GetFields().FirstOrDefault(info => info.Name.ToLower() == args[0].ToLower());
                if (field == null)
                {
                    Main.NewText("Invalid ItemID.");
                    return true;
                }
                itemId = Convert.ToInt32(field.GetValue(null));
            }

            int count = 1;
            if (args.Length == 2)
            {
                if (!int.TryParse(args[1], out count))
                {
                    Main.NewText("Invalid count.");
                    return true;
                }
            }

            Main.player[Main.myPlayer].QuickSpawnItem(itemId, count);
            return true;
        }
    }
}
