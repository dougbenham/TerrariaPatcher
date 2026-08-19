using System.Collections.Generic;
using PluginLoader;
using Terraria;
using Terraria.ID;

namespace DoombubblesPlugins
{
    [PluginDescription("Whip self-buffs are no longer cancelled by using another whip, so the Spider, Snapthorn, Snowflake, " +
                       "Durendal and Dark Harvest buffs stack. Enemies still only carry one tag at a time.")]
    public class WhipBuffStacking : PluginBase, IPluginUpdate, IPluginPlayerPreUpdate
    {
        [SettingIds(typeof(BuffID))]
        private static readonly Setting<HashSet<int>> WorkingBuffs = new HashSet<int>
        {
            BuffID.SwordWhipPlayerBuff,
            BuffID.CoolWhipPlayerBuff,
            BuffID.ScytheWhipPlayerBuff,
            BuffID.ThornWhipPlayerBuff,
            BuffID.CobWhipPlayerBuff
        };

        private static readonly Dictionary<int, int> CurrentBuffs = new Dictionary<int, int>();

        public void OnPlayerPreUpdate(Player player)
        {
            if (Main.myPlayer != player.whoAmI) return;

            if (player.dead || player.mouseInterface) // Don't maintain while dead or hovering buff icons
            {
                CurrentBuffs.Clear();
                return;
            }

            foreach (var buffType in CurrentBuffs.Keys)
            {
                if (CurrentBuffs[buffType] > 1 && player.FindBuffIndex(buffType) == -1)
                {
                    player.AddBuff(buffType, CurrentBuffs[buffType] - 1);
                }
            }

            CurrentBuffs.Clear();

            for (var i = 0; i < player.buffType.Length; i++)
            {
                if (WorkingBuffs.Value.Contains(player.buffType[i]))
                {
                    CurrentBuffs[player.buffType[i]] = player.buffTime[i];
                }
            }
        }

        public void OnUpdate()
        {
            if (Main.gameMenu)
            {
                CurrentBuffs.Clear();
            }
        }
    }
}
