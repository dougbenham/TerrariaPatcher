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
        private static bool loaded;

        private static readonly Dictionary<Type, Array> dispatchCache = new Dictionary<Type, Array>();

        public static readonly string DataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TerrariaPatcher");

        private static string LogPath => Path.Combine(DataFolder, "PluginLoader.log");

        #endregion

        #region Diagnostics

        private static void Log(string message)
        {
            try
            {
                Directory.CreateDirectory(DataFolder);
                File.AppendAllText(LogPath, DateTime.Now.ToString("s") + " " + message + Environment.NewLine);
            }
            catch
            { }
        }

        private static void ReportLoadError(string message)
        {
            Log(message);
            MessageBox.Show(message + Environment.NewLine + Environment.NewLine + "See " + LogPath + " for details.",
                "Terraria", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private static void Disable(IPlugin plugin, Exception ex)
        {
            var name = plugin.GetType().Name;

            loadedPlugins.Remove(plugin);
            dispatchCache.Clear();

            Log("Plugin '" + name + "' threw and has been disabled for this session." + Environment.NewLine + ex);

            try
            {
                Main.NewText("Plugin '" + name + "' errored and has been disabled. See " + LogPath, Color.Red.R, Color.Red.G, Color.Red.B);
            }
            catch
            { }
        }

        #endregion

        #region Dispatch

        private static T[] PluginsOf<T>() where T : class, IPlugin
        {
	        if (!dispatchCache.TryGetValue(typeof(T), out var cached))
            {
                cached = loadedPlugins.OfType<T>().ToArray();
                dispatchCache[typeof(T)] = cached;
            }

            return (T[]) cached;
        }

        private static void Dispatch<T>(Action<T> action) where T : class, IPlugin
        {
            foreach (var plugin in PluginsOf<T>())
            {
                try
                {
                    action(plugin);
                }
                catch (Exception ex)
                {
                    Disable(plugin, ex);
                }
            }
        }

        private static bool DispatchAny<T>(Func<T, bool> func) where T : class, IPlugin
        {
            var ret = false;

            foreach (var plugin in PluginsOf<T>())
            {
                try
                {
                    ret = func(plugin) || ret;
                }
                catch (Exception ex)
                {
                    Disable(plugin, ex);
                }
            }

            return ret;
        }

        #endregion

        #region Load

        private static void Load()
        {
            if (loaded) return;
            loaded = true;

            try
            {
                var pluginsFolder = @".\Plugins\";
                var sharedFolder = Path.Combine(pluginsFolder, "Shared");

                if (!Utils.IsFolderWritable(Environment.CurrentDirectory))
                {
                    MessageBox.Show(
                        "Terraria cannot write to its own folder, so plugin settings (Plugins.ini) will not be saved." + Environment.NewLine + Environment.NewLine +
                        "If you are running via Steam, start Steam with elevated administrator privileges.", "Terraria",
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

                Compile(references.ToArray(), sharedFolder, GetPluginUnits(pluginsFolder, sharedFolder));

                LoadHotkeyBinds();
            }
            catch (Exception ex)
            {
                ReportLoadError("Failed to load plugins." + Environment.NewLine + ex);
            }
        }

        private static Dictionary<string, string[]> GetPluginUnits(string pluginsFolder, string sharedFolder)
        {
            var units = new Dictionary<string, string[]>();

            foreach (var file in Directory.EnumerateFiles(pluginsFolder, "*.cs"))
                units[Path.GetFileNameWithoutExtension(file)] = new[] { file };

            foreach (var folder in Directory.EnumerateDirectories(pluginsFolder))
            {
                if (string.Equals(Path.GetFullPath(folder), Path.GetFullPath(sharedFolder), StringComparison.OrdinalIgnoreCase))
                    continue;

                var sources = Directory.EnumerateFiles(folder, "*.cs", SearchOption.AllDirectories).ToArray();
                if (sources.Length > 0)
                    units[Path.GetFileName(folder)] = sources;
            }

            return units;
        }

        /// <summary>
        /// Compiles every plugin into one assembly. If that fails, falls back to compiling each plugin on its
        /// own so that a single plugin with a compile error does not cost the player every other plugin.
        /// </summary>
        private static void Compile(string[] references, string sharedFolder, Dictionary<string, string[]> units)
        {
            var shared = Directory.EnumerateFiles(sharedFolder, "*.cs", SearchOption.AllDirectories).ToArray();
            var everything = shared.Concat(units.Values.SelectMany(sources => sources)).ToArray();

            if (TryCompile(references, everything, out var assembly, out var combinedErrors))
            {
                Instantiate(assembly);
                return;
            }

            Log("Compiling all plugins together failed, retrying one plugin at a time." + Environment.NewLine + combinedErrors);

            var failures = new List<string>();
            foreach (var unit in units)
            {
	            if (TryCompile(references, shared.Concat(unit.Value).ToArray(), out var single, out var errors))
                    Instantiate(single);
                else
                {
                    failures.Add(unit.Key);
                    Log("Plugin '" + unit.Key + "' failed to compile:" + Environment.NewLine + errors);
                }
            }

            if (failures.Count > 0)
                ReportLoadError("These plugins failed to compile and were skipped: " + string.Join(", ", failures.ToArray()) + ".");
        }

        private static bool TryCompile(string[] references, string[] sources, out Assembly assembly, out string errors)
        {
            assembly = null;
            errors = null;

            // http://ayende.com/blog/1376/solving-the-assembly-load-context-problem
            var compilerParams = new CompilerParameters();
            compilerParams.GenerateInMemory = true;
            compilerParams.GenerateExecutable = false;
            compilerParams.TreatWarningsAsErrors = false;
            compilerParams.CompilerOptions = "/optimize";
            compilerParams.ReferencedAssemblies.AddRange(references);

            try
            {
                var compile = new CSharpCodeProvider().CompileAssemblyFromFile(compilerParams, sources);

                if (compile.Errors.HasErrors)
                {
                    errors = compile.Errors.Cast<CompilerError>().Aggregate("", (current, ce) => current + (ce + Environment.NewLine));
                    return false;
                }

                assembly = compile.CompiledAssembly;
                return true;
            }
            catch (Exception ex)
            {
                errors = ex.ToString();
                return false;
            }
        }

        private static void Instantiate(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes().Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract))
            {
                try
                {
                    loadedPlugins.Add((IPlugin) Activator.CreateInstance(type));
                }
                catch (Exception ex)
                {
                    ReportLoadError("Plugin '" + type.Name + "' failed to initialise and was skipped." + Environment.NewLine + ex);
                }
            }

            dispatchCache.Clear();
        }

        private static void LoadHotkeyBinds()
        {
            foreach (var keys in IniAPI.GetIniKeys("HotkeyBinds").ToList())
            {
                var val = IniAPI.ReadIni("HotkeyBinds", keys, null);
                var key = ParseHotkey(keys);

                if (string.IsNullOrEmpty(val) || !val.StartsWith("/") || key == null)
                    MessageBox.Show("Invalid record in [HotkeyBinds]: " + keys + ".", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else
                    RegisterHotkey(val, key);
            }
        }

        private static void ExtractAndReference(List<string> references, string dllName, bool forceExtract = false)
        {
            if (!references.Any(s => s.Contains(dllName)))
            {
                // Dynamic compilation requires assemblies to be stored on file, thus we must extract the embedded
                // resource to a file if we want to use it.
                var assembly = Assembly.GetEntryAssembly();
                var error = "Could not extract " + dllName + " from Terraria.";
                var resourceName = assembly.GetManifestResourceNames().FirstOrDefault(s => s.Contains(dllName));
                if (resourceName == null) throw new Exception(error);

                Directory.CreateDirectory(DataFolder);
                var path = Path.Combine(DataFolder, dllName);
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

                DispatchAny<IPluginChatCommand>(plugin => plugin.OnChatCommand(cmd, args));
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

            Dispatch<IPluginInitialize>(plugin => plugin.OnInitialize());
        }

        public static void OnDrawSplash()
        {
            Dispatch<IPluginDrawSplash>(plugin => plugin.OnDrawSplash());
        }

        public static void OnDrawInventory()
        {
            Dispatch<IPluginDrawInventory>(plugin => plugin.OnDrawInventory());
        }

        public static void OnDrawInterface()
        {
            Dispatch<IPluginDrawInterface>(plugin => plugin.OnDrawInterface());
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

                foreach (var hotkey in hotkeys.ToArray())
                {
                    if (keysdown.Contains(hotkey.Key) &&
                        (hotkey.IgnoreModifierKeys || (control == hotkey.Control && shift == hotkey.Shift && alt == hotkey.Alt)))
                    {
                        anyPresses = true;
                        if (fresh)
                        {
                            try
                            {
                                hotkey.Action();
                            }
                            catch (Exception ex)
                            {
                                Log("Hotkey '" + hotkey + "' threw." + Environment.NewLine + ex);
                            }
                        }
                    }
                }

                fresh = !anyPresses;
            }

            Dispatch<IPluginPreUpdate>(plugin => plugin.OnPreUpdate());
        }

        public static void OnUpdate()
        {
            Dispatch<IPluginUpdate>(plugin => plugin.OnUpdate());
        }

        public static void OnUpdateTime()
        {
            Dispatch<IPluginUpdateTime>(plugin => plugin.OnUpdateTime());
        }

        public static bool OnCheckXmas()
        {
            return DispatchAny<IPluginCheckSeason>(plugin => plugin.OnCheckXmas());
        }

        public static bool OnCheckHalloween()
        {
            return DispatchAny<IPluginCheckSeason>(plugin => plugin.OnCheckHalloween());
        }

        public static bool OnPlaySound(int type, int x, int y, int style)
        {
            return DispatchAny<IPluginPlaySound>(plugin => plugin.OnPlaySound(type, x, y, style));
        }

        #endregion

        #region Player

        public static void OnPlayerPreSpawn(Player player)
        {
            Dispatch<IPluginPlayerPreSpawn>(plugin => plugin.OnPlayerPreSpawn(player));
        }

        public static void OnPlayerSpawn(Player player)
        {
            Dispatch<IPluginPlayerSpawn>(plugin => plugin.OnPlayerSpawn(player));
        }

        public static void OnPlayerLoad(PlayerFileData playerFileData, Player player, BinaryReader binaryReader)
        {
            Dispatch<IPluginPlayerLoad>(plugin => plugin.OnPlayerLoad(playerFileData, player, binaryReader));
        }

        public static void OnPlayerSave(PlayerFileData playerFileData, Player player, BinaryWriter binaryWriter)
        {
            Dispatch<IPluginPlayerSave>(plugin => plugin.OnPlayerSave(playerFileData, player, binaryWriter));
        }

        public static void OnPlayerUpdate(Player player)
        {
            Dispatch<IPluginPlayerUpdate>(plugin => plugin.OnPlayerUpdate(player));
        }

        public static void OnPlayerPreUpdate(Player player)
        {
            Dispatch<IPluginPlayerPreUpdate>(plugin => plugin.OnPlayerPreUpdate(player));
        }

        public static void OnPlayerUpdateBuffs(Player player)
        {
            Dispatch<IPluginPlayerUpdateBuffs>(plugin => plugin.OnPlayerUpdateBuffs(player));
        }

        public static void OnPlayerUpdateEquips(Player player)
        {
            Dispatch<IPluginPlayerUpdateEquips>(plugin => plugin.OnPlayerUpdateEquips(player));
        }

        public static void OnPlayerUpdateArmorSets(Player player)
        {
            Dispatch<IPluginPlayerUpdateArmorSets>(plugin => plugin.OnPlayerUpdateArmorSets(player));
        }

        public static bool OnPlayerHurt(Player player, PlayerDeathReason damageSource, int damage, int hitDirection, bool pvp, bool quiet, bool crit, int cooldownCounter, bool dodgeable, out double result)
        {
            var captured = 0.0;

            var ret = DispatchAny<IPluginPlayerHurt>(plugin =>
            {
                if (!plugin.OnPlayerHurt(player, damageSource, damage, hitDirection, pvp, quiet, crit, cooldownCounter, dodgeable, out var temp))
                    return false;

                captured = temp;
                return true;
            });

            result = captured;
            return ret;
        }

        public static bool OnPlayerKillMe(Player player, PlayerDeathReason damageSource, double dmg, int hitDirection, bool pvp)
        {
            return DispatchAny<IPluginPlayerKillMe>(plugin => plugin.OnPlayerKillMe(player, damageSource, dmg, hitDirection, pvp));
        }

        public static void OnPlayerPickAmmo(Player player, Item weapon, ref int shoot, ref float speed, ref bool canShoot, ref int damage, ref float knockback, ref int usedAmmoItemId, bool dontConsume)
        {
            // Written out rather than using Dispatch() because ref parameters cannot be captured by a lambda.
            foreach (var plugin in PluginsOf<IPluginPlayerPickAmmo>())
            {
                try
                {
                    plugin.OnPlayerPickAmmo(player, weapon, ref shoot, ref speed, ref canShoot, ref damage, ref knockback, ref usedAmmoItemId, dontConsume);
                }
                catch (Exception ex)
                {
                    Disable(plugin, ex);
                }
            }
        }

        public static bool OnPlayerGetItem(Player player, WorldItem newItem, GetItemSettings settings, out Item resultItem)
        {
            Item captured = null;

            var ret = DispatchAny<IPluginPlayerGetItem>(plugin =>
            {
                if (!plugin.OnPlayerGetItem(player, newItem, settings, out var temp))
                    return false;

                captured = temp;
                return true;
            });

            resultItem = captured;
            return ret;
        }

        public static bool OnPlayerQuickBuff(Player player)
        {
            return DispatchAny<IPluginPlayerQuickBuff>(plugin => plugin.OnPlayerQuickBuff(player));
        }

        #endregion

        #region Item

        public static void OnItemSetDefaults(Item item)
        {
            Load();

            Dispatch<IPluginItemSetDefaults>(plugin => plugin.OnItemSetDefaults(item));
        }

        public static bool OnItemSlotRightClick(Item[] inv, int context, int slot)
        {
            return DispatchAny<IPluginItemSlotRightClick>(plugin => plugin.OnItemSlotRightClick(inv, context, slot));
        }

        public static bool OnItemRollAPrefix(Item item, UnifiedRandom random, ref int rolledPrefix, out bool result)
        {
	        result = false;
	        var ret = false;

            // Written out rather than using DispatchAny() because ref parameters cannot be captured by a lambda.
	        foreach (var plugin in PluginsOf<IPluginItemRollAPrefix>())
	        {
                try
                {
			        if (plugin.OnItemRollAPrefix(item, random, ref rolledPrefix, out var temp))
			        {
                        ret = true;
                        result = temp;
			        }
                }
                catch (Exception ex)
                {
                    Disable(plugin, ex);
                }
	        }

	        return ret;
        }

        #endregion

        #region Projectile

        public static void OnProjectileAI001(Projectile projectile)
        {
            Dispatch<IPluginProjectileAI>(plugin => plugin.OnProjectileAI001(projectile));
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
                        chatRet = DispatchAny<IPluginChatCommand>(plugin => plugin.OnChatCommand(cmd, args));
                        break;
                }
            }

            return chatRet;
        }

        #endregion

        #region Lighting

        public static bool OnLightingGetColor(int x, int y, out Color color)
        {
            var captured = Color.White;

            var ret = DispatchAny<IPluginLightingGetColor>(plugin =>
            {
                if (!plugin.OnLightingGetColor(x, y, out var temp))
                    return false;

                captured = temp;
                return true;
            });

            color = captured;
            return ret;
        }

        #endregion

        #region Chest

        public static void OnChestSetupShop(Chest chest, int type)
        {
            Dispatch<IPluginChestSetupShop>(plugin => plugin.OnChestSetupShop(chest, type));
        }

        #endregion

        #region NPC

        public static bool OnNPCLoot(NPC npc)
        {
            return DispatchAny<IPluginNPCLoot>(plugin => plugin.OnNPCLoot(npc));
        }

        #endregion
    }
}
