using System;
using PluginLoader;
using Terraria;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace TranscendPlugins
{
    public class PortableCraftingGuide : MarshalByRefObject, IPluginPreUpdate, IPluginUpdate, IPluginPlaySound, IPluginInitialize
    {
        private bool pcg;
        private Keys pcgKey;

        public void OnInitialize()
        {
            if (!Keys.TryParse(IniAPI.ReadIni("PortableCraftingGuide", "ToggleKey", "C", writeIt: true), out pcgKey))
                pcgKey = Keys.C;

            Loader.RegisterHotkey(() =>
            {
                pcg = !pcg;
                if (!pcg)
                {
                    Main.InGuideCraftMenu = false;
                    Main.player[Main.myPlayer].talkNPC = -1;
                }
            }, pcgKey);

            Keys invKey;
            Keys.TryParse(Main.cInv, out invKey);
            Loader.RegisterHotkey(() =>
            {
                pcg = false;
            }, invKey);
        }

        public void OnPreUpdate()
        {
            Set();
        }

        public void OnUpdate()
        {
            Set();
        }

        private void Set()
        {
            if (pcg)
            {
                Main.npcChatText = "";
                Main.player[Main.myPlayer].chest = -1;
                Main.player[Main.myPlayer].talkNPC = 22;
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
