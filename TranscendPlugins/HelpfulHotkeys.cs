using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace DoombubblesPlugins
{
    // Some code segments adapted from https://github.com/JavidPack/HelpfulHotkeys
    [PluginDescription("A set of quality of life hotkeys: auto recall, quick buff from favourited items only, quick use of " +
                       "any inventory slot, swapping armour and accessories with vanity or with the inventory, swapping the " +
                       "hotbar, cycling ammo, quick stacking to nearby chests, and toggling the ruler, autopause and frame skip.")]
    public class HelpfulHotkeys : PluginBase, IPluginPlayerUpdate, IPluginPlayerPickAmmo
    {
        private static readonly HotkeySetting AutoRecall = new Hotkey
        {
            Action = DoAutoRecall,
            Key = Keys.Home
        };

        private static readonly HotkeySetting QuickBuffFavoritedOnly = new Hotkey
        {
            Action = DoQuickBuffFavoritedOnly,
            Key = Keys.Z
        };

        private static readonly HotkeySetting SwapArmorVanity = new Hotkey
        {
            Action = DoSwapArmorVanity
        };

        private static readonly HotkeySetting SwapArmorInventory = new Hotkey
        {
            Action = DoSwapArmorInventory
        };

        private static readonly Setting<int> SwapArmorInventorySlot1 = 29;
        private static readonly Setting<int> SwapArmorInventorySlot2 = 39;
        private static readonly Setting<int> SwapArmorInventorySlot3 = 49;

        private static readonly HotkeySetting SwapHotbar = new Hotkey
        {
            Action = DoSwapHotbar
        };

        // Unbound rather than on the period key, which the Time plugin uses.
        private static readonly HotkeySetting CycleAmmo = new Hotkey
        {
            Action = () => DoCycleAmmo(true)
        };

        private static readonly Setting<bool> AutoCycleAmmo = false;

        private static readonly HotkeySetting ToggleAutoCycleAmmo = new Hotkey
        {
            Action = DoToggleAutoCycleAmmo
        };

        private static readonly HotkeySetting StackToNearbyChests = new Hotkey
        {
            Action = QuickStackToChests
        };

        private static readonly HotkeySetting RulerHotkey = new Hotkey
        {
            Action = ToggleRuler
        };

        private static readonly HotkeySetting SwitchFrameSkipMode = new Hotkey
        {
            Action = DoSwitchFrameSkipMode
        };

        private static readonly HotkeySetting ToggleAutoPause = new Hotkey
        {
            Action = DoToggleAutoPause,
            Key = Keys.Pause
        };

        public HelpfulHotkeys()
        {
            for (var slot = 10; slot <= 19; slot++)
            {
                AddSetting("QuickUseItem" + slot, new HotkeySetting(new Hotkey { Action = DoQuickUseItem(slot) }));
            }

            for (var slot = 0; slot < Player.SupportedSlotsAccs; slot++)
            {
                AddSetting("SwapAccessoryVanity" + (slot + 1), new HotkeySetting(new Hotkey { Action = SwapAccessoryVanity(slot) }));
            }

            AddSetting("SwapAccessoryVanityAll", new HotkeySetting(new Hotkey
            {
                Action = () =>
                {
                    for (var slot = 0; slot < Player.SupportedSlotsAccs; slot++)
                    {
                        SwapAccessoryVanity(slot)();
                    }
                }
            }));
        }

        private static readonly HashSet<int> RecallItems = new HashSet<int>
        {
            ItemID.MagicMirror,
            ItemID.IceMirror,
            ItemID.CellPhone,
            ItemID.RecallPotion,
            ItemID.PotionOfReturn,
            ItemID.Shellphone,
            ItemID.ShellphoneSpawn,
            ItemID.ShellphoneOcean,
            ItemID.ShellphoneHell
        };

        private static void DoAutoRecall()
        {
            for (var i = 0; i < Player.inventory.Length; i++)
            {
                if (!RecallItems.Contains(Player.inventory[i].type)) continue;

                QuickUseItemAt(i);
                break;
            }
        }

        private static void DoSwapArmorVanity()
        {
            var swapHappens = false;
            for (var slot = 10; slot < 13; slot++)
            {
                if (Player.armor[slot].type <= ItemID.None || Player.armor[slot].stack <= 0 ||
                    Player.armor[slot].vanity || Player.armor[slot - 10].type <= ItemID.None ||
                    Player.armor[slot - 10].stack <= 0) continue;

                Terraria.Utils.Swap(ref Player.armor[slot], ref Player.armor[slot - 10]);
                swapHappens = true;
            }

            if (swapHappens)
            {
                SoundEngine.PlaySound(SoundID.Grab);
            }
        }

        private static void DoSwapArmorInventory()
        {
            var swapHappens = false;

            var inventoryIndexes = new[]
            {
                SwapArmorInventorySlot1.Value, SwapArmorInventorySlot2.Value, SwapArmorInventorySlot3.Value
            };

            foreach (var inventoryIndex in inventoryIndexes)
            {
                if (inventoryIndex < 0 || inventoryIndex > 49) continue;

                var inventoryItem = Player.inventory[inventoryIndex];
                if (inventoryItem.IsAir || inventoryItem.headSlot == -1
                    && inventoryItem.bodySlot == -1 && inventoryItem.legSlot == -1) continue;

                var original = inventoryItem.type;
                var favorited = inventoryItem.favorited;
                ItemSlot.SwapEquip(Player.inventory, ItemSlot.Context.InventoryItem, inventoryIndex);
                Player.inventory[inventoryIndex].favorited = favorited;

                swapHappens |= original != inventoryItem.type;
            }

            if (swapHappens)
            {
                SoundEngine.PlaySound(SoundID.Grab);
            }
        }

        private static void DoSwapHotbar()
        {
            var swapHappens = false;

            for (var i = Main.InventoryItemSlotsStart; i < 10; i++)
            {
                if (Player.inventory[i + 10].IsAir) continue;

                Terraria.Utils.Swap(ref Player.inventory[i], ref Player.inventory[i + 10]);
                swapHappens = true;
            }

            if (swapHappens)
            {
                SoundEngine.PlaySound(SoundID.Grab);
            }
        }

        private static Action SwapAccessoryVanity(int accessory)
        {
            return () =>
            {
                var slot = 13 + accessory;

                if (Player.armor[slot].type <= ItemID.None || Player.armor[slot].stack <= 0 ||
                    Player.armor[slot].vanity || Player.armor[slot - 10].type <= ItemID.None ||
                    Player.armor[slot - 10].stack <= 0) return;

                Terraria.Utils.Swap(ref Player.armor[slot], ref Player.armor[slot - 10]);
                SoundEngine.PlaySound(SoundID.Grab);
            };
        }

        private static void DoCycleAmmo(bool playSound)
        {
            const int start = Main.InventoryAmmoSlotsStart;
            const int end = start + Main.InventoryAmmoSlotsCount - 1;

            var indexOfFirst = -1;
            for (var i = start; i < end; i++)
            {
                if (Player.inventory[i].type == ItemID.None) continue;

                indexOfFirst = i;
                break;
            }

            if (indexOfFirst == -1) return;

            var temp = Player.inventory[indexOfFirst];
            for (var i = indexOfFirst; i < end; i++)
            {
                Player.inventory[i] = Player.inventory[i + 1];
            }

            Player.inventory[end] = temp;

            if (playSound)
            {
                SoundEngine.PlaySound(SoundID.MenuTick);
            }

            autoCycledThisFrame = true;
        }

        private static void DoToggleAutoCycleAmmo()
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
            AutoCycleAmmo.Value = !AutoCycleAmmo;
            Main.NewText("Auto cycling ammo turned " + (AutoCycleAmmo ? "on" : "off"));
        }

        private static Action DoQuickUseItem(int slot)
        {
            return () => QuickUseItemAt(slot);
        }

        private static void QuickStackToChests()
        {
            if (Player.chest != -1)
            {
                ChestUI.QuickStack();
            }
            else
            {
                Player.QuickStackAllChests();
            }

            SoundEngine.PlaySound(SoundID.MenuTick);
        }

        private static void DoQuickBuffFavoritedOnly()
        {
            var backup = new Dictionary<Item, int>();

            try
            {
                foreach (var item in Player.inventory.Where(item => !item.favorited && item.buffType > 0))
                {
                    backup[item] = item.buffType;
                    item.buffType = 0;
                }

                Player.QuickBuff();
            }
            finally
            {
                foreach (var item in backup.Keys)
                {
                    item.buffType = backup[item];
                }
            }
        }

        private static void ToggleRuler()
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
            if (Player.builderAccStatus[Terraria.Player.BuilderAccToggleIDs.RulerLine] == 0)
                Player.builderAccStatus[Terraria.Player.BuilderAccToggleIDs.RulerLine] = 1;
            else
                Player.builderAccStatus[Terraria.Player.BuilderAccToggleIDs.RulerLine] = 0;
        }

        private static void DoSwitchFrameSkipMode()
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
            Main.CycleFrameSkipMode();
            Main.NewText("Frame Skip Mode is now: " +
                         Language.GetTextValue("LegacyMenu." + (247 + Main.FrameSkipMode)));
        }

        private static void DoToggleAutoPause()
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
            Main.autoPause = !Main.autoPause;
            Main.NewText("Autopause turned " + (Main.autoPause ? "on" : "off"));
        }

        private static int quickUsingItem = -1;
        private static int previousItem;

        private static void QuickUseItemAt(int index)
        {
            if (Player.selectedItem == index) return;

            var item = Player.inventory[index];

            if (item == null || !item.active) return;

            if (item.mountType != -1)
            {
                var mountDict = new Dictionary<Item, int>();

                foreach (var i in Player.inventory.Concat(Player.miscEquips))
                {
                    if (!i.active || i.mountType == -1 || item == i) continue;

                    mountDict[i] = i.mountType;
                    i.mountType = -1;
                }

                Player.QuickMount();

                foreach (var i in mountDict.Keys)
                {
                    i.mountType = mountDict[i];
                }

                return;
            }

            quickUsingItem = index;
            previousItem = Player.selectedItem;
            Player.selectedItemState.Select(index);
        }

        public void OnPlayerUpdate(Player player)
        {
            autoCycledThisFrame = false;
            if (Main.myPlayer != player.whoAmI) return;

            if (player.selectedItem == quickUsingItem)
            {
                quickUsingItem = -1;
                player.controlUseItem = true;
                player.ItemCheck();

                player.selectedItemState.Select(previousItem);
            }
        }

        private static bool autoCycledThisFrame;

        public void OnPlayerPickAmmo(Player player, Item weapon, ref int shoot, ref float speed, ref bool canShoot,
            ref int damage, ref float knockback, ref int usedAmmoItemId, bool dontConsume)
        {
            if (!AutoCycleAmmo || autoCycledThisFrame) return;

            DoCycleAmmo(false);
        }
    }
}
