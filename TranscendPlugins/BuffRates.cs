using PluginLoader;
using Terraria;
using Terraria.ID;
using TranscendPlugins.Shared.Extensions;

namespace TranscendPlugins
{
    [PluginDescription("Rewrites how strong the combat buff potions are. Each setting is the buff's bonus as a fraction, " +
                       "so 0.1 is the vanilla 10% for Wrath. Buff and item tooltips are updated to match.")]
    public class BuffRates : PluginBase, IPluginInitialize, IPluginPlayerUpdateBuffs, IPluginPlayerPickAmmo
    {
        private static class Indices
        {
            public const int Magic = 7;
            public const int Archery = 16;
            public const int IceBarrier = 62;
            public const int Endurance = 114;
            public const int Rage = 115;
            public const int Wrath = 117;
        }

        private static readonly Setting<float> Wrath = 0.1f;
        private static readonly Setting<float> Rage = 0.1f;
        private static readonly Setting<float> Endurance = 0.1f;
        private static readonly Setting<float> IceBarrier = 0.25f;
        private static readonly Setting<float> Archery = 0.2f;
        private static readonly Setting<float> Magic = 0.2f;

        public void OnInitialize()
        {
            DescribeBuffs();

            foreach (var setting in Settings)
                setting.Changed += DescribeBuffs;
        }

        private static void DescribeBuffs()
        {
            Lang._buffDescriptionCache[Indices.Magic].SetValue((Magic * 100) + "% increased magic damage");
            Lang._buffDescriptionCache[Indices.Archery].SetValue((Archery * 100) + "% increased arrow damage and speed");
            Lang._buffDescriptionCache[Indices.Endurance].SetValue((Endurance * 100) + "% reduced damage");
            Lang._buffDescriptionCache[Indices.IceBarrier].SetValue("Damage taken is reduced by " + (IceBarrier * 100) + "%");
            Lang._buffDescriptionCache[Indices.Rage].SetValue((Rage * 100) + "% increased critical chance");
            Lang._buffDescriptionCache[Indices.Wrath].SetValue((Wrath * 100) + "% increased damage");
            Lang._itemTooltipCache[ItemID.MagicPowerPotion].SetValue((Magic * 100) + "% increased magic damage");
            Lang._itemTooltipCache[ItemID.ArcheryPotion].SetValue((Archery * 100) + "% increased arrow speed and damage");
            Lang._itemTooltipCache[ItemID.EndurancePotion].SetValue("Reduces damage taken by " + (Endurance * 100) + "%");
            Lang._itemTooltipCache[ItemID.RagePotion].SetValue("Increases critical chance by " + (Rage * 100) + "%");
            Lang._itemTooltipCache[ItemID.WrathPotion].SetValue("Increases damage by " + (Wrath * 100) + "%");
        }

        public void OnPlayerUpdateBuffs(Player player)
        {
            for (int k = 0; k < 22; k++)
            {
                if (player.buffType[k] > 0 && player.buffTime[k] > 0)
                {
                    switch (player.buffType[k])
                    {
                        case Indices.Magic:
                            player.magicDamage += Magic - 0.2f;
                            break;
                        case Indices.IceBarrier:
                            if (player.statLife <= player.statLifeMax2 * 0.5)
                            {
                                player.endurance += IceBarrier - 0.25f;
                            }
                            break;
                        case Indices.Endurance:
                            player.endurance += Endurance - 0.1f;
                            break;
                        case Indices.Rage:
                            var r = (int) (Rage * 100) - 10;
                            player.meleeCrit += r;
                            player.rangedCrit += r;
                            player.magicCrit += r;
                            break;
                        case Indices.Wrath:
                            var w = Wrath - 0.1f;
                            player.meleeDamage += w;
                            player.rangedDamage += w;
                            player.magicDamage += w;
                            player.minionDamage += w;
                            break;
                    }
                }
            }
        }

        public void OnPlayerPickAmmo(Player player, Item item, ref int shoot, ref float speed, ref bool canShoot, ref int damage, ref float knockback, ref int usedAmmoItemId, bool dontConsume)
        {
            if (item.useAmmo == 1 && player.archery)
            {
                speed *= (1f + Archery) / 1.2f;
                if (speed > 20f)
                    speed = 20f;
                damage = (int) (damage * (1f + Archery) / 1.2f);
            }
        }
    }
}
