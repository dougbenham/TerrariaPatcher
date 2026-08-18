using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PluginLoader;
using Terraria;

namespace TranscendPlugins.Shared.UI
{
    public partial class SettingsWindow
    {
        private const int Padding = 12;
        private const int TitleHeight = 30;
        private const int LeftPaneWidth = 218;
        private const int PluginRowHeight = 24;
        private const int SettingRowHeight = 28;

        /// <summary>
        /// Draws the window. Called from the draw hook, where the mouse and the screen are measured the same way
        /// the window is, so the two can be compared without scaling either.
        /// </summary>
        public void Draw()
        {
            if (!IsOpen) return;

            var width = Math.Min(880, Main.screenWidth - 60);
            var height = Math.Min(560, Main.screenHeight - 60);
            var window = new Rectangle((Main.screenWidth - width) / 2, (Main.screenHeight - height) / 2, width, height);

            Gui.BeginFrame();

            Gui.Panel(window, Gui.PanelBack);

            // Anywhere inside the window belongs to the window, so a click that misses a control still does not
            // reach the world behind it.
            Gui.Hover(window);

            var inner = new Rectangle(window.X + Padding, window.Y + Padding,
                window.Width - Padding * 2, window.Height - Padding * 2);

            DrawTitle(new Rectangle(inner.X, inner.Y, inner.Width, TitleHeight));

            var body = new Rectangle(inner.X, inner.Y + TitleHeight + 6, inner.Width, inner.Bottom - (inner.Y + TitleHeight + 6));

            var left = new Rectangle(body.X, body.Y, LeftPaneWidth, body.Height);
            var right = new Rectangle(left.Right + Padding + 6, body.Y, body.Right - (left.Right + Padding + 6), body.Height);

            Gui.VerticalLine(left.Right + Padding / 2, body.Y, body.Height, Gui.Divider);

            DrawPluginList(left);
            DrawRightPane(right);

            Gui.EndFrame();

            // Spent, so that a frame drawn twice in one tick does not scroll twice.
            wheelNotches = 0;
        }

        private void DrawTitle(Rectangle area)
        {
            Gui.Text("Plugin Settings", new Vector2(area.X, area.Y + 2), Gui.TextHot, 1.1f);

            var close = new Rectangle(area.Right - 26, area.Y, 26, 24);
            if (Gui.Button(close, Gui.Close)) Close();

            var hint = "Esc closes";
            Gui.TextRight(hint, close.X - 10, area.Y + 4, Gui.TextDim);
        }

        #region Plugin list

        private void DrawPluginList(Rectangle area)
        {
            var filterRow = new Rectangle(area.X, area.Y, area.Width, 24);
            pluginFilter.Draw(filterRow);

            var list = new Rectangle(area.X, filterRow.Bottom + 8, area.Width - 10, area.Bottom - (filterRow.Bottom + 8));

            var matches = Matching();
            var visible = Math.Max(1, list.Height / PluginRowHeight);

            pluginScroll.Wheel(list, matches.Count, visible, wheelNotches);
            pluginScroll.Clamp(matches.Count, visible);

            for (var line = 0; line < visible; line++)
            {
                var index = pluginScroll.Offset + line;
                if (index >= matches.Count) break;

                DrawPluginRow(matches[index], new Rectangle(list.X, list.Y + line * PluginRowHeight, list.Width, PluginRowHeight));
            }

            if (matches.Count == 0)
                Gui.Text("No plugin matches.", new Vector2(list.X + 4, list.Y + 4), Gui.TextDim);

            pluginScroll.DrawBar(new Rectangle(list.Right + 4, list.Y, 6, list.Height), matches.Count, visible);
        }

