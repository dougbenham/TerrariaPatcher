using System;
using PluginLoader;
using Terraria;
using Utils = PluginLoader.Utils;

namespace TranscendPlugins
{
    // can't fully skip because async loading was added to Terraria
    public class FastSplash : MarshalByRefObject, IPluginDrawSplash
    {
        /// <inheritdoc />
        public void OnDrawSplash()
        {
            if (!Utils.IstModLoaderInstalled() && !Main.instance.quickSplash)
            {
                Main.instance.quickSplash = true;
                Main.instance.splashCounter = 199;
            }
        }
    }
}
