using System;
using System.Reflection;
using PluginLoader;
using Terraria;
using Terraria.Chat;
using Terraria.GameContent.Achievements;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace TranscendPlugins
{
    public class Events : MarshalByRefObject, IPlugin, IPluginChatCommand
    {
        private Keys bloodMoon, goblin, meteor, frost, pirates, martians, pumpkinMoon, frostMoon, lunarApocalypse, eclipse, moonLord;
        private MethodInfo triggerLunarApocalypse;
        private FieldInfo spawnMeteor;
        private MethodInfo dropMeteor;
        private bool SpawnMeteor
        {
            get { return (bool)spawnMeteor.GetValue(null); }
            set { spawnMeteor.SetValue(null, value); }
        }
        
        public Events()
        {
            var worldGen = Assembly.GetEntryAssembly().GetType("Terraria.WorldGen");
            triggerLunarApocalypse = worldGen.GetMethod("TriggerLunarApocalypse");
            spawnMeteor = worldGen.GetField("spawnMeteor");
            dropMeteor = worldGen.GetMethod("dropMeteor");

            if (!Keys.TryParse(IniAPI.ReadIni("Events", "Meteor", "NumPad0", writeIt: true), out meteor))
                meteor = Keys.NumPad0;
            if (!Keys.TryParse(IniAPI.ReadIni("Events", "BloodMoon", "NumPad1", writeIt: true), out bloodMoon))
                bloodMoon = Keys.NumPad1;
            if (!Keys.TryParse(IniAPI.ReadIni("Events", "GoblinArmy", "NumPad2", writeIt: true), out goblin))
                goblin = Keys.NumPad2;
            if (!Keys.TryParse(IniAPI.ReadIni("Events", "FrostLegion", "NumPad3", writeIt: true), out frost))
                frost = Keys.NumPad3;
            if (!Keys.TryParse(IniAPI.ReadIni("Events", "PirateInvasion", "NumPad4", writeIt: true), out pirates))
                pirates = Keys.NumPad4;
            if (!Keys.TryParse(IniAPI.ReadIni("Events", "SolarEclipse", "NumPad5", writeIt: true), out eclipse))
                eclipse = Keys.NumPad5;
            if (!Keys.TryParse(IniAPI.ReadIni("Events", "PumpkinMoon", "NumPad6", writeIt: true), out pumpkinMoon))
                pumpkinMoon = Keys.NumPad6;
            if (!Keys.TryParse(IniAPI.ReadIni("Events", "FrostMoon", "NumPad7", writeIt: true), out frostMoon))
                frostMoon = Keys.NumPad7;
            if (!Keys.TryParse(IniAPI.ReadIni("Events", "MartianMadness", "NumPad8", writeIt: true), out martians))
                martians = Keys.NumPad8;
            if (!Keys.TryParse(IniAPI.ReadIni("Events", "LunarApocalypse", "NumPad9", writeIt: true), out lunarApocalypse))
                lunarApocalypse = Keys.NumPad9;
            if (!Keys.TryParse(IniAPI.ReadIni("Events", "Moon Lord", "Add", writeIt: true), out moonLord))
                moonLord = Keys.Add;

            Loader.RegisterHotkey(() =>
            {
                if (Main.invasionType > 0)
                    Main.invasionSize = 0;
                else
                    Main.StartInvasion(1);
            }, goblin);
            Loader.RegisterHotkey(() =>
            {
                if (Main.invasionType > 0)
                    Main.invasionSize = 0;
                else
                    Main.StartInvasion(2);
            }, frost);
            Loader.RegisterHotkey(() =>
            {
                if (Main.invasionType > 0)
                    Main.invasionSize = 0;
                else
                    Main.StartInvasion(3);
            }, pirates);
            Loader.RegisterHotkey(() =>
            {
                if (Main.invasionType > 0)
                    Main.invasionSize = 0;
                else
                    Main.StartInvasion(4);
            }, martians);
            Loader.RegisterHotkey(() =>
            {
                if (Main.pumpkinMoon)
                    Main.stopMoonEvent();
                else
                    Main.startPumpkinMoon();
            }, pumpkinMoon);
            Loader.RegisterHotkey(() =>
            {
                if (Main.snowMoon)
                    Main.stopMoonEvent();
                else
                    Main.startSnowMoon();
            }, frostMoon);
            Loader.RegisterHotkey(() =>
            {
                if (Terraria.NPC.LunarApocalypseIsUp || Terraria.NPC.AnyNPCs(398))
                    StopLunarEvent();
                else
                    TriggerLunarApocalypse();
            }, lunarApocalypse);
            Loader.RegisterHotkey(() =>
            {
                if (Terraria.NPC.LunarApocalypseIsUp || Terraria.NPC.AnyNPCs(398))
                    StopLunarEvent();
                else
                    SpawnMoonLord();
            }, moonLord);
            Loader.RegisterHotkey(() =>
            {
                if (Main.bloodMoon)
                    Main.bloodMoon = false;
                else
                    TriggerBloodMoon();
            }, bloodMoon);
            Loader.RegisterHotkey(() =>
            {
                if (Main.eclipse)
                    Main.eclipse = false;
                else
                    TriggerEclipse();
            }, eclipse);
            Loader.RegisterHotkey(() =>
            {
                SpawnMeteor = false;
                DropMeteor();
            }, meteor);
        }
        
        private void DropMeteor()
        {
            dropMeteor.Invoke(null, null);
        }

        private void TriggerLunarApocalypse()
        {
            triggerLunarApocalypse.Invoke(null, null);
        }

        private void TriggerEclipse()
        {
            if (Main.netMode == 0)
            {
                Main.eclipse = true;
                Main.NewText(Lang.misc[20].Value, 50, 255, 130);
            }
            else
            {
                NetMessage.SendData(61, -1, -1, null, Main.myPlayer, -6f, 0f, 0f, 0, 0, 0);
            }
        }

        private void TriggerBloodMoon()
        {
            Main.bloodMoon = true;
            AchievementsHelper.NotifyProgressionEvent(4);
            if (Main.netMode == 0)
            {
                Main.NewText(Lang.misc[8].Value, 50, byte.MaxValue, 130);
            }
            else if (Main.netMode == 2)
            {
                ChatHelper.BroadcastChatMessage(Lang.misc[8].ToNetworkText(), new Microsoft.Xna.Framework.Color(50, 255, 130), -1);
            }
        }

        private void SpawnMoonLord()
        {
            if (Main.netMode == 0)
            {
                WorldGen.StartImpendingDoom(720);
            }
            else
            {
                NetMessage.SendData(61, -1, -1, null, Main.myPlayer, -8f, 0f, 0f, 0, 0, 0);
            }
        }

        private void StopLunarEvent()
        {
            Main.NewText("Stopped lunar event!", 50, 255, 130);
            Terraria.NPC.LunarApocalypseIsUp = false;
            for (int i = 0; i < 200; i++)
            {
                if (Main.npc[i].active)
                {
                    switch (Main.npc[i].type)
                    {
                        case 398: // Moon Lord
                        case 517: // Tower
                        case 422: // Tower
                        case 507: // Tower
                        case 493: // Tower
                            Main.npc[i].life = 0;
                            break;
                    }
                }
            }
        }

        public bool OnChatCommand(string command, string[] args)
        {
            if (command != "event" && command != "events") return false;

            Action usage = () =>
            {
                Main.NewText("Usage:");
                Main.NewText("  /event goblin|frostlegion|pirates|martians");
                Main.NewText("  /event pumpkin|frostmoon|bloodmoon|eclipse|meteor");
                Main.NewText("  /event lunar|moonlord|stoplunar");
            };

            if (args.Length == 0 || args[0] == "help")
            {
                usage();
                return true;
            }

            switch (args[0].ToLower())
            {
                case "goblin":
                    ToggleInvasion(1, "Goblin Army");
                    return true;
                case "frostlegion":
                    ToggleInvasion(2, "Frost Legion");
                    return true;
                case "pirates":
                    ToggleInvasion(3, "Pirate Invasion");
                    return true;
                case "martians":
                    ToggleInvasion(4, "Martian Madness");
                    return true;
                case "pumpkin":
                    TogglePumpkinMoon();
                    return true;
                case "frostmoon":
                    ToggleFrostMoon();
                    return true;
                case "bloodmoon":
                    ToggleBloodMoon();
                    return true;
                case "eclipse":
                    ToggleEclipse();
                    return true;
                case "meteor":
                    ForceMeteor();
                    return true;
                case "lunar":
                    ToggleLunarApocalypse();
                    return true;
                case "moonlord":
                    ToggleMoonLord();
                    return true;
                case "stoplunar":
                    StopLunarEvent();
                    return true;
                default:
                    usage();
                    return true;
            }
        }

        private void ToggleInvasion(int invasionId, string label)
        {
            if (Main.invasionType > 0)
            {
                Main.invasionSize = 0;
                Main.NewText(label + " stopped.");
            }
            else
            {
                Main.StartInvasion(invasionId);
                Main.NewText(label + " started.");
            }
        }

        private void TogglePumpkinMoon()
        {
            if (Main.pumpkinMoon)
                Main.stopMoonEvent();
            else
                Main.startPumpkinMoon();
        }

        private void ToggleFrostMoon()
        {
            if (Main.snowMoon)
                Main.stopMoonEvent();
            else
                Main.startSnowMoon();
        }

        private void ToggleLunarApocalypse()
        {
            if (Terraria.NPC.LunarApocalypseIsUp || Terraria.NPC.AnyNPCs(398))
                StopLunarEvent();
            else
                TriggerLunarApocalypse();
        }

        private void ToggleMoonLord()
        {
            if (Terraria.NPC.LunarApocalypseIsUp || Terraria.NPC.AnyNPCs(398))
                StopLunarEvent();
            else
                SpawnMoonLord();
        }

        private void ToggleBloodMoon()
        {
            if (Main.bloodMoon)
                Main.bloodMoon = false;
            else
                TriggerBloodMoon();
        }

        private void ToggleEclipse()
        {
            if (Main.eclipse)
                Main.eclipse = false;
            else
                TriggerEclipse();
        }

        private void ForceMeteor()
        {
            SpawnMeteor = false;
            DropMeteor();
            Main.NewText("Meteor forced.");
        }
    }
}
