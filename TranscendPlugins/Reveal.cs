using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;
using Terraria.Testing;

namespace TranscendPlugins
{
    [PluginDescription("Reveals the whole map. Open the fullscreen map and press the hotkey.")]
    public class Reveal : PluginBase
    {
        private static readonly HotkeySetting RevealKey = new Hotkey { Key = Keys.L, Action = RevealMap };

        private static void RevealMap()
        {
            if (!Main.mapFullscreen || Main.Map == null) return;

            Main.clearMap = true;
            DebugOptions.unlockMap = 1;
            Main.refreshMap = true;
        }
    }
}
