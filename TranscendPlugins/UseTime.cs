using System;
using PluginLoader;
using Terraria;
using Terraria.ID;

namespace TranscendPlugins
{
    [PluginDescription("Removes the delay on building tools: instant mining, block and wall placing, plus greatly extended " +
                       "block reach and item pickup range. /usetime and /autoreuse also retune the item you are holding. " +
                       "Works on a server, but a server started with secure=1 counts block placements and boots a client " +
                       "that places more than about one a frame for a couple of seconds, so turn MaxTileSpeed and " +
                       "MaxWallSpeed off on one of those.")]
    public class UseTime : PluginBase, IPluginItemSetDefaults, IPluginPlayerUpdateBuffs, IPluginPlayerUpdateArmorSets, IPluginChatCommand
    {
        private static readonly Setting<bool> MaxPickSpeed = true; // Pick / Hammer / Axe
        private static readonly Setting<bool> MaxTileSpeed = true; // Placing blocks / wire
        private static readonly Setting<bool> MaxWallSpeed = true; // Placing wall
        private static readonly Setting<bool> MaxReachRange = true; // Block reach
        private static readonly Setting<bool> MaxItemPickupRange = true; // Item pickup range

        private readonly string confPath = Environment.CurrentDirectory + "\\ItemConfig.ini";
        private readonly int initialTileRangeX, initialTileRangeY, initialDefaultItemGrabRange;
        private bool resetUseTime;

        public UseTime()
        {
            initialTileRangeX = Player.tileRangeX;
            initialTileRangeY = Player.tileRangeY;
            initialDefaultItemGrabRange = Player.defaultItemGrabRange;
        }

        public void OnItemSetDefaults(Item item)
        {
            if (resetUseTime) return;

            if (MaxPickSpeed && (item.axe > 0 ||
                                 item.pick > 0 ||
                                 item.hammer > 0))
                item.useTime = 1;

            if (MaxTileSpeed &&
                (item.createTile >= 0 ||
                 item.type == ItemID.Wrench ||
                 item.type == ItemID.BlueWrench ||
                 item.type == ItemID.GreenWrench ||
                 item.type == ItemID.WireCutter ||
                 item.type == ItemID.Actuator))
                item.useTime = 1;

            if (MaxWallSpeed && item.createWall > 0)
                item.useTime = 1;
        }

        public void OnPlayerUpdateBuffs(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                if (MaxReachRange)
                {
                    Player.tileRangeX = 100;
                    Player.tileRangeY = 100;
                }
                else
                {
                    Player.tileRangeX = initialTileRangeX;
                    Player.tileRangeY = initialTileRangeY;
                }

                Player.defaultItemGrabRange = MaxItemPickupRange ? 700 : initialDefaultItemGrabRange;
            }
        }

        public void OnPlayerUpdateArmorSets(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                // Disables effects of Builder Buff and the following items: Architect Gizmo Pack, Brick Layer, Portable Cement Mixer
                player.tileSpeed = 1;
                player.wallSpeed = 1;
            }
        }

        public bool OnChatCommand(string command, string[] args)
        {
            if (command != "usetime" && command != "autoreuse" && command != "range") return false;

            if (!(command == "usetime" && args.Length <= 1) &&
                !(command == "autoreuse" && args.Length == 0) &&
                !(command == "range" && args.Length == 0))
            {
                Main.NewText("Usage:");
                Main.NewText("   /autoreuse");
                Main.NewText("   /usetime [num]");
                Main.NewText("   /range");
                Main.NewText("Example:");
                Main.NewText("   /usetime 1");
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

            switch (command)
            {
                case "usetime":
                    if (args.Length == 1)
                    {
                        int num;
                        if (!int.TryParse(args[0], out num))
                        {
                            Main.NewText("Invalid num.");
                            break;
                        }

                        if (num == 0)
                        {
	                        Main.NewText("Warning, using 0 can break items.");
                        }

                        item.useTime = num;

                        IniAPI.WriteIni("item" + item.type, "useTime", num.ToString(), confPath);
                    }
                    else
                    {
                        IniAPI.WriteIni("item" + item.type, "useTime", null, confPath);

                        // Clone item (preserve stack/favorited/prefix/auto-reuse)
                        var stack = item.stack;
                        bool favorited = item.favorited;
                        var prefix = item.prefix;
                        var autoreuse = item.autoReuse;
                        resetUseTime = true;
                        item.netDefaults(item.type);
                        resetUseTime = false;
                        item.Prefix(prefix);
                        item.autoReuse = autoreuse;
                        item.stack = stack;
                        item.favorited = favorited;
                    }

                    Main.NewText("UseTime = " + item.useTime);
                    break;
                case "autoreuse":
                    item.autoReuse = !item.autoReuse;
                    Main.NewText("AutoReuse = " + item.autoReuse);

                    IniAPI.WriteIni("item" + item.type, "autoReuse", item.autoReuse.ToString(), confPath);
                    break;
                case "range":
                    MaxReachRange.Value = !MaxReachRange;
                    MaxItemPickupRange.Value = MaxReachRange;
                    Main.NewText("Block reach and item pickup range is " + (MaxReachRange ? "enhanced" : "back to normal") + ".");
                    break;
            }
            return true;
        }
    }
}
