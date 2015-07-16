using System;
using PluginLoader;
using Terraria;

namespace Ruffi123456789Plugins
{
    public class MoreAccessorySlots : MarshalByRefObject, IPluginPlayerUpdateBuffs
    {
        private bool force;
        private int slots;

        public MoreAccessorySlots()
        {
            if (!bool.TryParse(IniAPI.ReadIni("MoreAccessorySlots", "Force", "False", writeIt: true), out force))
                force = false;
            if (!int.TryParse(IniAPI.ReadIni("MoreAccessorySlots", "Count", "2", writeIt: true), out slots))
                slots = 2;

            if (slots > 2) slots = 2; // above 2 crashes Terraria
            if (slots < 0) slots = 0;
        }

        public void OnPlayerUpdateBuffs(Player player)
        {
            if (player.whoAmI != Main.myPlayer) return;

            if (force)
                player.extraAccessory = true;

            if (player.extraAccessory)
                player.extraAccessorySlots = slots;
        }
    }
}
