using System.Collections.Generic;
using PluginLoader;
using Terraria;
using Terraria.ID;

namespace DoombubblesPlugins
{
    [PluginDescription("Sandgun shots no longer turn into sand blocks where they land, so the gun can be used as an ordinary weapon.")]
    public class SandgunTidy : PluginBase, IPluginProjectileSetDefaults
    {
        private static readonly Setting<HashSet<int>> Sand = new HashSet<int>
        {
            ProjectileID.SandBallGun, ProjectileID.EbonsandBallGun, 
            ProjectileID.PearlSandBallGun, ProjectileID.CrimsandBallGun
        };

        public void OnProjectileSetDefaults(Projectile projectile)
        {
            if (Sand.Value.Contains(projectile.type))
                projectile.noDropItem = true;
        }
    }
}
