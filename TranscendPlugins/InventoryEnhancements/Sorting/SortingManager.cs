using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace GTRPlugins.Sorting
{
    public class SortingManager
    {
        List<SortingSet> sortingSets = new List<SortingSet>();

        public SortingManager()
        {
            sortingSets.Clear();
            Sets.Add();
            sortingSets = SortingSet.OrganizeSets();
        }
        public void ReloadConfig()
        {
            sortingSets.Clear();
            sortingSets = SortingSet.OrganizeSets();
        }
        public void Sort(int mode = 0)
        {
            if (Main.gameMenu) return;
            Player player = Main.player[Main.myPlayer];
            List<Item> toSortList = new List<Item>();
            if (player.chest == -1 || Inventory_Enhancements.config.SortChests == false)
            {
                for (int i = 10; i < 50; i++)
                {
                    Item item = player.inventory[i];
                    if (!item.favorited)
                    {
                        toSortList.Add(item);
                        player.inventory[i] = new Item();
                    }
                }
                Item[] sorted = ApplySortings(toSortList, mode).ToArray();
                if (sorted.Length == 0) return;
                int sortCounter = 0;
                for (int i = 10; i < 50; i++)
                {
                    if (sorted[sortCounter].type == 0)
                    {
                        sortCounter++;
                        continue;
                    }
                    Item item = player.inventory[i];
                    if (item.favorited) continue;
                    player.inventory[i] = sorted[sortCounter];
                    if (sortCounter < sorted.Count() - 1)
                    {
                        sortCounter++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                Chest chest;
                if (player.chest == -2) chest = player.bank;
                else if (player.chest == -3) chest = player.bank2;
                else chest = Main.chest[player.chest];
                for (int i = 0; i < 40; i++)
                {
                    Item item = chest.item[i];
                    if (!item.favorited)
                    {
                        toSortList.Add(item);
                        chest.item[i] = new Item();
                    }
                }
                Item[] sorted = ApplySortings(toSortList, mode).ToArray();
                if (sorted.Length == 0) return;
                int sortCounter = 0;
                for (int i = 0; i < 40; i++)
                {
                    if (sorted.Length > 0)
                    {
                        chest.item[i] = sorted[i];
                        if (sortCounter < sorted.Count() - 1)
                        {
                            sortCounter++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                if (Main.netMode == 1)
                {
                    if (player.chest > -1)
                    {
                        for (int i = 0; i < 40; i++)
                        {
                            NetMessage.SendData(32, -1, -1, null, player.chest, i);
                        }
                    }
                    else NetMessage.SendData(33, -1, -1, null, Main.player[Main.myPlayer].chest);
                }
            }

        }

        private List<Item> ApplySortings(List<Item> input, int mode)
        {
            List<Item> sorted = new List<Item>();
            foreach (SortingSet s in sortingSets)
            {
                List<Item> temp = s.sortOutValid(ref input);
                if (mode == 1) temp = temp.OrderBy(x => x.Name).ThenByDescending(x => x.stack).ToList();
                else if (mode == 2) temp = temp.OrderByDescending(X => X.rare).ThenByDescending(x => x.stack).ToList();
                else if (mode == 3) temp = temp.OrderByDescending(x => x.value).ThenByDescending(x => x.stack).ToList();
                else temp = temp.OrderBy(x => x.type).ThenByDescending(x => x.stack).ToList();
                foreach (Item i in temp)
                {
                    sorted.Add(i);
                }
            }
            if (mode == 1) input = input.OrderBy(x => x.Name).ThenByDescending(x => x.stack).ToList();
            else if (mode == 2) input = input.OrderByDescending(x => x.rare).ThenByDescending(x => x.stack).ToList();
            else if (mode == 3) input = input.OrderByDescending(x => x.value).ThenByDescending(x => x.stack).ToList();
            else input = input.OrderBy(x => x.type).ThenByDescending(x => x.stack).ToList();
            foreach (Item i in input)
            {
                sorted.Add(i);
            }
            sorted.RemoveAll(x => x.type == 0);
            return sorted;
        }
    }
}
