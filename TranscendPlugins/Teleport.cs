using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;
using Terraria.ID;

namespace TranscendPlugins
{
    public class Teleport : MarshalByRefObject, IPluginUpdate
    {
        private int planteraBulbTileLookup;
        private Keys planteraKey, teleportKey;

        public Teleport()
        {
            var mapHelper = Assembly.GetEntryAssembly().GetType("Terraria.Map.MapHelper");
            var tileToLookup = mapHelper.GetMethod("TileToLookup");
            planteraBulbTileLookup = (int) tileToLookup.Invoke(null, new object[] {TileID.PlanteraBulb, 0});

            if (!Keys.TryParse(IniAPI.ReadIni("Teleport", "PlanteraKey", "P", writeIt: true), out planteraKey))
                planteraKey = Keys.P;
            if (!Keys.TryParse(IniAPI.ReadIni("Teleport", "TeleportKey", "F", writeIt: true), out teleportKey))
                teleportKey = Keys.F;

            Loader.RegisterHotkey(() =>
            {
                if (Main.mapFullscreen && Main.Map != null)
                {
                    for (int i = 0; i < Main.Map.MaxWidth; i++)
                    {
                        for (int j = 0; j < Main.Map.MaxHeight; j++)
                        {
                            if (Main.Map[i, j].Type == planteraBulbTileLookup)
                            {
                                Player player = Main.player[Main.myPlayer];
                                player.position = new Vector2(i * 16, j * 16);
                                player.velocity = Vector2.Zero;
                                player.fallStart = (int) (player.position.Y / 16f);
                                return;
                            }
                        }
                    }
                }
            }, planteraKey);

            Loader.RegisterHotkey(() =>
            {
                var player = Main.player[Main.myPlayer];
                var vector = new Vector2(Main.mouseX + Main.screenPosition.X, Main.mouseY + Main.screenPosition.Y);
                player.Teleport(vector, 3, 0);
                player.velocity = Vector2.Zero;
                NetMessage.SendData(65, -1, -1, "", 0, player.whoAmI, vector.X, vector.Y, 3, 0, 0);
            }, teleportKey);
        }

        public void OnUpdate()
        {
            if (Main.mapFullscreen && Main.mouseRight && Main.keyState.IsKeyUp(Keys.LeftControl))
            {
                int num = Main.maxTilesX * 16;
                int num2 = Main.maxTilesY * 16;
                Vector2 vector = new Vector2((float)Main.mouseX, (float)Main.mouseY);
                vector.X -= (float)(Main.screenWidth / 2);
                vector.Y -= (float)(Main.screenHeight / 2);
                Vector2 mapFullscreenPos = Main.mapFullscreenPos;
                Vector2 vector2 = mapFullscreenPos;
                vector /= 16f;
                vector *= 16f / Main.mapFullscreenScale;
                vector2 += vector;
                vector2 *= 16f;
                Player player = Main.player[Main.myPlayer];
                vector2.Y -= (float)player.height;
                if (vector2.X < 0f)
                {
                    vector2.X = 0f;
                }
                else if (vector2.X + (float)player.width > (float)num)
                {
                    vector2.X = (float)(num - player.width);
                }
                if (vector2.Y < 0f)
                {
                    vector2.Y = 0f;
                }
                else if (vector2.Y + (float)player.height > (float)num2)
                {
                    vector2.Y = (float)(num2 - player.height);
                }
                player.position = vector2;
                player.velocity = Vector2.Zero;
                player.fallStart = (int)(player.position.Y / 16f);
            }
        }
    }
}
