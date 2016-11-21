using System;
using PluginLoader;
using Terraria;

namespace TranscendPlugins
{
    public class Turrets : MarshalByRefObject, IPluginPlayerUpdateArmorSets
    {
        private int turrets;

        public Turrets()
        {
            if (!int.TryParse(IniAPI.ReadIni("Turrets", "Max", "100", writeIt: true), out turrets))
                turrets = 100;
        }
        public void OnPlayerUpdateArmorSets(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
                player.maxTurrets = turrets;
        }
    }
}
