using System;
using PluginLoader;
using Terraria;

namespace TranscendPlugins
{
    public class Minions : MarshalByRefObject, IPluginPlayerUpdateArmorSets
    {
        private int minions;

        public Minions()
        {
            if (!int.TryParse(IniAPI.ReadIni("Minions", "Max", "100", writeIt: true), out minions))
                minions = 100;
        }
        public void OnPlayerUpdateArmorSets(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
                player.maxMinions = minions;
        }
    }
}
