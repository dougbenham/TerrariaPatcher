using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;

namespace MrBlueSLPlugins
{
    [PluginDescription("Shines a light at your cursor.")]
    public class Flashlight : PluginBase, IPluginPlayerUpdate
    {
        public Flashlight() : base(enabledByDefault: false, toggleKey: Keys.U)
        { }

        public void OnPlayerUpdate(Player player)
        {
            if (Enabled && player.whoAmI == Main.myPlayer)
            {
                Lighting.AddLight((int)(Main.mouseX + Main.screenPosition.X) / 16, (int)(Main.mouseY + Main.screenPosition.Y) / 16, 1f, 1f, 1f);
            }
        }
    }
}
