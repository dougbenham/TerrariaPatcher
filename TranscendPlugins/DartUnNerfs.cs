using PluginLoader;
using Terraria;
using Terraria.ID;

namespace DoombubblesPlugins
{
    [PluginDescription("Undoes the 1.4.5 dart nerfs: Crystal Darts get 7 hits with no damage dropoff, Ichor Darts go back " +
                       "to 10 damage, and Cursed Dart Flames back to 3 hits. Cursed Darts themselves keep 1 hit by default.")]
    public class DartUnNerfs : PluginBase, IPluginProjectileAI, IPluginItemSetDefaults
    {
        private static readonly Setting<int> CrystalDartPenetrate = 7;
        private static readonly Setting<bool> CrystalDamageDropoff = false;
        private static readonly Setting<int> IchorDartDamage = 10;
        private static readonly Setting<int> CursedDartPenetrate = 1;
        private static readonly Setting<int> CursedDartFlamePenetrate = 3;

        public void OnItemSetDefaults(Item item)
        {
            if (item.type == ItemID.IchorDart)
            {
                item.damage = IchorDartDamage;
            }
        }

        public void OnProjectileAI001(Projectile projectile)
        {
            switch (projectile.type)
            {
                case ProjectileID.CrystalDart:
                {
                    if (projectile.originalDamage <= 0)
                    {
                        projectile.originalDamage = projectile.damage;
                        projectile.penetrate = CrystalDartPenetrate;
                    }
                    else if (!CrystalDamageDropoff)
                    {
                        projectile.damage = projectile.originalDamage;
                    }

                    break;
                }
                case ProjectileID.CursedDart:
                {
                    if (projectile.originalDamage <= 0)
                    {
                        projectile.originalDamage = projectile.damage;
                        projectile.penetrate = CursedDartPenetrate;
                    }

                    break;
                }
                case ProjectileID.CursedDartFlame:
                {
                    if (projectile.originalDamage <= 0)
                    {
                        projectile.originalDamage = projectile.damage;
                        projectile.penetrate = CursedDartFlamePenetrate;
                    }

                    break;
                }
            }
        }
    }
}
