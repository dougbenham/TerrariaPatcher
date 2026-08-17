using System.Linq;
using PluginLoader;
using Terraria;

namespace DoombubblesPlugins
{
    [PluginDescription("A full stack of ammo never runs down: firing from a stack of 9999 leaves it at 9999. Consumable " +
                       "thrown weapons have their own threshold, 999 by default.")]
    public class PermaAmmo : PluginBase, IPluginPlayerPickAmmo, IPluginPlayerUpdate
    {
        private static readonly Setting<int> RequiredCount = 9999;
        private static readonly Setting<int> ThrownRequiredCount = 999;

        public void OnPlayerPickAmmo(Player player, Item weapon, ref int shoot, ref float speed, ref bool canShoot,
            ref int damage, ref float knockback, ref int usedAmmoItemId, bool dontConsume)
        {
            if (dontConsume) return;

            foreach (var item in player.inventory.Where(item =>
                         item.active && weapon.useAmmo == item.ammo && item.stack == RequiredCount - 1))
            {
                item.stack++;
            }
        }

        public void OnPlayerUpdate(Player player)
        {
            if (player.HeldItem != null && player.HeldItem.active && player.itemTime == player.itemTimeMax &&
                player.HeldItem.damage > 0 && player.HeldItem.consumable &&
                player.HeldItem.stack == ThrownRequiredCount - 1)
            {
                player.HeldItem.stack++;
            }
        }
    }
}
