using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;
using Terraria.ID;

namespace DoombubblesPlugins
{
    [PluginDescription("Projectiles that share immunity frames with every other copy of themselves get their own " +
                       "instead, so a swarm of Baby Slimes, Mini Spiders, Bees, Toxic Flask clouds or Chik shards all " +
                       "land their hits rather than taking turns.")]
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
            ProjectileID.FlinxMinion,
            ProjectileID.PalworldMinionFoxsparksFlames,
            ProjectileID.ClingerStaff,
            ProjectileID.MonkStaffT2,
            ProjectileID.WeatherPainShot,
            ProjectileID.VolatileGelatinBall,
            ProjectileID.KrakenWave,
            ProjectileID.ExplosiveBullet,
            ProjectileID.ArcSurge,
            ProjectileID.ChlorophyteClaymoreSporeCloud,
            ProjectileID.CrystalShardMelee,
            ProjectileID.ToxicCloud,
            ProjectileID.ToxicCloud2,
            ProjectileID.ToxicCloud3,
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
