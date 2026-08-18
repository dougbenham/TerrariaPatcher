using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using PluginLoader;

namespace TranscendPlugins.Shared.UI
{
    /// <summary>
    /// Draws one setting as a row of label, a control to match its type, and an arrow that puts it back to the
    /// value its plugin declared.
    /// </summary>
    public class SettingEditor
    {
        /// <summary>
        /// What the window should do next as a result of the row being clicked, for the things a row cannot do on
        /// its own because they replace what the window is showing.
        /// </summary>
        public enum Request
        {
            None,
            OpenPicker
        }

        /// <summary>
        /// The text boxes, kept between frames because a box being edited holds text that is not in the setting yet.
        /// </summary>
        private readonly Dictionary<Setting, TextBox> boxes = new Dictionary<Setting, TextBox>();

        /// <summary>
        /// The hotkey setting listening for a key, if any. Cleared once a key arrives, by the window's update.
        /// </summary>
        public Setting Capturing;

        public void Forget()
        {
            boxes.Clear();
            Capturing = null;
        }

        public Request Draw(Setting setting, Rectangle row)
        {
            const int resetWidth = 26;
            const int gap = 10;

            var labelWidth = (int) (row.Width * 0.42f);
            var label = new Rectangle(row.X, row.Y, labelWidth, row.Height);
            var control = new Rectangle(row.X + labelWidth + gap, row.Y,
                row.Width - labelWidth - gap - resetWidth - gap, row.Height);
            var reset = new Rectangle(row.Right - resetWidth, row.Y, resetWidth, row.Height);

            if (Gui.Hover(row) && !string.IsNullOrEmpty(setting.Description))
                Gui.Tooltip(setting.Description);

            Gui.Text(Gui.Fit(setting.Label, label.Width), new Vector2(label.X, label.Y + (label.Height - Gui.LineHeight) / 2f), Gui.TextNormal);

            var request = DrawControl(setting, control);

            if (Gui.ResetButton(reset, !setting.IsDefault, "Back to " + Describe(setting.Default)))
            {
                Reset(setting);
                return Request.None;
            }

            return request;
        }

        private Request DrawControl(Setting setting, Rectangle area)
        {
            var type = setting.ValueType;

            if (type == typeof(bool))
            {
                DrawBool(setting, area);
                return Request.None;
            }

            if (type == typeof(Hotkey))
            {
                DrawHotkey(setting, area);
                return Request.None;
            }

            if (type.IsEnum)
            {
                DrawEnum(setting, area);
                return Request.None;
            }

            Type element;
            if (setting.IdClass != null && SettingConverter.TryGetElementType(type, out element))
                return DrawCollection(setting, area);

            DrawText(setting, area);
            return Request.None;
        }

        #region Controls

        private void DrawBool(Setting setting, Rectangle area)
        {
            var on = setting.Serialize() == "true";
            var box = new Rectangle(area.X, area.Y + (area.Height - 22) / 2, 22, 22);

            Gui.Tick(box, on);

            if (Gui.Click(box) || Gui.Click(area))
                Set(setting, on ? "false" : "true");
        }

        private void DrawHotkey(Setting setting, Rectangle area)
        {
            var listening = ReferenceEquals(Capturing, setting);
            var binding = setting.Serialize();
            var text = listening
                ? "press a key, or Escape to clear"
                : (string.IsNullOrEmpty(binding) || binding == "None" ? "unbound" : binding);

            var box = new Rectangle(area.X, area.Y + 2, Math.Min(area.Width, 210), area.Height - 4);
            var hovered = Gui.Hover(box);

            Gui.Fill(box, listening ? Gui.RowSelected : Gui.PanelInner);
            Gui.Border(box, listening ? Gui.TextHot : (hovered ? Gui.Divider : Gui.PanelInner));
            Gui.TextCentered(Gui.Fit(text, box.Width - 8), box, listening ? Gui.TextHot : Gui.TextNormal);

            if (Gui.Click(box))
            {
                TextBox.Unfocus();
                Capturing = listening ? null : setting;
            }
        }

        private void DrawEnum(Setting setting, Rectangle area)
        {
            var names = Enum.GetNames(setting.ValueType);
            if (names.Length == 0) return;

            var current = Array.IndexOf(names, setting.Serialize());
            if (current < 0) current = 0;

            var arrow = 22;
            var left = new Rectangle(area.X, area.Y, arrow, area.Height);
            var right = new Rectangle(area.X + Math.Min(area.Width, 210) - arrow, area.Y, arrow, area.Height);
            var middle = new Rectangle(left.Right, area.Y, right.X - left.Right, area.Height);

            Gui.TextCentered(Gui.ArrowLeft, left, Gui.Hover(left) ? Gui.TextHot : Gui.TextNormal);
            Gui.TextCentered(Gui.ArrowRight, right, Gui.Hover(right) ? Gui.TextHot : Gui.TextNormal);
            Gui.TextCentered(Gui.Fit(PluginBase.Prettify(names[current]), middle.Width), middle, Gui.TextNormal);

            if (Gui.Click(left)) Set(setting, names[(current - 1 + names.Length) % names.Length]);
            else if (Gui.Click(right) || Gui.Click(middle)) Set(setting, names[(current + 1) % names.Length]);
        }

