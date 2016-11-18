using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;
using Terraria.DataStructures;

namespace ZeromaruPlugins
{
    public class GodMode : MarshalByRefObject, IPluginUpdate, IPluginPlayerHurt, IPluginPlayerKillMe
    {
        enum Mode
        {
            Off = 0,
            DemiGod = 1,
            God = 2
        }
        private Mode mode = Mode.Off;
        private Keys key;

        public GodMode()
        {
            if (!Keys.TryParse(IniAPI.ReadIni("GodMode", "Key", "G", writeIt: true), out key)) key = Keys.G;
            if (!Mode.TryParse(IniAPI.ReadIni("GodMode", "Mode", "Off", writeIt: true), out mode)) mode = Mode.Off;

            Color green = Color.Green;
            Action update = () =>
            {
                IniAPI.WriteIni("GodMode", "Mode", mode.ToString());
                Main.NewText("God Mode: " + mode, green.R, green.G, green.B, false);
            };

            Loader.RegisterHotkey(() =>
            {
                if (mode == Mode.God) mode = Mode.Off;
                else mode++;
                update();
            }, key);

            Loader.RegisterHotkey(() =>
            {
                if (mode == Mode.Off) mode = Mode.God;
                else mode--;
                update();
            }, key, shift: true);
        }

        public void OnUpdate()
        {
            if (mode == Mode.God)
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

        public bool OnPlayerHurt(Player player, PlayerDeathReason damageSource, int damage, int hitDirection, bool pvp, bool quiet, bool crit, int cooldownCounter, out double result)
        {
            result = 0.0;
            return mode == Mode.God;
        }

        public bool OnPlayerKillMe(Player player, PlayerDeathReason damageSource, double dmg, int hitDirection, bool pvp)
        {
            return mode == Mode.God || mode == Mode.DemiGod;
        }
    }
}