using PluginLoader;
using Terraria;
using Terraria.ID;

namespace DoombubblesPlugins
{
    [PluginDescription("The Dryad sells the seeds for every herb, so you do not have to wait for one to be in bloom. " +
                       "Works in multiplayer, where only your own shop is stocked with them.")]
    public class DryadSeeds : PluginBase, IPluginChestSetupShop, IPluginUpdate
    {
        private static readonly int[] seeds =
        {
            ItemID.DaybloomSeeds,
            ItemID.MoonglowSeeds,
            ItemID.BlinkrootSeeds,
            ItemID.DeathweedSeeds,
            ItemID.WaterleafSeeds,
            ItemID.FireblossomSeeds,
            ItemID.ShiverthornSeeds
        };

        private static bool alreadyHappenedThisFrame;

        public void OnUpdate()
        {
            alreadyHappenedThisFrame = false;
        }

        public void OnChestSetupShop(Chest chest, int type)
        {
            if (type != 3 || alreadyHappenedThisFrame) return;
            alreadyHappenedThisFrame = true;

            foreach (var seed in seeds)
            {
                var item = new Item();
                item.SetDefaults(seed);
                chest.AddItemToShop(item);
            }
        }
    }
}
