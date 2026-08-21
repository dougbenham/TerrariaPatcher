using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;
using Terraria.ID;

namespace TranscendPlugins
{
    [PluginDescription("Removes the delay on building tools: instant mining, block and wall placing, plus block reach " +
                       "and item pickup range added on top of whatever the game already gives you, so the Journey " +
                       "mode Increased Placement Range power and reach accessories still count. /usetime and " +
                       "/autoreuse retune the item you are holding and /range switches the extra range off and on. " +
                       "Works on a server, but a server started with secure=1 counts block placements and boots a " +
                       "client that places more than about one a frame for a couple of seconds, so turn MaxTileSpeed " +
                       "and MaxWallSpeed off on one of those.")]
    public class UseTime : PluginBase, IPluginPlayerUpdateBuffs, IPluginPlayerUpdateArmorSets, IPluginChatCommand
    {
        private static readonly Setting<bool> MaxPickSpeed = true; // Pick / Hammer / Axe
        private static readonly Setting<bool> MaxTileSpeed = true; // Placing blocks / wire
        private static readonly Setting<bool> MaxWallSpeed = true; // Placing wall

        [SettingDescription("Whether Bonus Block Reach is active.")]
        private static readonly Setting<bool> EnableBonusBlockReach = true;

        [SettingRange(0, 1000)]
        [SettingDescription("Additional tiles of block reach.")]
        private static readonly Setting<int> BonusBlockReach = 100;

        [SettingDescription("Whether Bonus Item Pickup Range is active.")]
        private static readonly Setting<bool> EnableBonusItemPickupRange = true;

        [SettingRange(0, 10000)]
        [SettingDescription("Additional pixels of item pickup range.")]
        private static readonly Setting<int> BonusItemPickupRange = 700;

        /// <summary>
        /// An item whose use time is being held at one, with the value to give it back.
        /// </summary>
        private struct Hastened
        {
            public Item Item;
            public int UseTime;
        }

        private readonly string confPath = Environment.CurrentDirectory + "\\ItemConfig.ini";

        /// <summary>
        /// Terraria's own base pickup range, read before anything has changed it, since nothing puts it back.
        /// </summary>
        private readonly int baseItemGrabRange;

        private readonly List<Hastened> hastened = new List<Hastened>();

        /// <summary>
        /// Whether ItemConfig.ini names a use time for an item type, cached because it is asked once a frame.
        /// </summary>
        private readonly Dictionary<int, bool> configuredUseTime = new Dictionary<int, bool>();

        public UseTime() : base(toggleKey: Keys.None)
        {
            baseItemGrabRange = Player.defaultItemGrabRange;

            EnabledChanged += OnEnabledChanged;
        }

        private void OnEnabledChanged()
        {
            if (Enabled) return;

            Player.defaultItemGrabRange = baseItemGrabRange;
            ReleaseHastenedItems();
        }

        public void OnPlayerUpdateBuffs(Player player)
        {
            if (player.whoAmI != Main.myPlayer) return;

            // Player.ResetEffects has just put the block reach back to its base for this frame, the Journey mode
            // Increased Placement Range power included, and Player.UpdateEquips adds the reach accessories after
            // this runs, so adding here leaves both of them worth what the game says they are worth.
            if (EnableBonusBlockReach)
            {
                Player.tileRangeX += BonusBlockReach;
                Player.tileRangeY += BonusBlockReach;
            }

            // Nothing puts this one back each frame, so it is assigned from the base rather than added to. The
            // magnet accessories and the Journey mode pickup bonus are added to it in Player.GetItemGrabRange and
            // so are unaffected either way.
            Player.defaultItemGrabRange = EnableBonusItemPickupRange
                ? baseItemGrabRange + BonusItemPickupRange
                : baseItemGrabRange;
        }

