using System;
using PluginLoader;
using Terraria;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace TranscendPlugins
{
    [PluginDescription("Opens the Guide's crafting menu anywhere with a hotkey, so you can see and craft his recipes without walking back to him.")]
    public class PortableCraftingGuide : PluginBase, IPluginPreUpdate, IPluginUpdate, IPluginPlaySound, IPluginInitialize
    {
        private static readonly HotkeySetting ToggleKey = new Hotkey { Key = Keys.C, Action = Toggle };

        private static bool pcg;

        private static void Toggle()
        {
            pcg = !pcg;

            if (!pcg)
            {
                Main.InGuideCraftMenu = false;
                Player.SetTalkNPC(-1);
            }
        }

        public void OnInitialize()
        {
            // Closing the inventory closes the crafting menu with it.
            Keys invKey;
            Keys.TryParse(Main.cInv, out invKey);
            Loader.RegisterHotkey(() => pcg = false, invKey);
        }

        public void OnPreUpdate()
        {
            Set();
        }

        public void OnUpdate()
        {
            Set();
        }

        private static void Set()
        {
            if (pcg)
            {
                Main.npcChatText = "";
                Player.chest = -1;
                Player.SetTalkNPC(22);
                Main.InGuideCraftMenu = true;
                Main.playerInventory = true;
            }
        }

        public bool OnPlaySound(int type, int x, int y, int style)
        {
            return (pcg && type == 11); // skip menu close sound
        }
    }
}
