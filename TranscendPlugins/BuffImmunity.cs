using System;
using System.Collections.Generic;
using System.Linq;
using PluginLoader;
using Terraria;
using Terraria.ID;

namespace TranscendPlugins
{
    public class BuffImmunity : MarshalByRefObject, IPluginPlayerUpdateBuffs
    {
        private List<int> buffs;
 
        public BuffImmunity()
        {
            buffs = new List<int>();
            IniAPI.ReadIni("BuffImmunity", "Buffs", "Blackout, Darkness, Webbed", writeIt: true).Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList().ForEach(buff =>
            {
                buff = buff.Trim().ToLower();

                int buffId;
                if (!int.TryParse(buff, out buffId))
                {
                    var field = typeof(BuffID).GetFields().FirstOrDefault(info => info.Name.ToLower() == buff);
                    if (field == null)
                    {
                        Main.NewText("Invalid BuffID (" + buff + ").");
                        return;
                    }
                    buffId = Convert.ToInt32(field.GetValue(null));
                }

                buffs.Add(buffId);
            });
        }

        public void OnPlayerUpdateBuffs(Player player)
        {
            foreach (var type in buffs)
            {
                var index = player.HasBuff(type);
                if (index >= 0)
                    player.DelBuff(index);
                player.buffImmune[type] = true;
            }
        }
    }
}
