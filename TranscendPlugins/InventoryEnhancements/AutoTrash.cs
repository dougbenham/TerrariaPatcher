using Terraria;

namespace GTRPlugins
{
    internal class AutoTrash
    {
        public static void Trash()
        {
            Player player = Main.player[Main.myPlayer];
            int i = 10;
            while (i < 51)
            {
                if (InventoryEnhancements.config.TrashList.Contains(player.inventory[i].type))
                {
                    if (!player.inventory[i].favorited)
                    {
                        player.trashItem = player.inventory[i];
                        player.inventory[i] = new Item();
                    }
                }
                i++;
            }
        }
    }
}
