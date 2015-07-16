using System;
using PluginLoader;
using Terraria;

namespace TranscendPlugins
{
    public class SkipSplash : MarshalByRefObject, IPlugin
    {
        public SkipSplash()
        {
            Main.showSplash = false;
        }
    }
}
