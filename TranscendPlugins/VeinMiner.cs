using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace TranscendPlugins
{
    [PluginDescription("Mining one block of an ore or gem vein mines the rest of it, up to MaxBlocks. " +
                       "Works in multiplayer, where the vein is mined more slowly so the server never sees " +
                       "it as tile removal spam.")]
    public class VeinMiner : PluginBase, IPluginUpdate, IPluginPlayerPickTile
    {
        [SettingRange(1, 10000)]
        [SettingDescription("The most blocks one swing will mine out of a single vein.")]
        private static readonly Setting<int> MaxBlocks = 300;

        /// <summary>
        /// Limits a vein to the distance you could mine it by hand.
        /// </summary>
        [SettingDescription("Keeps a vein within the reach you could mine it by hand.")]
        private static readonly Setting<bool> RangeLimit = false;

        /// <summary>
        /// Charges mana per block, so a vein costs something. A better pickaxe costs less per block.
        /// </summary>
        [SettingDescription("Charges mana per block, less with a better pickaxe.")]
        private static readonly Setting<bool> RequireMana = false;

        [SettingIds(typeof(TileID))]
        [SettingDescription("The tiles a swing mines the whole vein of.")]
        private static readonly Setting<HashSet<ushort>> Tiles = new HashSet<ushort>
        {
            TileID.Copper, TileID.Tin, TileID.Iron, TileID.Lead, TileID.Silver, TileID.Tungsten,
            TileID.Gold, TileID.Platinum, TileID.Demonite, TileID.Crimtane, TileID.Meteorite,
            TileID.Obsidian, TileID.Hellstone, TileID.Cobalt, TileID.Palladium, TileID.Mythril,
            TileID.Orichalcum, TileID.Adamantite, TileID.Titanium, TileID.Chlorophyte, TileID.LunarOre,
            TileID.Amethyst, TileID.Topaz, TileID.Sapphire, TileID.Emerald, TileID.Ruby, TileID.Diamond
        };

        /// <summary>
        /// Divided by the pickaxe power to get the mana each block costs, so a Copper Pickaxe pays about 2.9 and a
        /// Luminite Pickaxe about 0.4.
        /// </summary>
        private const float ManaCostPerBlock = 100f;

        /// <summary>
        /// A server raises RemoteClient.SpamDeleteBlock by one for every tile removal a client sends it, lowers it by
        /// five each tick from Main.UpdateServer, and boots the client at 500. Staying under that drain keeps the
        /// counter flat no matter how long the vein is, so mining a vein can never be what gets you kicked.
        /// </summary>
        private const int BlocksPerTickInMultiplayer = 4;

        /// <summary>
        /// No packets are sent in single player, so the only limit is how much tile update the game will take.
        /// </summary>
        private const int BlocksPerTickInSinglePlayer = 32;

        private readonly Queue<Point> queue = new Queue<Point>();
        private readonly HashSet<int> seen = new HashSet<int>();
        private int veinType = -1;
        private int veinPickPower;
        private int mined;
        private float owedMana;

        /// <summary>
        /// The vein tile the player is currently swinging at, recorded while it still exists. If it has gone by the
        /// next update then that swing is what broke it.
        /// </summary>
        private Point swungAt;
        private int swungAtType = -1;
        private int swungAtPickPower;

        public VeinMiner() : base(toggleKey: Keys.None)
        { }

        /// <summary>
        /// Runs for every swing that lands on a tile, including drills and the other tools that pick tiles through
        /// projectiles, since they all come through this same method.
        /// </summary>
        public void OnPlayerPickTile(Player player, int x, int y, int pickPower)
        {
            if (player.whoAmI != Main.myPlayer) return;
            if (queue.Count > 0 || swungAtType >= 0) return;
            if (!WorldGen.InWorld(x, y, 1)) return;

            var tile = Main.tile[x, y];
            if (tile == null || !tile.active() || !Tiles.Value.Contains(tile.type)) return;

            swungAt = new Point(x, y);
            swungAtType = tile.type;
            swungAtPickPower = pickPower;
        }

        public void OnUpdate()
        {
            var player = Player;

            if (Main.gameMenu || player == null || !player.active || player.dead)
            {
                Stop();
                return;
            }

            if (queue.Count > 0)
            {
                MineQueued(player);
                return;
            }

            if (swungAtType < 0) return;

            var from = swungAt;
            var type = swungAtType;
            var pickPower = swungAtPickPower;

            swungAtType = -1;

            // Still there, so that swing only damaged it. The player has not finished a block yet.
            var tile = Main.tile[from.X, from.Y];
            if (tile != null && tile.active() && tile.type == type) return;

            veinType = type;
            veinPickPower = pickPower;
            mined = 0;
            owedMana = 0f;
            seen.Clear();
            queue.Clear();

            Spread(player, from.X, from.Y);

            if (queue.Count > 0) MineQueued(player);
        }

        private void MineQueued(Player player)
        {
            var budget = Main.netMode == 0 ? BlocksPerTickInSinglePlayer : BlocksPerTickInMultiplayer;

            while (budget-- > 0 && queue.Count > 0 && mined < MaxBlocks)
            {
                var point = queue.Dequeue();

                var tile = Main.tile[point.X, point.Y];
                if (tile == null || !tile.active() || tile.type != veinType) continue;

                if (!PayMana(player))
                {
                    Stop();
                    return;
                }

                // On a client the server is the one that drops the item, so mining it here as well would duplicate it.
                WorldGen.KillTile(point.X, point.Y, false, false, Main.netMode == 1);

                if (Main.netMode != 0)
                    NetMessage.SendData(17, -1, -1, null, 0, point.X, point.Y, 0f, 0, 0, 0);

                mined++;

                Spread(player, point.X, point.Y);
            }

            if (queue.Count == 0 || mined >= MaxBlocks) Stop();
        }

        /// <summary>
        /// Charges for one block. Costs below a full point of mana are carried over rather than rounded away, so a
        /// strong pickaxe is cheaper per block instead of free.
        /// </summary>
        private bool PayMana(Player player)
        {
            if (!RequireMana) return true;

            owedMana += ManaCostPerBlock / (veinPickPower > 0 ? veinPickPower : 1);

            var due = (int) owedMana;
            if (due < 1) return true;

            if (!player.CheckMana(due, true)) return false;

            owedMana -= due;
            return true;
        }

        private void Spread(Player player, int x, int y)
        {
            for (var dx = -1; dx <= 1; dx++)
            {
                for (var dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    Enqueue(player, x + dx, y + dy);
                }
            }
        }

        private void Enqueue(Player player, int x, int y)
        {
            if (!WorldGen.InWorld(x, y, 1)) return;

            // Every tile is looked at once, whether or not it turns out to be part of the vein.
            if (!seen.Add(y * Main.maxTilesX + x)) return;

            var tile = Main.tile[x, y];
            if (tile == null || !tile.active() || tile.type != veinType) return;

            // The same reach the game gives the player for mining by hand, so a vein is never followed through a wall or across the map.
            if (RangeLimit &&
                !player.IsInTileInteractionRange(x, y, TileReachCheckSettings.Simple, Held(player).tileBoost)) return;

            queue.Enqueue(new Point(x, y));
        }

        private static Item Held(Player player)
        {
            return player.inventory[player.selectedItem];
        }

        private void Stop()
        {
            swungAtType = -1;
            queue.Clear();
            seen.Clear();
            veinType = -1;
            veinPickPower = 0;
            mined = 0;
            owedMana = 0f;
        }
    }
}
