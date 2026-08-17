using System.Linq;
using PluginLoader;
using Terraria;
using Terraria.ID;

namespace DoombubblesPlugins
{
    [PluginDescription("Minion projectiles that share immunity frames get their own instead, so Baby Slimes, Vampire Frogs, " +
                       "Imp Fireballs, Mini Spiders, Retanimini, Spazmamini and Tempests no longer take turns hitting.")]
    public class MinionLocalIFrames : PluginBase, IPluginUpdate
    {
        private static readonly Setting<int[]> AffectedProjectiles = new int[]
        {
            ProjectileID.ImpFireball,
            ProjectileID.VampireFrog,
            ProjectileID.BabySlime,
            ProjectileID.VenomSpider,
            ProjectileID.JumperSpider,
            ProjectileID.DangerousSpider,
            ProjectileID.Retanimini,
            ProjectileID.Spazmamini,
            ProjectileID.Tempest
        };

        private static void StaticToLocal(Projectile projectile)
        {
            if (!projectile.usesIDStaticNPCImmunity || projectile.usesLocalNPCImmunity) return;

            projectile.localNPCHitCooldown = projectile.idStaticNPCHitCooldown;
            projectile.idStaticNPCHitCooldown = -1;
            projectile.usesIDStaticNPCImmunity = false;
            projectile.usesLocalNPCImmunity = true;
        }

        public void OnUpdate()
        {
            if (Main.gameMenu) return;

            foreach (var projectile in Main.projectile)
            {
                if (projectile == null || !projectile.active) continue;

                if (AffectedProjectiles.Value.Contains(projectile.type))
                {
                    StaticToLocal(projectile);
                }
            }
        }
    }
}
