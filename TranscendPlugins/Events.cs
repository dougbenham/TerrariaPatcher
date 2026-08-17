using System.Reflection;
using PluginLoader;
using Terraria;
using Terraria.Chat;
using Terraria.GameContent.Achievements;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace TranscendPlugins
{
    [PluginDescription("Starts and stops world events from the number pad: meteors, blood moons, eclipses, the goblin, " +
                       "frost, pirate and martian invasions, the pumpkin and frost moons, the lunar apocalypse and the Moon Lord.")]
    public class Events : PluginBase
    {
        private static readonly HotkeySetting Meteor = new Hotkey
        {
            Key = Keys.NumPad0,
            Action = () =>
            {
                SpawnMeteor = false;
                dropMeteor.Invoke(null, null);
            }
        };

        private static readonly HotkeySetting BloodMoon = new Hotkey
        {
            Key = Keys.NumPad1,
            Action = () =>
            {
                if (Main.bloodMoon)
                    Main.bloodMoon = false;
                else
                    TriggerBloodMoon();
            }
        };

        private static readonly HotkeySetting GoblinArmy = new Hotkey { Key = Keys.NumPad2, Action = () => ToggleInvasion(1) };
        private static readonly HotkeySetting FrostLegion = new Hotkey { Key = Keys.NumPad3, Action = () => ToggleInvasion(2) };
        private static readonly HotkeySetting PirateInvasion = new Hotkey { Key = Keys.NumPad4, Action = () => ToggleInvasion(3) };

        private static readonly HotkeySetting SolarEclipse = new Hotkey
        {
            Key = Keys.NumPad5,
            Action = () =>
            {
                if (Main.eclipse)
                    Main.eclipse = false;
                else
                    TriggerEclipse();
            }
        };

        private static readonly HotkeySetting PumpkinMoon = new Hotkey
        {
            Key = Keys.NumPad6,
            Action = () =>
            {
                if (Main.pumpkinMoon)
                    Main.stopMoonEvent();
                else
                    Main.startPumpkinMoon();
            }
        };

        private static readonly HotkeySetting FrostMoon = new Hotkey
        {
            Key = Keys.NumPad7,
            Action = () =>
            {
                if (Main.snowMoon)
                    Main.stopMoonEvent();
                else
                    Main.startSnowMoon();
            }
        };

        private static readonly HotkeySetting MartianMadness = new Hotkey { Key = Keys.NumPad8, Action = () => ToggleInvasion(4) };

        private static readonly HotkeySetting LunarApocalypse = new Hotkey
        {
            Key = Keys.NumPad9,
            Action = () =>
            {
                if (Terraria.NPC.LunarApocalypseIsUp || Terraria.NPC.AnyNPCs(398))
                    StopLunarEvent();
                else
                    triggerLunarApocalypse.Invoke(null, null);
            }
        };

        private static readonly HotkeySetting MoonLord = new Hotkey
        {
            Key = Keys.Add,
            Action = () =>
            {
                if (Terraria.NPC.LunarApocalypseIsUp || Terraria.NPC.AnyNPCs(398))
                    StopLunarEvent();
                else
                    SpawnMoonLord();
            }
        };

        private static readonly MethodInfo triggerLunarApocalypse;
        private static readonly FieldInfo spawnMeteor;
        private static readonly MethodInfo dropMeteor;

        static Events()
        {
            var worldGen = Assembly.GetEntryAssembly().GetType("Terraria.WorldGen");
            triggerLunarApocalypse = worldGen.GetMethod("TriggerLunarApocalypse");
            spawnMeteor = worldGen.GetField("spawnMeteor");
            dropMeteor = worldGen.GetMethod("dropMeteor");
        }

        private static bool SpawnMeteor
        {
            get { return (bool) spawnMeteor.GetValue(null); }
            set { spawnMeteor.SetValue(null, value); }
        }

        private static void ToggleInvasion(int type)
        {
            if (Main.invasionType > 0)
                Main.invasionSize = 0;
            else
                Main.StartInvasion(type);
        }

        private static void TriggerEclipse()
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

        private static void TriggerBloodMoon()
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

        private static void SpawnMoonLord()
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

        private static void StopLunarEvent()
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
    }
}
