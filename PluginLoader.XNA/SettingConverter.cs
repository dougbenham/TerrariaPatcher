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
                var pairs = new List<KeyValuePair<object, string>>();
                foreach (DictionaryEntry entry in (IDictionary) value)
                    pairs.Add(new KeyValuePair<object, string>(entry.Key,
                        Serialize(entry.Key, key) + ": " + Serialize(entry.Value, element)));

                return string.Join(", ", InReadableOrder(pairs, key));
            }

            if (TryGetElementType(type, out element))
            {
                var items = new List<KeyValuePair<object, string>>();
                foreach (var item in (IEnumerable) value)
                    items.Add(new KeyValuePair<object, string>(item, Serialize(item, element)));

                // A list or an array is written in the order it holds its values, because that order is part of
                // what the plugin was given. A set has no order of its own, so it is written in order of value.
                return string.Join(", ", IsUnordered(type)
                    ? InReadableOrder(items, element)
                    : items.Select(entry => entry.Value).ToArray());
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

        /// <summary>
        /// Whether the collection keeps its values in no particular order of its own, so that the order they come
        /// out in is an accident of how they were added rather than anything the plugin asked for.
        /// </summary>
        private static readonly IComparer<object> ValueOrder = new ComparableOrder();

        /// <summary>
        /// Orders values by what they are worth, taking a null as coming first rather than as something to throw
        /// over.
        /// </summary>
        private class ComparableOrder : IComparer<object>
        {
            public int Compare(object left, object right)
            {
                if (left == null) return right == null ? 0 : -1;
                if (right == null) return 1;

                return ((IComparable) left).CompareTo(right);
            }
        }

        private static bool IsUnordered(Type type)
        {
            return type.IsGenericType &&
                   (type.GetGenericTypeDefinition() == typeof(HashSet<>) ||
                    type.GetGenericTypeDefinition() == typeof(Dictionary<,>));
        }

        /// <summary>
        /// Puts the values of an unordered collection in order, so that the same contents always come out as the
        /// same text. Without this a set holding exactly the values its plugin declared would not read back as the
        /// same string, because a set rebuilt from the file enumerates in whatever order its buckets ended up in,
        /// and the setting would never look like it was still at its default.
        /// </summary>
        private static string[] InReadableOrder(List<KeyValuePair<object, string>> entries, Type valueType)
        {
            var unordered = entries.Select(entry => entry.Value).ToArray();

            // Ordered on the value rather than on the text, so that 9 comes before 10.
            if (!typeof(IComparable).IsAssignableFrom(valueType)) return unordered;

            try
            {
                // Ordered into a new array rather than sorted in place, so that a comparison that throws leaves
                // the values as they were instead of half rearranged.
                return entries.OrderBy(entry => entry.Key, ValueOrder).Select(entry => entry.Value).ToArray();
            }
            catch (Exception ex)
            {
                Loader.Report("Could not order a setting of " + valueType.Name + ": " + ex.Message);
                return unordered;
            }
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
