using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PluginLoader;
using Terraria;
using Terraria.GameInput;

namespace TranscendPlugins.Shared.UI
{
    public partial class SettingsWindow
    {
        private const int Padding = 12;
        private const int LeftPaneWidth = 218;
        private const float TitleScale = 1.1f;

        /// <summary>
        /// Every height is worked out from how tall the font actually is, so that nothing can end up shorter than
        /// the text it holds whatever language the game is running in.
        /// </summary>
        private static int ControlHeight
        {
            get { return Gui.RowHeight; }
        }

        private static int TitleHeight
        {
            get { return (int) Gui.Measure("Ag", TitleScale).Y + 8; }
        }

        private static int PluginRowHeight
        {
            get { return Gui.RowHeight; }
        }

        private static int SettingRowHeight
        {
            get { return Gui.RowHeight + 4; }
        }

        /// <summary>
        /// Draws the window. Called from the draw hook, where the mouse and the screen are measured the same way
        /// the window is, so the two can be compared without scaling either.
        /// </summary>
        public void Draw()
        {
            if (!IsOpen) return;

            // Terraria only takes typed characters while the IME service is on, and it turns the service off
            // again from its own chat layer whenever WritingText is not set. That layer is drawn just after this
            // one, and PlayerInput clears WritingText at the end of every update, so saying so once a tick is not
            // enough: it has to be said again here, in the draw, or the service spends most of each frame off and
            // the characters typed in that time are dropped. Backspace is unaffected, being read from the
            // keyboard directly, which is why it alone appears to work without this.
            PlayerInput.WritingText = true;
            Main.instance.HandleIME();

            var width = Math.Min(880, Main.screenWidth - 60);
            var height = Math.Min(Math.Max(560, Gui.RowHeight * 15), Main.screenHeight - 60);
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
            Gui.TextLeftCentered("TerrariaPatcher Plugin Settings", area, Gui.TextHot, TitleScale);
            
            var close = new Rectangle(area.Right - ControlHeight, area.Y, ControlHeight, ControlHeight);
            if (Gui.Button(close, Gui.Close)) Close();
        }

        #region Plugin list

        private void DrawPluginList(Rectangle area)
        {
            var filterRow = new Rectangle(area.X, area.Y, area.Width, ControlHeight);
            pluginFilter.Draw(filterRow);

            var list = new Rectangle(area.X, filterRow.Bottom + 8, area.Width - 10, area.Bottom - (filterRow.Bottom + 8));

            var matches = Matching();
            var visible = Math.Max(1, list.Height / PluginRowHeight);

            visiblePluginRows = visible;

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

            var box = Math.Min(18, row.Height - 4);
            var tick = new Rectangle(row.X + 2, row.Y + (row.Height - box) / 2, box, box);
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
            Gui.TextLeftCentered(Gui.Fit(plugin.Name, name.Width - 4), name,
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

            var footerHeight = ControlHeight + 8;
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
            var nameRow = new Rectangle(area.X, area.Y, area.Width - switchWidth - 10, ControlHeight);

            Gui.TextLeftCentered(Gui.Fit(selected.Name, nameRow.Width), nameRow, Gui.TextHot, 1.0f);

            if (!ReferenceEquals(selected, Owner))
            {
                var box = new Rectangle(area.Right - switchWidth, area.Y, switchWidth, ControlHeight);
                var tickSize = Math.Min(18, box.Height - 4);
                var tick = new Rectangle(box.Right - tickSize - 2, box.Y + (box.Height - tickSize) / 2, tickSize, tickSize);

                Gui.TextRight(selected.Enabled ? "on" : "off", tick.X - 6,
                    box.Y + (box.Height - Gui.TextHeight) / 2f,
                    selected.Enabled ? Gui.TextGood : Gui.TextDim);
                Gui.Tick(tick, selected.Enabled);

                if (Gui.Hover(box))
                    Gui.Tooltip(selected.Enabled
                        ? "Switch " + selected.Name + " off. Anything it has already done stays done until you restart."
                        : "Switch " + selected.Name + " on.");

                if (Gui.Click(box)) selected.Enabled = !selected.Enabled;
            }

            var y = nameRow.Bottom + 2;

            foreach (var line in DescriptionLines(area.Width))
            {
                Gui.Text(line, new Vector2(area.X, y), Gui.TextDim);
                y += (int) Gui.TextHeight + 2;
            }

            if (selected.RequiresRestart && changed.Contains(selected.Name))
            {
                var note = new Rectangle(area.X, y + 2, area.Width, (int) Gui.TextHeight + 2);

                Gui.Text("Restart Terraria for these changes to fully apply.",
                    new Vector2(note.X, note.Y), Gui.TextWarn);

                if (Gui.Hover(note))
                    Gui.Tooltip(selected.Name + " changes things the game then keeps its own copy of, so what it" +
                                " has already set stays as it was until the game is started again.");

                y = note.Bottom;
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
            var button = new Rectangle(footer.Right - width, footer.Y + 3, width, ControlHeight);

            // Not offered for the plugin that draws this window: reloading it would retire the copy whose draw
            // this call is part of, leaving a window on screen that nothing updates any more.
            if (!selected.RequiresRestart && !ReferenceEquals(selected, Owner))
            {
                var reload = new Rectangle(footer.X, button.Y, 110, button.Height);

                if (Gui.Hover(reload))
                    Gui.Tooltip("Compile " + selected.Name + "'s source again and swap the running copy for the" +
                                " new one, for picking up an edit to its .cs file without restarting. Its settings" +
                                " are kept.");

                if (Gui.Button(reload, "Hot Reload"))
                {
                    HotReload();
                    return;
                }
            }

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
                Gui.TextRight("Everything is at its default", button.X - 10,
                    button.Y + (button.Height - Gui.TextHeight) / 2f, Gui.TextDim);
        }

        /// <summary>
        /// Builds the selected plugin from its source again and picks the new copy back up, since the old object
        /// the window was showing no longer exists once the loader has swapped it out.
        /// </summary>
        private void HotReload()
        {
            var name = selected.Name;

            string error;
            if (!Loader.Reload(selected, out error))
            {
                Main.NewText(error, Gui.TextBad.R, Gui.TextBad.G, Gui.TextBad.B);
                return;
            }

            changed.Remove(name);
            Refresh();

            foreach (var plugin in plugins)
            {
                if (plugin.Name != name) continue;

                selected = plugin;
                ReadSettings();
                break;
            }

            settingScroll.Reset();
            picker.Close();
            resetArmed = 0;

            Main.NewText(name + " was reloaded from source.", Gui.TextGood.R, Gui.TextGood.G, Gui.TextGood.B);
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
