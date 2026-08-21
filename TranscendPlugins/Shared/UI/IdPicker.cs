using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PluginLoader;

namespace TranscendPlugins.Shared.UI
{
    /// <summary>
    /// Picks what a setting holds from the ids its plugin declared it comes from, as a filtered list of tick
    /// boxes with the chosen ones gathered at the top. A setting holding a collection takes as many as are
    /// ticked; one holding a single value takes whichever is clicked and closes.
    /// </summary>
    public class IdPicker
    {
        public Setting Setting { get; private set; }

        private IdDomain domain;

        /// <summary>
        /// Whether the setting stores the name of each constant rather than its number.
        /// </summary>
        private bool byName;

        /// <summary>
        /// Whether the setting holds one value rather than a collection of them, in which case picking a row
        /// replaces what is there instead of adding to it.
        /// </summary>
        private bool single;

        /// <summary>
        /// Set when a single value has been picked, so that the list closes itself rather than waiting to be
        /// dismissed once there is nothing left to choose.
        /// </summary>
        private bool picked;

        /// <summary>
        /// What the setting holds, in the order it holds it, so that ticking one value on and off again leaves the
        /// setting written exactly as it was and still counting as its default.
        /// </summary>
        private readonly List<string> values = new List<string>();

        /// <summary>
        /// The same values as a set, for telling at a glance whether a row is ticked.
        /// </summary>
        private readonly HashSet<string> chosen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// How many of the values the id class does not declare. They are left in the setting untouched, so that
        /// saving cannot quietly drop something the player put there by hand.
        /// </summary>
        private int unrecognised;

        /// <summary>
        /// The rows as they are shown: the chosen ones, a null standing for the divider, then the rest. Only
        /// rebuilt when the filter changes or the player asks, so that ticking a row does not move it out from
        /// under the mouse mid-click.
        /// </summary>
        private readonly List<IdDomain.Entry> rows = new List<IdDomain.Entry>();

        private readonly TextBox filter = new TextBox { Placeholder = "filter" };
        private readonly Scroller scroller = new Scroller();

        private bool needsOrdering;

        public IdPicker()
        {
            filter.Changed = text => needsOrdering = true;
        }

        /// <summary>
        /// Whether a setting can be edited this way, rather than as a line of text.
        /// </summary>
        public static bool CanEdit(Setting setting)
        {
            if (setting.IdClass == null) return false;

            return Holds(setting) != null;
        }

        /// <summary>
        /// Whether a setting holds one value rather than a collection of them.
        /// </summary>
        public static bool IsSingle(Setting setting)
        {
            Type element;
            return !SettingConverter.TryGetElementType(setting.ValueType, out element);
        }

        /// <summary>
        /// The type of the individual values a setting holds, or null where they are not ones an id can be
        /// written as.
        /// </summary>
        private static Type Holds(Setting setting)
        {
            Type element;
            if (!SettingConverter.TryGetElementType(setting.ValueType, out element))
                element = setting.ValueType;

            return element == typeof(string) || SettingConverter.IsNumeric(element) ? element : null;
        }

        public void Open(Setting setting)
        {
            Setting = setting;
            domain = IdDomain.For(setting.IdClass);

            single = IsSingle(setting);
            byName = Holds(setting) == typeof(string);
            picked = false;

            filter.Text = "";
            scroller.Reset();

            Read();
            Order();
        }

        public void Close()
        {
            Setting = null;
            TextBox.Unfocus();
        }

        /// <summary>
        /// Loads what the setting holds now, which is also how an edit made in Plugins.ini while the window is open
        /// finds its way in.
        /// </summary>
        private void Read()
        {
            values.Clear();
            chosen.Clear();
            unrecognised = 0;

            // A single value is taken whole rather than split on commas, since it is one value however it reads.
            var held = single
                ? new[] { Setting.Serialize().Trim() }
                : SettingEditor.Split(Setting.Serialize());

            foreach (var value in held)
            {
                if (value.Length == 0) continue;

                var entry = domain.Find(value);

                if (entry == null)
                {
                    values.Add(value);
                    unrecognised++;
                    continue;
                }

                var key = entry.KeyFor(byName);
                if (chosen.Add(key)) values.Add(key);
            }
        }

