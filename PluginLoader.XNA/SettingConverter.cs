using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;

namespace PluginLoader
{
    /// <summary>
    /// Converts setting values to and from the plain text stored in Plugins.ini.
    /// </summary>
    /// <remarks>
    /// Collections are written as comma separated values, dictionaries as comma separated <c>key: value</c>
    /// pairs, so that everything in the file stays hand editable.
    /// </remarks>
    public static class SettingConverter
    {
        private static readonly char[] itemSeparator = { ',' };

        public static string Serialize(object value, Type type)
        {
            if (value == null) return "";

            if (type == typeof(string)) return (string) value;
            if (type == typeof(bool)) return ((bool) value) ? "true" : "false";
            if (type == typeof(Hotkey)) return ((Hotkey) value).ToBinding();
            if (type == typeof(Color)) return SerializeColor((Color) value);
            if (type.IsEnum) return Enum.GetName(type, value) ?? Convert.ToInt64(value).ToString(CultureInfo.InvariantCulture);
            if (IsNumeric(type)) return string.Format(CultureInfo.InvariantCulture, "{0}", value);

            if (TryGetDictionaryTypes(type, out var key, out var element))
            {
                var pairs = new List<string>();
                foreach (DictionaryEntry entry in (IDictionary) value)
                    pairs.Add(Serialize(entry.Key, key) + ": " + Serialize(entry.Value, element));

                return string.Join(", ", pairs.ToArray());
            }

            if (TryGetElementType(type, out element))
            {
                var items = (from object item in (IEnumerable) value select Serialize(item, element)).ToArray();
                return string.Join(", ", items);
            }

            throw new NotSupportedException("Settings of type " + type.Name + " cannot be stored in Plugins.ini.");
        }

        public static object Deserialize(string text, Type type)
        {
            if (text == null) text = "";
            text = text.Trim();

            if (type == typeof(string)) return text;
            if (type == typeof(bool)) return bool.Parse(text);
            if (type == typeof(Hotkey)) return Loader.ParseHotkey(text) ?? new Hotkey();
            if (type == typeof(Color)) return DeserializeColor(text);
            if (type.IsEnum) return Enum.Parse(type, text, true);
            if (IsNumeric(type)) return Convert.ChangeType(text, type, CultureInfo.InvariantCulture);

            if (TryGetDictionaryTypes(type, out var key, out var element))
            {
                var dictionary = (IDictionary) Activator.CreateInstance(type);
                foreach (var entry in Split(text))
                {
                    var colon = entry.IndexOf(':');
                    if (colon < 0)
                        throw new FormatException("Expected 'key: value' but found '" + entry + "'.");

                    dictionary[Deserialize(entry.Substring(0, colon), key)] = Deserialize(entry.Substring(colon + 1), element);
                }

                return dictionary;
            }

            if (TryGetElementType(type, out element))
            {
                var items = Split(text).Select(item => Deserialize(item, element)).ToArray();

                var array = Array.CreateInstance(element, items.Length);
                items.CopyTo(array, 0);

                if (type.IsArray) return array;

                // List<T> and HashSet<T> both take the items as one IEnumerable<T>, which HashSet<T> needs since it
                // is not an IList.
                return Activator.CreateInstance(type, new object[] { array });
            }

            throw new NotSupportedException("Settings of type " + type.Name + " cannot be stored in Plugins.ini.");
        }

        /// <summary>
        /// True if the type round trips through <see cref="Serialize"/> and <see cref="Deserialize"/>.
        /// </summary>
        public static bool IsSupported(Type type)
        {
            if (type == typeof(string) || type == typeof(bool) || type == typeof(Hotkey) ||
                type == typeof(Color) || type.IsEnum || IsNumeric(type))
                return true;

            if (TryGetDictionaryTypes(type, out var key, out var element))
                return IsSupported(key) && IsSupported(element);

            return TryGetElementType(type, out element) && IsSupported(element);
        }

        private static IEnumerable<string> Split(string text)
        {
            return text.Split(itemSeparator, StringSplitOptions.RemoveEmptyEntries)
                .Select(item => item.Trim())
                .Where(item => item.Length > 0);
        }

        /// <summary>
        /// The shape of a setting's type, so that an editor can pick a control to match it.
        /// </summary>
        public static bool IsNumeric(Type type)
        {
            return type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort) ||
                   type == typeof(int) || type == typeof(uint) || type == typeof(long) || type == typeof(ulong) ||
                   type == typeof(float) || type == typeof(double) || type == typeof(decimal);
        }

        public static bool TryGetDictionaryTypes(Type type, out Type key, out Type element)
        {
            key = null;
            element = null;

            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Dictionary<,>)) return false;

            var arguments = type.GetGenericArguments();
            key = arguments[0];
            element = arguments[1];
            return true;
        }

        public static bool TryGetElementType(Type type, out Type element)
        {
            element = null;

            if (type.IsArray && type.GetArrayRank() == 1)
            {
                element = type.GetElementType();
                return true;
            }

            if (type.IsGenericType &&
                (type.GetGenericTypeDefinition() == typeof(List<>) || type.GetGenericTypeDefinition() == typeof(HashSet<>)))
            {
                element = type.GetGenericArguments()[0];
                return true;
            }

            return false;
        }

        private static string SerializeColor(Color color)
        {
            var text = color.R + " " + color.G + " " + color.B;
            return color.A == byte.MaxValue ? text : text + " " + color.A;
        }

        private static Color DeserializeColor(string text)
        {
            var parts = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
                throw new FormatException("Expected a colour as 'R G B' or 'R G B A' but found '" + text + "'.");

            return new Color(byte.Parse(parts[0]), byte.Parse(parts[1]), byte.Parse(parts[2]),
                parts.Length > 3 ? byte.Parse(parts[3]) : byte.MaxValue);
        }
    }
}
