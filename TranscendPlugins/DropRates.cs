using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;

namespace BlahPlugins
{
    public class DropRates : MarshalByRefObject, IPluginNPCLoot
    {
        private byte factor = 1;
        private Keys inc, dec;
        private bool recursionFlag = false;

        public DropRates()
        {
            if (!Keys.TryParse(IniAPI.ReadIni("DropRates", "IncKey", "P", writeIt: true), out inc)) inc = Keys.P;
            if (!Keys.TryParse(IniAPI.ReadIni("DropRates", "DecKey", "O", writeIt: true), out dec)) dec = Keys.O;
            factor = byte.Parse(IniAPI.ReadIni("DropRates", "Factor", "1", writeIt: true));

            Color yellow = Color.Yellow;
            Loader.RegisterHotkey(() =>
            {
                if (factor < 20)
                    factor++;
                IniAPI.WriteIni("DropRates", "Factor", factor.ToString());
                Main.NewText("Drop Rates multiplied by " + factor, yellow.R, yellow.G, yellow.B, false);
            }, inc);

            Loader.RegisterHotkey(() =>
            {
                if (factor > 0)
                    factor--;
                IniAPI.WriteIni("DropRates", "Factor", factor.ToString());
                Main.NewText("Drop Rates multiplied by " + factor, yellow.R, yellow.G, yellow.B, false);
            }, dec);
        }

        public bool OnNPCLoot(NPC npc)
        {
            if (recursionFlag) return false; // flag is set, avoid recursion

            recursionFlag = true;
            for (int i = 0; i < factor; i++)
                npc.NPCLoot();
            recursionFlag = false;

            return true;
        }
    }
}