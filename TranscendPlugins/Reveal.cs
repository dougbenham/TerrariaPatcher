using System;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;

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

            Loader.RegisterHotkey(() =>
            {
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
                                //if (Main.tile[i, j] == null || (!Main.tile[i, j].active() && Main.tile[i, j].type == 0) || !Main.tileBlockLight[Main.tile[i, j].type])
                                Main.Map.Update(i, j, 255);
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
                                Main.Map.Update(i, j, mapLight[i, j]);
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