        public void OnPlayerUpdateArmorSets(Player player)
        {
            if (player.whoAmI != Main.myPlayer) return;

            ReleaseHastenedItems();

            var held = player.HeldItem;
            Hasten(held);

            // Swapping one block for another mines it with the best pickaxe in the inventory rather than with the
            // item being held.
            if (MaxPickSpeed)
            {
                var pickaxe = player.GetBestPickaxe();
                if (!ReferenceEquals(pickaxe, held)) Hasten(pickaxe);
            }

            // Player.RollerSkateMovement divides sixteen by the held item's use time scaled by tileSpeed, and any
            // bonus that raises tileSpeed rounds that divisor to zero once the use time is one. Holding the
            // multiplier at one keeps the division safe and costs nothing, since a use time of one already places
            // a block every frame. tileSpeed is still the additive form here; Player.Update inverts it afterwards.
            if (held.useTime <= 1 && (held.createTile >= 0 || held.tileWand > 0)) player.tileSpeed = 1;
        }

        /// <summary>
        /// Holds an item's use time at one until the next frame, recording what it was so that switching the
        /// setting off, or the plugin off, gives the item straight back.
        /// </summary>
        private void Hasten(Item item)
        {
            if (!ShouldHasten(item)) return;

            hastened.Add(new Hastened { Item = item, UseTime = item.useTime });
            item.useTime = 1;
        }

        private void ReleaseHastenedItems()
        {
            foreach (var entry in hastened)
            {
                // An item that is no longer at one has been retuned since, by /usetime or by a reforge, and that
                // newer value is the one to keep.
                if (entry.Item.useTime == 1) entry.Item.useTime = entry.UseTime;
            }

            hastened.Clear();
        }

        private bool ShouldHasten(Item item)
        {
            // Nothing to gain on an item that is already at one, and no value worth recording either.
            if (item == null || item.type == ItemID.None || item.useTime <= 1) return false;

            // A use time the player set with /usetime is theirs to keep.
            if (HasConfiguredUseTime(item.type)) return false;

            if (MaxPickSpeed && (item.axe > 0 ||
                                 item.pick > 0 ||
                                 item.hammer > 0))
                return true;

            if (MaxTileSpeed &&
                (item.createTile >= 0 ||
                 item.type == ItemID.Wrench ||
                 item.type == ItemID.BlueWrench ||
                 item.type == ItemID.GreenWrench ||
                 item.type == ItemID.WireCutter ||
                 item.type == ItemID.Actuator))
                return true;

            return MaxWallSpeed && item.createWall > 0;
        }

        /// <summary>
        /// Whether ItemConfig.ini gives an item type a use time of its own, which /usetime writes and the
        /// ItemConfig plugin applies to every item of that type.
        /// </summary>
        private bool HasConfiguredUseTime(int type)
        {
            bool configured;
            if (!configuredUseTime.TryGetValue(type, out configured))
            {
                configured = IniAPI.ReadIni("item" + type, "useTime", "", path: confPath) != "";
                configuredUseTime[type] = configured;
            }

            return configured;
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

            if (command == "range")
            {
                EnableBonusBlockReach.Value = !EnableBonusBlockReach;
                EnableBonusItemPickupRange.Value = EnableBonusBlockReach;
                Main.NewText("Block reach and item pickup range is " + (EnableBonusBlockReach ? "extended" : "back to normal") + ".");
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

                        IniAPI.WriteIni("item" + item.type, "useTime", num.ToString(), confPath);
                        configuredUseTime[item.type] = true;

                        ReleaseHastenedItems();
                        item.useTime = num;
                    }
                    else
                    {
                        IniAPI.WriteIni("item" + item.type, "useTime", null, confPath);
                        configuredUseTime[item.type] = false;

                        ReleaseHastenedItems();

                        // Clone item (preserve stack/favorited/prefix/auto-reuse)
                        var stack = item.stack;
                        bool favorited = item.favorited;
                        var prefix = item.prefix;
                        var autoreuse = item.autoReuse;
                        item.netDefaults(item.type);
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
            }
            return true;
        }
    }
}
