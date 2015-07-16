using System;
using PluginLoader;
using Terraria;
using Terraria.ID;

namespace TranscendPlugins
{
    public class BuffRates : MarshalByRefObject, IPluginPlayerUpdateBuffs, IPluginItemSetDefaults, IPluginPlayerPickAmmo
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
        private float wrath, rage, magic, archery, endurance, iceBarrier;

        public BuffRates()
        {
            if (!float.TryParse(IniAPI.ReadIni("Buffs", "Wrath", (0.1f).ToString(), writeIt: true), out wrath))
                wrath = 0.1f;
            if (!float.TryParse(IniAPI.ReadIni("Buffs", "Rage", (0.1f).ToString(), writeIt: true), out rage))
                rage = 0.1f;
            if (!float.TryParse(IniAPI.ReadIni("Buffs", "Endurance", (0.1f).ToString(), writeIt: true), out endurance))
                endurance = 0.1f;
            if (!float.TryParse(IniAPI.ReadIni("Buffs", "IceBarrier", (0.25f).ToString(), writeIt: true), out iceBarrier))
                iceBarrier = 0.25f;
            if (!float.TryParse(IniAPI.ReadIni("Buffs", "Archery", (0.2f).ToString(), writeIt: true), out archery))
                archery = 0.2f;
            if (!float.TryParse(IniAPI.ReadIni("Buffs", "Magic", (0.2f).ToString(), writeIt: true), out magic))
                magic = 0.2f;
        }

        public void OnPlayerUpdateBuffs(Player player)
        {
            Main.buffTip[Indices.Magic] = (magic * 100) + "% increased magic damage";
            Main.buffTip[Indices.Archery] = (archery * 100) + "% increased arrow damage and speed";
            Main.buffTip[Indices.Endurance] = (endurance * 100) + "% reduced damage";
            Main.buffTip[Indices.IceBarrier] = "Damage taken is reduced by " + (iceBarrier * 100) + "%";
            Main.buffTip[Indices.Rage] = (rage * 100) + "% increased critical chance";
            Main.buffTip[Indices.Wrath] = (wrath * 100) + "% increased damage";

            for (int k = 0; k < 22; k++)
            {
                if (player.buffType[k] > 0 && player.buffTime[k] > 0)
                {
                    switch (player.buffType[k])
                    {
                        case Indices.Magic:
                            player.magicDamage += magic - 0.2f;
                            break;
                        case Indices.IceBarrier:
                            if (player.statLife <= player.statLifeMax2 * 0.5)
                            {
                                this.endurance += iceBarrier - 0.25f;
                            }
                            break;
                        case Indices.Endurance:
                            player.endurance += endurance - 0.1f;
                            break;
                        case Indices.Rage:
                            var r = (int) (rage * 100) - 10;
                            player.meleeCrit += r;
                            player.rangedCrit += r;
                            player.magicCrit += r;
                            player.thrownCrit += r;
                            break;
                        case Indices.Wrath:
                            var w = wrath - 0.1f;
                            player.thrownDamage += w;
                            player.meleeDamage += w;
                            player.rangedDamage += w;
                            player.magicDamage += w;
                            player.minionDamage += w;
                            break;
                    }
                }
            }
        }

        public void OnItemSetDefaults(Item item)
        {
            if (item != null)
            {
                switch (item.netID)
                {
                    case ItemID.MagicPowerPotion:
                        item.toolTip = (magic * 100) + "% increased magic damage";
                        break;
                    case ItemID.ArcheryPotion:
                        item.toolTip = (archery * 100) + "% increased arrow speed and damage";
                        break;
                    case ItemID.FrozenTurtleShell:
                        item.toolTip = "Puts a shell around the owner when below 50% life"; // bug fix, Terraria usually says 20% in the tooltip but should say 50%
                        break;
                    case ItemID.EndurancePotion:
                        item.toolTip = "Reduces damage taken by " + (endurance * 100) + "%";
                        break;
                    case ItemID.RagePotion:
                        item.toolTip = "Increases critical chance by " + (rage * 100) + "%";
                        break;
                    case ItemID.WrathPotion:
                        item.toolTip = "Increases damage by " + (wrath * 100) + "%";
                        break;
                }
            }
        }

        public void OnPlayerPickAmmo(Player player, Item item, ref int shoot, ref float speed, ref bool canShoot, ref int damage, ref float knockback)
        {
            if (item.useAmmo == 1 && player.archery)
            {
                speed *= (1f + archery) / 1.2f;
                if (speed > 20f)
                    speed = 20f;
                damage = (int) (damage * (1f + archery) / 1.2f);
            }
        }
    }
}
