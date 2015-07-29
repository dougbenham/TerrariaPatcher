using System;
using System.Collections.Generic;
using GTRPlugins.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PluginLoader;
using Terraria;
using Terraria.UI;
using Terraria.UI.Chat;

namespace GTRPlugins
{
    public class ChestSearch : IPluginPreUpdate, IPluginDrawInventory
    {
        private class ChestItem : IComparable<ChestItem>
        {
            public int chest;
            public int slot;
            public float distance;

            public Item GetItem()
            {
                return Main.chest[chest].item[slot];
            }

            public Chest GetChest()
            {
                return Main.chest[chest];
            }

            public int CompareTo(ChestItem comp)
            {
                if (comp == null)
                {
                    return 1;
                }
                int nameComp = string.Compare(GetItem().name, comp.GetItem().name, StringComparison.InvariantCulture);
                if (nameComp != 0)
                {
                    return nameComp;
                }
                if (distance < comp.distance)
                {
                    return -1;
                }
                else if (distance > comp.distance)
                {
                    return 1;
                }
                return 0;
            }
        }

        private const float Range = 200;
        public static bool ShowChestSearch = false;
        private static bool SearchTextFocus = false;
        private static string SearchText = "";
        private static Button ShowChestSearchButton;
        private static Button ClearTextFieldButton;

        static ChestSearch()
        {
            ShowChestSearchButton = new Button("Search Chests", new Vector2(500, 278), ShowChestSearchHandler);
            ShowChestSearchButton.Scale = 0.9f;
            ClearTextFieldButton = new Button("Clear", new Vector2(506, 328), ClearTextFieldHandler);
            ClearTextFieldButton.Scale = 0.8f;
        }

        private static void ShowChestSearchHandler(object sender, EventArgs e)
        {
            if (!ShowChestSearch)
            {
                Open();
            }
            else if (ShowChestSearch)
            {
                Close();
            }
        }

        private static void ClearTextFieldHandler(object sender, EventArgs e)
        {
            FocusTextField();
            SearchText = "";
        }

        private static void Open()
        {
            Main.chatMode = false;
            Main.editSign = false;
            Main.editChest = false;
            Main.recBigList = false;
            FocusTextField();
            SearchText = "";
            ShowChestSearch = true;
        }

        private static void Close()
        {
            ShowChestSearch = false;
            SearchTextFocus = false;
            Main.blockInput = false;
        }

        private static void FocusTextField()
        {
            Main.clrInput();
            SearchTextFocus = true;
        }

