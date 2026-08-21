using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;
using Terraria.GameContent.Items;

namespace DoombubblesPlugins
{
    [PluginDescription("Raises how many whip tag effects you can keep going at once, so the Spider, Snapthorn, " +
                       "Snowflake, Durendal and Dark Harvest buffs stack without the Silver Bracer, Mobius Strip, " +
                       "Wicked Armlet or Twilight Grasp. 5 is the maximum the game can hold, 1 is vanilla " +
                       "without wearing those accessories, and wearing them still counts for at least as much as they " +
                       "are worth (without going over the game maximum of 5).")]
    public class WhipBuffStacking : PluginBase, IPluginPlayerUpdateEquips
    {
        [SettingRange(1, 5)]
        [SettingDescription("How many whip tag effects can be active at once.")]
        private static readonly Setting<int> MaxTagEffects = 5;

        public WhipBuffStacking() : base(toggleKey: Keys.None)
        { }

        public void OnPlayerUpdateEquips(Player player)
        {
            if (player.whoAmI != Main.myPlayer) return;

            var count = MaxTagEffects > TagEffectStack.MaxEffects ? TagEffectStack.MaxEffects : MaxTagEffects.Value;

            // Player.ResetEffects puts the count back to one every frame and Player.UpdateEquips has just added the
            // tag accessories, so this is the last word before Player.TagEffectStack trims the stack down to it.
            if (count > player.maxTagEffects) player.maxTagEffects = count;
        }
    }
}
