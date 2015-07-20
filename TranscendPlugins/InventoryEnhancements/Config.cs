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
        [JsonProperty("Swap Hotbar")]
        public char HotbarSwapKey = 'X';
        [JsonProperty("Quick Stack")]
        public char QSKey = 'C';
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
                List<int> firstInstanceList = Json.GetFirstInstanceList<int>("Trash List", path);
                this.TrashList = firstInstanceList;
                config.TrashList = this.TrashList;
                char? firstInstance4 = Json.GetFirstInstance<char>("Sort", path);
                char? c = firstInstance4;
                if ((c.HasValue ? new int?((int)c.GetValueOrDefault()) : null).HasValue)
                {
                    this.SortKey = firstInstance4.Value;
                    config.SortKey = this.SortKey;
                }
                char? firstInstance5 = Json.GetFirstInstance<char>("Swap Hotbar", path);
                c = firstInstance5;
                if ((c.HasValue ? new int?((int)c.GetValueOrDefault()) : null).HasValue)
                {
                    this.HotbarSwapKey = firstInstance5.Value;
                    config.HotbarSwapKey = this.HotbarSwapKey;
                }
                char? firstInstance6 = Json.GetFirstInstance<char>("Quick Stack", path);
                c = firstInstance6;
                if ((c.HasValue ? new int?((int)c.GetValueOrDefault()) : null).HasValue)
                {
                    this.QSKey = firstInstance6.Value;
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