        public void OnPreUpdate()
        {
            // Hide chest search when conflicting interfaces are displayed
            if ((Main.chatMode || Main.editSign || Main.editChest || Main.recBigList || Main.player[Main.myPlayer].chest != -1 || !Main.playerInventory) && ShowChestSearch)
            {
                Close();
            }

            if (ShowChestSearch)
            {
                if (SearchTextFocus)
                {
                    string searchText = SearchText;
                    SearchText = Main.GetInputText(SearchText);
                    if (searchText != SearchText)
                    {
                        Main.PlaySound(12, -1, -1, 1);
                    }
                    Main.blockInput = true;
                }
                if (!SearchTextFocus || Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
                {
                    SearchTextFocus = false;
                    Main.blockInput = false;
                }
            }
        }

        public void OnDrawInventory()
        {
            if (Main.player[Main.myPlayer].chest == -1 && Main.npcShop == 0)
            {
                ShowChestSearchButton.Draw();
            }
            if (ShowChestSearch)
            {
                // Locate items
                Player player = Main.player[Main.myPlayer];
                Vector2 playerPos = player.Center;
                List<ChestItem> itemList = new List<ChestItem>();
                for (int i = 0; i < 1000; i++)
                {
                    if (Main.chest[i] != null && !IsPlayerInChest(i) && !Chest.isLocked(Main.chest[i].x, Main.chest[i].y))
                    {
                        Vector2 pos = new Vector2((float)(Main.chest[i].x * 16 + 16), (float)(Main.chest[i].y * 16 + 16));
                        float dist = (pos - playerPos).Length();
                        if (dist < Range)
                        {
                            for (int j = 0; j < Main.chest[i].item.Length; j++)
                            {
                                if (Main.chest[i].item[j] != null && Main.chest[i].item[j].type > 0 && Main.chest[i].item[j].stack > 0 && Main.chest[i].item[j].name.ToLower().Contains(SearchText.ToLower()))
                                {
                                    itemList.Add(new ChestItem() { chest = i, slot = j, distance = dist });
                                }
                            }
                        }
                    }
                }
                itemList.Sort();
                // Draw text field
                ClearTextFieldButton.Draw();
                int textFieldX = 150;
                int textFieldY = 316;
                if ((Main.mouseX > textFieldX) && (Main.mouseX < (textFieldX + 350)) && (Main.mouseY > textFieldY) && (Main.mouseY < (textFieldY + Main.textBackTexture.Height)))
                {
                    Main.player[Main.myPlayer].mouseInterface = true;
                    if (Main.mouseLeftRelease && Main.mouseLeft)
                    {
                        FocusTextField();
                        Main.mouseLeftRelease = false;
                    }
                    if (Main.mouseRightRelease && Main.mouseRight)
                    {
                        FocusTextField();
                        SearchText = "";
                        Main.mouseRightRelease = false;
                    }
                }
                else if (SearchTextFocus && Main.mouseLeftRelease && Main.mouseLeft)
                {
                    Main.player[Main.myPlayer].mouseInterface = true;
                    SearchTextFocus = false;
                }

                Main.spriteBatch.Draw(Main.textBackTexture, new Vector2(textFieldX, textFieldY), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, 0, 175, Main.textBackTexture.Height)), new Microsoft.Xna.Framework.Color(160, 160, 160, 160), 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(Main.textBackTexture, new Vector2(textFieldX + 175, textFieldY), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(Main.textBackTexture.Width - 175, 0, 175, Main.textBackTexture.Height)), new Microsoft.Xna.Framework.Color(160, 160, 160, 160), 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
                string searchText = SearchText;
                if (SearchTextFocus)
                {
                    Main.instance.textBlinkerCount++;
                    if (Main.instance.textBlinkerCount >= 20)
                    {
                        if (Main.instance.textBlinkerState == 0)
                        {
                            Main.instance.textBlinkerState = 1;
                        }
                        else
                        {
                            Main.instance.textBlinkerState = 0;
                        }
                        Main.instance.textBlinkerCount = 0;
                    }
                    if (Main.instance.textBlinkerState == 1)
                    {
                        searchText = searchText + "|";
                    }
                }
                Vector2 searchTextPosition = new Vector2(textFieldX + 10, textFieldY + 6);
                for (int i = 0; i < ChatManager.ShadowDirections.Length; i++)
                {
                    Main.spriteBatch.DrawString(Main.fontMouseText, searchText, searchTextPosition + ChatManager.ShadowDirections[i] * 2, Color.Black, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
                }
                float pulse = (float)Main.mouseTextColor / 255;
                Color textColor = SearchTextFocus ? Color.Silver : Color.White;
                Main.spriteBatch.DrawString(Main.fontMouseText, searchText, searchTextPosition, new Color(textColor.R * pulse, textColor.G * pulse, textColor.B * pulse), 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);

                // Draw item slots
                int invContext = 3;
                int slotSize = 42;
                int offsetX = 150;
                int offsetY = 358;
                int width = (Main.screenWidth - offsetX - 260) / slotSize;
                int height = Main.screenHeight - 42;
                Main.inventoryScale = 0.75f;
                for (int i = 0; i < itemList.Count; i++)
                {
                    int x = (i % width) * slotSize + offsetX;
                    int y = ((int)(i / width)) * slotSize + offsetY;
                    if (y > height)
                    {
                        break;
                    }
                    Item item = itemList[i].GetItem();
                    if (Main.mouseX >= x && Main.mouseX <= x + slotSize && Main.mouseY >= y && Main.mouseY <= y + slotSize)
                    {
                        player.mouseInterface = true;
                        if ((float)Main.mouseX <= (float)x + (float)Main.inventoryBackTexture.Width * Main.inventoryScale && (float)Main.mouseY <= (float)y + (float)Main.inventoryBackTexture.Height * Main.inventoryScale)
                        {
                            int prevPlayerChest = player.chest;
                            Chest chest = itemList[i].GetChest();
                            player.chest = itemList[i].chest;
                            ItemSlot.Handle(chest.item, invContext, itemList[i].slot);

                            // Sparkle that shit
                            for (int j = 0; j < 4; j++)
                            {
                                int k = Dust.NewDust(new Vector2(chest.x * 16 + 10, chest.y * 16 + 15), 10, 10, 66, 0f, 0f, 100, new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB), 1f);
                                Main.dust[k].noGravity = true;
                            }
                            player.chest = prevPlayerChest;
                        }
                    }
                    ItemSlot.Draw(Main.spriteBatch, ref item, invContext, new Vector2((float)x, (float)y), default(Microsoft.Xna.Framework.Color));
                }
            }
        }

        private static bool IsPlayerInChest(int i)
        {
            for (int j = 0; j < 255; j++)
            {
                if (Main.player[j].chest == i && j != Main.myPlayer)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
