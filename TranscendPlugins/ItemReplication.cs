using System;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;
using Terraria.Audio;

namespace RyanPlugins
{
    public class ItemReplication : MarshalByRefObject, IPluginItemSlotRightClick
    {
		private Keys replicateKey;

        public ItemReplication()
		{
            if (!Keys.TryParse(IniAPI.ReadIni("ItemReplication", "ReplicateKey", "R", writeIt: true), out replicateKey))
				replicateKey = Keys.R;
		}

        public bool OnItemSlotRightClick(Item[] inv, int context, int slot)
        {
            int[] contexts = new int[]{
								 0, //InventoryItem
								 1, //InventoryCoin
								 2, //InventoryAmmo
								 3, //ChestItem
								 4, //BankItem
								 6, //TrashItem
								 8, //EquipArmor
								 9, //EquipArmorVanity
								 10, //EquipAccessory
								 11, //EquipAccessoryVanity
								 12, //EquipDye
								 16, //EquipGrapple
								 17, //EquipMount
								 18, //EquipMinecart
								 19, //EquipPet
								 20 //EquipLight
							 };
            var invItem = inv[slot];
            invItem.newAndShiny = false;

            if (Main.stackSplit <= 1 && Main.mouseRight && Main.keyState.IsKeyDown(replicateKey) && contexts.Contains(context))
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

                        if (Main.stackSplit == 0)
                        {
                            Main.stackSplit = 15;
                        }
                        else
                        {
                            Main.stackSplit = Main.stackDelay;
                        }

                        if (context == 3 && Main.netMode == 1)
                        {
                            NetMessage.SendData(32, -1, -1, null, Main.player[Main.myPlayer].chest, (float)slot, 0f, 0f, 0, 0, 0);
                        }
                    }
                    return true;
                }

                if ((!Main.mouseItem.IsNotTheSameAs(invItem) && Main.mouseItem.stack < Main.mouseItem.maxStack) || Main.mouseItem.type == 0)
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

                    if (context == 3 && Main.netMode == 1)
                    {
                        NetMessage.SendData(32, -1, -1, null, Main.player[Main.myPlayer].chest, (float)slot, 0f, 0f, 0, 0, 0);
                    }
                }
                return true;
            }
            return false;
        }
    }
}
