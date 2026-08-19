using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using PluginLoader;
using Terraria.ID;

namespace TranscendPlugins.Shared.UI
{
    /// <summary>
    /// The values a content id class such as <c>TileID</c> or <c>BuffID</c> declares, ready to be offered as a list
    /// to pick from. Built once per id class, since reflecting over <c>ItemID</c> turns up several thousand of them.
    /// </summary>
    public class IdDomain
    {
        public class Entry
        {
            public int Id;

            /// <summary>
            /// The name of the constant, which is what a collection of strings stores.
            /// </summary>
            public string Constant;

            /// <summary>
            /// The name the game itself uses, where there is one, so that the list reads the way the item does in
            /// game rather than the way it is spelled in the source.
            /// </summary>
            public string Display;

            /// <summary>
            /// What this entry looks like in Plugins.ini.
            /// </summary>
            public string KeyFor(bool byName)
            {
                return byName ? Constant : Id.ToString();
            }
        }

        private static readonly Dictionary<Type, IdDomain> cache = new Dictionary<Type, IdDomain>();

        public Type IdClass { get; private set; }
        public List<Entry> Entries { get; private set; }

        private readonly Dictionary<string, Entry> byKeyAsName = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<int, Entry> byId = new Dictionary<int, Entry>();

        public static IdDomain For(Type idClass)
        {
            if (idClass == null) return null;

            IdDomain domain;
            if (!cache.TryGetValue(idClass, out domain))
            {
                domain = new IdDomain(idClass);
                cache[idClass] = domain;
            }

            return domain;
        }

        private IdDomain(Type idClass)
        {
            IdClass = idClass;
            Entries = new List<Entry>();

            foreach (var field in idClass.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (!IsIdField(field)) continue;

                // Not a piece of content, just how many there are.
                if (field.Name == "Count") continue;

                var id = Convert.ToInt32(field.GetValue(null));
                if (id < 0 || byId.ContainsKey(id)) continue;

                var entry = new Entry
                {
                    Id = id,
                    Constant = field.Name,
                    Display = DisplayName(idClass, id, field.Name)
                };

                Entries.Add(entry);
                byId[id] = entry;
                if (!byKeyAsName.ContainsKey(entry.Constant)) byKeyAsName[entry.Constant] = entry;
            }

            Entries.Sort((left, right) => string.Compare(left.Display, right.Display, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Whether a field holds a content id. The id classes do not agree on a width: <c>ItemID</c> uses int,
        /// <c>NPCID</c> short, and <c>TileID</c> and <c>WallID</c> ushort.
        /// </summary>
        private static bool IsIdField(FieldInfo field)
        {
            var type = field.FieldType;
            return type == typeof(int) || type == typeof(uint)
                || type == typeof(short) || type == typeof(ushort)
                || type == typeof(sbyte) || type == typeof(byte);
        }

        /// <summary>
        /// Finds the entry a value written in Plugins.ini stands for, whether that is an id or a constant name.
        /// Returns null for a value the id class does not declare, which is left in the setting untouched so that
        /// opening the picker can never quietly drop something.
        /// </summary>
        public Entry Find(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;

            int id;
            if (int.TryParse(key, out id))
            {
                Entry byNumber;
                return byId.TryGetValue(id, out byNumber) ? byNumber : null;
            }

            Entry byName;
            return byKeyAsName.TryGetValue(key, out byName) ? byName : null;
        }

        private static string DisplayName(Type idClass, int id, string constant)
        {
            try
            {
                if (idClass == typeof(ItemID))
                {
                    var name = Lang.GetItemNameValue(id);
                    if (!string.IsNullOrEmpty(name)) return name;
                }
                else if (idClass == typeof(BuffID))
                {
                    var name = Lang.GetBuffName(id);
                    if (!string.IsNullOrEmpty(name)) return name;
                }
                else if (idClass == typeof(NPCID))
                {
                    var name = Lang.GetNPCNameValue(id);
                    if (!string.IsNullOrEmpty(name)) return name;
                }
                else if (idClass == typeof(PrefixID))
                {
                    var text = id >= 0 && id < Lang.prefix.Length ? Lang.prefix[id] : null;
                    if (text != null && !string.IsNullOrEmpty(text.Value)) return text.Value;
                }
            }
            catch
            {
                // A name the language files have nothing for. The constant reads well enough on its own.
            }

            return PluginBase.Prettify(constant);
        }

        /// <summary>
        /// Draws an entry's icon where the id class is one with artwork worth showing. Returns whether anything was
        /// drawn, so that a domain without icons does not leave a gap in every row.
        /// </summary>
        public bool DrawIcon(Entry entry, Rectangle area)
        {
            try
            {
                Texture2D texture;
                Rectangle frame;

                if (IdClass == typeof(ItemID))
                {
                    if (entry.Id <= 0 || entry.Id >= TextureAssets.Item.Length) return false;

                    Main.instance.LoadItem(entry.Id);
                    texture = TextureAssets.Item[entry.Id].Value;

                    var animation = entry.Id < Main.itemAnimations.Length ? Main.itemAnimations[entry.Id] : null;
                    frame = animation != null ? animation.GetFrame(texture) : texture.Bounds;
                }
                else if (IdClass == typeof(BuffID))
                {
                    if (entry.Id <= 0 || entry.Id >= TextureAssets.Buff.Length) return false;

                    texture = TextureAssets.Buff[entry.Id].Value;
                    frame = texture.Bounds;
                }
                else return false;

                if (texture == null || frame.Width <= 0 || frame.Height <= 0) return false;

                var scale = Math.Min(1f, Math.Min(area.Width / (float) frame.Width, area.Height / (float) frame.Height));
                var size = new Vector2(frame.Width, frame.Height) * scale;

                Main.spriteBatch.Draw(texture,
                    new Vector2(area.X + (area.Width - size.X) / 2f, area.Y + (area.Height - size.Y) / 2f),
                    frame, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

                return true;
            }
            catch
            {
                // Artwork that is not loaded and cannot be. The row still reads fine without it.
                return false;
            }
        }

        /// <summary>
        /// Whether this id class has artwork, so that a list can leave room for it up front.
        /// </summary>
        public bool HasIcons
        {
            get { return IdClass == typeof(ItemID) || IdClass == typeof(BuffID); }
        }
    }
}
