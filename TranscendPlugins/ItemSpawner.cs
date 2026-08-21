using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;
using Terraria.ID;
using TranscendPlugins.Shared;

namespace Ruffi123456789Plugins
{
    [PluginDescription("Spawns any item with /item by id or name, with an optional count, and finds an item's id with " +
                       "/item search. Holding Control and typing an item id on the number pad spawns it too.")]
    public class ItemSpawner : PluginBase, IPluginUpdate, IPluginChatCommand
    {
        private string toSpawn = "";

        private static IdLookup items;

        private static IdLookup Items
        {
            get { return items ?? (items = new IdLookup(typeof(ItemID))); }
        }

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
                    Main.player[Main.myPlayer].QuickSpawnItem(null, id);
                toSpawn = "";
            }
        }

        public bool OnChatCommand(string command, string[] args)
        {
            if (command != "item") return false;

            var option = args.Length > 0 ? args[0].ToLower() : "";

            if (args.Length < 1 || args.Length > 2 || option == "help")
            {
                Main.NewText("Usage:");
                Main.NewText("  /item id [count]");
                Main.NewText("  /item name [count]");
                Main.NewText("  /item search text");
                Main.NewText("Example:");
                Main.NewText("  /item 123");
                Main.NewText("  /item 85 5");
                Main.NewText("  /item ChlorophyteBullet 200");
                Main.NewText("  /item search bullet");
                return true;
            }

            if (option == "search")
            {
                if (args.Length < 2)
                {
                    Main.NewText("Usage: /item search text");
                    return true;
                }

                IdLookup.Report(Items.Search(args[1]), "items");
                return true;
            }

            int itemId;
            if (!int.TryParse(args[0], out itemId) && !Items.TryGetId(args[0], out itemId))
            {
                Main.NewText("Invalid ItemID.");
                return true;
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

            Main.player[Main.myPlayer].QuickSpawnItem(null, itemId, count);
            return true;
        }
    }
}
