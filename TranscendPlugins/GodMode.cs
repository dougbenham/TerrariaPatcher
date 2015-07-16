using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;

namespace ZeromaruPlugins
{
    public class GodMode : MarshalByRefObject, IPluginUpdate
    {
        private bool god = false;
        private Keys godmodeKey;

        public GodMode()
        {
            if (!Keys.TryParse(IniAPI.ReadIni("GodMode", "GodModeKey", "G", writeIt: true), out godmodeKey))
                godmodeKey = Keys.G;

            Color green = Color.Green;
            Loader.RegisterHotkey(() =>
            {
                god = !god;
                Main.NewText("God Mode " + (god ? "Enabled" : "Disabled"), green.R, green.G, green.B, false);
            }, godmodeKey);
        }

        public void OnUpdate()
        {
            if (god)
            {
                var player = Main.player[Main.myPlayer];
                player.statLife = player.statLifeMax2;
                player.statMana = player.statManaMax2;
                player.breath = player.breathMax + 1;
                player.noFallDmg = true;
                player.immune = true;
                player.immuneTime = 10;
                player.immuneAlpha = 0;
            }
        }
    }
}