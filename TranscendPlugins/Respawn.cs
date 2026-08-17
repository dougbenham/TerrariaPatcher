using PluginLoader;
using Terraria;

namespace Ruffi123456789Plugins
{
    [PluginDescription("Caps how long you stay dead. Time is the respawn delay in seconds, 0 for an instant respawn.")]
    public class Respawn : PluginBase, IPluginUpdate
    {
        private static readonly Setting<int> Time = 0;

        /// <summary>
        /// Main.frameRate is 0 until the game has measured it, which would make the timer read as 0 seconds and the
        /// cap never apply.
        /// </summary>
        private static int FrameRate
        {
            get { return Main.frameRate > 0 ? Main.frameRate : 60; }
        }

        private static int RespawnTimerInSeconds
        {
            get { return Player.respawnTimer / FrameRate; }
            set { Player.respawnTimer = value * FrameRate; }
        }

        public void OnUpdate()
        {
            if (RespawnTimerInSeconds > Time)
                RespawnTimerInSeconds = Time;
        }
    }
}
