using System.Collections.Generic;
using System.IO;
using System.Linq;
using GTRPlugins.Utils;
using Terraria;

namespace GTRPlugins
{
    internal class SortingSet
    {//
        public int[] ids;
        public string name;
        //
        public static Dictionary<int, string> orderOfLists = new Dictionary<int, string> { };
        public static List<SortingSet> _addedSets = new List<SortingSet> { };

        private static string setConfigPath = Main.SavePath + Path.DirectorySeparatorChar + "Sorting Order.json";

        public SortingSet(string name, int[] ids)
        {
            this.ids = ids;
            this.name = name;
        }

        public static List<SortingSet> OrganizeSets()
        {
            LoadSets();
            var returnSet = new List<SortingSet> { };
            if (orderOfLists.Count == 0) return returnSet;
            for (var i = 1; i < orderOfLists.Count; i++)
            {
                var set = GetSetFromName(orderOfLists[i]);
                if (set != null && orderOfLists.ContainsKey(i)) returnSet.Add(set);
                //foreach (KeyValuePair<int, string> kvp in orderOfLists)
                //{
                //    if (kvp.Key == i)
                //    {
                //        returnSet.Add(GetSetFromName(kvp.Value));
                //        break;
                //    }
                //}
            }

            foreach (var set in _addedSets)
            {
                if (!returnSet.Contains(set)) returnSet.Add(set);
            }
            return returnSet;
        }

        public static int[] GetIdsFromName(string name)
        {
            var toret = new List<int> { };
            foreach (var set in _addedSets)
            {
                if (set.name == name)
                {
                    foreach (var i in set.ids)
                    {
                        toret.Add(i);
                    }
                }
            }
            return toret.ToArray();
        }

        public static SortingSet GetSetFromName(string name)
        {
            return _addedSets.FirstOrDefault(set => set.name == name);
        }

        private static void LoadSets()
        {
            try
            {
                orderOfLists = Json.DeSerialize<Dictionary<int, string>>(setConfigPath);
                if (orderOfLists.Count < _addedSets.Count)
                {
                    foreach (var set in _addedSets)
                    {
                        if (!(orderOfLists.Any(x => x.Value.ToLower() == set.name.ToLower())))
                        {
                            orderOfLists.Add(orderOfLists.Count, set.name);
                        }
                    }
                    Json.Serialize(orderOfLists, setConfigPath);
                }
                if (orderOfLists.Count == 0)
                {
                    GenOrder();
                    Json.Serialize(orderOfLists, setConfigPath);
                }
            }
            catch
            {
                try
                {
                    GenOrder();
                    Json.Serialize(orderOfLists, setConfigPath);
                }
                catch
                {
                    GenOrder();
                }
            }
        }

        private static void GenOrder()
        {
            orderOfLists.Clear();
            foreach (var set in _addedSets)
            {
                orderOfLists.Add(orderOfLists.Count + 1, set.name);
            }
        }

        public List<Item> sortOutValid(ref List<Item> items)
        {
            var temp = new List<Item> { };
            for (var i = 0; i < items.Count(); i++)
            {
                if (ids.Contains(items[i].type))
                {
                    temp.Add(items[i]);
                    items.Remove(items[i]);
                    i--;
                }
            }
            temp = temp.OrderBy(x => x.stack).ThenBy(x => x.type).ToList();
            return temp;
        }
    }
}
