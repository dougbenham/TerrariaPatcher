using System.Collections.Generic;
using System.Linq;
using PluginLoader;
using Terraria;
using Terraria.Enums;
using Terraria.ID;

namespace DoombubblesPlugins
{
    [PluginDescription("The Dryad sells herb seeds. By default she only stocks the seeds whose herb is currently blooming; " +
                       "turn BloomConditions off and she sells them all the time.")]
    public class DryadSeeds : PluginBase, IPluginChestSetupShop, IPluginUpdate
    {
        private static readonly Setting<bool> BloomConditions = true;

        private static bool alreadyHappenedThisFrame;

        public void OnUpdate()
        {
            alreadyHappenedThisFrame = false;
        }

        public void OnChestSetupShop(Chest chest, int type)
        {
            if (type != 3 || alreadyHappenedThisFrame) return;
            alreadyHappenedThisFrame = true;

            var seedBlooming = new Dictionary<int, bool>
            {
                { ItemID.DaybloomSeeds, Main.dayTime },
                { ItemID.MoonglowSeeds, !Main.dayTime },
                { ItemID.BlinkrootSeeds, Main.moonPhase % 2 == 0 },
                { ItemID.DeathweedSeeds, Main.bloodMoon || (!Main.dayTime && Main.moonPhase == (int) MoonPhase.Full) },
                { ItemID.WaterleafSeeds, Main.raining },
                { ItemID.FireblossomSeeds, !Main.raining && Main.dayTime && Main.time > 40500 },
                { ItemID.ShiverthornSeeds, Main.moonPhase % 2 == 1 }
            };

            foreach (var kvp in seedBlooming.Where(kvp => !BloomConditions || kvp.Value))
            {
                var item = new Item();
                item.SetDefaults(kvp.Key);
                chest.AddItemToShop(item);
            }
        }
    }
}
