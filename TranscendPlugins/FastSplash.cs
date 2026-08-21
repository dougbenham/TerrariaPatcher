using System;
using PluginLoader;
using Terraria;
using Utils = PluginLoader.Utils;

namespace TranscendPlugins
{
    // can't fully skip because async loading was added to Terraria
    [PluginDescription("Skips most of the Re-Logic splash screen so the game reaches the menu sooner.")]
    public class FastSplash : PluginBase, IPluginDrawSplash
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
