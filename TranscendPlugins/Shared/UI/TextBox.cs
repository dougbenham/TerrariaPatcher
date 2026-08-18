using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameInput;

namespace TranscendPlugins.Shared.UI
{
    /// <summary>
    /// A line of editable text, fed by the same <see cref="Main.GetInputText"/> that Terraria's own sign, chest and
    /// server address fields use, so it gets backspace repeat, clipboard and IME handling for free.
    /// </summary>
    /// <remarks>
    /// <see cref="UpdateFocused"/> has to be called once a tick from a plugin update hook, never from a draw hook:
    /// GetInputText takes the keys typed since it last ran, and a draw can happen more than once a tick.
    /// </remarks>
    public class TextBox
    {
        /// <summary>
        /// The one box taking typing, since the keys typed since the last tick can only be handed to one of them.
        /// </summary>
        private static TextBox focused;

        private static int blinker;

        public string Text = "";
        public string Placeholder = "";

        /// <summary>
        /// Raised for every keystroke, for a box such as a filter that acts as it is typed.
        /// </summary>
        public Action<string> Changed;

        /// <summary>
        /// Raised on Enter, or when the box stops being the focused one, for a box whose value is only worth
        /// reading once it is finished.
        /// </summary>
        public Action<string> Committed;

        /// <summary>
        /// Raised on Escape, for a box that should put back whatever it was editing.
        /// </summary>
        public Action Cancelled;

        public bool Focused
        {
            get { return ReferenceEquals(focused, this); }
        }

        public static bool AnyFocused
        {
            get { return focused != null; }
        }

        public void Focus()
        {
            if (Focused) return;

            Unfocus();

            focused = this;
            blinker = 0;

            // Drops keys typed before the box was clicked, so that the click does not bring a stray character with it.
            Main.clrInput();
        }

        /// <summary>
        /// Takes the focus off whichever box has it, committing what was typed into it.
        /// </summary>
        public static void Unfocus()
        {
            var box = focused;
            if (box == null) return;

            focused = null;

            if (box.Committed != null) box.Committed(box.Text);
        }

        /// <summary>
        /// Hands the keys typed since the last tick to the focused box. Called once a tick while the window is open.
        /// </summary>
        public static void UpdateFocused()
        {
            blinker = (blinker + 1) % 40;

            var box = focused;
            if (box == null) return;

            PlayerInput.WritingText = true;
            Main.instance.HandleIME();

            var before = box.Text;
            box.Text = Main.GetInputText(box.Text);

            if (Main.inputTextEscape)
            {
                focused = null;
                if (box.Cancelled != null) box.Cancelled();
                return;
            }

            if (box.Text != before)
            {
                Gui.PlayClick();
                if (box.Changed != null) box.Changed(box.Text);
            }

            if (Main.inputTextEnter)
            {
                focused = null;
                if (box.Committed != null) box.Committed(box.Text);
            }
        }

        public void Draw(Rectangle area)
        {
            var hovered = Gui.Hover(area);

            Gui.Fill(area, Gui.PanelInner);
            Gui.Border(area, Focused ? Gui.TextHot : (hovered ? Gui.Divider : Gui.PanelInner));

            var padding = 6;
            var showPlaceholder = !Focused && string.IsNullOrEmpty(Text);
            var text = showPlaceholder ? Placeholder : Text;
            var color = showPlaceholder ? Gui.TextDim : Gui.TextNormal;

            // The end of the text is what matters while typing, so a long value scrolls rather than being cut short.
            var width = area.Width - padding * 2;
            var caret = Focused && blinker < 20 ? "|" : "";
            var visible = Trim(text, width - Gui.Measure("|").X);

            var y = area.Y + (area.Height - Gui.LineHeight) / 2f;
            Gui.Text(visible + caret, new Vector2(area.X + padding, y), color);

            if (Gui.Click(area)) Focus();
        }

        /// <summary>
        /// The tail of the text that fits the width, so that the caret stays in view.
        /// </summary>
        private static string Trim(string text, float width)
        {
            if (string.IsNullOrEmpty(text) || Gui.Measure(text).X <= width) return text;

            for (var start = 1; start < text.Length; start++)
            {
                var tail = text.Substring(start);
                if (Gui.Measure(tail).X <= width) return tail;
            }

            return "";
        }
    }
}
