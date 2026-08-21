using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework.Input;
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

        private readonly Setting<bool> enabled;

        private readonly HotkeySetting toggleKey;

        public string Name => GetType().Name;

        public string Description => GetDescription(GetType());

        public IList<Setting> Settings => settings.AsReadOnly();

        /// <summary>
        /// Whether the plugin receives any of the hooks it implements. Switching it off leaves the plugin loaded,
        /// so anything it did once, such as changing an item's stats, stays done until the game is restarted.
        /// </summary>
        public bool Enabled
        {
            get { return enabled.Value; }
            set { enabled.Value = value; }
        }

        /// <summary>
        /// Raised whenever <see cref="Enabled"/> changes, however it was changed: from the settings window, from a
        /// chat command, or by Plugins.ini being edited while the game is running. A plugin that has changed
        /// something the game will go on using, such as a recipe, handles this to put it back when it is switched
        /// off and to apply it again when it is switched back on.
        /// </summary>
        /// <remarks>
        /// Raised after the loader has stopped delivering hooks to a plugin being switched off, so a handler is
        /// the last thing the plugin is asked to do and can count on no further hook arriving to undo its work.
        /// </remarks>
        public event Action EnabledChanged;

        /// <summary>
        /// The setting behind <see cref="Enabled"/>, for a settings menu that wants to show it apart from the rest.
        /// </summary>
        public Setting EnabledSetting => enabled;

        /// <summary>
        /// The hotkey that switches the plugin on and off, for a plugin that asked for one, and null for one that
        /// did not. A plugin can ask for one and leave it unbound, so that the player has a ToggleKey to bind in
        /// Plugins.ini or the settings window rather than a plugin that is only switched from the menu.
        /// </summary>
        public HotkeySetting ToggleKeySetting => toggleKey;

        /// <summary>
        /// Every setting except <see cref="EnabledSetting"/>.
        /// </summary>
        public IEnumerable<Setting> ConfigurableSettings
        {
            get { return settings.Where(setting => !ReferenceEquals(setting, enabled)); }
        }

        /// <summary>
        /// Whether the plugin does permanent changes to the game that won't be removed if the plugin is disabled.
        /// </summary>
        public virtual bool RequiresRestart => this is IPluginInitialize ||
                                               this is IPluginItemSetDefaults;

        /// <summary>
        /// Whether the plugin's hotkeys and chat commands still reach it while it is switched off. A plugin whose
        /// own hotkey or command is how it gets switched back on has to say so, or there would be no way back.
        /// A plugin whose toggle hotkey is bound to a key gets this for free; one whose toggle hotkey is left
        /// unbound does not, because there is no key to switch it back on with until the player binds one.
        /// </summary>
        public virtual bool RespondsWhileDisabled => toggleKey != null && toggleKey.Value.Key != Keys.None;

        protected static Player Player => Main.LocalPlayer;

        /// <param name="toggleKey">
        /// The key that switches the plugin on and off, or <see cref="Keys.None"/> for a ToggleKey the player can
        /// bind but that starts out unbound.
        /// </param>
        protected PluginBase(Keys toggleKey)
            : this(true, new Hotkey { Key = toggleKey })
        { }

        /// <param name="enabledByDefault">
        /// Whether the plugin starts switched on, for one that does nothing until the player asks for it.
        /// </param>
        /// <param name="toggleKey">
        /// The key that switches the plugin on and off, or <see cref="Keys.None"/> for a ToggleKey the player can
        /// bind but that starts out unbound.
        /// </param>
        protected PluginBase(bool enabledByDefault, Keys toggleKey)
            : this(enabledByDefault, new Hotkey { Key = toggleKey })
        { }

        /// <param name="enabledByDefault">
        /// Whether the plugin starts switched on, for one that does nothing until the player asks for it.
        /// </param>
        /// <param name="toggleKey">
        /// The binding that switches the plugin on and off, registered under the name ToggleKey so that the player
        /// can rebind it, or null for a plugin that is only switched from the settings window. A binding whose key
        /// is <see cref="Keys.None"/> still registers the setting, giving the player something to bind.
        /// </param>
        protected PluginBase(bool enabledByDefault = true, Hotkey toggleKey = null)
        {
            // Registered first so that it is the first setting in the plugin's section of Plugins.ini, and so that
            // a plugin cannot declare a second setting under the same name.
            enabled = AddSetting("Enabled", new Setting<bool>(enabledByDefault));
            enabled.Description = "Whether " + Name + " does anything at all.";
            enabled.Changed += RaiseEnabledChanged;

            if (toggleKey != null)
            {
                toggleKey.Action = Toggle;
                this.toggleKey = AddSetting("ToggleKey", new HotkeySetting(toggleKey));
                this.toggleKey.Description = "Switches " + Name + " on and off.";
            }

            foreach (var field in GetType()
                .GetFields(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(field => typeof(Setting).IsAssignableFrom(field.FieldType)))
            {
                var setting = (Setting) field.GetValue(this);

                if (setting == null)
                    throw new InvalidOperationException(Name + "." + field.Name + " has no value.");

                Describe(setting, field);

                AddSetting(field.Name, setting);
            }
        }

        protected virtual void Toggle()
        {
            Enabled = !Enabled;

            Main.NewText(Name + " " + (Enabled ? "enabled" : "disabled"), 150, 150, 150);
        }

        private void RaiseEnabledChanged()
        {
            Loader.PluginEnabledChanged();

            var handler = EnabledChanged;
            if (handler == null) return;

            try
            {
                handler();
            }
            catch (Exception ex)
            {
                Loader.Report(Name + " threw on being switched " + (Enabled ? "on" : "off") + ": " + ex.Message);
            }
        }

        /// <summary>
        /// Reads the attributes a setting's field carries, so that the settings menu has something better to show
        /// than the field name on its own.
        /// </summary>
        private static void Describe(Setting setting, FieldInfo field)
        {
            var label = (SettingLabelAttribute) field
                .GetCustomAttributes(typeof(SettingLabelAttribute), false).FirstOrDefault();
            var description = (SettingDescriptionAttribute) field
                .GetCustomAttributes(typeof(SettingDescriptionAttribute), false).FirstOrDefault();
            var range = (SettingRangeAttribute) field
                .GetCustomAttributes(typeof(SettingRangeAttribute), false).FirstOrDefault();
            var ids = (SettingIdsAttribute) field
                .GetCustomAttributes(typeof(SettingIdsAttribute), false).FirstOrDefault();

            setting.Label = label != null ? label.Label : Prettify(field.Name);
            if (description != null) setting.Description = description.Description;
            if (range != null)
            {
                setting.Minimum = range.Minimum;
                setting.Maximum = range.Maximum;
            }
            if (ids != null) setting.IdClass = ids.IdClass;
        }

        /// <summary>
        /// Splits a field name on its capitals, so that <c>MaxBlocksPerVein</c> reads as "Max Blocks Per Vein".
        /// Runs of capitals are left alone, so <c>UseNPCNames</c> keeps its NPC.
        /// </summary>
        public static string Prettify(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;

            var text = new StringBuilder(name.Length + 8);

            for (var i = 0; i < name.Length; i++)
            {
                var character = name[i];

                if (i > 0 && char.IsUpper(character) &&
                    (!char.IsUpper(name[i - 1]) || (i + 1 < name.Length && char.IsLower(name[i + 1]))))
                    text.Append(' ');

                text.Append(character);
            }

            return text.ToString();
        }

        /// <summary>
        /// Registers a setting that cannot be declared as a field, such as one of a numbered series.
        /// </summary>
        protected TSetting AddSetting<TSetting>(string name, TSetting setting) where TSetting : Setting
        {
            if (settings.Any(existing => existing.Name == name))
                throw new InvalidOperationException(Name + " already has a setting named " + name + "." +
                    (name == "Enabled"
                        ? " Every plugin is given an Enabled setting of its own, so declare this one under another" +
                          " name and, if it is what switches the plugin on and off, use the given one instead."
                        : ""));

            setting.Section = Name;
            setting.Name = name;
            if (setting.Label == null) setting.Label = Prettify(name);

            var hotkey = setting as HotkeySetting;
            if (hotkey != null) hotkey.Value.Owner = this;

            setting.Register();

            settings.Add(setting);

            return setting;
        }

        public Setting GetSetting(string name)
        {
            return settings.FirstOrDefault(setting => string.Equals(setting.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public void ResetSettings()
        {
            foreach (var setting in settings)
                setting.Reset();
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
