using System;
using System.Collections.Generic;
using System.Linq;
using PluginLoader;
using Terraria;
using Terraria.ID;

namespace TranscendPlugins
{
    [PluginDescription("Makes you permanently immune to the listed debuffs, and strips them if you already have them. " +
                       "Buffs may be listed by BuffID name or by number.")]
    public class BuffImmunity : PluginBase, IPluginPlayerUpdateBuffs
    {
        private static readonly Setting<string[]> Buffs = new[]
        {
            "PotionSickness", "ManaSickness", "Blackout", "Darkness", "Webbed"
        };

        private static List<int> immune = new List<int>();

        public BuffImmunity()
        {
            Resolve();
            Buffs.Changed += Resolve;
        }

        private static void Resolve()
        {
            var resolved = new List<int>();

            foreach (var buff in Buffs.Value.Select(buff => buff.Trim()).Where(buff => buff.Length > 0))
            {
                int buffId;
                if (!int.TryParse(buff, out buffId))
                {
                    var field = typeof(BuffID).GetFields().FirstOrDefault(info => string.Equals(info.Name, buff, StringComparison.OrdinalIgnoreCase));
                    if (field == null)
                    {
                        Main.NewText("Invalid BuffID (" + buff + ").");
                        continue;
                    }

                    buffId = Convert.ToInt32(field.GetValue(null));
                }

                resolved.Add(buffId);
            }

            immune = resolved;
        }

        public void OnPlayerUpdateBuffs(Player player)
        {
            foreach (var type in immune)
            {
                for (int j = 0; j < 22; j++)
                {
                    if (player.buffType[j] == type)
                        player.DelBuff(j);
                }
                player.buffImmune[type] = true;
            }
        }
    }
}
