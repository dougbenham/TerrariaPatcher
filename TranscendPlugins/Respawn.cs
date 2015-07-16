using System;
using PluginLoader;
using Terraria;

namespace Ruffi123456789Plugins
{
    public class Respawn : MarshalByRefObject, IPluginUpdate
    {
        int respawnDelay;

        public Respawn()
        {
            if (!int.TryParse(IniAPI.ReadIni("Respawn", "Delay", "0", writeIt: true), out respawnDelay))
                respawnDelay = 0;
        }

        public void OnUpdate()
        {
            Main.player[Main.myPlayer].respawnTimer = respawnDelay;
        }
    }
}