using System;
using GTRPlugins.Utils;
using PluginLoader;
using Terraria;
using Terraria.Audio;

namespace GTRPlugins
{
    public class InventoryEnhancementPlugin : MarshalByRefObject, IPluginInitialize, IPluginDrawInventory, IPluginPreUpdate, IPluginPlayerQuickBuff, IPluginPlayerGetItem
    {
        public void OnInitialize()
        {
            AutoSort.Init(null, null);
            Inventory_Enhancements.Init(null, null);
        }

        public void OnDrawInventory()
        {
            Inventory_Enhancements_UI.DrawInventory(null, null);
        }

        public void OnPreUpdate()
        {
            Inventory_Enhancements.Update(null, null);
            Inventory_Enhancements_UI.Update(null, null);
            CycleAmmo.Update(null, null);
            AutoTrash.Update(null, null);
            Input.Update();
        }

        public bool OnPlayerQuickBuff(Player player)
        {
            if (player.noItems) return true;

            if (player.chest != -1)
            {
                Chest chest;
                if (player.chest > -1)
                {
                    chest = Main.chest[player.chest];
                }
                else if (player.chest == -2)
                {
                    chest = player.bank;
                }
                else
                {
                    chest = player.bank2;
                }
                if (player.CountBuffs() == 22) return true;

                SoundStylePair soundStylePair = null;
                for (int i = 0; i < 40; i++)
                {
                    if (chest.item[i].stack > 0 && chest.item[i].type > 0 && chest.item[i].buffType > 0 && !chest.item[i].summon && chest.item[i].buffType != 90)
                    {
                        int num3 = chest.item[i].buffType;
                        bool flag = true;
                        for (int j = 0; j < 22; j++)
                        {
                            if (num3 == 27 && (player.buffType[j] == num3 || player.buffType[j] == 101 || player.buffType[j] == 102))
                            {
                                flag = false;
                                break;
                            }
                            if (player.buffType[j] == num3)
                            {
                                flag = false;
                                break;
                            }
                            if (Main.meleeBuff[num3] && Main.meleeBuff[player.buffType[j]])
                            {
                                flag = false;
                                break;
                            }
                        }
                        if (Main.lightPet[chest.item[i].buffType] || Main.vanityPet[chest.item[i].buffType])
                        {
                            for (int k = 0; k < 22; k++)
                            {
                                if (Main.lightPet[player.buffType[k]] && Main.lightPet[chest.item[i].buffType])
                                {
                                    flag = false;
                                }
                                if (Main.vanityPet[player.buffType[k]] && Main.vanityPet[chest.item[i].buffType])
                                {
                                    flag = false;
                                }
                            }
                        }
                        if (chest.item[i].mana > 0 && flag)
                        {
                            if (player.statMana >= (int)((float)chest.item[i].mana * player.manaCost))
                            {
                                player.manaRegenDelay = (int)player.maxRegenDelay;
                                player.statMana -= (int)((float)chest.item[i].mana * player.manaCost);
                            }
                            else
                            {
                                flag = false;
                            }
                        }
                        if (player.whoAmI == Main.myPlayer && chest.item[i].type == 603 && !Main.cEd)
                        {
                            flag = false;
                        }
                        if (num3 == 27)
                        {
                            num3 = Main.rand.Next(3);
                            if (num3 == 0)
                            {
                                num3 = 27;
                            }
                            if (num3 == 1)
                            {
                                num3 = 101;
                            }
                            if (num3 == 2)
                            {
                                num3 = 102;
                            }
                        }
                        if (flag)
                        {
                            soundStylePair = chest.item[i].UseSound;
                            int num4 = chest.item[i].buffTime;
                            if (num4 == 0)
                            {
                                num4 = 3600;
                            }
                            player.AddBuff(num3, num4, true);
                            if (chest.item[i].consumable)
                            {
                                chest.item[i].stack--;
                                if (chest.item[i].stack <= 0)
                                {
                                    chest.item[i].type = 0;
                                    chest.item[i].name = "";
                                }
                            }
                        }
                    }
                }
                if (Main.netMode == 1)
                {
                    if (player.chest < 0)
                    {
                        for (int l = 0; l < 40; l++)
                        {
                            NetMessage.SendData(32, -1, -1, "", player.chest, (float)l, 0f, 0f, 0, 0, 0);
                        }
                    }
                    else
                    {
                        NetMessage.SendData(33, -1, -1, "", Main.player[Main.myPlayer].chest, 0f, 0f, 0f, 0, 0, 0);
                    }
                }
                if (soundStylePair != null)
                {
                    Main.PlaySound(soundStylePair, player.position);
                    Recipe.FindRecipes();
                }
            }

            return false;
        }
        
        public bool OnPlayerGetItem(Player player, Item newItem, out Item resultItem)
        {
            if (AutoTrash.Trash(player, newItem))
            {
                resultItem = new Item();
                return true;
            }
            resultItem = null;
            return false;
        }
    }
}
