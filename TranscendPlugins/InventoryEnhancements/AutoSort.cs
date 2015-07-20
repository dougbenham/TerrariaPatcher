using GTRPlugins.Sorting;

namespace GTRPlugins
{
    public static class AutoSort
    {
        public static SortingManager manager;
        public static void Init()
        {
            manager = new SortingManager();
        }
        public static void Sort(int mode)
        {
            manager.Sort(mode);
        }
        public static void ReloadConfig()
        {
            manager.ReloadConfig();
        }
    }
}
