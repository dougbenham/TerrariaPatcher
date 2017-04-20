using System;
using PluginLoader;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace BlahPlugins
{
    public class EnhancedCellPhone : MarshalByRefObject, IPluginPlayerPreUpdate, IPluginDrawInterface
    {
        private Mode mode = Mode.Home;
        enum Mode
        {
            Home = 0,
            LeftOcean = 1,
            RightOcean = 2,
            Hell = 3,
            Random = 4
        }

        public EnhancedCellPhone()
        {
            if (!Mode.TryParse(IniAPI.ReadIni("EnhancedCellPhone", "Mode", "Home", writeIt: true), out mode)) mode = Mode.Home;
        }

        public void OnPlayerPreUpdate(Player player)
        {
            if (player.whoAmI != Main.myPlayer) return;

            if (player.inventory[player.selectedItem].type == ItemID.CellPhone)
            {
                if (Main.mouseItem.type == ItemID.CellPhone) return; // don't allow it to be on your cursor

                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    if (mode == Mode.Home) return;

                    player.mouseInterface = true;
                    Main.mouseLeftRelease = false;
                    if (mode == Mode.LeftOcean)
                    {
                        // left ocean
                        player.Teleport(new Vector2(200 * 16, (float)(Main.worldSurface / 2f) * 16f), 3);
                        if (!Main.tile[(int)(player.position.X / 16f), (int)(player.position.Y / 16f) + 3].active())
                        {
                            while (!Main.tile[(int)(player.position.X / 16f), (int)(player.position.Y / 16f) + 4].active())
                            {
                                player.position.Y += 16f;
                            }
                        }
                        else
                        {
                            while (Main.tile[(int)(player.position.X / 16f), (int)(player.position.Y / 16f) + 4].active())
                            {
                                player.position.Y -= 16f;
                            }
                        }
                        player.fallStart = (int)(player.position.Y / 16f);
                        if (Main.netMode == 1) NetMessage.SendTileSquare(player.whoAmI, 200, (int)Main.worldSurface / 2, 10);
                    }
                    else if (mode == Mode.RightOcean)
                    {
                        // right ocean
                        player.Teleport(new Vector2((Main.maxTilesX - 200) * 16, (float)(Main.worldSurface / 2f) * 16f), 3);
                        if (!Main.tile[(int)(player.position.X / 16f), (int)(player.position.Y / 16f) + 3].active())
                        {
                            while (!Main.tile[(int)(player.position.X / 16f), (int)(player.position.Y / 16f) + 4].active())
                            {
                                player.position.Y += 16f;
                            }
                        }
                        else
                        {
                            while (Main.tile[(int)(player.position.X / 16f), (int)(player.position.Y / 16f) + 4].active())
                            {
                                player.position.Y -= 16f;
                            }
                        }
                        player.fallStart = (int)(player.position.Y / 16f);
                        if (Main.netMode == 1) NetMessage.SendTileSquare(player.whoAmI, Main.maxTilesX - 200, (int)Main.worldSurface / 2, 10);
                    }
                    else if (mode == Mode.Hell)
                    {
                        // hell
                        player.Teleport(new Vector2((Main.maxTilesX / 2) * 16, (float)(Main.maxTilesY - 180) * 16f), 3);
                        if (!Main.tile[(int)(player.position.X / 16f), (int)(player.position.Y / 16f) + 3].active())
                        {
                            while (!Main.tile[(int)(player.position.X / 16f), (int)(player.position.Y / 16f) + 4].active())
                            {
                                player.position.Y += 16f;
                                if ((int)(player.position.Y / 16f) > Main.maxTilesY)
                                {
                                    player.position.Y = (float)(Main.maxTilesY * 16) - 130f;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            while (Main.tile[(int)(player.position.X / 16f), (int)(player.position.Y / 16f) + 4].active())
                            {
                                player.position.Y -= 16f;
                            }
                        }
                        player.fallStart = (int)(player.position.Y / 16f);
                        if (Main.netMode == 1) NetMessage.SendTileSquare(player.whoAmI, Main.maxTilesX / 2, (int)Main.maxTilesY - 180, 10);
                    }
                    else if (mode == Mode.Random)
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

                    if (mode == Mode.Random) mode = Mode.Home;
                    else mode++;
                    IniAPI.WriteIni("EnhancedCellPhone", "Mode", mode.ToString());
                    Main.NewText("Enhanced CellPhone: " + mode, 255, 235, 150, false);
                }
            }
        }
    }
}