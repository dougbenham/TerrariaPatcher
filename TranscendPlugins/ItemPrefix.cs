using System;
using System.Reflection;
using PluginLoader;
using Terraria;
using Terraria.ID;
using Terraria.Utilities;

namespace TranscendPlugins
{
    public class ItemPrefix : MarshalByRefObject, IPluginChatCommand, IPluginItemRollAPrefix
    {
        private bool keepStats = false;
        private bool enableFixedPrefixes;
        private int fixedAccessoryPrefix;
        
        public ItemPrefix()
        {
	        if (!bool.TryParse(IniAPI.ReadIni("ItemPrefix", "EnableFixedPrefixes", "True", writeIt: true), out enableFixedPrefixes))
		        enableFixedPrefixes = true;
	        var temp = IniAPI.ReadIni("ItemPrefix", "FixedAccessoryPrefix", "Warding", writeIt: true);
	        if (!int.TryParse(temp, out fixedAccessoryPrefix))
	        {
		        var field = typeof(PrefixID).GetField(temp, BindingFlags.Static | BindingFlags.Public);
				var fieldValue = field == null ? null : field.GetValue(null) as int?;
		        if (!fieldValue.HasValue)
		        {
			        MessageBox.Show(string.Format("[ItemPrefix] FixedAccessoryPrefix of '{0}' is invalid. Use a number or a valid prefix name.", temp), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
			        fixedAccessoryPrefix = PrefixID.Warding;
		        }
				else
					fixedAccessoryPrefix = fieldValue.Value;
	        }
        }
		
        private bool Correct(Item item, ref int rolledPrefix)
        {
	        float num = 1f;
	        float num2 = 1f;
	        float num3 = 1f;
	        float num4 = 1f;
	        float num5 = 1f;
	        float num6 = 1f;
	        int num7 = 0;
	        int num8 = 0;
	        int num9 = 0;
	        float num10 = 0;
	        if (!item.TryGetPrefixStatMultipliersForItem(rolledPrefix, out num, out num2, out num3, out num4, out num5, out num6, out num7, out num8, out num9, out num10))
            {
		        if (item.knockBack == 0)
			        rolledPrefix = PrefixID.Demonic;
		        else if (item.damage == 0)
			        rolledPrefix = PrefixID.Rapid;
		        else if (item.useAnimation == 0)
			        rolledPrefix = PrefixID.Godly;
		        else if (item.mana == 0)
			        rolledPrefix = PrefixID.Godly;
		        else
			        return false;
	        }
	        return true;
        }

        public bool OnItemRollAPrefix(Item item, UnifiedRandom random, ref int rolledPrefix, out bool result)
        {
			result = false;
			if (!enableFixedPrefixes)
				return false;

	        if (Terraria.GameContent.Prefixes.PrefixLegacy.ItemSets.SwordsHammersAxesPicks[item.type])
	        {
		        rolledPrefix = PrefixID.Legendary;
		        if (!Correct(item, ref rolledPrefix)) return false;
		        result = true;
		        return true;
	        }
	        if (Terraria.GameContent.Prefixes.PrefixLegacy.ItemSets.SpearsMacesChainsawsDrillsPunchCannon[item.type])
	        {
		        rolledPrefix = PrefixID.Godly;
		        if (!Correct(item, ref rolledPrefix)) return false;
		        result = true;
		        return true;
	        }
	        if (Terraria.GameContent.Prefixes.PrefixLegacy.ItemSets.GunsBows[item.type])
	        {
		        rolledPrefix = PrefixID.Unreal;
		        if (!Correct(item, ref rolledPrefix)) return false;
		        result = true;
		        return true;
	        }
	        if (Terraria.GameContent.Prefixes.PrefixLegacy.ItemSets.Magic[item.type])
	        {
		        rolledPrefix = PrefixID.Mythical;
		        if (!Correct(item, ref rolledPrefix)) return false;
		        result = true;
		        return true;
	        }
	        if (Terraria.GameContent.Prefixes.PrefixLegacy.ItemSets.Summon[item.type])
	        {
		        rolledPrefix = PrefixID.Ruthless;
		        if (!Correct(item, ref rolledPrefix)) return false;
		        result = true;
		        return true;
	        }
            if (Terraria.GameContent.Prefixes.PrefixLegacy.ItemSets.BoomerangsChakrams[item.type])
	        {
		        rolledPrefix = PrefixID.Godly;
		        if (!Correct(item, ref rolledPrefix)) return false;
		        result = true;
		        return true;
	        }
	        if (Terraria.GameContent.Prefixes.PrefixLegacy.ItemSets.ItemsThatCanHaveLegendary2[item.type])
	        {
		        rolledPrefix = PrefixID.Legendary2;
		        if (!Correct(item, ref rolledPrefix)) return false;
		        result = true;
		        return true;
	        }
	        if (item.IsAPrefixableAccessory())
	        {
		        rolledPrefix = fixedAccessoryPrefix;
		        result = true;
		        return true;
	        }
			
	        return false;
        }

        public bool OnChatCommand(string command, string[] args)
        {
            if (command != "prefix") return false;

            if (args.Length < 1 || args.Length > 1 || args[0] == "help")
            {
                Main.NewText("Usage:");
                Main.NewText("   /prefix name");
                Main.NewText("   /prefix id");
                Main.NewText("   /prefix keep");
                Main.NewText("   /prefix help");
                Main.NewText("Example:");
                Main.NewText("   /prefix mythical");
                return true;
            }

            if (args[0] == "keep")
            {
                keepStats = !keepStats;
                Main.NewText("Using /prefix will now " + (keepStats ? "keep" : "reset") + " existing stats.");
                return true;
            }

            // get item on cursor, if nothing there, get hotbar item
            var item = Main.mouseItem;
            if (item.type == 0)
            {
                var player = Main.player[Main.myPlayer];
                item = player.inventory[player.selectedItem];
                if (item.type == 0)
                {
                    Main.NewText("No item selected.");
                    return true;
                }
            }


            int prefixId;
            if (!int.TryParse(args[0], out prefixId))
            {
                for (int i = 0; i < Lang.prefix.Length; i++)
                {
                    if (Lang.prefix[i].Value.ToLower() == args[0].ToLower())
                    {
                        prefixId = i;
                        break;
                    }
                }
                if (prefixId == 0)
                {
                    Main.NewText("Invalid prefix ID.");
                    return true;
                }
            }

            if (!keepStats)
            {
                // Clone item (preserve stack/favorited)
                var stack = item.stack;
                bool favorited = item.favorited;
                item.netDefaults(item.type);
                item.stack = stack;
                item.favorited = favorited;
            }

            if (prefixId != 0 && !item.Prefix(prefixId))
                Main.NewText("Invalid prefix ID for this item type.");
            else
                Main.NewText("Item reset (including usetime / autoreuse).");

            return true;
        }
    }
}
