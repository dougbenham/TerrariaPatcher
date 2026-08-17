using System.Linq;
using PluginLoader;
using Terraria;
using Terraria.ID;

namespace DoombubblesPlugins
{
    [PluginDescription("Sandgun shots no longer turn into sand blocks where they land, so the gun can be used as an ordinary weapon.")]
    public class SandgunTidy : PluginBase, IPluginUpdate
    {
        private static readonly Setting<int[]> Sand = new int[]
        {
            ProjectileID.SandBallGun, ProjectileID.EbonsandBallGun, ProjectileID.PearlSandBallGun,
            ProjectileID.CrimsandBallGun
        };

        public void OnUpdate()
        {
            if (Main.gameMenu) return;

            foreach (var projectile in Main.projectile)
            {
                if (projectile == null || !projectile.active) continue;

                if (Sand.Value.Contains(projectile.type))
                {
                    projectile.noDropItem = true;
                }
            }
        }
    }
}
