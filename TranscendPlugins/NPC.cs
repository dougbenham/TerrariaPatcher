using System;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using PluginLoader;
using Terraria;
using Terraria.ID;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace TranscendPlugins
{
    [PluginDescription("Controls monster spawning and spawns them on demand. SpawnLimit is how many can be alive at once " +
                       "and SpawnRate is a percentage of the vanilla rate; hotkeys adjust both, and Toggle stops spawning " +
                       "entirely and clears what is already out. /npc spawns any NPC by id or name.")]
    public class NPC : PluginBase, IPluginChatCommand
    {
        private static readonly Setting<int> SpawnLimit = 5;
        private static readonly Setting<int> SpawnRate = 100;

        private static readonly HotkeySetting Toggle = new Hotkey { Key = Keys.N, Action = ToggleSpawning };
        private static readonly HotkeySetting Increase = new Hotkey { Key = Keys.OemPlus, Action = () => ModifySpawnRate(20) };
        private static readonly HotkeySetting IncreaseLimit = new Hotkey { Key = Keys.OemPlus, Control = true, Action = () => ModifySpawnLimit(1) };
        private static readonly HotkeySetting Decrease = new Hotkey { Key = Keys.OemMinus, Action = () => ModifySpawnRate(-20) };
        private static readonly HotkeySetting DecreaseLimit = new Hotkey { Key = Keys.OemMinus, Control = true, Action = () => ModifySpawnLimit(-1) };

        private static readonly FieldInfo defaultMaxSpawns;
        private static readonly FieldInfo defaultSpawnRate;

        private static int previousSpawnLimit = 5;
        private static int previousSpawnRate = 100;

        private bool cnpc;

        static NPC()
        {
            var npc = Assembly.GetEntryAssembly().GetType("Terraria.NPC");
            defaultMaxSpawns = npc.GetField("defaultMaxSpawns", BindingFlags.Static | BindingFlags.NonPublic);
            defaultSpawnRate = npc.GetField("defaultSpawnRate", BindingFlags.Static | BindingFlags.NonPublic);
        }

        public NPC()
        {
            Apply();

            SpawnLimit.Changed += Apply;
            SpawnRate.Changed += Apply;
        }

        /// <summary>
        /// Pushes the settings into Terraria's own spawn fields, which hold a delay rather than a percentage.
        /// </summary>
        private static void Apply()
        {
            defaultMaxSpawns.SetValue(null, SpawnLimit.Value);
            defaultSpawnRate.SetValue(null, SpawnRate.Value == 0 ? int.MaxValue : 60000 / SpawnRate.Value);
        }

        private static void ToggleSpawning()
        {
            if (SpawnLimit.Value > 0)
            {
                previousSpawnLimit = SpawnLimit.Value;
                previousSpawnRate = SpawnRate.Value;
                SpawnLimit.Value = 0;
                SpawnRate.Value = 0;
                KillAllNPCs();
            }
            else
            {
                SpawnLimit.Value = previousSpawnLimit;
                SpawnRate.Value = previousSpawnRate;
            }

            Announce();
        }

        private static void ModifySpawnRate(int rate)
        {
            SpawnRate.Value = Clamp(SpawnRate.Value + rate, 0, 1000);
            Announce();
        }

        private static void ModifySpawnLimit(int limit)
        {
            SpawnLimit.Value = Clamp(SpawnLimit.Value + limit, 0, 150);

            if (SpawnLimit.Value == 0)
                KillAllNPCs();

            Announce();
        }

        private static int Clamp(int value, int min, int max)
        {
            return value < min ? min : (value > max ? max : value);
        }

        private static void Announce()
        {
            var purple = Color.Purple;
            Main.NewText("Spawn rate: " + SpawnRate.Value + "%", purple.R, purple.G, purple.B);
            Main.NewText("Spawn limit: " + SpawnLimit.Value, purple.R, purple.G, purple.B);
        }

        private static void KillAllNPCs()
        {
            for (int i = 0; i < Main.npc.Length; i++)
            {
                var npc = Main.npc[i];
                if (npc != null && !npc.townNPC)
                {
                    npc.life = 0;
                    npc.checkDead();
                    if (Main.netMode == 2)
                        NetMessage.SendData(23, -1, -1, null, i);
                }
            }
        }

        public bool OnChatCommand(string command, string[] args)
        {
            if (command != "npc") return false;

            if (args.Length < 1 || args.Length > 2 || args[0] == "help")
            {
                Main.NewText("Usage:");
                Main.NewText("  /npc id [count]");
                Main.NewText("  /npc name [count]");
                Main.NewText("  /npc cnpc (Toggles NPC spawn at cursor position)");
                Main.NewText("  /npc help");
                Main.NewText("Example:");
                Main.NewText("  /npc 21");
                Main.NewText("  /npc 21 20");
                Main.NewText("  /npc Skeleton 20");
                return true;
            }

            if (args[0] == "cnpc")
            {
                cnpc = !cnpc;
                Main.NewText("NPC spawn at cursor " + (cnpc ? "enabled" : "disabled"));
                return true;
            }

            int npcId;
            if (!int.TryParse(args[0], out npcId))
            {
                var field = typeof(NPCID).GetFields().FirstOrDefault(info => info.Name.ToLower() == args[0].ToLower());
                if (field != null)
                    npcId = Convert.ToInt32(field.GetValue(null));
            }
            if (npcId == 0)
            {
                Main.NewText("Invalid NPCID.");
                return true;
            }

            int count = 1;
            if (args.Length == 2)
            {
                if (!int.TryParse(args[1], out count))
                {
                    Main.NewText("Invalid count.");
                    return true;
                }
            }

            int x, y;
            if (cnpc)
            {
                x = (int)(Main.mouseX + Main.screenPosition.X);
                y = (int)(Main.mouseY + Main.screenPosition.Y);
            }
            else
            {
                x = (int)Player.Center.X;
                y = (int)Player.Center.Y - 150;
            }
            for (int i = 0; i < count; i++)
            {
                Terraria.NPC.NewNPC(Terraria.NPC.GetSpawnSourceForNaturalSpawn(), x, y, npcId);
            }
            return true;
        }
    }
}
