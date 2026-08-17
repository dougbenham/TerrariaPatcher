using PluginLoader;
using Terraria;

namespace DoombubblesPlugins
{
    [PluginDescription("Putting an item in the trash strips its prefix, so weapon and accessory drops stack together in chests.")]
    public class TrashRemovesPrefix : PluginBase, IPluginPlayerUpdate
    {
        public void OnPlayerUpdate(Player player)
        {
            if (player.trashItem != null && player.trashItem.active && player.trashItem.prefix > 0)
            {
                player.trashItem.ResetPrefix();
            }
        }
    }
}
