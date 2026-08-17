using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace RyanPlugins
{
    [PluginDescription("Duplicates items: hold the replicate key and right click a slot to add one to the stack, or hold Shift as well to fill it.")]
    public class ItemReplication : PluginBase, IPluginItemSlotRightClick
    {
        private static readonly Setting<Keys> ReplicateKey = Keys.R;

        private static bool SameItemIgnoringStack(Item a, Item b)
        {
	        if (a == null || b == null) return false;
	        if (a.type == 0 || b.type == 0) return false;
	        if (a.type != b.type) return false;
	        if (a.prefix != b.prefix) return false;
	        return true;
        }

        /// <summary>
        /// Tells the server about a slot this plugin changed. Without this the replicated item only exists on this
        /// client, and the server overwrites it the next time it syncs the slot.
        /// </summary>
        private static void SyncSlot(int context, int slot)
        {
            if (Main.netMode != 1) return;

            if (context == 3)
            {
                NetMessage.SendData(32, -1, -1, null, Main.player[Main.myPlayer].chest, slot);
                return;
            }

            var slotId = ToPlayerItemSlotId(context, slot);
            if (slotId < 0) return;

            NetMessage.SendData(5, -1, -1, null, Main.myPlayer, slotId);
        }

        /// <summary>
        /// Converts an <c>ItemSlot.Context</c> and its index into the flat slot id that message 5 addresses.
        /// Returns -1 for contexts that message 5 does not cover, such as the piggy bank.
        /// </summary>
        private static int ToPlayerItemSlotId(int context, int slot)
        {
            switch (context)
            {
                case 0: // InventoryItem
                case 1: // InventoryCoin
                case 2: // InventoryAmmo
                    return PlayerItemSlotID.Inventory0 + slot;
                case 6: // TrashItem
                    return PlayerItemSlotID.TrashItem;
                case 8:  // EquipArmor
                case 9:  // EquipArmorVanity
                case 10: // EquipAccessory
                case 11: // EquipAccessoryVanity
                    return PlayerItemSlotID.Armor0 + slot;
                case 12: // EquipDye
                    return PlayerItemSlotID.Dye0 + slot;
                case 16: // EquipGrapple
                case 17: // EquipMount
                case 18: // EquipMinecart
                case 19: // EquipPet
                case 20: // EquipLight
                    return PlayerItemSlotID.Misc0 + slot;
                default:
                    return -1;
            }
        }

        /// <summary>
        /// The <c>ItemSlot.Context</c> values a slot may be replicated from.
        /// </summary>
        private static readonly HashSet<int> Contexts = new HashSet<int>
        {
            0,  // InventoryItem
            1,  // InventoryCoin
            2,  // InventoryAmmo
            3,  // ChestItem
            4,  // BankItem
            6,  // TrashItem
            8,  // EquipArmor
            9,  // EquipArmorVanity
            10, // EquipAccessory
            11, // EquipAccessoryVanity
            12, // EquipDye
            16, // EquipGrapple
            17, // EquipMount
            18, // EquipMinecart
            19, // EquipPet
            20  // EquipLight
        };

        public bool OnItemSlotRightClick(Item[] inv, int context, int slot)
        {
            var invItem = inv[slot];
            invItem.newAndShiny = false;

            if (Main.stackSplit <= 1 && Main.mouseRight && Main.keyState.IsKeyDown(ReplicateKey) && Contexts.Contains(context))
            {
                bool shiftDown =
                    Main.keyState.IsKeyDown(Keys.LeftShift) ||
                    Main.keyState.IsKeyDown(Keys.RightShift);

                if (shiftDown)
                {
                    if (invItem.stack < invItem.maxStack)
                    {
                        invItem.stack = invItem.maxStack;

                        Recipe.UpdateRecipeList();
                        SoundEngine.PlaySound(12, -1, -1, 1);

                        Main.stackSplit = Main.stackSplit == 0 ? 15 : Main.stackDelay;

                        SyncSlot(context, slot);
                    }
                    return true;
                }

                if ((SameItemIgnoringStack(Main.mouseItem, invItem) && Main.mouseItem.stack < Main.mouseItem.maxStack) || Main.mouseItem.type == 0)
                {
                    if (Main.mouseItem.type == 0)
                    {
                        Main.mouseItem = invItem.Clone();
                        Main.mouseItem.stack = 0;

                        if (invItem.favorited && invItem.maxStack == 1)
                        {
                            Main.mouseItem.favorited = true;
                        }
                        else
                        {
                            Main.mouseItem.favorited = false;
                        }
                    }

                    Main.mouseItem.stack++;
                    Recipe.UpdateRecipeList();
                    SoundEngine.PlaySound(12, -1, -1, 1);

                    if (Main.stackSplit == 0)
                    {
                        Main.stackSplit = 15;
                    }
                    else
                    {
                        Main.stackSplit = Main.stackDelay;
                    }

                    SyncSlot(context, slot);
                }
                return true;
            }
            return false;
        }
    }
}
