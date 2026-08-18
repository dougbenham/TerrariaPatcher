using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;

namespace TranscendPlugins
{
    [PluginDescription("Lights the entire world evenly, so caves and night are as visible as daytime. Toggled in game with a hotkey.")]
    public class FullBright : PluginBase, IPluginLightingGetColor
    {
	    /// <summary>
	    /// Keeps hotkeys working when Enabled is false.
	    /// </summary>
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

            var green = Color.Green;
            Main.NewText("Full Bright " + (Enabled ? "enabled" : "disabled"), green.R, green.G, green.B);
        }

        public bool OnLightingGetColor(int x, int y, out Color color)
        {
            color = Color.White;
            return true;
        }
    }
}
