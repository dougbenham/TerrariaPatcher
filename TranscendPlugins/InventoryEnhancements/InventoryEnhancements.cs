using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using GTRPlugins.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.UI;

namespace GTRPlugins
{
    public static class InventoryEnhancements
    {
        public static Config config;
        public static bool _notInGame;
        public static string version = "1.0.9";
        public static void Init()
        {
            config = new Config();
            config.LoadConfig();
            AutoSort.Init();
        }
        public static void Update(GameTime gameTime)
        {
            if (Main.gameMenu && !_notInGame)
            {
                _notInGame = true;
            }
            if (!Main.gameMenu)
            {
                if (Input.KeyPressed(Config.CharToXnaKey(config.SortKey), true) && Main.keyState.IsKeyUp(Keys.LeftShift))
                {
                    AutoTrash.Trash();
                    Clean();
                    Sort();
                }
                if (Input.KeyPressed(Config.CharToXnaKey(config.SortKey), true) && Main.keyState.IsKeyDown(Keys.LeftShift))
                {
                    if (config.AutoTrash)
                    {
                        config.AutoTrash = false;
                        Main.NewText("Autotrash Disabled", 255, 20, 20, false);
                        config.SaveConfig();
                    }
                    else
                    {
                        config.AutoTrash = true;
                        Main.NewText("Autotrash Enabled", 255, 20, 20, false);
                        config.SaveConfig();
                    }
                }
                if (Input.KeyPressed(Config.CharToXnaKey(config.HotbarSwapKey), true))
                {
                    SwapHotbar(config.HotbarCycle);
                }
                if (Input.KeyPressed(Config.CharToXnaKey(config.QSKey), true))
                {
                    QuickStack();
                }
                if (Input.KeyPressed(Keys.O, true) && (Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.LeftAlt)))
                {
                    if (Main.keyState.IsKeyDown(Keys.LeftShift))
                    {
                        ReloadConfig(false);
                    }
                    else
                    {
                        LaunchConfigurator();
                    }
                }
                if (Main.netMode != 1 && config.AutoTrash)
                {
                    AutoTrash.Trash();
                }
                if (_notInGame)
                {
                    ReloadConfig(true);
                }
                _notInGame = false;
            }

            Input.Update();
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
            if (!silent)
            {
                Main.NewText("Inventory Enhancements Configuration Reloaded!", 0, 200, 160, false);
            }
            if (!char.IsLetter(config.SortKey))
            {
                Main.NewText("The key \"" + config.SortKey + "\" is not a valid binding. It must be a letter. Reverting to Z", 200, 20, 20, false);
                config.SortKey = 'Z';
            }
            if (!char.IsLetter(config.QSKey))
            {
                Main.NewText("The key \"" + config.QSKey + "\" is not a valid binding. It must be a letter. Reverting to C", 200, 20, 20, false);
                config.QSKey = 'C';
            }
            if (!char.IsLetter(config.HotbarSwapKey))
            {
                Main.NewText("The key \"" + config.HotbarSwapKey + "\" is not a valid binding. It must be a letter. Reverting to X", 200, 20, 20, false);
                config.HotbarSwapKey = 'X';
            }
            config.SaveConfig();
        }
        public static void QuickStack()
        {
            if (Main.player[Main.myPlayer].chest > -1 || Main.player[Main.myPlayer].chest == -2 || Main.player[Main.myPlayer].chest == -3)
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
            var text = Assembly.GetEntryAssembly().Location;
            text = Path.GetDirectoryName(text) + "\\Configurator.exe";
            if (!File.Exists(text))
            {
                Main.NewText("Configurator not installed.", 255, 20, 20, false);
            }
            else
            {
                var process = new Process();
                process.StartInfo.FileName = text;
                process.Exited += ProcessExited;
                process.EnableRaisingEvents = true;
                process.Start();
            }
        }
        public static void ProcessExited(object sender, EventArgs e)
        {
            ReloadConfig(false);
        }
    }
}
