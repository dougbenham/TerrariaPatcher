using System;
using PluginLoader;
using Terraria;

namespace Ruffi123456789Plugins
{
    public class Respawn : MarshalByRefObject, IPluginUpdate
    {
        private int maxTime;

        private int RespawnTimerInSeconds
        {
            get
            {
                if (Main.frameRate == 0) return 0;
                return Main.player[Main.myPlayer].respawnTimer / Main.frameRate;
            }
            set { Main.player[Main.myPlayer].respawnTimer = value * Main.frameRate; }
        }

        public Respawn()
        {
            if (!int.TryParse(IniAPI.ReadIni("Respawn", "Time", "0", writeIt: true), out maxTime)) maxTime = 0;
        }

        public void OnUpdate()
        {
            if (RespawnTimerInSeconds > maxTime)
                RespawnTimerInSeconds = maxTime;
        }
    }
}