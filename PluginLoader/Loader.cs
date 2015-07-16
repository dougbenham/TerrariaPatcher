﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CSharp;
using Microsoft.Xna.Framework;
using Terraria;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace PluginLoader
{
    public static class Loader
    {
        private static List<Hotkey> hotkeys = new List<Hotkey>();
        private static bool fresh = true;

        private static List<IPlugin> loadedPlugins = new List<IPlugin>(); 
        private static bool loaded, ingame;

        private static void Load()
        {
            if (!loaded)
            {
                loaded = true;

                var references = AppDomain.CurrentDomain
                        .GetAssemblies()
                        .Where(a => !a.IsDynamic)
                        .Select(a => a.Location).ToArray();

                foreach (var filename in Directory.EnumerateFiles(@".\Plugins\", "*.cs"))
                {
                    var source = File.ReadAllText(filename);

                    var compilerParams = new CompilerParameters();
                    compilerParams.GenerateInMemory = true;
                    compilerParams.GenerateExecutable = false;
                    compilerParams.TreatWarningsAsErrors = false;
                    compilerParams.CompilerOptions = "/optimize";

                    compilerParams.ReferencedAssemblies.AddRange(references);

                    var provider = new CSharpCodeProvider();
                    CompilerResults compile = provider.CompileAssemblyFromSource(compilerParams, source);

                    if (compile.Errors.HasErrors)
                    {
                        string text = "Compile error for plugin " + Path.GetFileNameWithoutExtension(filename) + ": ";
                        foreach (CompilerError ce in compile.Errors)
                        {
                            text += ce + Environment.NewLine;
                        }
                        throw new Exception(text);
                    }

                    foreach (var type in compile.CompiledAssembly.GetTypes().Where(type1 => type1.GetInterfaces().Contains(typeof(IPlugin))))
                    {
                        loadedPlugins.Add(Activator.CreateInstance(type) as IPlugin);
                    }
                }
            }
        }

        public static void RegisterHotkey(Action action, params Keys[] keys)
        {
            RegisterHotkey(new Hotkey() {Keys = keys, Action = action});
        }

        public static void RegisterHotkey(Hotkey hotkey)
        {
            if (hotkey.Keys.Length == 0) return;
            hotkeys.Add(hotkey);
        }

        public static bool IsAltModifierKeyDown()
        {
            return Main.keyState.IsKeyDown(Keys.LeftAlt) || Main.keyState.IsKeyDown(Keys.RightAlt);
        }

        public static bool IsControlModifierKeyDown()
        {
            return Main.keyState.IsKeyDown(Keys.LeftControl) || Main.keyState.IsKeyDown(Keys.RightControl);
        }

        public static bool IsShiftModifierKeyDown()
        {
            return Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift);
        }

        public static void OnUpdate()
        {
            Load();

            if (!ingame)
            {
                ingame = true;
                Main.NewText("Loaded " + loadedPlugins.Count + " plugins", Color.Purple.R, Color.Purple.G, Color.Purple.B, false);
            }

            if (!Main.blockInput && !Main.chatMode && !Main.editSign && !Main.editChest)
            {
                var keysdown = Main.keyState.GetPressedKeys();
                bool anyPresses = false;
                foreach (var hotkey in hotkeys)
                {
                    if (hotkey.Keys.All(keysdown.Contains))
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

        public static void OnPlayerUpdate(Player player)
        {
            Load();

            foreach (var plugin in loadedPlugins.OfType<IPluginPlayerUpdate>())
                plugin.OnPlayerUpdate(player);
        }

        public static void OnItemSetDefaults(Item item)
        {
            Load();

            foreach (var plugin in loadedPlugins.OfType<IPluginItemSetDefaults>())
                plugin.OnItemSetDefaults(item);
        }

        public static void OnPlayerUpdateBuffs(Player player)
        {
            Load();

            foreach (var plugin in loadedPlugins.OfType<IPluginPlayerUpdateBuffs>())
                plugin.OnPlayerUpdateBuffs(player);
        }

        public static void OnPlayerPickAmmo(Player player, Item weapon, ref int shoot, ref float speed, ref bool canShoot, ref int damage, ref float knockback)
        {
            Load();

            foreach (var plugin in loadedPlugins.OfType<IPluginPlayerPickAmmo>())
                plugin.OnPlayerPickAmmo(player, weapon, ref shoot, ref speed, ref canShoot, ref damage, ref knockback);
        }

        public static void OnProjectileAI001(Projectile projectile)
        {
            Load();

            foreach (var plugin in loadedPlugins.OfType<IPluginProjectileAI>())
                plugin.OnProjectileAI001(projectile);
        }

        public static bool OnItemSlotRightClick(Item[] inv, int context, int slot)
        {
            Load();

            bool ret = false;
            foreach (var plugin in loadedPlugins.OfType<IPluginItemSlotRightClick>())
                ret = plugin.OnItemSlotRightClick(inv, context, slot) || ret;

            return ret;
        }

        public static bool OnNetMessageSendData(int msgType, int remoteClient, int ignoreClient, string text, int number, float number2, float number3, float number4,
            int number5, int number6, int number7)
        {
            Load();

            bool ret = false, chatRet = false;

            foreach (var plugin in loadedPlugins.OfType<IPluginNetMessageSendData>())
                ret = ret || plugin.OnNetMessageSendData(msgType, remoteClient, ignoreClient, text, number, number2, number3, number4, number5, number6, number7);

            if (msgType == 25 && number == Main.myPlayer && !string.IsNullOrEmpty(text) && text[0] == '/')
            {
                var split = text.Substring(1).Split(new[] {' '}, 2);
                var cmd = split[0].ToLower();
                string[] args = split.Length > 1 ? split[1].Split(' ') : new string[0];

                foreach (var plugin in loadedPlugins.OfType<IPluginChatCommand>())
                    chatRet = plugin.OnChatCommand(cmd, args) || chatRet;

                if (chatRet) Main.chatText = string.Empty;
            }

            return ret || chatRet;
        }
    }
}