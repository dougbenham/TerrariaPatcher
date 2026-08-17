using System;
using PluginLoader;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace BlahPlugins
{
    [PluginDescription("Adds destinations to the Cell Phone beyond home: either ocean, hell, or a random spot. " +
                       "Click the icon by the phone to cycle through them. On a server you arrive in the air and fall, " +
                       "because the ground there is not sent until you are already at the destination.")]
    public class EnhancedCellPhone : PluginBase, IPluginPlayerPreUpdate, IPluginDrawInterface
    {
        enum Modes
        {
            Home = 0,
            LeftOcean = 1,
            RightOcean = 2,
            Hell = 3,
            Random = 4
        }

        private static readonly Setting<Modes> Mode = Modes.Home;

        public void OnPlayerPreUpdate(Player player)
        {
            if (player.whoAmI != Main.myPlayer) return;

            if (player.inventory[player.selectedItem].type == ItemID.CellPhone)
            {
                if (Main.mouseItem.type == ItemID.CellPhone) return; // don't allow it to be on your cursor

                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    if (Mode.Value == Modes.Home) return;

                    player.mouseInterface = true;
                    Main.mouseLeftRelease = false;
                    if (Mode.Value == Modes.LeftOcean)
                    {
                        // left ocean
                        player.Teleport(new Vector2(200 * 16, (float)(Main.worldSurface / 2f) * 16f), 3);
                        SettleOnGround(player);
                    }
                    else if (Mode.Value == Modes.RightOcean)
                    {
                        // right ocean
                        player.Teleport(new Vector2((Main.maxTilesX - 200) * 16, (float)(Main.worldSurface / 2f) * 16f), 3);
                        SettleOnGround(player);
                    }
                    else if (Mode.Value == Modes.Hell)
                    {
                        // hell
                        player.Teleport(new Vector2((Main.maxTilesX / 2) * 16, (float)(Main.maxTilesY - 180) * 16f), 3);
                        SettleOnGround(player);
                    }
                    else if (Mode.Value == Modes.Random)
                    {
                        if (Main.netMode == 0)
                        {
                            player.TeleportationPotion();
                        }
                        else if (Main.netMode == 1 && player.whoAmI == Main.myPlayer)
                        {
                            NetMessage.SendData(73);
                        }
                    }
                    for (int num91 = 0; num91 < 70; num91++)
                    {
                        Dust.NewDust(player.position, player.width, player.height, 15, 0f, 0f, 150, default(Color), 1.5f);
                    }
                }
            }
        }

        /// <summary>
        /// Puts the player on the ground under where they arrived. Nothing to do on a client: it holds no tiles for a
        /// part of the world it has not been near, so there is no ground to find until the server sends the area, and
        /// the player falls onto it then.
        /// </summary>
        private static void SettleOnGround(Player player)
        {
            if (Main.netMode == 0)
            {
                if (!GroundBelow(player, 3))
                {
                    while (!GroundBelow(player, 4) && player.position.Y / 16f < Main.maxTilesY - 10)
                        player.position.Y += 16f;
                }
                else
                {
                    while (GroundBelow(player, 4) && player.position.Y > 0f)
                        player.position.Y -= 16f;
                }
            }

            player.fallStart = (int)(player.position.Y / 16f);
        }

        private static bool GroundBelow(Player player, int tilesBelow)
        {
            var x = (int)(player.position.X / 16f);
            var y = (int)(player.position.Y / 16f) + tilesBelow;

            if (!WorldGen.InWorld(x, y, 1)) return false;

            var tile = Main.tile[x, y];
            return tile != null && tile.active();
        }

        public void OnDrawInterface()
        {
            var player = Main.player[Main.myPlayer];
            if (player.inventory[player.selectedItem].type == ItemID.CellPhone)
            {
                if (Main.mouseItem.type == ItemID.CellPhone) return; // don't allow it to be on your cursor

                if (Main.mouseRight && Main.mouseRightRelease)
                {
                    player.mouseInterface = true;
                    Main.mouseRightRelease = false;

                    Mode.Value = Mode.Value == Modes.Random ? Modes.Home : Mode.Value + 1;
                    Main.NewText("Enhanced CellPhone: " + Mode.Value, 255, 235, 150);
                }
            }
        }
    }
}