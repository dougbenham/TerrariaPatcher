using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace GTRPlugins.Sorting
{
    public class SortingManager
    {
        private List<SortingSet> sortingSets = new List<SortingSet>();
        public SortingManager()
        {
            this.sortingSets.Clear();
            Sets.Add();
            this.sortingSets = SortingSet.OrganizeSets();
        }
        public void ReloadConfig()
        {
            this.sortingSets.Clear();
            this.sortingSets = SortingSet.OrganizeSets();
        }
        public void Sort(int mode = 0)
        {
            if (!Main.gameMenu)
            {
                var player = Main.player[Main.myPlayer];
                var list = new List<Item>();
                if (player.chest == -1)
                {
                    for (var i = 10; i < 50; i++)
                    {
                        var item = player.inventory[i];
                        if (!item.favorited)
                        {
                            list.Add(item);
                            player.inventory[i] = new Item();
                        }
                    }
                    var array = this.ApplySortings(list, mode).ToArray();
                    var num = 0;
                    for (var i = 10; i < 50; i++)
                    {
                        if (array[num].type == 0)
                        {
                            num++;
                        }
                        else
                        {
                            var item = player.inventory[i];
                            if (!item.favorited)
                            {
                                player.inventory[i] = array[num];
                                if (num >= array.Count() - 1)
                                {
                                    break;
                                }
                                num++;
                            }
                        }
                    }
                }
                else
                {
                    Chest chest;
                    if (player.chest == -2)
                    {
                        chest = player.bank;
                    }
                    else if (player.chest == -3)
                    {
                        chest = player.bank2;
                    }
                    else
                    {
                        chest = Main.chest[player.chest];
                    }
                    for (var i = 0; i < 40; i++)
                    {
                        var item = chest.item[i];
                        if (!item.favorited)
                        {
                            list.Add(item);
                            chest.item[i] = new Item();
                        }
                    }
                    var array = this.ApplySortings(list, mode).ToArray();
                    var num = 0;
                    for (var i = 0; i < 40; i++)
                    {
                        if (array.Length > 0)
                        {
                            chest.item[i] = array[i];
                            if (num >= array.Count() - 1)
                            {
                                break;
                            }
                            num++;
                        }
                    }
                    if (Main.netMode == 1)
                    {
                        if (player.chest < 0)
                        {
                            for (var i = 0; i < 40; i++)
                            {
                                NetMessage.SendData(32, -1, -1, "", player.chest, (float)i, 0f, 0f, 0, 0, 0);
                            }
                        }
                        else
                        {
                            NetMessage.SendData(33, -1, -1, "", Main.player[Main.myPlayer].chest, 0f, 0f, 0f, 0, 0, 0);
                        }
                    }
                }
            }
        }
        private List<Item> ApplySortings(List<Item> input, int mode)
        {
            var list = new List<Item>();
            foreach (var current in this.sortingSets)
            {
                var list2 = current.sortOutValid(ref input);
                if (mode == 1)
                {
                    list2 = (
                        from x in list2
                        orderby x.name, x.stack descending
                        select x).ToList<Item>();
                }
                else if (mode == 2)
                {
                    list2 = (
                        from X in list2
                        orderby X.rare descending
                        select X).ThenByDescending(x => x.stack).ToList();
                }
                else
                {
                    list2 = (
                        from x in list2
                        orderby x.type, x.stack descending
                        select x).ToList<Item>();
                }
                list.AddRange(list2);
            }
            if (mode == 1)
            {
                input = (
                    from x in input
                    orderby x.name, x.stack descending
                    select x).ToList<Item>();
            }
            else if (mode == 2)
            {
                input = (
                    from x in input
                    orderby x.rare descending, x.stack descending
                    select x).ToList<Item>();
            }
            else
            {
                input = (
                    from x in input
                    orderby x.type, x.stack descending
                    select x).ToList<Item>();
            }
            foreach (var current2 in input)
            {
                list.Add(current2);
            }
            list.RemoveAll(x => x.type == 0);
            return list;
        }
    }
}
