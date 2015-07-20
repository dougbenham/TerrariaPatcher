using System;
using PluginLoader;
using Terraria;

namespace GTRPlugins
{
    public class InventoryEnhancementPlugin : MarshalByRefObject, IPluginPlayerGetItem, IPluginUpdate
    {
        public InventoryEnhancementPlugin()
        {
            InventoryEnhancements.Init();
        }

        public bool OnPlayerGetItem(Player player, Item newItem, out Item resultItem)
        {
            if (InventoryEnhancements.config.TrashList.Contains(newItem.type) && InventoryEnhancements.config.AutoTrash && Main.netMode == 0 && player.whoAmI == Main.myPlayer)
            {
                player.trashItem = newItem;
                resultItem = new Item();
                return true;
            }
            resultItem = null;
            return false;
        }

        public void OnUpdate()
        {
            InventoryEnhancements.Update(null);
        }
    }
}
