using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;

namespace TranscendPlugins
{
    public class FullBright : MarshalByRefObject, IPluginLightingGetColor
    {
        private bool fullbright = false;
        private Keys fullbrightKey;

        public FullBright()
        {
            if (!Keys.TryParse(IniAPI.ReadIni("FullBright", "FullBrightKey", "Y", writeIt: true), out fullbrightKey))
                fullbrightKey = Keys.Y;
            if (!bool.TryParse(IniAPI.ReadIni("FullBright", "FullBrightDefault", "false", writeIt: true), out fullbright))
                fullbright = false;

            Color green = Color.Green;
            Loader.RegisterHotkey(() =>
            {
                fullbright = !fullbright;
                IniAPI.WriteIni("FullBright", "FullBrightDefault", fullbright.ToString());
                Main.NewText("Full Bright " + (fullbright ? "Enabled" : "Disabled"), green.R, green.G, green.B, false);
            }, fullbrightKey);
        }

        public bool OnLightingGetColor(int x, int y, out Color color)
        {
            color = Color.White;
            return fullbright;
        }
    }
}