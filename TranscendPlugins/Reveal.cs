using System;
using System.Reflection;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;
using Terraria.ID;

namespace TranscendPlugins
{
    public class Reveal : MarshalByRefObject, IPlugin
    {
        private bool revealed = false;
        private byte[,] mapLight;
        private Keys revealKey;

        public Reveal()
        {
            if (!Keys.TryParse(IniAPI.ReadIni("Reveal", "RevealKey", "L", writeIt: true), out revealKey))
                revealKey = Keys.L;
            //var drawMap = typeof (Main).GetMethod("DrawMap", BindingFlags.NonPublic | BindingFlags.Instance);
            /*Main.renderCount = 0;
            Lighting.LightTiles(0, Main.maxTilesX, 0, Main.maxTilesY);
            Main.spriteBatch.Begin();
            drawMap.Invoke(Main.instance, null);
            Main.spriteBatch.End();
            Main.NewText("Light");
            return;*/
            //if (Main.tile[i, j] == null || Main.tile[i, j].type == 0 || !Main.tileBlockLight[Main.tile[i, j].type])

            //var drawMap = typeof (Main).GetMethod("DrawMap", BindingFlags.NonPublic | BindingFlags.Instance);

            Loader.RegisterHotkey(() =>
            {
                Main.renderCount = 3;
                Main.mapTime = 0;
                Main.mapEnabled = true;

                var width = (int) (Main.screenWidth / 16f + 3);
                var height = (int) (Main.screenHeight / 16f + 3);
                Main.NewText("Width: " + (Main.maxTilesX / width));
                Main.NewText("Height: " + (Main.maxTilesY / height));
                for (int x = 0; x < Main.maxTilesX; x += width)
                {
                    for (int y = 0; y < Main.maxTilesY; y += height)
                    {
                        Lighting.LightTiles(x, x + width, y, y + height);
                    }
                }
                /*
                Main.firstTileX = (int)(Main.screenPosition.X / 16f - 1f);
                Main.lastTileX = (int)((Main.screenPosition.X + (float)Main.screenWidth) / 16f) + 2;
                Main.firstTileY = (int)(Main.screenPosition.Y / 16f - 1f);
                Main.lastTileY = (int)((Main.screenPosition.Y + (float)Main.screenHeight) / 16f) + 2;*/

                if (Main.mapFullscreen && Main.Map != null)
                {
                    if (!revealed)
                    {
                        revealed = true;

                        if (mapLight == null)
                            mapLight = new byte[Main.Map.MaxWidth, Main.Map.MaxHeight];

                        for (int i = 0; i < Main.Map.MaxWidth; i++)
                        {
                            for (int j = 0; j < Main.Map.MaxHeight; j++)
                            {
                                mapLight[i, j] = Main.Map[i, j].Light;
                                if (Main.tile[i, j] == null || (!Main.tile[i, j].active() && Main.tile[i, j].type == 0) || !Main.tileBlockLight[Main.tile[i, j].type])
                                    Main.Map.UpdateLighting(i, j, 255);
                            }
                        }
                    }
                    else
                    {
                        revealed = false;

                        for (int i = 0; i < Main.Map.MaxWidth; i++)
                        {
                            for (int j = 0; j < Main.Map.MaxHeight; j++)
                            {
                                Main.Map.UpdateLighting(i, j, mapLight[i, j]);
                            }
                        }
                    }

                    Main.updateMap = true;
                    Main.refreshMap = true;
                }
            }, revealKey);
        }
    }
}
