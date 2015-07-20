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
        [JsonProperty("Subsort Mode")]
        public int SubsortMode = 0;
        [JsonProperty("Cycle Hotbar")]
        public bool HotbarCycle = false;
        [JsonProperty("Auto Trash")]
        public bool AutoTrash = false;
        [JsonProperty("Sort")]
        public char SortKey = 'Z';
        [JsonProperty("Sorting Key Enabled")]
        public bool SortHotkeyEnabled = true;
        [JsonProperty("Swap Hotbar")]
        public char HotbarSwapKey = 'X';
        [JsonProperty("Swap Hotbar Key Enabled")]
        public bool HotbarSwapKeyEnabled = true;
        [JsonProperty("Quick Stack")]
        public char QSKey = 'C';
        [JsonProperty("Quick Stack Key Enabled")]
        public bool QSHotkeyEnabled = true;
        [JsonProperty("Sort Chests")]
        public bool SortChests = true;
        [JsonProperty("Trash List")]
        public List<int> TrashList = new List<int>
		{
			2338,
			2339,
			2337
		};
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
            string path = Main.SavePath + Path.DirectorySeparatorChar + "IEConfig.json";
            try
            {
                Config config = new Config();
                int? firstInstance = Json.GetFirstInstance<int>("Subsort Mode", path);
                if (firstInstance.HasValue && firstInstance < 3 && firstInstance > -1)
                {
                    this.SubsortMode = firstInstance.Value;
                    config.SubsortMode = this.SubsortMode;
                }
                else
                {
                    config.SubsortMode = 0;
                }
                bool? firstInstance2 = Json.GetFirstInstance<bool>("Hotbar Cycle", path);
                if (firstInstance2.HasValue)
                {
                    this.HotbarCycle = firstInstance2.Value;
                    config.HotbarCycle = this.HotbarCycle;
                }
                else
                {
                    config.HotbarCycle = false;
                }
                bool? firstInstance3 = Json.GetFirstInstance<bool>("Auto Trash", path);
                if (firstInstance3.HasValue)
                {
                    this.AutoTrash = firstInstance3.Value;
                    config.AutoTrash = this.AutoTrash;
                }
                else
                {
                    config.AutoTrash = false;
                }
                bool? firstInstance4 = Json.GetFirstInstance<bool>("Sorting Key Enabled", path);
                if (firstInstance4.HasValue)
                {
                    this.SortHotkeyEnabled = firstInstance4.Value;
                    config.SortHotkeyEnabled = this.SortHotkeyEnabled;
                }
                else
                {
                    config.SortHotkeyEnabled = true;
                }
                bool? firstInstance5 = Json.GetFirstInstance<bool>("Quick Stack Key Enabled", path);
                if (firstInstance5.HasValue)
                {
                    this.QSHotkeyEnabled = firstInstance5.Value;
                    config.QSHotkeyEnabled = this.QSHotkeyEnabled;
                }
                else
                {
                    config.QSHotkeyEnabled = true;
                }
                bool? firstInstance6 = Json.GetFirstInstance<bool>("Swap Hotbar Key Enabled", path);
                if (firstInstance6.HasValue)
                {
                    this.HotbarSwapKeyEnabled = firstInstance6.Value;
                    config.HotbarSwapKeyEnabled = this.HotbarSwapKeyEnabled;
                }
                else
                {
                    config.HotbarSwapKeyEnabled = false;
                }
                bool? firstInstance7 = Json.GetFirstInstance<bool>("Sort Chests", path);
                if (firstInstance7.HasValue)
                {
                    this.SortChests = firstInstance7.Value;
                    config.SortChests = this.SortChests;
                }
                else
                {
                    config.SortChests = true;
                }
                List<int> firstInstanceList = Json.GetFirstInstanceList<int>("Trash List", path);
                this.TrashList = firstInstanceList;
                config.TrashList = this.TrashList;
                char? firstInstance8 = Json.GetFirstInstance<char>("Sort", path);
                char? c = firstInstance8;
                if ((c.HasValue ? new int?((int)c.GetValueOrDefault()) : null).HasValue)
                {
                    this.SortKey = firstInstance8.Value;
                    config.SortKey = this.SortKey;
                }
                char? firstInstance9 = Json.GetFirstInstance<char>("Swap Hotbar", path);
                c = firstInstance9;
                if ((c.HasValue ? new int?((int)c.GetValueOrDefault()) : null).HasValue)
                {
                    this.HotbarSwapKey = firstInstance9.Value;
                    config.HotbarSwapKey = this.HotbarSwapKey;
                }
                char? firstInstance10 = Json.GetFirstInstance<char>("Quick Stack", path);
                c = firstInstance10;
                if ((c.HasValue ? new int?((int)c.GetValueOrDefault()) : null).HasValue)
                {
                    this.QSKey = firstInstance10.Value;
                    config.QSKey = this.QSKey;
                }
                Json.Serialize<Config>(config, path);
            }
            catch
            {
                try
                {
                    Config obj = new Config();
                    Json.Serialize<Config>(obj, path);
                }
                catch
                {
                    this.SubsortMode = 0;
                    this.HotbarCycle = false;
                    this.TrashList = new List<int>
					{
						2338,
						2339,
						2337
					};
                    this.AutoTrash = false;
                }
            }
        }
        public void SaveConfig()
        {
            string path = Main.SavePath + Path.DirectorySeparatorChar + "IEConfig.json";
            try
            {
                Json.Serialize<Config>(this, path);
            }
            catch
            {
            }
        }
        public static Keys CharToXnaKey(char character)
        {
            return (Keys)(char.IsLetter(character) ? char.ToUpper(character) : character);
        }
    }
}