        /// <summary>
        /// The plugins the search box matches. Worked out only when the search changes, since this runs on every
        /// frame the window is up.
        /// </summary>
        private List<PluginBase> Matching()
        {
            var text = pluginFilter.Text.Trim();
            if (matchedFor == text) return matched;

            matchedFor = text;
            matched.Clear();

            foreach (var plugin in plugins)
            {
                if (text.Length == 0 || plugin.Name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                    matched.Add(plugin);
            }

            return matched;
        }

        private void DrawPluginRow(PluginBase plugin, Rectangle row)
        {
            var isSelected = ReferenceEquals(plugin, selected);
            var hovered = Gui.Hover(row);

            if (isSelected) Gui.Fill(row, Gui.RowSelected);
            else if (hovered) Gui.Fill(row, Gui.RowHover);

            var tick = new Rectangle(row.X + 2, row.Y + (row.Height - 16) / 2, 16, 16);
            var locked = ReferenceEquals(plugin, Owner);

            Gui.Tick(tick, plugin.Enabled);

            if (locked)
            {
                if (Gui.Hover(tick)) Gui.Tooltip(plugin.Name + " is what draws this window, so it stays switched on.");
            }
            else if (Gui.Click(tick))
            {
                Select(plugin);
                plugin.Enabled = !plugin.Enabled;
                return;
            }

            var name = new Rectangle(tick.Right + 6, row.Y, row.Right - (tick.Right + 6), row.Height);
            Gui.Text(Gui.Fit(plugin.Name, name.Width - 4),
                new Vector2(name.X, name.Y + (name.Height - Gui.LineHeight) / 2f),
                plugin.Enabled ? (isSelected ? Gui.TextHot : Gui.TextNormal) : Gui.TextDim);

            if (Gui.Click(name)) Select(plugin);
        }

        private void Select(PluginBase plugin)
        {
            if (ReferenceEquals(plugin, selected)) return;

            selected = plugin;
            ReadSettings();
            settingScroll.Reset();
            picker.Close();
            editor.Capturing = null;
            resetArmed = 0;
            TextBox.Unfocus();
        }

        #endregion

        #region Settings

        private void DrawRightPane(Rectangle area)
        {
            if (selected == null)
            {
                Gui.Text("No plugins are loaded.", new Vector2(area.X, area.Y), Gui.TextDim);
                return;
            }

            if (picker.Setting != null)
            {
                if (!picker.Draw(area, selected.Name + " " + Gui.Crumb + " " + picker.Setting.Label, wheelNotches))
                    picker.Close();

                return;
            }

            var headerHeight = DrawHeader(area);

            var footerHeight = 32;
            var list = new Rectangle(area.X, area.Y + headerHeight, area.Width - 10,
                area.Bottom - footerHeight - 8 - (area.Y + headerHeight));

            DrawSettings(list);
            DrawFooter(new Rectangle(area.X, area.Bottom - footerHeight, area.Width, footerHeight));
        }

        /// <summary>
        /// The plugin's name, what it does, and its own switch. Returns how tall it turned out, since the
        /// description runs to as many lines as it needs.
        /// </summary>
        private int DrawHeader(Rectangle area)
        {
            var switchWidth = 90;

            Gui.Text(Gui.Fit(selected.Name, area.Width - switchWidth - 10), new Vector2(area.X, area.Y), Gui.TextHot, 1.0f);

            if (!ReferenceEquals(selected, Owner))
            {
                var box = new Rectangle(area.Right - switchWidth, area.Y - 2, switchWidth, 24);
                var tick = new Rectangle(box.Right - 20, box.Y + 2, 18, 18);

                Gui.TextRight(selected.Enabled ? "on" : "off", tick.X - 6, box.Y + 3,
                    selected.Enabled ? Gui.TextGood : Gui.TextDim);
                Gui.Tick(tick, selected.Enabled);

                if (Gui.Hover(box))
                    Gui.Tooltip(selected.Enabled
                        ? "Switch " + selected.Name + " off. Anything it has already done stays done until you restart."
                        : "Switch " + selected.Name + " on.");

                if (Gui.Click(box)) selected.Enabled = !selected.Enabled;
            }

            var y = area.Y + 24;

            foreach (var line in DescriptionLines(area.Width))
            {
                Gui.Text(line, new Vector2(area.X, y), Gui.TextDim);
                y += (int) Gui.LineHeight;
            }

            y += 6;
            Gui.HorizontalLine(area.X, y, area.Width, Gui.Divider);

            return y + 8 - area.Y;
        }

        private void DrawSettings(Rectangle list)
        {
            var settings = shown;

            if (settings.Count == 0)
            {
                Gui.Text(selected.Name + " has nothing to configure.", new Vector2(list.X, list.Y + 4), Gui.TextDim);
                return;
            }

            var visible = Math.Max(1, list.Height / SettingRowHeight);

            settingScroll.Wheel(list, settings.Count, visible, wheelNotches);
            settingScroll.Clamp(settings.Count, visible);

            for (var line = 0; line < visible; line++)
            {
                var index = settingScroll.Offset + line;
                if (index >= settings.Count) break;

                var setting = settings[index];
                var row = new Rectangle(list.X, list.Y + line * SettingRowHeight, list.Width, SettingRowHeight);

                if (editor.Draw(setting, row) == SettingEditor.Request.OpenPicker && IdPicker.CanEdit(setting))
                {
                    TextBox.Unfocus();
                    picker.Open(setting);
                    return;
                }
            }

            settingScroll.DrawBar(new Rectangle(list.Right + 4, list.Y, 6, list.Height), settings.Count, visible);
        }

        private void DrawFooter(Rectangle footer)
        {
            var width = 180;
            var button = new Rectangle(footer.Right - width, footer.Y + 3, width, 24);

            var anyChanged = false;
            foreach (var setting in selected.Settings)
            {
                if (setting.IsDefault) continue;

                anyChanged = true;
                break;
            }

            if (Gui.Button(button, resetArmed > 0 ? "Sure? Click again" : "Reset all to defaults", anyChanged))
            {
                if (resetArmed > 0)
                {
                    selected.ResetSettings();
                    resetArmed = 0;
                    editor.Forget();
                }
                else resetArmed = 120;
            }

            if (!anyChanged)
                Gui.TextRight("Everything is at its default", button.X - 10, footer.Y + 7, Gui.TextDim);
        }

        /// <summary>
        /// The plugin's description broken into lines, worked out again only when the plugin or the width changes.
        /// </summary>
        private List<string> DescriptionLines(int width)
        {
            if (ReferenceEquals(describedPlugin, selected) && describedWidth == width) return described;

            describedPlugin = selected;
            describedWidth = width;
            described.Clear();

            if (!string.IsNullOrEmpty(selected.Description))
                described.AddRange(Wrap(selected.Description, width));

            return described;
        }

        /// <summary>
        /// Breaks text into lines that fit a width.
        /// </summary>
        private static IEnumerable<string> Wrap(string text, float width)
        {
            var line = "";

            foreach (var word in text.Split(' '))
            {
                var candidate = line.Length == 0 ? word : line + " " + word;

                if (Gui.Measure(candidate).X <= width) line = candidate;
                else
                {
                    if (line.Length > 0) yield return line;
                    line = word;
                }
            }

            if (line.Length > 0) yield return line;
        }

        #endregion
    }
}
