using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;

namespace TranscendPlugins
{
    [PluginDescription("Lights every block fully.")]
    public class FullBright : PluginBase, IPluginLightingGetColor
    {
	    /// <inheritdoc/>
	    public override bool RespondsWhileDisabled
	    {
		    get { return true; }
	    }

        [SettingDescription("Lights the world evenly while it is switched on.")]
        private readonly HotkeySetting ToggleKey = new Hotkey { Key = Keys.Y };

        public FullBright() : base(enabledByDefault: false)
        {
            ToggleKey.Value.Action = Toggle;
        }

        private void Toggle()
        {
            Enabled = !Enabled;

            Main.NewText("Full Bright " + (Enabled ? "enabled" : "disabled"), 150, 150, 150);
        }

        public bool OnLightingGetColor(int x, int y, out Color color)
        {
            color = Color.White;
            return true;
        }
    }
}
