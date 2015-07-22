using System.Collections.Generic;
using System.IO;
using GTRPlugins.Utils;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using Terraria;

namespace GTRPlugins
{
    public class Config
    {
        [JsonPropertyAttribute("Subsort Mode")]
        public int SubsortMode = 0;
        [JsonPropertyAttribute("Cycle Hotbar")]
        public bool HotbarCycle = false;
        [JsonPropertyAttribute("Auto Trash")]
        public bool AutoTrash = false;

        [JsonPropertyAttribute("Sort")]
        public char SortKey = 'Z';
        [JsonPropertyAttribute("Swap Hotbar")]
        public char HotbarSwapKey = 'X';
        [JsonPropertyAttribute("Quick Stack")]
        public char QSKey = 'C';
        [JsonPropertyAttribute("Cycle Ammo")]
        public char CAKey = 'V';
        [JsonPropertyAttribute("Sorting Key Enabled")]
        public bool SortHotkeyEnabled = true;
        [JsonPropertyAttribute("Swap Hotbar Key Enabled")]
        public bool HotbarSwapKeyEnabled = true;
        [JsonPropertyAttribute("Quick Stack Key Enabled")]
        public bool QSHotkeyEnabled = true;
        [JsonPropertyAttribute("Cycle Ammo Key Enabled")]
        public bool CAHotkeyEnabled = true;

        [JsonPropertyAttribute("Sort Chests")]
        public bool SortChests = true;

        [JsonPropertyAttribute("Trash List")]
        public List<int> TrashList = new List<int> { 2338, 2339, 2337 };


        public Config()
        {

        }

        public Config(int subsort, bool cycle)
        {
            this.SubsortMode = subsort;
            this.HotbarCycle = cycle;
        }

        public void LoadConfig()
        {
            Config temp;

            string path = Main.SavePath + Path.DirectorySeparatorChar + "IEConfig.json";
            try
            {
                Config ToSer = new Config();

                int? _subsortMode = Json.GetFirstInstance<int>("Subsort Mode", path);
                if (_subsortMode != null && _subsortMode < 4 && _subsortMode > -1)
                {
                    SubsortMode = (int)_subsortMode;
                    ToSer.SubsortMode = SubsortMode;
                }
                else ToSer.SubsortMode = 0;

                bool? _hotbarCycle = Json.GetFirstInstance<bool>("Cycle Hotbar", path);
                if (_hotbarCycle != null)
                {
                    HotbarCycle = (bool)_hotbarCycle;
                    ToSer.HotbarCycle = HotbarCycle;
                }
                else ToSer.HotbarCycle = false;

                bool? _autoTrash = Json.GetFirstInstance<bool>("Auto Trash", path);
                if (_autoTrash != null)
                {
                    AutoTrash = (bool)_autoTrash;
                    ToSer.AutoTrash = AutoTrash;
                }
                else ToSer.AutoTrash = false;

                bool? _sortKeyEnabled = Json.GetFirstInstance<bool>("Sorting Key Enabled", path);
                if (_sortKeyEnabled != null)
                {
                    SortHotkeyEnabled = (bool)_sortKeyEnabled;
                    ToSer.SortHotkeyEnabled = SortHotkeyEnabled;
                }
                else ToSer.SortHotkeyEnabled = true;

                bool? _caKeyEnabled = Json.GetFirstInstance<bool>("Cycle Ammo Key Enabled", path);
                if (_caKeyEnabled != null)
                {
                    CAHotkeyEnabled = (bool)_caKeyEnabled;
                    ToSer.CAHotkeyEnabled = CAHotkeyEnabled;
                }
                else ToSer.CAHotkeyEnabled = true;

                bool? _qsKeyEnabled = Json.GetFirstInstance<bool>("Quick Stack Key Enabled", path);
                if (_qsKeyEnabled != null)
                {
                    QSHotkeyEnabled = (bool)_qsKeyEnabled;
                    ToSer.QSHotkeyEnabled = QSHotkeyEnabled;
                }
                else ToSer.QSHotkeyEnabled = true;

                bool? _hsKeyEnabled = Json.GetFirstInstance<bool>("Swap Hotbar Key Enabled", path);
                if (_hsKeyEnabled != null)
                {
                    HotbarSwapKeyEnabled = (bool)_hsKeyEnabled;
                    ToSer.HotbarSwapKeyEnabled = HotbarSwapKeyEnabled;
                }
                else ToSer.HotbarSwapKeyEnabled = false;

                bool? _sortChests = Json.GetFirstInstance<bool>("Sort Chests", path);
                if (_sortChests != null)
                {
                    SortChests = (bool)_sortChests;
                    ToSer.SortChests = SortChests;
                }
                else ToSer.SortChests = true;

                List<int> _trashList = Json.GetFirstInstanceList<int>("Trash List", path);
                TrashList = _trashList;
                ToSer.TrashList = TrashList;

                char? _sortKey = Json.GetFirstInstance<char>("Sort", path);
                if (_sortKey != null)
                {
                    SortKey = (char)_sortKey;
                    ToSer.SortKey = SortKey;
                }

                char? _caKey = Json.GetFirstInstance<char>("Cycle Ammo", path);
                if (_caKey != null)
                {
                    CAKey = (char)_caKey;
                    ToSer.CAKey = CAKey;
                }

                char? _hotbarSwapKey = Json.GetFirstInstance<char>("Swap Hotbar", path);
                if (_hotbarSwapKey != null)
                {
                    HotbarSwapKey = (char)_hotbarSwapKey;
                    ToSer.HotbarSwapKey = HotbarSwapKey;
                }

                char? _qsKey = Json.GetFirstInstance<char>("Quick Stack", path);
                if (_qsKey != null)
                {
                    QSKey = (char)_qsKey;
                    ToSer.QSKey = QSKey;
                }

                Json.Serialize(ToSer, path);
            }
            catch
            {
                try
                {
                    temp = new Config();
                    Json.Serialize(temp, path);
                }
                catch
                {
                    this.SubsortMode = 0;
                    this.HotbarCycle = false;
                    this.TrashList = new List<int> { 2338, 2339, 2337 };
                    this.AutoTrash = false;
                }
            }
        }

        public void SaveConfig()
        {
            string path = Main.SavePath + Path.DirectorySeparatorChar + "IEConfig.json";
            try
            {
                Json.Serialize(this, path);
            }
            catch
            {

            }
        }

        public static Keys CharToXnaKey(char character)
        {
            return (Keys)((int)(char.IsLetter(character) ? char.ToUpper(character) : character));
        }
    }
}
