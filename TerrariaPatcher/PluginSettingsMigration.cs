extern alias PluginLoaderXNA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PluginLoaderXNA::PluginLoader;

namespace TerrariaPatcher
{
    /// <summary>
    /// Moves settings in an existing Plugins.ini to the section and key names the plugins read today. Settings now
    /// always live under a section named after the plugin, with the setting's own name as the key.
    /// </summary>
    internal static class PluginSettingsMigration
    {
        /// <summary>
        /// Sections that were renamed wholesale, keeping their keys.
        /// </summary>
        private static readonly Dictionary<string, string> Sections = new Dictionary<string, string>
        {
            { "Buffs", "BuffRates" },
            { "CoinGunModifications", "CoinGun" }
        };

        /// <summary>
        /// Sections belonging to plugins that no longer exist, which are dropped rather than carried over.
        /// </summary>
        private static readonly string[] Retired =
        {
            "MoreAccessorySlots"
        };

        /// <summary>
        /// Individual settings that moved section, changed name, or both.
        /// </summary>
        private static readonly string[][] Keys =
        {
            new[] { "Spawning", "SpawnLimit", "NPC", "SpawnLimit" },
            new[] { "Spawning", "SpawnRate", "NPC", "SpawnRate" },
            new[] { "FullBright", "FullBrightKey", "FullBright", "ToggleKey" },
            new[] { "FullBright", "FullBrightDefault", "FullBright", "Enabled" },
            new[] { "InfiniteFlight", "FlightKey", "InfiniteFlight", "ToggleKey" },
            new[] { "Events", "Moon Lord", "Events", "MoonLord" },

            // Minions and Turrets now add to what your gear gives you rather than replacing it. There is no value
            // that means the same under both, so the old number is carried over as the bonus: someone who had raised
            // the cap keeps at least as many as before rather than silently dropping to the new default.
            new[] { "Minions", "Max", "Minions", "Bonus" },
            new[] { "Turrets", "Max", "Turrets", "Bonus" }
        };

        /// <summary>
        /// Renames what can be renamed and returns how many settings were moved.
        /// </summary>
        public static int Migrate(string iniPath)
        {
            if (!File.Exists(iniPath)) return 0;

            var moved = Sections.Sum(section => MoveSection(section.Key, section.Value, iniPath));

            moved += Retired.Sum(section => RemoveSection(section, iniPath));

            moved += Keys.Sum(key => MoveKey(key[0], key[1], key[2], key[3], iniPath));

            moved += SplitHotkey("GodMode", "Key", "NextMode", "PreviousMode", "Shift,", iniPath);
            moved += DeriveHotkey("NPC", "Increase", "IncreaseLimit", "Control,", iniPath);
            moved += DeriveHotkey("NPC", "Decrease", "DecreaseLimit", "Control,", iniPath);

            return moved;
        }

        private static int MoveSection(string oldSection, string newSection, string iniPath)
        {
            var moved = IniAPI.GetIniKeys(oldSection, iniPath)
                .ToArray()
                .Sum(key => MoveKey(oldSection, key, newSection, key, iniPath));

            DeleteEmptySection(oldSection, iniPath);

            return moved;
        }

        /// <summary>
        /// Drops a section and everything in it, and returns how many settings went with it.
        /// </summary>
        private static int RemoveSection(string section, string iniPath)
        {
            var count = IniAPI.GetIniKeys(section, iniPath).Count();

            // A null key name tells WritePrivateProfileString to take the whole section, entries included.
            IniAPI.WriteIni(section, null, null, iniPath);

            return count;
        }

        private static int MoveKey(string oldSection, string oldKey, string newSection, string newKey, string iniPath)
        {
            var value = Read(oldSection, oldKey, iniPath);
            if (value == null) return 0;

            if (Read(newSection, newKey, iniPath) == null)
                IniAPI.WriteIni(newSection, newKey, value, iniPath);

            IniAPI.WriteIni(oldSection, oldKey, null, iniPath);

            DeleteEmptySection(oldSection, iniPath);

            return 1;
        }

        /// <summary>
        /// Replaces a single binding with the pair of bindings that took its place, the second one gaining a modifier.
        /// </summary>
        private static int SplitHotkey(string section, string oldKey, string newKey, string modifiedKey, string modifier, string iniPath)
        {
            var value = Read(section, oldKey, iniPath);
            if (value == null) return 0;

            if (Read(section, newKey, iniPath) == null)
                IniAPI.WriteIni(section, newKey, value, iniPath);

            if (Read(section, modifiedKey, iniPath) == null)
                IniAPI.WriteIni(section, modifiedKey, modifier + value, iniPath);

            IniAPI.WriteIni(section, oldKey, null, iniPath);

            return 1;
        }

        /// <summary>
        /// Adds a binding that was split out of an existing one, which keeps its own name.
        /// </summary>
        private static int DeriveHotkey(string section, string key, string modifiedKey, string modifier, string iniPath)
        {
            var value = Read(section, key, iniPath);
            if (value == null || Read(section, modifiedKey, iniPath) != null) return 0;

            IniAPI.WriteIni(section, modifiedKey, modifier + value, iniPath);

            return 1;
        }

        private static void DeleteEmptySection(string section, string iniPath)
        {
            if (IniAPI.GetIniKeys(section, iniPath).Any()) return;

            IniAPI.WriteIni(section, null, null, iniPath);
        }

        /// <summary>
        /// The setting's value, or null if the file has no entry for it.
        /// </summary>
        private static string Read(string section, string key, string iniPath)
        {
            const string missing = "\u0001";

            var value = IniAPI.ReadIni(section, key, missing, 2048, iniPath);

            return value == missing ? null : value;
        }
    }
}
