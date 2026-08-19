using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;

namespace MrBlueSLPlugins
{
    [PluginDescription("Shines a light at your cursor.")]
    public class Flashlight : PluginBase, IPluginPlayerUpdate
    {
	    /// <inheritdoc/>
	    public override bool RespondsWhileDisabled
	    {
		    get { return true; }
	    }

        private readonly HotkeySetting ToggleKey = new Hotkey { Key = Keys.U };
        
        public Flashlight() : base(enabledByDefault: false)
        {
	        ToggleKey.Value.Action = Toggle;
        }

        private void Toggle()
        {
            Enabled = !Enabled;
            Main.NewText("Flashlight " + (Enabled ? "enabled" : "disabled"), 150, 150, 150);
        }

        public void OnPlayerUpdate(Player player)
        {
            if (Enabled && player.whoAmI == Main.myPlayer)
            {
                Lighting.AddLight((int)(Main.mouseX + Main.screenPosition.X) / 16, (int)(Main.mouseY + Main.screenPosition.Y) / 16, 1f, 1f, 1f);
            }
        }
    }
}
