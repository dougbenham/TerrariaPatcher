using Terraria;

namespace GTRPlugins
{
    internal class AutoTrash
    {
        public static void Trash()
        {
            Player player = Main.player[Main.myPlayer];
            for (int i = 10; i < 51; i++)
            {
                if (Inventory_Enhancements.config.TrashList.Contains(player.inventory[i].type) && !player.inventory[i].favorited)
                {
                    player.trashItem = player.inventory[i];
                    player.inventory[i] = new Item();
                }
            }
        }
    }
}
