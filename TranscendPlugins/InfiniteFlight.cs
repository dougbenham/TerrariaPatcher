using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;

namespace ZeromaruPlugins
{
    [PluginDescription("Wings, rocket boots, and flying carpets never run out while enabled.")]
    public class InfiniteFlight : PluginBase, IPluginUpdate
    {
	    /// <inheritdoc/>
	    public override bool RespondsWhileDisabled
	    {
		    get { return true; }
	    }

        private readonly HotkeySetting ToggleKey = new Hotkey { Key = Keys.I };
		
        public InfiniteFlight()
        {
	        ToggleKey.Value.Action = Toggle;
        }

        private void Toggle()
        {
	        Enabled = !Enabled;

	        Main.NewText("Infinite Flight " + (Enabled ? "enabled" : "disabled"), 150, 150, 150);
        }

        public void OnUpdate()
        {
            Player.rocketTime = 1;
            Player.carpetTime = 1;
            Player.wingTime = 1f;
        }
    }
}
