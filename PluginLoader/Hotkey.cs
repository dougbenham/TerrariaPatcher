using System;
using Microsoft.Xna.Framework.Input;

namespace PluginLoader
{
    public class Hotkey : IEquatable<Hotkey>
    {
        public bool Control { get; set; }
        public bool Shift { get; set; }
        public bool Alt { get; set; }

        private bool _ignoreModifierKeys;
        public bool IgnoreModifierKeys
        {
            get
            {
                if (Key == Keys.LeftControl || Key == Keys.RightControl ||
                    Key == Keys.LeftAlt || Key == Keys.RightAlt ||
                    Key == Keys.LeftShift || Key == Keys.RightShift)
                    return true;
                return _ignoreModifierKeys;
            }
            set { _ignoreModifierKeys = value; }
        }

        public Keys Key { get; set; }

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
