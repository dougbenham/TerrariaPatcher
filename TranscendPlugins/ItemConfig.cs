using System;
using System.Collections.Generic;
using PluginLoader;
using Terraria;

/// Updated July 24, 2015
/// Updates: https://gist.github.com/YellowAfterlife/1edaa4060191823ee366
namespace YellowAfterlifePlugins
{
    /// Item modification rules are defined as following:
    ///  [item3507]
    ///  name=Copper Sword Plus
    ///  damage=20
    /// A list of possible fields can be seen below.
    /// "string" is text
    /// "float" is any number
    ///	"int" is "rounded" number (1,2,3,...)
    ///	"bool" is toggle ("true"/"false")
    public class ItemConfigRule
    {
        public string name;
        public bool? autoReuse; // aka "auto-swing" for weapons
        public int? damage;
        public float? knockback;
        public int? crit;
        public int? defense;
        public int? useTime; // item "use time"/cooldown, in frames
        public int? useTime2; // secondary use time (used by weapons with two attacks)
        public int? holdStyle;
        public int? useStyle;
        public int? maxStack;
        public float? scale; // size (1.0 is normal)
        public string toolTip;
        public string toolTip2;
    }
    public class ItemConfig : IPluginItemSetDefaults
    {
        Dictionary<int, ItemConfigRule> rules;

        #region Read INI

        private string confPath = Environment.CurrentDirectory + "\\ItemConfig.ini";

        private string LoadString(string section, string field)
        {
            string s = IniAPI.ReadIni(section, field, "", path: confPath);
            return s != "" ? s : null;
        }

        private int? LoadInt(string section, string field)
        {
            string s = IniAPI.ReadIni(section, field, "", path: confPath);
            if (s != "")
            {
                return int.Parse(s);
            }
            else return null;
        }

        private float? LoadFloat(string section, string field)
        {
            string s = IniAPI.ReadIni(section, field, "", path: confPath);
            if (s != "")
            {
                return float.Parse(s);
            }
            else return null;
        }

        private bool? LoadBool(string section, string field)
        {
            string s = IniAPI.ReadIni(section, field, "", path: confPath);
            if (s != "")
            {
                s = s.ToLower();
                return (s == "1" || s == "true" || s == "yes");
            }
            else return null;
        }

        #endregion

        public ItemConfig()
        {
            rules = new Dictionary<int, ItemConfigRule>();
            IniAPI.WriteIni("header", "hint", "Add rules below; See ItemConfig.cs for instructions.", confPath);
            foreach (string section in IniAPI.GetIniSections(confPath))
            {
                if (section.ToLower().StartsWith("item"))
                {
                    int id = int.Parse(section.Substring(4));
                    var r = new ItemConfigRule
                    {
                        name = LoadString(section, "name"),
                        autoReuse = LoadBool(section, "autoReuse"),
                        damage = LoadInt(section, "damage"),
                        knockback = LoadFloat(section, "knockback"),
                        crit = LoadInt(section, "crit"),
                        defense = LoadInt(section, "defense"),
                        useTime = LoadInt(section, "useTime"),
                        useTime2 = LoadInt(section, "useTime2"),
                        holdStyle = LoadInt(section, "holdStyle"),
                        useStyle = LoadInt(section, "useStyle"),
                        maxStack = LoadInt(section, "maxStack"),
                        scale = LoadFloat(section, "scale"),
                        toolTip = LoadString(section, "toolTip"),
                        toolTip2 = LoadString(section, "toolTip2")
                    };
                    rules[id] = r;
                }
            }
        }

        public void OnItemSetDefaults(Item item)
        {
            ItemConfigRule r;
            if (rules.TryGetValue(item.type, out r))
            {
                if (r.name != null) item.name = r.name;
                if (r.autoReuse != null) item.autoReuse = (bool)r.autoReuse;
                if (r.damage != null) item.damage = (int)r.damage;
                if (r.knockback != null) item.knockBack = (float)r.knockback;
                if (r.crit != null) item.crit = (int)r.crit - 4;
                if (r.defense != null) item.defense = (int)r.defense;
                if (r.useTime != null) item.useAnimation = (int)r.useTime;
                if (r.useTime2 != null) item.useTime = (int)r.useTime2;
                if (r.holdStyle != null) item.holdStyle = (int)r.holdStyle;
                if (r.useStyle != null) item.useStyle = (int)r.useStyle;
                if (r.maxStack != null) item.maxStack = (int)r.maxStack;
                if (r.scale != null) item.scale = (float)r.scale;
                if (r.toolTip != null) item.toolTip = r.toolTip;
                if (r.toolTip2 != null) item.toolTip2 = r.toolTip2;
            }
        }
    }
}