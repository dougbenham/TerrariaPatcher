using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.CSharp;
using Microsoft.Xna.Framework;
using Terraria;
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
        private static bool loaded, ingame;

        #endregion
        
        #region Load

        private static void Load()
        {
            if (!loaded)
            {
                loaded = true;

                var references = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                    .Select(a => a.Location).ToList();
                var jsonBaseName = "Newtonsoft.Json.dll";
                if (!references.Any(s => s.Contains(jsonBaseName)))
                {
                    // Dynamic compilation requires assemblies to be stored on file, thus we must extract the Newtonsoft.Json.dll embedded resource to a temp file if we want to use it.
                    var assembly = Assembly.GetEntryAssembly();
                    var error = "Could not extract Newtonsoft.Json.dll from Terraria.";
                    var resourceName = assembly.GetManifestResourceNames().FirstOrDefault(s => s.Contains(jsonBaseName));
                    if (resourceName == null) throw new Exception(error);

                    var newtonsoftFileName = Path.Combine(".", jsonBaseName);
                    if (!File.Exists(newtonsoftFileName))
                    {
                        using (var stream = assembly.GetManifestResourceStream(resourceName))
                        {
                            if (stream == null) throw new Exception(error);

                            using (var fileStream = new FileStream(newtonsoftFileName, FileMode.Create))
                            {
                                stream.CopyTo(fileStream);
                            }
                        }
                    }

                    references.Add(newtonsoftFileName);
                }

                references.Add(Generate("Shared.dll", references.ToArray(), Directory.EnumerateFiles(@".\Plugins\Shared\", "*.cs", SearchOption.AllDirectories).ToArray()));
                var referencesArray = references.ToArray();

                foreach (var filename in Directory.EnumerateFiles(@".\Plugins\", "*.cs"))
                    Load(Path.GetFileNameWithoutExtension(filename), referencesArray, filename);

                foreach (var folder in Directory.EnumerateDirectories(@".\Plugins\"))
                {
                    var name = Path.GetFileName(folder);
                    if (name == "Shared") continue;

                    Load(name, referencesArray, Directory.EnumerateFiles(folder, "*.cs", SearchOption.AllDirectories).ToArray());
                }

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
        }

        private static void Load(string name, string[] references, params string[] sources)
        {
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
                var text = "Compile error for plugin " + name + ": ";
                foreach (CompilerError ce in compile.Errors)
                {
                    text += ce + Environment.NewLine;
                }
                throw new Exception(text);
            }

            foreach (var type in compile.CompiledAssembly.GetTypes().Where(type1 => type1.GetInterfaces().Contains(typeof (IPlugin))))
            {
                loadedPlugins.Add(Activator.CreateInstance(type) as IPlugin);
            }
        }

        private static string Generate(string target, string[] references, params string[] sources)
        {
            var compilerParams = new CompilerParameters();
            compilerParams.OutputAssembly = target;
            compilerParams.GenerateInMemory = false;
            compilerParams.GenerateExecutable = false;
            compilerParams.TreatWarningsAsErrors = false;
            compilerParams.CompilerOptions = "/optimize";

            compilerParams.ReferencedAssemblies.AddRange(references);

            var provider = new CSharpCodeProvider();
            var compile = provider.CompileAssemblyFromFile(compilerParams, sources);

            if (compile.Errors.HasErrors)
            {
                var text = "Compile error for shared source: ";
                foreach (CompilerError ce in compile.Errors)
                {
                    text += ce + Environment.NewLine;
                }
                throw new Exception(text);
            }

            return compile.CompiledAssembly.Location;
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

        public static IReadOnlyCollection<Hotkey> GetHotkeys()
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

        public static void OnDrawInventory()
        {
            foreach (var plugin in loadedPlugins.OfType<IPluginDrawInventory>())
                plugin.OnDrawInventory();
        }

        public static void OnUpdate()
        {
            if (!ingame)
            {
                ingame = true;
                Main.NewText("Loaded " + loadedPlugins.Count + " plugins", Color.Purple.R, Color.Purple.G, Color.Purple.B, false);
            }

            if (!Main.blockInput && !Main.chatMode && !Main.editSign && !Main.editChest)
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

            foreach (var plugin in loadedPlugins.OfType<IPluginUpdate>())
                plugin.OnUpdate();
        }

        public static void OnUpdateTime()
        {
            foreach (var plugin in loadedPlugins.OfType<IPluginUpdateTime>())
                plugin.OnUpdateTime();
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

        public static bool OnPlayerHurt(Player player, int damage, int hitDirection, bool pvp, bool quiet, string deathText, bool crit, out double result)
        {
            result = 0.0;
            var ret = false;
            foreach (var plugin in loadedPlugins.OfType<IPluginPlayerHurt>())
            {
                double temp;
                if (plugin.OnPlayerHurt(player, damage, hitDirection, pvp, quiet, deathText, crit, out temp))
                {
                    ret = true;
                    result = temp;
                }
            }

            return ret;
        }

        public static bool OnPlayerKillMe(Player player, double dmg, int hitDirection, bool pvp, string deathText)
        {
            var ret = false;
            foreach (var plugin in loadedPlugins.OfType<IPluginPlayerKillMe>())
                ret = plugin.OnPlayerKillMe(player, dmg, hitDirection, pvp, deathText) || ret;

            return ret;
        }

        public static void OnPlayerPickAmmo(Player player, Item weapon, ref int shoot, ref float speed, ref bool canShoot, ref int damage, ref float knockback)
        {
            foreach (var plugin in loadedPlugins.OfType<IPluginPlayerPickAmmo>())
                plugin.OnPlayerPickAmmo(player, weapon, ref shoot, ref speed, ref canShoot, ref damage, ref knockback);
        }

        public static bool OnPlayerGetItem(Player player, Item newItem, out Item resultItem)
        {
            resultItem = null;
            var ret = false;
            foreach (var plugin in loadedPlugins.OfType<IPluginPlayerGetItem>())
            {
                Item temp;
                if (plugin.OnPlayerGetItem(player, newItem, out temp))
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

        #endregion

        #region Projectile

        public static void OnProjectileAI001(Projectile projectile)
        {
            foreach (var plugin in loadedPlugins.OfType<IPluginProjectileAI>())
                plugin.OnProjectileAI001(projectile);
        }

        #endregion

        #region NetMessage

        public static bool OnNetMessageSendData(int msgType, int remoteClient, int ignoreClient, string text, int number, float number2, float number3, float number4,
            int number5, int number6, int number7)
        {
            bool ret = false, chatRet = false;

            foreach (var plugin in loadedPlugins.OfType<IPluginNetMessageSendData>())
                ret = ret || plugin.OnNetMessageSendData(msgType, remoteClient, ignoreClient, text, number, number2, number3, number4, number5, number6, number7);

            if (msgType == 25 && number == Main.myPlayer && !string.IsNullOrEmpty(text) && text[0] == '/')
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
                if (chatRet) Main.chatText = string.Empty;
            }

            return ret || chatRet;
        }

        #endregion

        #region Lighting

        public static bool OnLightingGetColor(int x, int y, out Color color)
        {
            color = Color.White;
            var ret = false;
            foreach (var plugin in loadedPlugins.OfType<IPluginLightingGetColor>())
            {
                Color temp;
                var result = plugin.OnLightingGetColor(x, y, out temp);
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
