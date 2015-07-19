using System;
using PluginLoader;
using Terraria;

namespace Ruffi123456789Plugins
{
    public class Respawn : MarshalByRefObject, IPluginUpdate
    {
        bool instantRespawn;

        public Respawn()
        {
            if (!bool.TryParse(IniAPI.ReadIni("Respawn", "Instant", "true", writeIt: true), out instantRespawn))
                instantRespawn = true;
        }

        public void OnUpdate()
        {
            if (instantRespawn)
                Main.player[Main.myPlayer].respawnTimer = 0;
        }
    }
}