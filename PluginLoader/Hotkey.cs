using System;
using Microsoft.Xna.Framework.Input;

namespace PluginLoader
{
    public class Hotkey : IEquatable<Hotkey>
    {
        public bool Control { get; set; }
        public bool Shift { get; set; }
        public bool Alt { get; set; }
        public bool IgnoreModifierKeys { get; set; }

        private Keys _key;
        public Keys Key
        {
            get { return _key; }
            set
            {
                _key = value;
                if (_key == Keys.LeftControl || _key == Keys.RightControl ||
                    _key == Keys.LeftAlt || _key == Keys.RightAlt ||
                    _key == Keys.LeftShift || _key == Keys.RightShift)
                    IgnoreModifierKeys = true;
            }
        }

        public Action Action { get; set; }

        /// <summary>
        /// If non-null, it stores the chat command associated with this hotkey.
        /// </summary>
        public string Tag { get; set; }

        public override string ToString()
        {
            return (Control ? "Control," : "") + (Shift ? "Shift," : "") + (Alt ? "Alt," : "") + Key + " " + Tag;
        }

        public bool Equals(Hotkey other)
        {
            if (other == null) return false;

            return this.Key == other.Key &&
                   this.Control == other.Control &&
                   this.Shift == other.Shift &&
                   this.Alt == other.Alt &&
                   this.IgnoreModifierKeys == other.IgnoreModifierKeys;
        }
    }
}
