using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;
using Terraria.ID;

namespace DoombubblesPlugins
{
    [PluginDescription("Minion projectiles that share immunity frames get their own instead, so Baby Slimes, Vampire Frogs, " +
                       "Imp Fireballs, Mini Spiders, Retanimini, Spazmamini and Tempests no longer take turns hitting.")]
    public class MinionLocalIFrames : PluginBase, IPluginProjectileSetDefaults
    {
        [SettingIds(typeof(ProjectileID))]
        private static readonly Setting<HashSet<int>> AffectedProjectiles = new HashSet<int>
        {
            ProjectileID.ImpFireball,
            ProjectileID.VampireFrog,
            ProjectileID.BabySlime,
            ProjectileID.VenomSpider,
            ProjectileID.JumperSpider,
            ProjectileID.DangerousSpider,
            ProjectileID.Retanimini,
            ProjectileID.Spazmamini,
            ProjectileID.Tempest,
            ProjectileID.Bee,
            ProjectileID.GiantBee,
            ProjectileID.FrostHydra,
            ProjectileID.KrakenWave,
            ProjectileID.Chik,
            ProjectileID.ExplosiveBullet,
            ProjectileID.ToxicFlask,
            ProjectileID.SporeTrap,
            ProjectileID.SporeTrap2,
        };

        public MinionLocalIFrames() : base(toggleKey: Keys.None)
        { }

        public void OnProjectileSetDefaults(Projectile projectile)
        {
            if (!AffectedProjectiles.Value.Contains(projectile.type)) return;

            if (!projectile.usesIDStaticNPCImmunity || projectile.usesLocalNPCImmunity) return;

            projectile.localNPCHitCooldown = projectile.idStaticNPCHitCooldown;
            projectile.idStaticNPCHitCooldown = -1;
            projectile.usesIDStaticNPCImmunity = false;
            projectile.usesLocalNPCImmunity = true;
        }
    }
}
