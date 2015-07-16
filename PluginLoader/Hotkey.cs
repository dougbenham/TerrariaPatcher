using System;
using Microsoft.Xna.Framework.Input;

namespace PluginLoader
{
    public class Hotkey
    {
        public Keys[] Keys { get; set; }
        public Action Action { get; set; }
    }
}
