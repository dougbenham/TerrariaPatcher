using PluginLoader;
using Terraria;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace TranscendPlugins
{
    [PluginDescription("Opens the Guide's crafting menu anywhere with a hotkey, so you can see and craft his recipes without walking back to him.")]
    public class PortableCraftingGuide : PluginBase, IPluginPreUpdate, IPluginUpdate, IPluginPlaySound
    {
        private readonly HotkeySetting ToggleKey = new Hotkey { Key = Keys.C };
		private bool pcg;
        
        public PortableCraftingGuide()
        {
	        ToggleKey.Value.Action = Toggle;
        }

        private void Toggle()
        {
            pcg = !pcg;

            if (pcg)
	            Player.OpenInventory(true);
            else
            {
                Main.InGuideCraftMenu = false;
                Player.SetTalkNPC(-1);
            }
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
	        if (!Main.playerInventory)
		        pcg = false;

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