        private Request DrawCollection(Setting setting, Rectangle area)
        {
            var count = Count(setting.Serialize());

            var box = new Rectangle(area.X, area.Y + 2, Math.Min(area.Width, 210), area.Height - 4);
            var label = count == 1 ? "1 chosen" : count + " chosen";

            if (Gui.Button(box, label + "  " + Gui.ArrowInto)) return Request.OpenPicker;

            return Request.None;
        }

        private void DrawText(Setting setting, Rectangle area)
        {
            var box = Box(setting);
            var isColor = setting.ValueType == typeof(Color);
            var swatch = isColor ? 22 : 0;

            var field = new Rectangle(area.X, area.Y + 2, area.Width - swatch - (isColor ? 6 : 0), area.Height - 4);
            box.Draw(field);

            if (isColor)
            {
                var preview = new Rectangle(field.Right + 6, field.Y, swatch, field.Height);
                Color color;
                if (TryReadColor(setting, out color))
                {
                    Gui.Fill(preview, color);
                    Gui.Border(preview, Gui.Divider);
                }
            }
        }

        #endregion

        #region Values

        /// <summary>
        /// The box for a setting, made on the first frame the setting is shown and filled from its current value.
        /// </summary>
        private TextBox Box(Setting setting)
        {
            TextBox box;
            if (boxes.TryGetValue(setting, out box))
            {
                // Left alone while it is being typed into, so that what the player is halfway through writing is
                // not overwritten by the value that is still saved.
                if (!box.Focused) box.Text = setting.Serialize();
                return box;
            }

            var owned = setting;
            box = new TextBox { Text = setting.Serialize() };
            box.Committed = text => Commit(owned, text);
            box.Cancelled = () => { box.Text = owned.Serialize(); };

            boxes[setting] = box;
            return box;
        }

        private void Commit(Setting setting, string text)
        {
            if (text == setting.Serialize()) return;

            if (SettingConverter.IsNumeric(setting.ValueType))
            {
                double number;
                if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
                {
                    Revert(setting);
                    return;
                }

                if (setting.Minimum.HasValue && number < setting.Minimum.Value) number = setting.Minimum.Value;
                if (setting.Maximum.HasValue && number > setting.Maximum.Value) number = setting.Maximum.Value;

                if (IsWholeNumber(setting.ValueType)) number = Math.Round(number);

                text = number.ToString("R", CultureInfo.InvariantCulture);
            }

            Set(setting, text);
            Revert(setting);
        }

        private void Set(Setting setting, string text)
        {
            try
            {
                setting.SetFrom(text);
            }
            catch (Exception ex)
            {
                Terraria.Main.NewText("Could not set " + setting.FullName + ": " + ex.Message, 230, 130, 130);
            }
        }

        private void Reset(Setting setting)
        {
            setting.Reset();
            Revert(setting);
        }

        /// <summary>
        /// Puts a box back in step with the value that is actually saved, after the setting has changed under it.
        /// </summary>
        private void Revert(Setting setting)
        {
            TextBox box;
            if (boxes.TryGetValue(setting, out box)) box.Text = setting.Serialize();
        }

        private static bool TryReadColor(Setting setting, out Color color)
        {
            color = Color.White;

            try
            {
                color = (Color) SettingConverter.Deserialize(setting.Serialize(), typeof(Color));
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsWholeNumber(Type type)
        {
            return type != typeof(float) && type != typeof(double) && type != typeof(decimal);
        }

        /// <summary>
        /// The values of a collection setting, in the comma separated form Plugins.ini stores them in.
        /// </summary>
        public static IEnumerable<string> Split(string text)
        {
            if (string.IsNullOrEmpty(text)) yield break;

            foreach (var part in text.Split(','))
            {
                var trimmed = part.Trim();
                if (trimmed.Length > 0) yield return trimmed;
            }
        }

        /// <summary>
        /// How many values a collection setting holds.
        /// </summary>
        public static int Count(string text)
        {
            var count = 0;
            foreach (var value in Split(text)) count++;
            return count;
        }

        private static string Describe(string value)
        {
            if (string.IsNullOrEmpty(value)) return "nothing";

            return Gui.Measure(value).X > 300 ? value.Substring(0, 40) + Gui.Ellipsis : value;
        }

        #endregion
    }
}
