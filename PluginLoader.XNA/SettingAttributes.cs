using System;

namespace PluginLoader
{
    /// <summary>
    /// Explains what a setting does. Shown as a tooltip in the in-game settings menu.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class SettingDescriptionAttribute : Attribute
    {
        public string Description { get; private set; }

        public SettingDescriptionAttribute(string description)
        {
            Description = description;
        }
    }

    /// <summary>
    /// Overrides the label the in-game settings menu shows for a setting. Without one the field's own name is
    /// split on its capitals, so <c>MaxBlocksPerVein</c> reads as "Max Blocks Per Vein".
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class SettingLabelAttribute : Attribute
    {
        public string Label { get; private set; }

        public SettingLabelAttribute(string label)
        {
            Label = label;
        }
    }

    /// <summary>
    /// The range a numeric setting is held to when it is edited in the in-game settings menu.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class SettingRangeAttribute : Attribute
    {
        public double Minimum { get; private set; }
        public double Maximum { get; private set; }

        public SettingRangeAttribute(double minimum, double maximum)
        {
            Minimum = minimum;
            Maximum = maximum;
        }
    }

    /// <summary>
    /// Names the content id class a collection setting holds values from, such as <c>TileID</c> or <c>BuffID</c>,
    /// so that the in-game settings menu can offer the ids as a list to tick instead of a line of text to type.
    /// Collections without one are still edited as text.
    /// </summary>
    /// <remarks>
    /// A collection of <c>int</c> holds the ids themselves, a collection of <c>string</c> holds the names of the
    /// constants, matching how each is written to Plugins.ini.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class SettingIdsAttribute : Attribute
    {
        public Type IdClass { get; private set; }

        public SettingIdsAttribute(Type idClass)
        {
            IdClass = idClass;
        }
    }
}
