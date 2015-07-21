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
            if (Main.gameMenu)
            {
                return;
            }
            var player = Main.player[Main.myPlayer];
            var list = new List<Item>();
            if (player.chest == -1 || !Inventory_Enhancements.config.SortChests)
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
                if (!array.Any())
                {
                    return;
                }
                var num = 0;
                for (var j = 10; j < 50; j++)
                {
                    if (array[num].type == 0)
                    {
                        num++;
                    }
                    else
                    {
                        var item2 = player.inventory[j];
                        if (!item2.favorited)
                        {
                            player.inventory[j] = array[num];
                            if (num >= array.Count() - 1)
                            {
                                return;
                            }
                            num++;
                        }
                    }
                }
                return;
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
                for (var k = 0; k < 40; k++)
                {
                    var item3 = chest.item[k];
                    if (!item3.favorited)
                    {
                        list.Add(item3);
                        chest.item[k] = new Item();
                    }
                }
                var array2 = this.ApplySortings(list, mode).ToArray();
                if (!array2.Any())
                {
                    return;
                }
                var num2 = 0;
                for (var l = 0; l < 40; l++)
                {
                    if (array2.Length > 0)
                    {
                        chest.item[l] = array2[l];
                        if (num2 >= array2.Count() - 1)
                        {
                            break;
                        }
                        num2++;
                    }
                }
                if (Main.netMode == 1)
                {
                    if (player.chest > -1)
                    {
                        for (var m = 0; m < 40; m++)
                        {
                            NetMessage.SendData(32, -1, -1, "", player.chest, (float)m, 0f, 0f, 0, 0, 0);
                        }
                        return;
                    }
                    NetMessage.SendData(33, -1, -1, "", Main.player[Main.myPlayer].chest, 0f, 0f, 0f, 0, 0, 0);
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
                        select X).ThenByDescending((Item x) => x.stack).ToList();
                }
                else if (mode == 3)
                {
                    list2 = (
                        from x in list2
                        orderby x.value descending, x.stack descending
                        select x).ToList<Item>();
                }
                else
                {
                    list2 = (
                        from x in list2
                        orderby x.type, x.stack descending
                        select x).ToList<Item>();
                }
                foreach (var current2 in list2)
                {
                    list.Add(current2);
                }
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
            else if (mode == 3)
            {
                input = (
                    from x in input
                    orderby x.value descending, x.stack descending
                    select x).ToList<Item>();
            }
            else
            {
                input = (
                    from x in input
                    orderby x.type, x.stack descending
                    select x).ToList<Item>();
            }
            list.AddRange(input);
            list.RemoveAll(x => x.type == 0);
            return list;
        }
    }
}
