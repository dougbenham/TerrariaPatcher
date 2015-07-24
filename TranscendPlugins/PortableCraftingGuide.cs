using System;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;

namespace TranscendPlugins
{
    public class PortableCraftingGuide : MarshalByRefObject, IPluginUpdate, IPluginPlaySound
    {
        private bool pcg;
        private Keys pcgKey;

        public PortableCraftingGuide()
        {
            if (!Keys.TryParse(IniAPI.ReadIni("PortableCraftingGuide", "ToggleKey", "C", writeIt: true), out pcgKey))
                pcgKey = Keys.C;

            Loader.RegisterHotkey(() =>
            {
                pcg = !pcg;
            }, pcgKey);

            Loader.RegisterHotkey(() =>
            {
                pcg = false;
            }, Keys.Escape);
        }

        public void OnUpdate()
        {
            if (pcg)
            {
                Main.npcChatText = "";
                Main.player[Main.myPlayer].chest = -1;
                Main.player[Main.myPlayer].talkNPC = 22;
                Main.craftGuide = true;
                Main.playerInventory = true;
            }
        }

        public bool OnPlaySound(int type, int x, int y, int style)
        {
            return (pcg && type == 11); // skip menu close sound
        }
    }
}
