using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;
using Terraria.Testing;

namespace TranscendPlugins
{
    [PluginDescription("Reveals the whole map. Open the fullscreen map and press the hotkey. On a server it can only " +
                       "reveal the parts of the world the server has already sent you.")]
    public class Reveal : PluginBase
    {
        private static readonly HotkeySetting RevealKey = new Hotkey { Key = Keys.L, Action = RevealMap };

        private static void RevealMap()
        {
            if (!Main.mapFullscreen || Main.Map == null) return;

            // The reveal only unlocks sections Main.sectionManager has loaded, and a client is only sent the sections it has been near.
            if (Main.netMode != 0)
                Main.NewText("Only the parts of the world the server has sent you can be revealed.");

            Main.clearMap = true;
            DebugOptions.unlockMap = 1;
            Main.refreshMap = true;
        }
    }
}
