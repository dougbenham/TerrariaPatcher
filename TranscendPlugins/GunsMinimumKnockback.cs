using PluginLoader;
using Terraria;
using Terraria.ID;

namespace DoombubblesPlugins
{
    [PluginDescription("Gives every gun at least 1 base knockback, so bullet weapons can roll the prefixes that need it, such as Unreal.")]
    public class GunsMinimumKnockback : PluginBase, IPluginItemSetDefaults
    {
        public void OnItemSetDefaults(Item item)
        {
            if (item.useAmmo == AmmoID.Bullet && item.knockBack == 0)
            {
                item.knockBack = 1;
            }
        }
    }
}