        private void Save()
        {
            try
            {
                Setting.SetFrom(string.Join(", ", values.ToArray()));
            }
            catch (Exception ex)
            {
                Terraria.Main.NewText("Could not set " + Setting.FullName + ": " + ex.Message, 230, 130, 130);
            }
        }

        /// <summary>
        /// Takes a row. A collection gains or loses the value, keeping the ones already there in the order they
        /// were written; a single value is replaced by it, since there is only room for the one.
        /// </summary>
        private void Toggle(IdDomain.Entry entry)
        {
            var key = entry.KeyFor(byName);

            if (single)
            {
                values.Clear();
                chosen.Clear();

                values.Add(key);
                chosen.Add(key);

                Save();

                picked = true;
                return;
            }

            if (chosen.Remove(key)) values.Remove(key);
            else
            {
                chosen.Add(key);
                values.Add(key);
            }

            Save();
        }

        /// <summary>
        /// Splits the entries the filter matches into the chosen ones and the rest.
        /// </summary>
        private void Order()
        {
            needsOrdering = false;
            rows.Clear();

            var text = filter.Text.Trim();
            var rest = new List<IdDomain.Entry>();

            foreach (var entry in domain.Entries)
            {
                if (!Matches(entry, text)) continue;

                if (chosen.Contains(entry.KeyFor(byName))) rows.Add(entry);
                else rest.Add(entry);
            }

            if (rows.Count > 0 && rest.Count > 0) rows.Add(null);
            rows.AddRange(rest);

            scroller.Reset();
        }

        private static bool Matches(IdDomain.Entry entry, string text)
        {
            if (text.Length == 0) return true;

            return entry.Display.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   entry.Constant.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   entry.Id.ToString() == text;
        }

        /// <summary>
        /// Called once a tick while the picker is open, to pick up an edit made to Plugins.ini behind its back.
        /// </summary>
        public void Update()
        {
            var before = values.Count;
            Read();

            if (values.Count != before) needsOrdering = true;
        }

        /// <summary>
        /// Draws the picker in place of the settings list. Returns false once the player has asked to go back.
        /// </summary>
        public bool Draw(Rectangle area, string breadcrumb, int wheelNotches)
        {
            if (needsOrdering) Order();

            var control = Gui.RowHeight;

            var back = new Rectangle(area.X, area.Y, control, control);
            var goBack = Gui.Button(back, Gui.ArrowLeft);

            var crumb = new Rectangle(back.Right + 8, area.Y, area.Width - control - 220, control);
            Gui.TextLeftCentered(Gui.Fit(breadcrumb, crumb.Width), crumb, Gui.TextNormal);
            Gui.TextRight(single ? Chosen() : values.Count + " chosen", area.Right,
                area.Y + (control - Gui.TextHeight) / 2f, Gui.TextDim);

            var filterRow = new Rectangle(area.X, back.Bottom + 8, area.Width - control - 4, control);
            filter.Draw(filterRow);

            var resort = new Rectangle(area.Right - control, filterRow.Y, control, control);
            if (Gui.Button(resort, Gui.Resort)) Order();
            if (Gui.Hover(resort)) Gui.Tooltip("Gather the chosen ones back to the top");

            var footerHeight = control + 6;
            var list = new Rectangle(area.X, filterRow.Bottom + 8, area.Width - 10,
                area.Bottom - footerHeight - 8 - (filterRow.Bottom + 8));

            DrawList(list, wheelNotches);
            DrawFooter(new Rectangle(area.X, area.Bottom - footerHeight, area.Width, footerHeight));

            return !goBack && !picked;
        }

        /// <summary>
        /// What a single value setting is holding, read for showing rather than for saving.
        /// </summary>
        private string Chosen()
        {
            if (values.Count == 0) return "nothing chosen";

            var entry = domain.Find(values[0]);
            return entry == null ? values[0] : entry.Display;
        }

