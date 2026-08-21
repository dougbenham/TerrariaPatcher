using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;

namespace Ruffi123456789Plugins
{
    [PluginDescription("Gives you extra accessory slots. Count is capped at 2 because more than that crashes Terraria. " +
                       "Force grants the slots even without a Demon Heart.")]
    public class MoreAccessorySlots : PluginBase, IPluginPlayerUpdateBuffs
    {
        private static readonly Setting<bool> Force = false;
        private static readonly Setting<int> Count = 2;

        public MoreAccessorySlots() : base(toggleKey: Keys.None)
        { }

        public void OnPlayerUpdateBuffs(Player player)
        {
            if (player.whoAmI != Main.myPlayer) return;

            if (Force)
                player.extraAccessory = true;

            if (player.extraAccessory)
                player.extraAccessorySlots = Count < 0 ? 0 : (Count > 2 ? 2 : Count.Value);
        }
    }
}
