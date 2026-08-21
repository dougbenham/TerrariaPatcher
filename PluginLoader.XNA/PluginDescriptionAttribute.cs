using System;

namespace PluginLoader
{
    /// <summary>
    /// Describes what a plugin does. Shown in the patcher's plugin list and by the /plugins chat command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PluginDescriptionAttribute : Attribute
    {
        public string Description { get; private set; }

        public PluginDescriptionAttribute(string description)
        {
            Description = description;
        }
    }
}
