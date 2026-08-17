using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;
using Terraria.DataStructures;

namespace ZeromaruPlugins
{
    [PluginDescription("Invincibility, on a hotkey. DemiGod stops you dying but still lets you take damage, God keeps your " +
                       "health, mana and breath topped up and ignores damage entirely.")]
    public class GodMode : PluginBase, IPluginUpdate, IPluginPlayerHurt, IPluginPlayerKillMe
    {
        private enum Modes
        {
            Off = 0,
            DemiGod = 1,
            God = 2
        }

        private static readonly Setting<Modes> Mode = Modes.Off;

        private static readonly HotkeySetting NextMode = new Hotkey { Key = Keys.G, Action = () => Cycle(true) };
        private static readonly HotkeySetting PreviousMode = new Hotkey { Key = Keys.G, Shift = true, Action = () => Cycle(false) };

        private static void Cycle(bool forwards)
        {
            if (forwards)
                Mode.Value = Mode.Value == Modes.God ? Modes.Off : Mode.Value + 1;
            else
                Mode.Value = Mode.Value == Modes.Off ? Modes.God : Mode.Value - 1;

            var green = Color.Green;
            Main.NewText("God Mode: " + Mode.Value, green.R, green.G, green.B);
        }

        public void OnUpdate()
        {
            if (Mode.Value != Modes.God) return;

            Player.statLife = Player.statLifeMax2;
            Player.statMana = Player.statManaMax2;
            Player.breath = Player.breathMax + 1;
            Player.noFallDmg = true;
            Player.immune = true;
            Player.immuneTime = 10;
            Player.immuneAlpha = 0;
        }

        public bool OnPlayerHurt(Player player, PlayerDeathReason damageSource, int damage, int hitDirection, bool pvp, bool quiet, bool crit, int cooldownCounter, bool dodgeable, out double result)
        {
            result = 0.0;
            return Mode.Value == Modes.God;
        }

        public bool OnPlayerKillMe(Player player, PlayerDeathReason damageSource, double dmg, int hitDirection, bool pvp)
        {
            return Mode.Value == Modes.God || Mode.Value == Modes.DemiGod;
        }
    }
}
