using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.IO;
using Terraria.Utilities;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace PluginLoader
{
    public static class Loader
    {
        #region Data

        private static List<Hotkey> hotkeys = new List<Hotkey>();
        private static Keys[] keysdown;
        private static bool control, shift, alt;
        private static bool fresh = true;

        private static List<IPlugin> loadedPlugins = new List<IPlugin>();
        private static bool loaded, reachedMenu;

        #endregion
        
        #region Load

        private static void Load()
        {
            if (!loaded)
            {
                loaded = true;
                
                try
                {
                    var pluginsFolder = @".\Plugins\";
                    var sharedFolder = Path.Combine(pluginsFolder, "Shared");

                    if (!Utils.IsProcessElevated)
                    {
                        MessageBox.Show("Elevated administrator privileges not detected, you may run into issues! If you are running via Steam, please start Steam with elevated administrator privileges.", "Terraria",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    if (!Directory.Exists(pluginsFolder))
                    {
                        MessageBox.Show(@"Your Terraria\Plugins folder is missing.", "Terraria",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Environment.Exit(0);
                    }

                    if (!Directory.Exists(sharedFolder))
                    {
                        MessageBox.Show(@"Your Terraria\Plugins\Shared folder is missing.", "Terraria",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Environment.Exit(0);
                    }
                    
                    var references = AppDomain.CurrentDomain
                        .GetAssemblies()
                        .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                        .Select(a => a.Location).ToList();
                    ExtractAndReference(references, "Newtonsoft.Json.dll");
                    ExtractAndReference(references, "ReLogic.dll", true);

                    Load(references.ToArray(), Directory.EnumerateFiles(pluginsFolder, "*.cs", SearchOption.AllDirectories).ToArray());

                    // Load hotkey binds
                    var result = IniAPI.GetIniKeys("HotkeyBinds").ToList();
                    foreach (var keys in result)
                    {
                        var val = IniAPI.ReadIni("HotkeyBinds", keys, null);
                        var key = ParseHotkey(keys);

                        if (string.IsNullOrEmpty(val) || !val.StartsWith("/") || key == null)
                            MessageBox.Show("Invalid record in [HotkeyBinds]: " + key + ".", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        else
                            RegisterHotkey(val, key);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), string.Empty);
                    throw;
                }
            }
        }

        private static void ExtractAndReference(List<string> references, string dllName, bool forceExtract = false)
        {
            if (!references.Any(s => s.Contains(dllName)))
            {
                // Dynamic compilation requires assemblies to be stored on file, thus we must extract the Newtonsoft.Json.dll embedded resource to a temp file if we want to use it.
                var assembly = Assembly.GetEntryAssembly();
                var error = "Could not extract " + dllName + " from Terraria.";
                var resourceName = assembly.GetManifestResourceNames().FirstOrDefault(s => s.Contains(dllName));
                if (resourceName == null) throw new Exception(error);

                var path = Path.Combine(".", dllName);
                if (!File.Exists(path) || forceExtract)
                {
                    using (var stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        if (stream == null) throw new Exception(error);

                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            stream.CopyTo(fileStream);
                        }
                    }
                }

                references.Add(path);
            }
        }

        private static void Load(string[] references, params string[] sources)
        {
            // http://ayende.com/blog/1376/solving-the-assembly-load-context-problem
            var compilerParams = new CompilerParameters();
            compilerParams.GenerateInMemory = true;
            compilerParams.GenerateExecutable = false;
            compilerParams.TreatWarningsAsErrors = false;
            compilerParams.CompilerOptions = "/optimize";
            compilerParams.ReferencedAssemblies.AddRange(references);

            var provider = new CSharpCodeProvider();
            var compile = provider.CompileAssemblyFromFile(compilerParams, sources);

            if (compile.Errors.HasErrors)
            {
                throw new Exception(compile.Errors.Cast<CompilerError>().Aggregate("", (current, ce) => current + (ce + Environment.NewLine)));
            }

            foreach (var type in compile.CompiledAssembly.GetTypes().Where(type1 => type1.GetInterfaces().Contains(typeof (IPlugin))))
            {
                loadedPlugins.Add(Activator.CreateInstance(type) as IPlugin);
            }
        }

        #endregion
        
        #region Hotkeys

        public static void RegisterHotkey(string command, Keys key, bool control = false, bool shift = false, bool alt = false, bool ignoreModifierKeys = false)
        {
            RegisterHotkey(command, new Hotkey() { Key = key, Control = control, Shift = shift, Alt = alt, IgnoreModifierKeys = ignoreModifierKeys });
        }

        public static void RegisterHotkey(string command, Hotkey key)
        {
            key.Tag = command;
            key.Action = () =>
            {
                var split = command.Substring(1).Split(new[] {' '}, 2);
                var cmd = split[0].ToLower();
                var args = split.Length > 1 ? split[1].Split(' ') : new string[0];

                foreach (var plugin in loadedPlugins.OfType<IPluginChatCommand>())
                    plugin.OnChatCommand(cmd, args);
            };
            RegisterHotkey(key);
        }

        public static void RegisterHotkey(Action action, Keys key, bool control = false, bool shift = false, bool alt = false, bool ignoreModifierKeys = false)
        {
            RegisterHotkey(new Hotkey() { Action = action, Key = key, Control = control, Shift = shift, Alt = alt, IgnoreModifierKeys = ignoreModifierKeys });
        }

        public static void RegisterHotkey(Action action, Hotkey key)
        {
            key.Action = action;
            RegisterHotkey(key);
        }

        public static void RegisterHotkey(Hotkey hotkey)
        {
            hotkeys.Add(hotkey);
        }

        public static void UnregisterHotkey(Keys key, bool control = false, bool shift = false, bool alt = false, bool ignoreModifierKeys = false)
        {
            UnregisterHotkey(new Hotkey() { Key = key, Control = control, Shift = shift, Alt = alt, IgnoreModifierKeys = ignoreModifierKeys });
        }

        public static void UnregisterHotkey(Hotkey hotkey)
        {
            hotkeys.RemoveAll(key => key.Equals(hotkey));
        }

        public static ICollection<Hotkey> GetHotkeys()
        {
            return hotkeys.AsReadOnly();
        }

        public static Hotkey ParseHotkey(string hotkey)
        {
            var key = Keys.None;
            var control = false;
            var shift = false;
            var alt = false;
            bool hotkeyParseFailed = false;
            foreach (var keyStr in hotkey.Split(','))
            {
                switch (keyStr.ToLower())
                {
                    case "control":
                        control = true;
                        break;
                    case "shift":
                        shift = true;
                        break;
                    case "alt":
                        alt = true;
                        break;
                    default:
                        if (key != Keys.None || !Keys.TryParse(keyStr, true, out key)) hotkeyParseFailed = true;
                        break;
                }
            }

            if (hotkeyParseFailed || key == Keys.None)
                return null;

            return new Hotkey() {Key = key, Control = control, Alt = alt, Shift = shift};
        }

        public static bool IsAltModifierKeyDown()
        {
            return alt;
        }

        public static bool IsControlModifierKeyDown()
        {
            return control;
        }

        public static bool IsShiftModifierKeyDown()
        {
            return shift;
        }

        #endregion

        #region Main
        
        public static void OnInitialize()
        {
            Load();

            foreach (var plugin in loadedPlugins.OfType<IPluginInitialize>())
                plugin.OnInitialize();
        }

        public static void OnDrawSplash()
        {
            foreach (var plugin in loadedPlugins.OfType<IPluginDrawSplash>())
                plugin.OnDrawSplash();
        }

        public static void OnDrawInventory()
        {
            foreach (var plugin in loadedPlugins.OfType<IPluginDrawInventory>())
                plugin.OnDrawInventory();
        }

        public static void OnDrawInterface()
        {
            foreach (var plugin in loadedPlugins.OfType<IPluginDrawInterface>())
                plugin.OnDrawInterface();
        }

        public static void OnPreUpdate()
        {
            if (Main.showSplash)
                return;

            if (Main.menuMode != 10)
                return;

            if (!Main.blockInput && !Main.drawingPlayerChat && !Main.editSign && !Main.editChest)
            {
                keysdown = Main.keyState.GetPressedKeys();
                control = keysdown.Contains(Keys.LeftControl) || keysdown.Contains(Keys.RightControl);
                shift = keysdown.Contains(Keys.LeftShift) || keysdown.Contains(Keys.RightShift);
                alt = keysdown.Contains(Keys.LeftAlt) || keysdown.Contains(Keys.RightAlt);
                var anyPresses = false;
                foreach (var hotkey in hotkeys)
                {
                    if (keysdown.Contains(hotkey.Key) &&
                        (hotkey.IgnoreModifierKeys || (control == hotkey.Control && shift == hotkey.Shift && alt == hotkey.Alt)))
                    {
                        anyPresses = true;
                        if (fresh) hotkey.Action();
                    }
                }

                fresh = !anyPresses;
            }

            foreach (var plugin in loadedPlugins.OfType<IPluginPreUpdate>())
                plugin.OnPreUpdate();
        }

        public static void OnUpdate()
        {
            foreach (var plugin in loadedPlugins.OfType<IPluginUpdate>())
                plugin.OnUpdate();
        }

        public static void OnUpdateTime()
        {
            foreach (var plugin in loadedPlugins.OfType<IPluginUpdateTime>())
                plugin.OnUpdateTime();
        }

        public static bool OnCheckXmas()
        {
            var ret = false;

            foreach (var plugin in loadedPlugins.OfType<IPluginCheckSeason>())
                ret = plugin.OnCheckXmas() || ret;

            return ret;
        }

        public static bool OnCheckHalloween()
        {
            var ret = false;

            foreach (var plugin in loadedPlugins.OfType<IPluginCheckSeason>())
                ret = plugin.OnCheckHalloween() || ret;

            return ret;
        }

        public static bool OnPlaySound(int type, int x, int y, int style)
        {
            var ret = false;
            foreach (var plugin in loadedPlugins.OfType<IPluginPlaySound>())
                ret = plugin.OnPlaySound(type, x, y, style) || ret;

            return ret;
        }

        #endregion

        #region Player

        public static void OnPlayerPreSpawn(Player player)
        {
            foreach (var plugin in loadedPlugins.OfType<IPluginPlayerPreSpawn>())
                plugin.OnPlayerPreSpawn(player);
        }

        public static void OnPlayerSpawn(Player player)
        {
            foreach (var plugin in loadedPlugins.OfType<IPluginPlayerSpawn>())
                plugin.OnPlayerSpawn(player);
        }

        public static void OnPlayerLoad(PlayerFileData playerFileData, Player player, BinaryReader binaryReader)
        {
            foreach (var plugin in loadedPlugins.OfType<IPluginPlayerLoad>())
                plugin.OnPlayerLoad(playerFileData, player, binaryReader);
        }

        public static void OnPlayerSave(PlayerFileData playerFileData, Player player, BinaryWriter binaryWriter)
        {
            foreach (var plugin in loadedPlugins.OfType<IPluginPlayerSave>())
                plugin.OnPlayerSave(playerFileData, player, binaryWriter);
        }

        public static void OnPlayerUpdate(Player player)
        {
            foreach (var plugin in loadedPlugins.OfType<IPluginPlayerUpdate>())
                plugin.OnPlayerUpdate(player);
        }

        public static void OnPlayerPreUpdate(Player player)
        {
            foreach (var plugin in loadedPlugins.OfType<IPluginPlayerPreUpdate>())
                plugin.OnPlayerPreUpdate(player);
        }

        public static void OnPlayerUpdateBuffs(Player player)
        {
            foreach (var plugin in loadedPlugins.OfType<IPluginPlayerUpdateBuffs>())
                plugin.OnPlayerUpdateBuffs(player);
        }

        public static void OnPlayerUpdateEquips(Player player)
        {
            foreach (var plugin in loadedPlugins.OfType<IPluginPlayerUpdateEquips>())
                plugin.OnPlayerUpdateEquips(player);
        }

        public static void OnPlayerUpdateArmorSets(Player player)
        {
            foreach (var plugin in loadedPlugins.OfType<IPluginPlayerUpdateArmorSets>())
                plugin.OnPlayerUpdateArmorSets(player);
        }

        public static bool OnPlayerHurt(Player player, PlayerDeathReason damageSource, int damage, int hitDirection, bool pvp, bool quiet, bool crit, int cooldownCounter, bool dodgeable, out double result)
        {
            result = 0.0;
            var ret = false;
            foreach (var plugin in loadedPlugins.OfType<IPluginPlayerHurt>())
            {
	            if (plugin.OnPlayerHurt(player, damageSource, damage, hitDirection, pvp, quiet, crit, cooldownCounter, dodgeable, out var temp))
                {
                    ret = true;
                    result = temp;
                }
            }

            return ret;
        }

        public static bool OnPlayerKillMe(Player player, PlayerDeathReason damageSource, double dmg, int hitDirection, bool pvp)
        {
            var ret = false;
            foreach (var plugin in loadedPlugins.OfType<IPluginPlayerKillMe>())
                ret = plugin.OnPlayerKillMe(player, damageSource, dmg, hitDirection, pvp) || ret;

            return ret;
        }

        public static void OnPlayerPickAmmo(Player player, Item weapon, ref int shoot, ref float speed, ref bool canShoot, ref int damage, ref float knockback, ref int usedAmmoItemId, bool dontConsume)
        {
            foreach (var plugin in loadedPlugins.OfType<IPluginPlayerPickAmmo>())
                plugin.OnPlayerPickAmmo(player, weapon, ref shoot, ref speed, ref canShoot, ref damage, ref knockback, ref usedAmmoItemId, dontConsume);
        }

        public static bool OnPlayerGetItem(Player player, WorldItem newItem, GetItemSettings settings, out Item resultItem)
        {
            resultItem = null;
            var ret = false;
            foreach (var plugin in loadedPlugins.OfType<IPluginPlayerGetItem>())
            {
	            if (plugin.OnPlayerGetItem(player, newItem, settings, out var temp))
                {
                    ret = true;
                    resultItem = temp;
                }
            }

            return ret;
        }

        public static bool OnPlayerQuickBuff(Player player)
        {
            var ret = false;
            foreach (var plugin in loadedPlugins.OfType<IPluginPlayerQuickBuff>())
                ret = plugin.OnPlayerQuickBuff(player) || ret;

            return ret;
        }

        #endregion
        
        #region Item

        public static void OnItemSetDefaults(Item item)
        {
            Load();

            foreach (var plugin in loadedPlugins.OfType<IPluginItemSetDefaults>())
                plugin.OnItemSetDefaults(item);
        }

        public static bool OnItemSlotRightClick(Item[] inv, int context, int slot)
        {
            var ret = false;
            foreach (var plugin in loadedPlugins.OfType<IPluginItemSlotRightClick>())
                ret = plugin.OnItemSlotRightClick(inv, context, slot) || ret;

            return ret;
        }

        public static bool OnItemRollAPrefix(Item item, UnifiedRandom random, ref int rolledPrefix, out bool result)
        {
	        result = false;
	        var ret = false;
	        foreach (var plugin in loadedPlugins.OfType<IPluginItemRollAPrefix>())
	        {
		        if (plugin.OnItemRollAPrefix(item, random, ref rolledPrefix, out var temp))
		        {
                    ret = true;
                    result = temp;
		        }
	        }

	        return ret;
        }

        #endregion

        #region Projectile

        public static void OnProjectileAI001(Projectile projectile)
        {
            foreach (var plugin in loadedPlugins.OfType<IPluginProjectileAI>())
                plugin.OnProjectileAI001(projectile);
        }

        #endregion

        #region NetMessage

        public static bool OnSendChatMessageFromClient(ChatMessage msg)
        {
            var text = msg.Text;
            bool chatRet = false;
            
            if (!string.IsNullOrEmpty(text) && text[0] == '/')
            {
                var split = text.Substring(1).Split(new[] {' '}, 2);
                var cmd = split[0].ToLower();
                var args = split.Length > 1 ? split[1].Split(' ') : new string[0];

                switch (cmd)
                {
                    case "plugins":
                        Main.NewText(string.Join(", ", loadedPlugins.Select(plugin => plugin.GetType().Name)), Color.Purple.R, Color.Purple.G, Color.Purple.B);
                        chatRet = true;
                        break;
                    default:
                        foreach (var plugin in loadedPlugins.OfType<IPluginChatCommand>())
                            chatRet = plugin.OnChatCommand(cmd, args) || chatRet;
                        break;
                }
            }

            return chatRet;
        }

        #endregion

        #region Lighting

        public static bool OnLightingGetColor(int x, int y, out Color color)
        {
            color = Color.White;
            var ret = false;
            foreach (var plugin in loadedPlugins.OfType<IPluginLightingGetColor>())
            {
	            var result = plugin.OnLightingGetColor(x, y, out var temp);
                if (result)
                {
                    ret = true;
                    color = temp;
                }
            }

            return ret;
        }

        #endregion

        #region Chest

        public static void OnChestSetupShop(Chest chest, int type)
        {
            foreach (var plugin in loadedPlugins.OfType<IPluginChestSetupShop>())
            {
                plugin.OnChestSetupShop(chest, type);
            }
        }

        #endregion

        #region NPC

        public static bool OnNPCLoot(NPC npc)
        {
            var ret = false;
            foreach (var plugin in loadedPlugins.OfType<IPluginNPCLoot>())
                ret = plugin.OnNPCLoot(npc) || ret;

            return ret;
        }

        #endregion
    }
}
