using System;
using System.Collections.Generic;
using PluginLoader;
using Terraria;
using Terraria.ID;
using TranscendPlugins.Shared.Extensions;

/// Original taken from: https://gist.github.com/YellowAfterlife/1edaa4060191823ee366
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
    /// 
    /// string name;
    /// bool? autoReuse; // aka "auto-swing" for weapons
    /// int? damage;
    /// float? knockback;
    /// int? crit;
    /// int? defense;
    /// int? useTime; // item "use time"/cooldown, in frames
    /// int? useAnimation;
    /// int? holdStyle;
    /// int? useStyle;
    /// int? maxStack;
    /// float? scale; // size (1.0 is normal)
    /// string toolTip;
    /// string toolTip2;
    public class ItemConfig : IPluginItemSetDefaults
    {
        #region Read INI

        private string confPath = Environment.CurrentDirectory + "\\ItemConfig.ini";
        private readonly HashSet<string> _sections;

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
            IniAPI.WriteIni("header", "hint", "Add rules below; See ItemConfig.cs for instructions.", confPath);
            _sections = new HashSet<string>(IniAPI.GetIniSections(confPath));
        }

        public void OnItemSetDefaults(Item item)
        {
            var section = "item" + item.type;
            if (!_sections.Contains(section))
                return;

            var name = LoadString(section, "name");
            var autoReuse = LoadBool(section, "autoReuse");
            var damage = LoadInt(section, "damage");
            var knockback = LoadFloat(section, "knockback");
            var crit = LoadInt(section, "crit");
            var defense = LoadInt(section, "defense");
            var useTime = LoadInt(section, "useTime");
            var useAnimation = LoadInt(section, "useAnimation");
            var holdStyle = LoadInt(section, "holdStyle");
            var useStyle = LoadInt(section, "useStyle");
            var maxStack = LoadInt(section, "maxStack");
            var scale = LoadFloat(section, "scale");
            var toolTip = LoadString(section, "toolTip");

            if (name != null) Lang._itemNameCache[ItemID.FromNetId((short) item.type)].SetValue(name);
            if (autoReuse != null) item.autoReuse = (bool) autoReuse;
            if (damage != null) item.damage = (int) damage;
            if (knockback != null) item.knockBack = (float) knockback;
            if (crit != null) item.crit = (int) crit;
            if (defense != null) item.defense = (int) defense;
            if (useTime != null) item.useTime = (int) useTime;
            if (useAnimation != null) item.useAnimation = (int) useAnimation;
            if (holdStyle != null) item.holdStyle = (int) holdStyle;
            if (useStyle != null) item.useStyle = (int) useStyle;
            if (maxStack != null) item.maxStack = (int) maxStack;
            if (scale != null) item.scale = (float) scale;
            if (toolTip != null) item.ToolTip.SetValue(toolTip);
        }
    }
}