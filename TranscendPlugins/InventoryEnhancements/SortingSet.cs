using System.Collections.Generic;
using System.IO;
using System.Linq;
using GTRPlugins.Utils;
using Terraria;

namespace GTRPlugins
{
    internal class SortingSet
    {
        public int[] ids;
        public string name;
        public static Dictionary<int, string> orderOfLists = new Dictionary<int, string>();
        public static List<SortingSet> _addedSets = new List<SortingSet>();
        private static string setConfigPath = Main.SavePath + Path.DirectorySeparatorChar + "Sorting Order.json";
        public SortingSet(string name, int[] ids)
        {
            this.ids = ids;
            this.name = name;
        }
        public static List<SortingSet> OrganizeSets()
        {
            LoadSets();
            var list = new List<SortingSet>();
            List<SortingSet> result;
            if (orderOfLists.Count == 0)
            {
                result = list;
            }
            else
            {
                for (var i = 1; i < orderOfLists.Count; i++)
                {
                    if (orderOfLists.ContainsKey(i))
                    {
                        list.Add(GetSetFromName(orderOfLists[i]));
                    }
                }
                foreach (var current in _addedSets)
                {
                    if (!list.Contains(current))
                    {
                        list.Add(current);
                    }
                }
                result = list;
            }
            return result;
        }
        public static int[] GetIdsFromName(string name)
        {
            var list = new List<int>();
            foreach (var current in _addedSets)
            {
                if (current.name == name)
                {
                    var array = current.ids;
                    for (var i = 0; i < array.Length; i++)
                    {
                        var item = array[i];
                        list.Add(item);
                    }
                }
            }
            return list.ToArray();
        }
        public static SortingSet GetSetFromName(string name)
        {
            SortingSet result;
            foreach (var current in _addedSets)
            {
                if (current.name == name)
                {
                    result = current;
                    return result;
                }
            }
            result = null;
            return result;
        }
        private static void LoadSets()
        {
            try
            {
                orderOfLists = Json.DeSerialize<Dictionary<int, string>>(setConfigPath);
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
            foreach (var current in _addedSets)
            {
                orderOfLists.Add(orderOfLists.Count + 1, current.name);
            }
        }
        public List<Item> sortOutValid(ref List<Item> items)
        {
            var list = new List<Item>();
            for (var i = 0; i < items.Count(); i++)
            {
                if (ids.Contains(items[i].type))
                {
                    list.Add(items[i]);
                    items.Remove(items[i]);
                    i--;
                }
            }
            return (
                from x in list
                orderby x.stack, x.type
                select x).ToList<Item>();
        }
    }
}
