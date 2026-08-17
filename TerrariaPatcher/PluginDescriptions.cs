using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TerrariaPatcher
{
    /// <summary>
    /// Reads the [PluginDescription] attribute out of plugin source files. Plugins are distributed as source and
    /// only compiled when the game starts, so the patcher has to read the description as text.
    /// </summary>
    internal static class PluginDescriptions
    {
        private const string AttributeName = "PluginDescription";

        /// <summary>
        /// Maps every plugin in the folder, whether a single .cs file or a folder of them, to its description.
        /// Plugins without one are mapped to null.
        /// </summary>
        public static Dictionary<string, string> ReadAll(string pluginsFolder, string sharedFolder)
        {
            var descriptions = new Dictionary<string, string>();

            foreach (var folder in Directory.EnumerateDirectories(pluginsFolder).Where(s => s != sharedFolder))
            {
                var name = Path.GetFileName(folder);
                var main = Path.Combine(folder, name + ".cs");

                descriptions[name] = File.Exists(main)
                    ? ReadFile(main)
                    : Directory.EnumerateFiles(folder, "*.cs", SearchOption.AllDirectories).Select(ReadFile).FirstOrDefault(text => text != null);
            }

            foreach (var file in Directory.EnumerateFiles(pluginsFolder, "*.cs"))
                descriptions[Path.GetFileNameWithoutExtension(file)] = ReadFile(file);

            return descriptions;
        }

        private static string ReadFile(string path)
        {
            try
            {
                return Read(File.ReadAllText(path));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the text of the first [PluginDescription("...")] in the source, or null if it has none.
        /// </summary>
        public static string Read(string source)
        {
            var start = source.IndexOf(AttributeName, StringComparison.Ordinal);

            while (start >= 0)
            {
                var open = source.IndexOf('(', start);
                if (open < 0) return null;

                if (IsAttributeUsage(source, start) && source.Substring(start + AttributeName.Length, open - start - AttributeName.Length).Trim().Length == 0)
                {
                    var description = ReadArguments(source, open);
                    if (description != null) return description;
                }

                start = source.IndexOf(AttributeName, start + AttributeName.Length, StringComparison.Ordinal);
            }

            return null;
        }

        /// <summary>
        /// True if the name is preceded by the opening bracket of an attribute rather than being part of, say,
        /// the attribute class's own declaration.
        /// </summary>
        private static bool IsAttributeUsage(string source, int start)
        {
            for (var i = start - 1; i >= 0; i--)
            {
                if (source[i] == '[') return true;
                if (!char.IsWhiteSpace(source[i])) return false;
            }

            return false;
        }

        /// <summary>
        /// Joins the string literals of the attribute's argument list, so that a description may be written as
        /// several concatenated strings.
        /// </summary>
        private static string ReadArguments(string source, int open)
        {
            var text = new StringBuilder();
            var depth = 0;

            for (var i = open; i < source.Length; i++)
            {
                var c = source[i];

                if (c == '(') depth++;
                else if (c == ')')
                {
                    depth--;
                    if (depth == 0) return text.Length == 0 ? null : text.ToString();
                }
                else if (c == '"' || (c == '@' && i + 1 < source.Length && source[i + 1] == '"'))
                {
                    var verbatim = c == '@';
                    if (verbatim) i++;

                    i = ReadLiteral(source, i, verbatim, text);
                }
            }

            return null;
        }

        /// <summary>
        /// Appends the contents of the literal starting at the quote to the builder, and returns the index of
        /// its closing quote.
        /// </summary>
        private static int ReadLiteral(string source, int quote, bool verbatim, StringBuilder text)
        {
            for (var i = quote + 1; i < source.Length; i++)
            {
                var c = source[i];

                if (verbatim)
                {
                    if (c != '"') text.Append(c);
                    else if (i + 1 < source.Length && source[i + 1] == '"')
                    {
                        text.Append('"');
                        i++;
                    }
                    else return i;

                    continue;
                }

                if (c == '\\' && i + 1 < source.Length)
                {
                    text.Append(Unescape(source[++i]));
                    continue;
                }

                if (c == '"') return i;

                text.Append(c);
            }

            return source.Length - 1;
        }

        private static char Unescape(char escaped)
        {
            switch (escaped)
            {
                case 'n': return '\n';
                case 'r': return '\r';
                case 't': return '\t';
                case '0': return '\0';
                default: return escaped;
            }
        }
    }
}
