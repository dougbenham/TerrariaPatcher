using System;
using PluginLoader;
using Terraria;

namespace TranscendPlugins
{
    public class Minions : MarshalByRefObject, IPluginPlayerUpdateBuffs
    {
        private int minions;

        public Minions()
        {
            if (!int.TryParse(IniAPI.ReadIni("Minions", "Max", "100", writeIt: true), out minions))
                minions = 100;
        }
        public void OnPlayerUpdateBuffs(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
                player.maxMinions = minions;
        }
    }
}
