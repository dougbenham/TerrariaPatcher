using System;
using System.Collections.Generic;
using GTRPlugins.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;
using Terraria.UI;
using Terraria.UI.Chat;

namespace GTRPlugins
{
    public class ChestSearch : IPluginUpdate, IPluginDrawInventory
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
                int num = string.Compare(GetItem().name, comp.GetItem().name, StringComparison.InvariantCulture);
                if (num != 0)
                {
                    return num;
                }
                if (distance < comp.distance)
                {
                    return -1;
                }
                if (distance > comp.distance)
                {
                    return 1;
                }
                return 0;
            }
        }

        public bool ShowChestSearch;
        private bool SearchTextFocus;
        private string SearchText;
        private Button ShowChestSearchButton;
        private Button ClearTextFieldButton;

        public ChestSearch()
        {
            ShowChestSearch = false;
            SearchTextFocus = false;
            SearchText = "";
            ShowChestSearchButton = new Button("Search Chests", new Vector2(500f, 278f), ShowChestSearchHandler);
            ShowChestSearchButton.Scale = 0.9f;
            ClearTextFieldButton = new Button("Clear", new Vector2(506f, 328f), ClearTextFieldHandler);
            ClearTextFieldButton.Scale = 0.8f;
        }
        private void ShowChestSearchHandler(object sender, EventArgs e)
        {
            if (!ShowChestSearch)
            {
                Open();
                return;
            }
            if (ShowChestSearch)
            {
                Close();
            }
        }
        private void ClearTextFieldHandler(object sender, EventArgs e)
        {
            FocusTextField();
            SearchText = "";
        }
        private void Open()
        {
            Main.chatMode = false;
            Main.editSign = false;
            Main.editChest = false;
            Main.recBigList = false;
            FocusTextField();
            SearchText = "";
            ShowChestSearch = true;
        }
        private void Close()
        {
            ShowChestSearch = false;
            SearchTextFocus = false;
            Main.blockInput = false;
        }
        private void FocusTextField()
        {
            Main.clrInput();
            SearchTextFocus = true;
        }
        public void OnUpdate()
        {
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
                if (!SearchTextFocus || Main.keyState.IsKeyDown(Keys.Escape))
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
                Player player = Main.player[Main.myPlayer];
                Vector2 center = player.Center;
                List<ChestItem> list = new List<ChestItem>();
                for (int i = 0; i < 1000; i++)
                {
                    if (Main.chest[i] != null && !IsPlayerInChest(i) && !Chest.isLocked(Main.chest[i].x, Main.chest[i].y))
                    {
                        Vector2 value = new Vector2(Main.chest[i].x * 16 + 16, Main.chest[i].y * 16 + 16);
                        float num = (value - center).Length();
                        if (num < 400f)
                        {
                            for (int j = 0; j < Main.chest[i].item.Length; j++)
                            {
                                if (Main.chest[i].item[j] != null && Main.chest[i].item[j].type > 0 && Main.chest[i].item[j].stack > 0 && Main.chest[i].item[j].name.ToLower().Contains(SearchText.ToLower()))
                                {
                                    list.Add(new ChestItem
                                    {
                                        chest = i,
                                        slot = j,
                                        distance = num
                                    });
                                }
                            }
                        }
                    }
                }
                list.Sort();
                ClearTextFieldButton.Draw();
                int num2 = 150;
                int num3 = 316;
                if (Main.mouseX > num2 && Main.mouseX < num2 + 350 && Main.mouseY > num3 && Main.mouseY < num3 + Main.textBackTexture.Height)
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
                Main.spriteBatch.Draw(Main.textBackTexture, new Vector2(num2, num3), new Rectangle(0, 0, 175, Main.textBackTexture.Height), new Color(160, 160, 160, 160), 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(Main.textBackTexture, new Vector2(num2 + 175, num3), new Rectangle(Main.textBackTexture.Width - 175, 0, 175, Main.textBackTexture.Height), new Color(160, 160, 160, 160), 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
                string text = SearchText;
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
                        text += "|";
                    }
                }
                Vector2 vector = new Vector2(num2 + 10, num3 + 6);
                for (int k = 0; k < ChatManager.ShadowDirections.Length; k++)
                {
                    Main.spriteBatch.DrawString(Main.fontMouseText, text, vector + ChatManager.ShadowDirections[k] * 2f, Color.Black, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
                }
                float num4 = Main.mouseTextColor / 255f;
                Color color = SearchTextFocus ? Color.Silver : Color.White;
                Main.spriteBatch.DrawString(Main.fontMouseText, text, vector, new Color(color.R * num4, color.G * num4, color.B * num4), 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
                int context = 3;
                int num5 = 42;
                int num6 = 150;
                int num7 = 358;
                int num8 = (Main.screenWidth - num6 - 260) / num5;
                int num9 = Main.screenHeight - 42;
                Main.inventoryScale = 0.75f;
                for (int l = 0; l < list.Count; l++)
                {
                    int num10 = l % num8 * num5 + num6;
                    int num11 = l / num8 * num5 + num7;
                    if (num11 > num9)
                    {
                        return;
                    }
                    Item item = list[l].GetItem();
                    if (Main.mouseX >= num10 && Main.mouseX <= num10 + num5 && Main.mouseY >= num11 && Main.mouseY <= num11 + num5)
                    {
                        player.mouseInterface = true;
                        if (Main.mouseX <= num10 + Main.inventoryBackTexture.Width * Main.inventoryScale && Main.mouseY <= num11 + Main.inventoryBackTexture.Height * Main.inventoryScale)
                        {
                            int chest = player.chest;
                            Chest chest2 = list[l].GetChest();
                            player.chest = list[l].chest;
                            ItemSlot.Handle(chest2.item, context, list[l].slot);
                            for (int m = 0; m < 4; m++)
                            {
                                int num12 = Dust.NewDust(new Vector2(chest2.x * 16 + 10, chest2.y * 16 + 15), 10, 10, 66, 0f, 0f, 100, new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB), 1f);
                                Main.dust[num12].noGravity = true;
                            }
                            player.chest = chest;
                        }
                    }
                    ItemSlot.Draw(Main.spriteBatch, ref item, context, new Vector2(num10, num11));
                }
            }
        }
        private bool IsPlayerInChest(int i)
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
