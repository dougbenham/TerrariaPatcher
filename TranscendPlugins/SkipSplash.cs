using System;
using PluginLoader;
using Terraria;
using Utils = PluginLoader.Utils;

namespace TranscendPlugins
{
    public class SkipSplash : MarshalByRefObject, IPlugin
    {
        public SkipSplash()
        {
            if (Utils.IstModLoaderInstalled())
                Main.showSplash = false;
        }
    }
}
