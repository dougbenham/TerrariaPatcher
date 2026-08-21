using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;
using Terraria.ID;

namespace DoombubblesPlugins
{
    [PluginDescription("As an alternative to the Persistent Buffs patch, this plugin makes carrying a big enough stack of a " +
                       "buff potion keeps its buff permanently active. 30 potions by default, counting your inventory, piggy " +
                       "bank and void bag; buff stations such as the Crystal Ball only need 1. Well Fed buffs never override " +
                       "a better one, so a greater food still upgrades you.")]
    public class PermaBuffs : PluginBase, IPluginPlayerUpdate
    {
        [SettingRange(1, 999)]
        [SettingDescription("How many potions you must carry for the buff to stay on.")]
        private static readonly Setting<int> ItemRequiredCount = 30;
        [SettingRange(1, 999)]
        [SettingDescription("How many of a buff station's item you must carry.")]
        private static readonly Setting<int> StationRequiredCount = 1;
        [SettingDescription("Adds up every stack of an item that grants the same buff, across your inventory and " +
                            "all your banks, instead of needing one stack big enough on its own.")]
        private static readonly Setting<bool> CumulativeTotal = false;

        [SettingIds(typeof(BuffID))]
        [SettingDescription("The buffs a big enough stack of the potion keeps permanently active.")]
        private static readonly Setting<HashSet<int>> AllowedItemBuffs = new HashSet<int>
        {
            BuffID.ObsidianSkin, BuffID.Regeneration, BuffID.Swiftness, BuffID.Gills, BuffID.Ironskin,
            BuffID.ManaRegeneration, BuffID.MagicPower, BuffID.Featherfall, BuffID.Spelunker, BuffID.Invisibility,
            BuffID.Shine, BuffID.NightOwl, BuffID.Battle, BuffID.Thorns, BuffID.WaterWalking, BuffID.Archery,
            BuffID.Hunter, BuffID.Gravitation, BuffID.WellFed, BuffID.WeaponImbueVenom,
            BuffID.WeaponImbueCursedFlames, BuffID.WeaponImbueFire, BuffID.WeaponImbueGold, BuffID.WeaponImbueIchor,
            BuffID.WeaponImbueNanites, BuffID.WeaponImbueConfetti, BuffID.WeaponImbuePoison, BuffID.Mining,
            BuffID.Heartreach, BuffID.Calm, BuffID.Builder, BuffID.Titan, BuffID.Flipper, BuffID.Summoning,
            BuffID.Dangersense, BuffID.AmmoReservation, BuffID.Lifeforce, BuffID.Endurance, BuffID.Rage,
            BuffID.Inferno, BuffID.Wrath, BuffID.Fishing, BuffID.Sonar, BuffID.Crate, BuffID.Warmth, BuffID.WellFed2,
            BuffID.WellFed3, BuffID.TorchGodPotion, BuffID.Tipsy, BuffID.Lucky, BuffID.BiomeSight
        };

        private static readonly Setting<Dictionary<int, int>> StationBuffs = new Dictionary<int, int>
        {
            { ItemID.CrystalBall, BuffID.Clairvoyance },
            { ItemID.Campfire, BuffID.Campfire },
            { ItemID.HeartLantern, BuffID.HeartLamp },
            { ItemID.AmmoBox, BuffID.AmmoBox },
            { ItemID.BewitchingTable, BuffID.Bewitched },
            { ItemID.WaterCandle, BuffID.WaterCandle },
            { ItemID.PeaceCandle, BuffID.PeaceCandle },
            { ItemID.StarinaBottle, BuffID.StarInBottle },
            { ItemID.SharpeningStation, BuffID.Sharpened },
            { ItemID.SliceOfCake, BuffID.SugarRush },
            { ItemID.CatBast, BuffID.CatBast },
            { ItemID.WarTable, BuffID.WarTable }
        };

        /// <summary>
        /// The buffs that only need <see cref="StationRequiredCount"/> of their item, kept as a set because it is
        /// consulted for every buff every frame. Rebuilt whenever StationBuffs is edited.
        /// </summary>
        private static HashSet<int> allowedStationBuffs;

        public PermaBuffs() : base(toggleKey: Keys.None)
        {
            StationBuffs.Changed += RebuildAllowedStationBuffs;
            RebuildAllowedStationBuffs();
        }

        private static void RebuildAllowedStationBuffs()
        {
            allowedStationBuffs = new HashSet<int>(StationBuffs.Value.Values) { BuffID.Kite };
        }

        public void OnPlayerUpdate(Player player)
        {
            if (Main.myPlayer != player.whoAmI) return;

            var items = player.inventory
                .Concat(player.bank.item)
                .Concat(player.bank2.item)
                .Concat(player.bank3.item)
                .Concat(player.bank4.item)
                .Where(item => !item.IsAir);

            var buffCounts = new Dictionary<int, int>();

            foreach (var item in items)
            {
                int buffType;

                if (AllowedItemBuffs.Value.Contains(item.buffType))
                {
                    buffType = item.buffType;
                }
                else if (StationBuffs.Value.ContainsKey(item.type))
                {
                    buffType = StationBuffs.Value[item.type];
                }
                else if (ItemID.Sets.IsAKite[item.type])
                {
                    buffType = BuffID.Kite;
                }
                else continue;

                if (!buffCounts.ContainsKey(buffType)) buffCounts[buffType] = 0;

                if (CumulativeTotal)
                {
                    buffCounts[buffType] += item.stack;
                }
                else if (item.stack > buffCounts[buffType])
                {
                    buffCounts[buffType] = item.stack;
                }
            }

            foreach (var buffType in buffCounts.Keys)
            {
                var required = allowedStationBuffs.Contains(buffType) ? StationRequiredCount : ItemRequiredCount;

                if (buffCounts[buffType] < required) continue;

                var index = player.FindBuffIndex(buffType);
                if (index == -1)
                {
                    if (BuffID.Sets.IsWellFed[buffType] && // Don't override better Well Fed buff
                        player.buffType.Any(b => BuffID.Sets.IsWellFed[b] && b > buffType))
                    {
                        continue;
                    }

                    player.AddBuff(buffType, 2);
                }
                else if (player.buffTime[index] < 2)
                {
                    player.buffTime[index] = 2;
                }
            }
        }
    }
}
