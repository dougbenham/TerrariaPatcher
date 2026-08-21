using System;
using System.Collections.Generic;
using System.Reflection;

namespace TranscendPlugins.Shared
{
    /// <summary>
    /// The named ids a content id class such as <c>ItemID</c> or <c>NPCID</c> declares, for looking an id up by name
    /// and for searching by part of a name. Ids with no constant of their own, such as the negative item net ids, are
    /// not listed, so this is only ever used to resolve a name — never to decide whether an id is valid.
    /// </summary>
    public class IdLookup
    {
        private readonly Dictionary<string, int> idsByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly List<KeyValuePair<int, string>> names = new List<KeyValuePair<int, string>>();

        public IdLookup(Type idClass)
        {
            foreach (var field in idClass.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.FieldType != typeof(int) && field.FieldType != typeof(short)) continue;
                if (idsByName.ContainsKey(field.Name)) continue;

                var id = Convert.ToInt32(field.GetValue(null));

                idsByName.Add(field.Name, id);
                names.Add(new KeyValuePair<int, string>(id, field.Name));
            }
        }

        public bool TryGetId(string name, out int id)
        {
            return idsByName.TryGetValue(name, out id);
        }

        /// <summary>
        /// Every name containing <paramref name="text"/>, in the form <c>Name (id)</c>.
        /// </summary>
        public List<string> Search(string text)
        {
            var matches = new List<string>();

            foreach (var entry in names)
            {
                if (entry.Value.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                    matches.Add(entry.Value + " (" + entry.Key + ")");
            }

            return matches;
        }

        /// <summary>
        /// Prints the matches for a search, capped so that a short query cannot flood the chat.
        /// </summary>
        public static void Report(List<string> matches, string what)
        {
            const int limit = 20;

            if (matches.Count == 0)
            {
                Terraria.Main.NewText("No " + what + " matched. Try a shorter piece of the name.");
                return;
            }

            if (matches.Count > limit)
            {
                Terraria.Main.NewText("Found " + matches.Count + " " + what + ", showing the first " + limit + ":");
                matches = matches.GetRange(0, limit);
            }

            Terraria.Main.NewText(string.Join(", ", matches.ToArray()));
        }
    }
}
