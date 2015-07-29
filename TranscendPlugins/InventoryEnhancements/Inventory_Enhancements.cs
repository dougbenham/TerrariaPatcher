using System;
using System.Diagnostics;
using System.IO;
using GTRPlugins.Utils;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.UI;

namespace GTRPlugins
{
    public static class Inventory_Enhancements
    {
        public static Config config;
        public static bool _notInGame;
        public static string version = "1.1.4.5";

        public static void Init(object sender, EventArgs e)
        {
            config = new Config();
            config.LoadConfig();
        }

        public static void Update(object sender, EventArgs e)
        {
            if (Main.gameMenu && !_notInGame) _notInGame = true;
            if (Main.gameMenu) return;
            if (Input.KeyPressed(Config.CharToXnaKey(config.SortKey)) && Main.keyState.IsKeyUp(Keys.LeftShift) && config.SortHotkeyEnabled)
            {
                AutoTrash.Trash();
                Clean();
                Sort();
            }
            if (Input.KeyPressed(Config.CharToXnaKey(config.HotbarSwapKey)) && config.HotbarSwapKeyEnabled)
            {
                SwapHotbar(config.HotbarCycle);
            }
            if (Input.KeyPressed(Config.CharToXnaKey(config.QSKey)) && config.QSHotkeyEnabled)
            {
                QuickStack();
            }
            if (Input.KeyPressed(Keys.O) && (Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.LeftAlt)))
            {
                if (Main.keyState.IsKeyDown(Keys.LeftShift)) ReloadConfig();
                else LaunchConfigurator();
            }
            if (_notInGame) ReloadConfig(true);
            _notInGame = false;
        }

        public static void Clean()
        {
            AutoClean.Clean();
        }

        public static void Sort()
        {
            AutoSort.Sort(config.SubsortMode);
            Main.PlaySound(7, -1, -1, 1);
        }

        public static void SwapHotbar(bool cycle = false)
        {
            HotbarSwap.Swap(config.HotbarCycle);
            Main.PlaySound(7, -1, -1, 1);
        }

        public static void ReloadConfig(bool silent = false)
        {
            AutoSort.ReloadConfig();
            config.LoadConfig();
            if (!silent) Main.NewText("Inventory Enhancements Configuration Reloaded!", 0, 200, 160);
            if (!char.IsLetter(config.SortKey) && config.SortHotkeyEnabled == true)
            {
                Main.NewText("The key \"" + config.SortKey + "\" is not a valid binding. It must be a letter. Reverting to Z", 200, 20, 20);
                config.SortKey = 'Z';
            }
            if (!char.IsLetter(config.QSKey) && config.QSHotkeyEnabled == true)
            {
                Main.NewText("The key \"" + config.QSKey + "\" is not a valid binding. It must be a letter. Reverting to C", 200, 20, 20);
                config.QSKey = 'C';
            }
            if (!char.IsLetter(config.HotbarSwapKey) && config.HotbarSwapKeyEnabled == true)
            {
                Main.NewText("The key \"" + config.HotbarSwapKey + "\" is not a valid binding. It must be a letter. Reverting to X", 200, 20, 20);
                config.HotbarSwapKey = 'X';
            }
            if (!char.IsLetter(config.CAKey) && config.CAHotkeyEnabled == true)
            {
                Main.NewText("The key \"" + config.CAKey + "\" is not a valid binding. It must be a letter. Reverting to V", 200, 20, 20);
                config.CAKey = 'V';
            }
            config.SaveConfig();
        }

        public static void QuickStack()
        {
            if (Main.player[Main.myPlayer].chest > -1 || (Main.player[Main.myPlayer].chest == -2 || Main.player[Main.myPlayer].chest == -3))
            {
                ChestUI.QuickStack();
            }
            else
            {
                Main.player[Main.myPlayer].QuickStackAllChests();
                Recipe.FindRecipes();
            }
        }

        public static void LaunchConfigurator()
        {
            string launchDir = System.Reflection.Assembly.GetEntryAssembly().Location;
            launchDir = Path.GetDirectoryName(launchDir) + @"\Configurator.exe";
            if (!File.Exists(launchDir))
            {
                Main.NewText("Configurator not installed.", 255, 20, 20);
                return;
            }
            Process pr = new Process();
            pr.StartInfo.FileName = launchDir;
            pr.Exited += ProcessExited;
            pr.EnableRaisingEvents = true;
            pr.Start();
        }

        public static void ProcessExited(object sender, System.EventArgs e)
        {
            ReloadConfig();
        }
    }
}
