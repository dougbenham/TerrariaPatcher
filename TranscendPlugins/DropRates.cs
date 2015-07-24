using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;
using Terraria.ID;

namespace BlahPlugins
{
    public class DropRates : MarshalByRefObject, IPluginNPCLoot
    {
        private byte factor = 1;
        private bool rare = false;
        private Keys inc, dec, toggle;
        private bool recursionFlag = false;

        public DropRates()
        {
            if (!Keys.TryParse(IniAPI.ReadIni("DropRates", "IncKey", "P", writeIt: true), out inc)) inc = Keys.P;
            if (!Keys.TryParse(IniAPI.ReadIni("DropRates", "DecKey", "O", writeIt: true), out dec)) dec = Keys.O;
            if (!Keys.TryParse(IniAPI.ReadIni("DropRates", "RareToggle", "RightControl", writeIt: true), out toggle)) toggle = Keys.RightControl;
            if (!bool.TryParse(IniAPI.ReadIni("DropRates", "Rare", "False", writeIt: true), out rare)) rare = false;
            factor = byte.Parse(IniAPI.ReadIni("DropRates", "Factor", "1", writeIt: true));

            Color yellow = new Color(255, 235, 150);
            Loader.RegisterHotkey(() =>
            {
                if (factor < 20) factor++;
                IniAPI.WriteIni("DropRates", "Factor", factor.ToString());
                Main.NewText("Drop Rates multiplied by " + factor, yellow.R, yellow.G, yellow.B, false);
            }, inc);

            Loader.RegisterHotkey(() =>
            {
                if (factor > 1) factor--;
                IniAPI.WriteIni("DropRates", "Factor", factor.ToString());
                Main.NewText("Drop Rates multiplied by " + factor, yellow.R, yellow.G, yellow.B, false);
            }, dec);

            Loader.RegisterHotkey(() =>
            {
                rare = !rare;
                IniAPI.WriteIni("DropRates", "Rare", rare.ToString());
                Main.NewText("Rare Drops Only " + (rare ? "On" : "Off"), yellow.R, yellow.G, yellow.B, false);
            }, toggle);
        }
        public bool OnNPCLoot(NPC npc)
        {
            if (!rare)
            {
                if (recursionFlag) return false; // flag is set, avoid recursion

                recursionFlag = true;
                for (int i = 0; i < factor; i++)
                    npc.NPCLoot();
                recursionFlag = false;

                return true;
            }
            else
            {
                if (factor > 1)
                {
                    int hundred = 100 / factor;
                    int oneFifty = 150 / factor;
                    int oneSeventyFive = 175 / factor;
                    int twoHundred = 200 / factor;
                    int twoFifty = 250 / factor;
                    int threeHundred = 300 / factor;
                    int fourHundred = 400 / factor;
                    int fiveHundred = 500 / factor;
                    int thousand = 1000 / factor;
                    int twoThousand = 2000 / factor;
                    int twentyFiveHundred = 2500 / factor;
                    int fourThousand = 4000 / factor;
                    int eightThousand = 8000 / factor;
                    if (Main.hardMode && npc.value > 0)
                    {
                        if (Main.rand.Next(twentyFiveHundred) == 0 && Main.player[(int)Player.FindClosest(npc.position, npc.width, npc.height)].ZoneJungle)
                        {
                            Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ItemID.JungleKey, 1, false, 0, false);
                        }
                        if (Main.rand.Next(twentyFiveHundred) == 0 && Main.player[(int)Player.FindClosest(npc.position, npc.width, npc.height)].ZoneCorrupt)
                        {
                            Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ItemID.CorruptionKey, 1, false, 0, false);
                        }
                        if (Main.rand.Next(twentyFiveHundred) == 0 && Main.player[(int)Player.FindClosest(npc.position, npc.width, npc.height)].ZoneCrimson)
                        {
                            Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ItemID.CrimsonKey, 1, false, 0, false);
                        }
                        if (Main.rand.Next(twentyFiveHundred) == 0 && Main.player[(int)Player.FindClosest(npc.position, npc.width, npc.height)].ZoneHoly)
                        {
                            Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ItemID.HallowedKey, 1, false, 0, false);
                        }
                        if (Main.rand.Next(twentyFiveHundred) == 0 && Main.player[(int)Player.FindClosest(npc.position, npc.width, npc.height)].ZoneSnow)
                        {
                            Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ItemID.FrozenKey, 1, false, 0, false);
                        }
                    }
                    if (npc.type >= 212 && npc.type <= 215)
                    {
                        if (Main.rand.Next(eightThousand) == 0)
                        {
                            Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ItemID.CoinGun, 1, false, -1, false);
                        }
                        if (Main.rand.Next(fourThousand) == 0)
                        {
                            Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ItemID.LuckyCoin, 1, false, -1, false);
                        }
                        if (Main.rand.Next(twoThousand) == 0)
                        {
                            Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ItemID.DiscountCard, 1, false, -1, false);
                        }
                        if (Main.rand.Next(twoThousand) == 0)
                        {
                            Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ItemID.PirateStaff, 1, false, -1, false);
                        }
                        if (Main.rand.Next(thousand) == 0)
                        {
                            Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ItemID.GoldRing, 1, false, -1, false);
                        }
                    }
                    if (npc.type == 216)
                    {
                        if (Main.rand.Next(twoThousand) == 0)
                        {
                            Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ItemID.CoinGun, 1, false, -1, false);
                        }
                        if (Main.rand.Next(thousand) == 0)
                        {
                            Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ItemID.LuckyCoin, 1, false, -1, false);
                        }
                        if (Main.rand.Next(fiveHundred) == 0)
                        {
                            Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ItemID.DiscountCard, 1, false, -1, false);
                        }
                        if (Main.rand.Next(fiveHundred) == 0)
                        {
                            Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ItemID.PirateStaff, 1, false, -1, false);
                        }
                        if (Main.rand.Next(twoFifty) == 0)
                        {
                            Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ItemID.GoldRing, 1, false, -1, false);
                        }
                    }
                    if (npc.type == 110 && Main.rand.Next(twoHundred) == 0)
                    {
                        Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ItemID.Marrow, 1, false, -1, false);
                    }
                    if (npc.type == 154 && Main.rand.Next(hundred) == 0)
                    {
                        Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ItemID.FrozenTurtleShell, 1, false, -1, false);
                    }
                    if (npc.type == 198 || npc.type == 199 || npc.type == 226)
                    {
                        if (Main.rand.Next(thousand) == 0)
                        {
                            Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ItemID.LizardEgg, 1, false, 0, false);
                        }
                    }
                    if (npc.type == 120 && Main.rand.Next(fiveHundred) == 0)
                    {
                        Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ItemID.RodofDiscord, 1, false, 0, false);
                    }
                    if (npc.type == 49 && Main.rand.Next(twoFifty) == 0)
                    {
                        Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ItemID.ChainKnife, 1, false, -1, false);
                    }
                    if (npc.type == 185 && Main.rand.Next(oneFifty) == 0)
                    {
                        Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ItemID.SnowballLauncher, 1, false, 0, false);
                    }
                    if (npc.type >= 269 && npc.type <= 280)
                    {
                        if (Main.rand.Next(fourHundred) == 0)
                        {
                            Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ItemID.WispinaBottle, 1, false, 0, false);
                        }
                        else if (Main.rand.Next(threeHundred) == 0)
                        {
                            Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ItemID.MagnetSphere, 1, false, -1, false);
                        }
                        else if (Main.rand.Next(twoHundred) == 0)
                        {
                            Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ItemID.Keybrand, 1, false, -1, false);
                        }
                    }
                    if (Main.bloodMoon && Main.hardMode && Main.rand.Next(thousand) == 0 && npc.value > 0f)
                    {
                        Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ItemID.KOCannon, 1, false, -1, false);
                    }
                    if (npc.type == 21 || npc.type == 201 || npc.type == 202 || npc.type == 203 || npc.type == 322 || npc.type == 323 || npc.type == 324 || (npc.type >= 449 && npc.type <= 452))
                    {
                        if (Main.rand.Next(fiveHundred) == 0)
                        {
                            Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ItemID.Skull, 1, false, 0, false);
                        }
                    }
                    else if (npc.type == 6)
                    {
                        if (Main.rand.Next(oneSeventyFive) == 0)
                        {
                            int num34 = Main.rand.Next(3);
                            if (num34 == 0)
                            {
                                Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ItemID.AncientShadowHelmet, 1, false, 0, false);
                            }
                            else if (num34 == 1)
                            {
                                Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ItemID.AncientShadowScalemail, 1, false, 0, false);
                            }
                            else
                            {
                                Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ItemID.AncientShadowGreaves, 1, false, 0, false);
                            }
                        }
                    }
                }
                return false;
            }
        }
    }
}