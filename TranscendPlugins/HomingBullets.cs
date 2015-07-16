using System;
using Microsoft.Xna.Framework;
using PluginLoader;
using Terraria;
using Terraria.ID;

namespace TranscendPlugins
{
    public class HomingBullets : MarshalByRefObject, IPluginProjectileAI
    {
        public void OnProjectileAI001(Projectile pProjectile)
        {
            if (pProjectile.owner != Main.myPlayer) return;
            if (pProjectile.type == ProjectileID.LunarFlare) return;
            if (pProjectile.type == ProjectileID.NebulaBlaze1) return;
            if (pProjectile.type == ProjectileID.NebulaBlaze2) return;
            if (pProjectile.type == ProjectileID.ChlorophyteBullet) return; // don't want to do tracking x2
            if (pProjectile.type == ProjectileID.VortexBeaterRocket) return; // don't want to do tracking x2
            if (pProjectile.type == ProjectileID.PygmySpear) return;
            if (pProjectile.type == ProjectileID.MiniRetinaLaser) return;
            
            float num138 = (float)Math.Sqrt((double)(pProjectile.velocity.X * pProjectile.velocity.X + pProjectile.velocity.Y * pProjectile.velocity.Y));
            float num139 = pProjectile.localAI[0];
            if (num139 == 0f)
            {
                pProjectile.localAI[0] = num138;
                num139 = num138;
            }
            if (pProjectile.alpha > 0)
            {
                pProjectile.alpha -= 25;
            }
            if (pProjectile.alpha < 0)
            {
                pProjectile.alpha = 0;
            }
            float num140 = pProjectile.position.X;
            float num141 = pProjectile.position.Y;
            float num142 = 300f;
            bool flag4 = false;
            int num143 = 0;
            if (pProjectile.ai[1] == 0f)
            {
                for (int num144 = 0; num144 < 200; num144++)
                {
                    if (Main.npc[num144].CanBeChasedBy(pProjectile, false) && (pProjectile.ai[1] == 0f || pProjectile.ai[1] == (float)(num144 + 1)))
                    {
                        float num145 = Main.npc[num144].position.X + (float)(Main.npc[num144].width / 2);
                        float num146 = Main.npc[num144].position.Y + (float)(Main.npc[num144].height / 2);
                        float num147 = Math.Abs(pProjectile.position.X + (float)(pProjectile.width / 2) - num145) + Math.Abs(pProjectile.position.Y + (float)(pProjectile.height / 2) - num146);
                        if (num147 < num142 && Collision.CanHit(new Vector2(pProjectile.position.X + (float)(pProjectile.width / 2), pProjectile.position.Y + (float)(pProjectile.height / 2)), 1, 1, Main.npc[num144].position, Main.npc[num144].width, Main.npc[num144].height))
                        {
                            num142 = num147;
                            num140 = num145;
                            num141 = num146;
                            flag4 = true;
                            num143 = num144;
                        }
                    }
                }
                if (flag4)
                {
                    pProjectile.ai[1] = (float)(num143 + 1);
                }
                flag4 = false;
            }
            if (pProjectile.ai[1] > 0f)
            {
                int num148 = (int)(pProjectile.ai[1] - 1f);
                if (Main.npc[num148].active && Main.npc[num148].CanBeChasedBy(pProjectile, true) && !Main.npc[num148].dontTakeDamage)
                {
                    float num149 = Main.npc[num148].position.X + (float)(Main.npc[num148].width / 2);
                    float num150 = Main.npc[num148].position.Y + (float)(Main.npc[num148].height / 2);
                    float num151 = Math.Abs(pProjectile.position.X + (float)(pProjectile.width / 2) - num149) + Math.Abs(pProjectile.position.Y + (float)(pProjectile.height / 2) - num150);
                    if (num151 < 1000f)
                    {
                        flag4 = true;
                        num140 = Main.npc[num148].position.X + (float)(Main.npc[num148].width / 2);
                        num141 = Main.npc[num148].position.Y + (float)(Main.npc[num148].height / 2);
                    }
                }
                else
                {
                    pProjectile.ai[1] = 0f;
                }
            }
            if (!pProjectile.friendly)
            {
                flag4 = false;
            }
            if (flag4)
            {
                float num152 = num139;
                Vector2 vector13 = new Vector2(pProjectile.position.X + (float)pProjectile.width * 0.5f, pProjectile.position.Y + (float)pProjectile.height * 0.5f);
                float num153 = num140 - vector13.X;
                float num154 = num141 - vector13.Y;
                float num155 = (float)Math.Sqrt((double)(num153 * num153 + num154 * num154));
                num155 = num152 / num155;
                num153 *= num155;
                num154 *= num155;
                int num156 = 8;
                pProjectile.velocity.X = (pProjectile.velocity.X * (float)(num156 - 1) + num153) / (float)num156;
                pProjectile.velocity.Y = (pProjectile.velocity.Y * (float)(num156 - 1) + num154) / (float)num156;
            }
        }
    }
}
