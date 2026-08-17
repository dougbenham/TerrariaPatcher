using PluginLoader;
using Terraria;
using Terraria.ID;

namespace EraselsPlugins
{
    [PluginDescription("The Merchant stocks the best healing and mana potions your character can use, upgrading his stock as your maximum life and mana grow.")]
    public class ShopSellsScalingPotions : PluginBase, IPluginChestSetupShop
    {
        public void OnChestSetupShop(Chest chest, int type)
        {
            if (type != 1) return;

            var player = Main.player[Main.myPlayer];

            if (player.statLifeMax >= 500)
                Upgrade(chest, ItemID.LesserHealingPotion, ItemID.SuperHealingPotion);
            else if (player.statLifeMax >= 300)
                Upgrade(chest, ItemID.LesserHealingPotion, ItemID.GreaterHealingPotion);
            else if (player.statLifeMax >= 200)
                Upgrade(chest, ItemID.LesserHealingPotion, ItemID.HealingPotion);

            if (player.statManaMax >= 400)
                Upgrade(chest, ItemID.LesserManaPotion, ItemID.SuperManaPotion);
            else if (player.statManaMax >= 200)
                Upgrade(chest, ItemID.LesserManaPotion, ItemID.GreaterManaPotion);
            else if (player.statManaMax >= 160)
                Upgrade(chest, ItemID.LesserManaPotion, ItemID.ManaPotion);
        }

        /// <summary>
        /// Replaces a potion the shop stocks wherever the game put it. The Merchant's slots shift with the world and
        /// with hardmode, so which slot holds which potion cannot be assumed.
        /// </summary>
        private static void Upgrade(Chest chest, int stocked, int replacement)
        {
            for (var i = 0; i < chest.item.Length; i++)
            {
                if (chest.item[i] == null || chest.item[i].type != stocked) continue;

                chest.item[i].SetDefaults(replacement);
                chest.item[i].isAShopItem = true;
                return;
            }
        }
    }
}
