using System;
using System.Reflection;
using System.Windows.Forms;
using PluginLoader;
using Terraria;
using Terraria.ID;

namespace TranscendPlugins
{
    public class UseTime : MarshalByRefObject, IPluginItemSetDefaults, IPluginPlayerUpdateBuffs, IPluginChatCommand
    {
        private bool maxTileSpeed, maxWallSpeed, maxPickSpeed, maxReachRange, maxItemPickupRange;
        private bool builderBuffWarning = false;

        private int DefaultItemGrabRange
        {
            set
            {
                var player = Assembly.GetEntryAssembly().GetType("Terraria.Player");
                var defaultItemGrabRange = player.GetField("defaultItemGrabRange", BindingFlags.Static | BindingFlags.NonPublic);
                defaultItemGrabRange.SetValue(null, value);
            }
        }

        public UseTime()
        {
            maxPickSpeed = bool.Parse(IniAPI.ReadIni("UseTime", "MaxPickSpeed", "true", writeIt: true)); // Pick / Hammer / Axe
            maxTileSpeed = bool.Parse(IniAPI.ReadIni("UseTime", "MaxTileSpeed", "true", writeIt: true)); // Placing blocks / wire
            maxWallSpeed = bool.Parse(IniAPI.ReadIni("UseTime", "MaxWallSpeed", "true", writeIt: true)); // Placing wall
            maxReachRange = bool.Parse(IniAPI.ReadIni("UseTime", "MaxReachRange", "true", writeIt: true)); // Block reach
            maxItemPickupRange = bool.Parse(IniAPI.ReadIni("UseTime", "MaxItemPickupRange", "true", writeIt: true)); // Item pickup range
        }

        public void OnItemSetDefaults(Item item)
        {
            if (maxPickSpeed && (item.axe > 0 ||
                                 item.pick > 0 ||
                                 item.hammer > 0))
                item.useTime = 0;

            if (maxTileSpeed &&
                (item.createTile >= 0 ||
                 item.type == ItemID.Wrench ||
                 item.type == ItemID.BlueWrench ||
                 item.type == ItemID.GreenWrench ||
                 item.type == ItemID.WireCutter ||
                 item.type == ItemID.Actuator))
                item.useTime = 0;

            if (maxWallSpeed && item.createWall > 0)
                item.useTime = 0;
        }

        public void OnPlayerUpdateBuffs(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                if (!builderBuffWarning)
                {
                    for (int k = 0; k < 22; k++)
                    {
                        if (player.buffType[k] == BuffID.Builder && player.buffTime[k] > 0)
                        {
                            builderBuffWarning = true;
                            MessageBox.Show("Please disable the Builder buff, it causes issues with the UseTime plugin.");
                        }
                    }
                }

                if (maxReachRange)
                {
                    Player.tileRangeX = 100;
                    Player.tileRangeY = 100;
                }
                if (maxItemPickupRange)
                    DefaultItemGrabRange = 700;
            }
        }

        public bool OnChatCommand(string command, string[] args)
        {
            if (command != "usetime" && command != "autoreuse") return false;

            if (!(command == "usetime" && args.Length == 1) && !(command == "autoreuse" && args.Length == 0))
            {
                Main.NewText("Usage:");
                Main.NewText("   /autoreuse");
                Main.NewText("   /usetime num");
                Main.NewText("Example:");
                Main.NewText("   /usetime 0");
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
                    int num;
                    if (!int.TryParse(args[0], out num))
                    {
                        Main.NewText("Invalid num.");
                        break;
                    }

                    item.useTime = num;
                    Main.NewText("UseTime = " + num);
                    break;
                case "autoreuse":
                    item.autoReuse = !item.autoReuse;
                    Main.NewText("AutoReuse = " + item.autoReuse);
                    break;
            }
            return true;
        }
    }
}
