using System.Reflection;
using PluginLoader;
using Terraria;
using Terraria.GameContent.Achievements;
using Terraria.GameContent.Events;
using Terraria.ID;
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
                dropMeteor.Invoke(null, new object[] { false });
            }
        };
        
        private static readonly HotkeySetting BloodMoon = new Hotkey { Key = Keys.NumPad1, Action = ToggleBloodMoon };
        private static readonly HotkeySetting GoblinArmy = new Hotkey { Key = Keys.NumPad2, Action = () => ToggleInvasion(InvasionID.GoblinArmy) };
        private static readonly HotkeySetting FrostLegion = new Hotkey { Key = Keys.NumPad3, Action = () => ToggleInvasion(InvasionID.SnowLegion) };
        private static readonly HotkeySetting PirateInvasion = new Hotkey { Key = Keys.NumPad4, Action = () => ToggleInvasion(InvasionID.PirateInvasion) };
		private static readonly HotkeySetting SolarEclipse = new Hotkey { Key = Keys.NumPad5, Action = ToggleSolarEclipse };
		private static readonly HotkeySetting PumpkinMoon = new Hotkey { Key = Keys.NumPad6, Action = TogglePumpkinMoon };
		private static readonly HotkeySetting FrostMoon = new Hotkey { Key = Keys.NumPad7, Action = ToggleFrostMoon };
		private static readonly HotkeySetting MartianMadness = new Hotkey { Key = Keys.NumPad8, Action = () => ToggleInvasion(InvasionID.MartianMadness) };
		private static readonly HotkeySetting LunarApocalypse = new Hotkey { Key = Keys.NumPad9, Action = ToggleLunarApocalypse };
		private static readonly HotkeySetting MoonLord = new Hotkey { Key = Keys.Add, Action = ToggleMoonLord };

        private static readonly FieldInfo spawnMeteor;
        private static readonly MethodInfo dropMeteor;

        static Events()
        {
            var worldGen = Assembly.GetEntryAssembly().GetType("Terraria.WorldGen");
            spawnMeteor = worldGen.GetField("spawnMeteor");
            dropMeteor = worldGen.GetMethod("dropMeteor");
        }

        private static bool SpawnMeteor
        {
            get { return (bool) spawnMeteor.GetValue(null); }
            set { spawnMeteor.SetValue(null, value); }
        }

        /// <summary>
        /// The world event ids a client can ask the server to start through message 61. The server applies these
        /// itself and broadcasts the result, which is the only way a client can change world state in multiplayer.
        /// </summary>
        private const int RequestPumpkinMoon = -4, RequestFrostMoon = -5, RequestEclipse = -6, RequestMartianMadness = -7, RequestMoonLord = -8, RequestBloodMoon = -10;

        private static void RequestFromServer(int what)
        {
	        NetMessage.SendData(61, -1, -1, null, Main.myPlayer, what, 0f, 0f, 0, 0, 0);
        }

        private static void RequestFromServer(int what, string refusal)
        {
	        if (refusal != null)
	        {
		        Main.NewText(refusal, 255, 50, 50);
		        return;
	        }

	        RequestFromServer(what);
        }

        private static void ToggleInvasion(int type)
        {
	        if (Main.invasionType > 0)
            {
	            if (Main.netMode == 0)
		            Main.invasionSize = 0;
	            else
		            Main.NewText("Stopping invasions is only handled on the server.", 50, 255, 130);
	            return;
            }

	        if (Main.netMode == 0)
		        Main.StartInvasion(type);
	        else
	        {
		        // The server takes an invasion request as the negated invasion id, except for Martian Madness: -4 is
		        // already the Pumpkin Moon, so that one has its own id.
		        RequestFromServer(type == InvasionID.MartianMadness ? RequestMartianMadness : -type);
	        }
        }

        private static void ToggleBloodMoon()
        {
	        if (Main.bloodMoon)
	        {
		        if (Main.netMode == 0)
			        Main.bloodMoon = false;
		        else
			        Main.NewText("Stopping blood moon is only handled on the server.", 50, 255, 130);
		        return;
	        }

	        if (Main.netMode == 0)
	        {
		        Main.bloodMoon = true;
		        AchievementsHelper.NotifyProgressionEvent(4);
		        Main.NewText(Lang.misc[8].Value, 50, byte.MaxValue, 130);
	        }
	        else
	        {
		        RequestFromServer(RequestBloodMoon,
			        Main.dayTime ? "The server only starts a blood moon at night." : null);
	        }
        }

        private static void ToggleSolarEclipse()
        {
	        if (Main.eclipse)
	        {
		        if (Main.netMode == 0)
			        Main.eclipse = false;
		        else
			        Main.NewText("Stopping solar eclipse is only handled on the server.", 50, 255, 130);
		        return;
	        }

	        if (Main.netMode == 0)
	        {
		        Main.eclipse = true;
		        Main.NewText(Lang.misc[20].Value, 50, 255, 130);
	        }
	        else
	        {
		        RequestFromServer(RequestEclipse,
			        Main.dayTime ? null : "The server only starts a solar eclipse during the day.");
	        }
        }

        private static void TogglePumpkinMoon()
        {
	        if (Main.pumpkinMoon)
	        {
		        if (Main.netMode == 0)
			        Main.stopMoonEvent();
		        else
			        Main.NewText("Stopping pumpkin moon is only handled on the server.", 50, 255, 130);
		        return;
	        }

	        if (Main.netMode == 0)
		        Main.startPumpkinMoon();
	        else
	        {
		        RequestFromServer(RequestPumpkinMoon,
			        Main.dayTime ? "The server only starts a pumpkin moon at night."
			        : DD2Event.Ongoing ? "The server will not start a pumpkin moon during the Old One's Army."
			        : null);
	        }
        }

        private static void ToggleFrostMoon()
        {
	        if (Main.snowMoon)
	        {
		        if (Main.netMode == 0)
			        Main.stopMoonEvent();
		        else
			        Main.NewText("Stopping frost moon is only handled on the server.", 50, 255, 130);
		        return;
	        }

	        if (Main.netMode == 0)
		        Main.startSnowMoon();
	        else
	        {
		        RequestFromServer(RequestFrostMoon,
			        Main.dayTime ? "The server only starts a frost moon at night."
			        : DD2Event.Ongoing ? "The server will not start a frost moon during the Old One's Army."
			        : null);
	        }
        }

        private static void ToggleLunarApocalypse()
        {
	        if (Terraria.NPC.LunarApocalypseIsUp || Terraria.NPC.AnyNPCs(398))
	        {
		        if (Main.netMode == 0)
		            StopLunarEvent();
		        else
			        Main.NewText("Stopping lunar apocalypse is only handled on the server.", 50, 255, 130);
	        }
	        else
	        {
		        if (Main.netMode == 0)
		            WorldGen.TriggerLunarApocalypse();
		        else
			        Main.NewText("Starting lunar apocalypse is only handled on the server.", 50, 255, 130);
	        }
        }

        private static void ToggleMoonLord()
        {
	        if (Terraria.NPC.LunarApocalypseIsUp || Terraria.NPC.AnyNPCs(398))
	        {
		        if (Main.netMode == 0)
		            StopLunarEvent();
		        else
			        Main.NewText("Stopping lunar apocalypse is only handled on the server.", 50, 255, 130);
	        }
	        else
	        {
		        if (Main.netMode == 0)
		        {
			        WorldGen.StartImpendingDoom(720);
		        }
		        else
		        {
			        RequestFromServer(RequestMoonLord,
				        !Main.hardMode ? "The server will not start the Moon Lord outside hardmode."
				        : !Terraria.NPC.downedGolemBoss ? "The server will not start the Moon Lord until Golem is defeated."
				        : null);
		        }
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
