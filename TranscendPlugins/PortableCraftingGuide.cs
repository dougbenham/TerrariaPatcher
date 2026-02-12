using System;
using PluginLoader;
using Terraria;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace TranscendPlugins
{
    public class PortableCraftingGuide : MarshalByRefObject, IPluginPreUpdate, IPluginUpdate, IPluginPlaySound, IPluginInitialize, IPluginChatCommand
    {
        private bool pcg;
        private Keys pcgKey;

        public void OnInitialize()
        {
            bool stored;
            if (bool.TryParse(IniAPI.ReadIni("PortableCraftingGuide", "Enabled", "false", writeIt: true), out stored))
                pcg = stored;

            if (!Keys.TryParse(IniAPI.ReadIni("PortableCraftingGuide", "ToggleKey", "C", writeIt: true), out pcgKey))
                pcgKey = Keys.C;

            Loader.RegisterHotkey(() =>
            {
                pcg = !pcg;
                Persist();
                if (!pcg)
                {
                    Main.InGuideCraftMenu = false;
                    Main.player[Main.myPlayer].SetTalkNPC(-1);
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
                Main.player[Main.myPlayer].SetTalkNPC(22);
                Main.InGuideCraftMenu = true;
                Main.playerInventory = true;
            }
        }

        public bool OnPlaySound(int type, int x, int y, int style)
        {
            return (pcg && type == 11); // skip menu close sound
        }

        public bool OnChatCommand(string command, string[] args)
        {
            if (command != "pcg") return false;

            string arg = args.Length > 0 ? args[0].ToLower() : "toggle";
            switch (arg)
            {
                case "on":
                    pcg = true;
                    break;
                case "off":
                    pcg = false;
                    break;
                case "toggle":
                case "":
                    pcg = !pcg;
                    break;
                case "help":
                    Main.NewText("Usage: /pcg [on|off|toggle]");
                    return true;
                default:
                    Main.NewText("Usage: /pcg [on|off|toggle]");
                    return true;
            }

            Persist();
            if (!pcg)
            {
                Main.InGuideCraftMenu = false;
                Main.player[Main.myPlayer].SetTalkNPC(-1);
            }
            Main.NewText("Portable Crafting Guide " + (pcg ? "enabled" : "disabled") + ".");
            return true;
        }

        private void Persist()
        {
            IniAPI.WriteIni("PortableCraftingGuide", "Enabled", pcg.ToString());
        }
    }
}
