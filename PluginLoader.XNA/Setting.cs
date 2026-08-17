using System;
using Microsoft.Xna.Framework.Input;

namespace PluginLoader
{
    /// <summary>
    /// A plugin value persisted to Plugins.ini under the plugin's own section. Edits made to the file while the
    /// game is running are picked up automatically.
    /// </summary>
    public abstract class Setting
    {
        /// <summary>
        /// Big enough to hold a setting such as a list of every buff id.
        /// </summary>
        private const int MaxLength = 8192;

        /// <summary>
        /// A value no setting can produce, used to detect an entry that Plugins.ini does not have at all.
        /// </summary>
        private const string Missing = "\u0001";

        protected object value;

        public string Section { get; internal set; }
        public string Name { get; internal set; }

        public string FullName => Section + "." + Name;

        public abstract Type ValueType { get; }

        public abstract string Serialize();
        public abstract void Deserialize(string text);

        /// <summary>
        /// Raised whenever the value changes, including when Plugins.ini is edited while the game is running.
        /// </summary>
        public event Action Changed;

        public override string ToString()
        {
            return Serialize();
        }

        internal virtual void Register()
        {
            Load();
        }

        /// <summary>
        /// Reads the value from Plugins.ini, writing the current value as the default if the file has no entry.
        /// Returns true if the value changed.
        /// </summary>
        public bool Load()
        {
            var before = Serialize();

            try
            {
                // Read with a sentinel default so that a value of "", such as an emptied list, is told apart from
                // the entry being missing altogether.
                var text = IniAPI.ReadIni(Section, Name, Missing, MaxLength);

                if (text == Missing)
                {
                    IniAPI.WriteIni(Section, Name, before);
                    return false;
                }

                if (text == before) return false;

                Deserialize(text);
            }
            catch (Exception ex)
            {
                Loader.Report("Could not read " + FullName + " from Plugins.ini: " + ex.Message);
                Deserialize(before);
                return false;
            }

            if (Serialize() == before) return false;

            RaiseChanged();
            return true;
        }

        /// <summary>
        /// Writes the value to Plugins.ini.
        /// </summary>
        public void Save()
        {
            try
            {
                IniAPI.WriteIni(Section, Name, Serialize());
            }
            catch (Exception ex)
            {
                Loader.Report("Could not save " + FullName + " to Plugins.ini: " + ex.Message);
            }
        }

        /// <summary>
        /// Parses and stores a new value, then writes it to Plugins.ini.
        /// </summary>
        public void SetFrom(string text)
        {
            var before = Serialize();

            Deserialize(text);

            if (Serialize() == before) return;

            Save();
            RaiseChanged();
        }

        protected void RaiseChanged()
        {
            var handler = Changed;
            if (handler == null) return;

            try
            {
                handler();
            }
            catch (Exception ex)
            {
                Loader.Report("Handler for " + FullName + " threw: " + ex.Message);
            }
        }
    }

    public class Setting<T> : Setting
    {
        public Setting()
        { }

        public Setting(T value)
        {
            this.value = value;
        }

        public T Value
        {
            get => (T) value;
            set
            {
                if (Equals(this.value, value)) return;

                this.value = value;
                Save();
                RaiseChanged();
            }
        }

        public override Type ValueType => typeof(T);

        public override string Serialize()
        {
            return SettingConverter.Serialize(value, typeof(T));
        }

        public override void Deserialize(string text)
        {
            value = SettingConverter.Deserialize(text, typeof(T));
        }

        public static implicit operator T(Setting<T> setting)
        {
            return setting.Value;
        }

        public static implicit operator Setting<T>(T value)
        {
            return new Setting<T> { value = value };
        }
    }

    /// <summary>
    /// A key binding stored in Plugins.ini, in the form <c>Control,Shift,K</c>.
    /// </summary>
    public class HotkeySetting : Setting<Hotkey>
    {
        public HotkeySetting()
            : this(new Hotkey())
        { }

        public HotkeySetting(Hotkey hotkey)
            : base(hotkey)
        { }

        /// <summary>
        /// Rebinds the registered hotkey in place, so that the binding can change without re-registering it.
        /// </summary>
        public override void Deserialize(string text)
        {
            var parsed = Loader.ParseHotkey(text) ?? new Hotkey();
            var hotkey = Value;

            hotkey.Key = parsed.Key;
            hotkey.Control = parsed.Control;
            hotkey.Shift = parsed.Shift;
            hotkey.Alt = parsed.Alt;
        }

        internal override void Register()
        {
            Value.Name = FullName;

            base.Register();

            Loader.RegisterHotkey(Value);
        }

        public static implicit operator HotkeySetting(Keys key)
        {
            return new HotkeySetting { value = new Hotkey { Key = key } };
        }

        public static implicit operator HotkeySetting(Hotkey hotkey)
        {
            return new HotkeySetting { value = hotkey };
        }
    }
}
