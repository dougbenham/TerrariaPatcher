using PluginLoader;
using Terraria;

namespace Ruffi123456789Plugins
{
    [PluginDescription("Caps how long you stay dead. Time is the respawn delay in seconds, 0 for an instant respawn.")]
    public class Respawn : PluginBase, IPluginUpdate
    {
        private static readonly Setting<int> Time = 0;

        private static int RespawnTimerInSeconds
        {
            get
            {
                if (Main.frameRate == 0) return 0;
                return Player.respawnTimer / Main.frameRate;
            }
            set { Player.respawnTimer = value * Main.frameRate; }
        }

        public void OnUpdate()
        {
            if (RespawnTimerInSeconds > Time)
                RespawnTimerInSeconds = Time;
        }
    }
}