        private void DrawList(Rectangle list, int wheelNotches)
        {
            var rowHeight = Gui.RowHeight;
            var visible = Math.Max(1, list.Height / rowHeight);

            scroller.Wheel(list, rows.Count, visible, wheelNotches);
            scroller.Clamp(rows.Count, visible);

            if (rows.Count == 0)
            {
                Gui.Text("Nothing matches that filter.", new Vector2(list.X + 4, list.Y + 4), Gui.TextDim);
                return;
            }

            for (var line = 0; line < visible; line++)
            {
                var index = scroller.Offset + line;
                if (index >= rows.Count) break;

                var row = new Rectangle(list.X, list.Y + line * rowHeight, list.Width, rowHeight);
                DrawRow(rows[index], row);
            }

            scroller.DrawBar(new Rectangle(list.Right + 4, list.Y, 6, list.Height), rows.Count, visible);
        }

        private void DrawRow(IdDomain.Entry entry, Rectangle row)
        {
            if (entry == null)
            {
                var middle = row.Y + row.Height / 2;
                Gui.HorizontalLine(row.X, middle, row.Width, Gui.Divider);
                var label = " available ";
                var size = Gui.Measure(label);
                var backing = new Rectangle((int) (row.X + (row.Width - size.X) / 2f) - 2, row.Y + 2, (int) size.X + 4, row.Height - 4);
                Gui.Fill(backing, Gui.PanelBack);
                Gui.TextCentered(label, row, Gui.TextDim);
                return;
            }

            var key = entry.KeyFor(byName);
            var on = chosen.Contains(key);
            var hovered = Gui.Hover(row);

            if (hovered) Gui.Fill(row, Gui.RowHover);

            var box = Math.Min(18, row.Height - 4);
            var tick = new Rectangle(row.X + 2, row.Y + (row.Height - box) / 2, box, box);
            Gui.Tick(tick, on);

            var x = tick.Right + 6;

            if (domain.HasIcons)
            {
                var icon = new Rectangle(x, row.Y + 2, 20, row.Height - 4);
                domain.DrawIcon(entry, icon);
                x = icon.Right + 6;
            }

            var detail = "(" + entry.Constant + " " + entry.Id + ")";
            var detailWidth = Gui.Measure(detail).X;
            var nameWidth = row.Right - x - detailWidth - 12;

            var y = row.Y + Math.Max(0f, (row.Height - Gui.TextHeight) / 2f);
            Gui.Text(Gui.Fit(entry.Display, nameWidth), new Vector2(x, y), on ? Gui.TextHot : Gui.TextNormal);
            Gui.TextRight(detail, row.Right - 4, y, Gui.TextDim);

            if (Gui.Click(row)) Toggle(entry);
        }

        private void DrawFooter(Rectangle footer)
        {
            var button = 110;
            var height = Gui.RowHeight;
            var y = footer.Y + 3;
            var x = footer.X;

            // A setting holding a single value has to hold something, so there is nothing to clear it to.
            if (!single)
            {
                if (Gui.Button(new Rectangle(x, y, button, height), "Clear all", chosen.Count > 0))
                {
                    // Leaves anything the id class does not declare where it was, as ticking rows off would.
                    values.RemoveAll(chosen.Contains);
                    chosen.Clear();

                    Save();
                    needsOrdering = true;
                }

                x += button + 8;
            }

            if (Gui.Button(new Rectangle(x, y, button, height), "Reset " + Gui.Revert, !Setting.IsDefault))
            {
                Setting.Reset();
                Read();
                needsOrdering = true;
            }

            if (unrecognised > 0)
            {
                var note = unrecognised + " value" + (unrecognised == 1 ? "" : "s") + " kept as written";
                Gui.TextRight(note, footer.Right, y + (height - Gui.TextHeight) / 2f, Gui.TextDim);
            }
        }
    }
}
