using System;
using Microsoft.Xna.Framework.Input;

namespace PluginLoader
{
    public class Hotkey
    {
        public bool Control { get; set; }
        public bool Shift { get; set; }
        public bool Alt { get; set; }
        public bool IgnoreModifierKeys { get; set; }
        public Keys Key { get; set; }
        public Action Action { get; set; }
    }
}
