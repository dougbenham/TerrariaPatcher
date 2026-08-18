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
            set => _ignoreModifierKeys = value;
        }

        public Keys Key { get; set; }

        public Action Action { get; set; }

        /// <summary>
        /// If non-null, it stores the chat command associated with this hotkey.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// If non-null, the Plugin.Setting this hotkey is bound to.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// If non-null, the plugin whose setting this hotkey is bound to. A hotkey belonging to a plugin that is
        /// switched off does nothing, unless the plugin responds while disabled so that the hotkey can switch it
        /// back on.
        /// </summary>
        public PluginBase Owner { get; internal set; }

        internal bool IsActive
        {
            get { return Owner == null || Owner.Enabled || Owner.RespondsWhileDisabled; }
        }

        /// <summary>
        /// True while the hotkey is held down, so that holding one hotkey does not suppress the others.
        /// </summary>
        internal bool Held { get; set; }

        /// <summary>
        /// The key combination on its own, in the form written to Plugins.ini.
        /// </summary>
        public string ToBinding()
        {
            return (Control ? "Control," : "") + (Shift ? "Shift," : "") + (Alt ? "Alt," : "") + Key;
        }

        public override string ToString()
        {
            var label = Tag ?? Name;
            return label == null ? ToBinding() : ToBinding() + " " + label;
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
