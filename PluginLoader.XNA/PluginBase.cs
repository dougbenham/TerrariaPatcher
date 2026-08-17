using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;

namespace PluginLoader
{
    /// <summary>
    /// Optional base class that gives a plugin settings stored in Plugins.ini, hotkeys bound through those
    /// settings, and a description. Every <see cref="Setting"/> field declared by the plugin is registered
    /// automatically under the field's own name.
    /// </summary>
    public abstract class PluginBase : MarshalByRefObject, IPlugin
    {
        private readonly List<Setting> settings = new List<Setting>();

        public string Name => GetType().Name;

        public string Description => GetDescription(GetType());

        public IList<Setting> Settings => settings.AsReadOnly();

        protected static Player Player => Main.LocalPlayer;

        protected PluginBase()
        {
            foreach (var field in GetType()
                .GetFields(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(field => typeof(Setting).IsAssignableFrom(field.FieldType)))
            {
                var setting = (Setting) field.GetValue(this);

                if (setting == null)
                    throw new InvalidOperationException(Name + "." + field.Name + " has no value.");

                AddSetting(field.Name, setting);
            }
        }

        /// <summary>
        /// Registers a setting that cannot be declared as a field, such as one of a numbered series.
        /// </summary>
        protected TSetting AddSetting<TSetting>(string name, TSetting setting) where TSetting : Setting
        {
            if (settings.Any(existing => existing.Name == name))
                throw new InvalidOperationException(Name + " already has a setting named " + name + ".");

            setting.Section = Name;
            setting.Name = name;
            setting.Register();

            settings.Add(setting);

            return setting;
        }

        public Setting GetSetting(string name)
        {
            return settings.FirstOrDefault(setting => string.Equals(setting.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        internal IEnumerable<Hotkey> Hotkeys()
        {
            return settings.OfType<HotkeySetting>().Select(setting => setting.Value);
        }

        public static string GetDescription(Type type)
        {
            var attribute = (PluginDescriptionAttribute) type
                .GetCustomAttributes(typeof(PluginDescriptionAttribute), false)
                .FirstOrDefault();

            return attribute?.Description;
        }
    }
}
