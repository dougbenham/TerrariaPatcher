using PluginLoader;
using Terraria;
using Terraria.ID;

namespace TranscendPlugins
{
    [PluginDescription("Makes you permanently immune to the listed debuffs, and strips them if you already have them.")]
    public class BuffImmunity : PluginBase, IPluginPlayerUpdateBuffs
    {
        [SettingIds(typeof(BuffID))]
        [SettingDescription("The debuffs you are permanently immune to.")]
        private static readonly Setting<int[]> Buffs = new[]
        {
            BuffID.PotionSickness, BuffID.ManaSickness, BuffID.Blackout, BuffID.Darkness, BuffID.Webbed
        };
		
        public void OnPlayerUpdateBuffs(Player player)
        {
            foreach (var type in Buffs.Value)
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
