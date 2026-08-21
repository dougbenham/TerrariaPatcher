using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;

namespace TranscendPlugins.Shared.UI
{
    /// <summary>
    /// Drawing and mouse handling for a window drawn straight onto Terraria's own sprite batch, in the immediate
    /// mode style the game itself uses: each control is drawn and asked whether it was clicked in the same call,
    /// so there is no widget tree to keep in step with the settings behind it.
    /// </summary>
    /// <remarks>
    /// Only ever called from <c>IPluginDrawUI.OnDrawUI</c>, where a batch is already open and Terraria has put
    /// <see cref="Main.mouseX"/> and <see cref="Main.screenWidth"/> into interface coordinates, so screen
    /// positions and mouse positions can be compared directly whatever the player's UI scale is.
    /// </remarks>
    public static class Gui
    {
        public static readonly Color PanelBack = new Color(63, 65, 151);
        public static readonly Color PanelInner = new Color(35, 40, 83);
        public static readonly Color RowHover = new Color(80, 90, 170);
        public static readonly Color RowSelected = new Color(96, 110, 200);
        public static readonly Color Divider = new Color(100, 110, 180);

        public static readonly Color TextNormal = new Color(233, 233, 233);
        public static readonly Color TextDim = new Color(150, 155, 175);
        public static readonly Color TextHot = Color.White;
        public static readonly Color TextGood = new Color(140, 230, 140);
        public static readonly Color TextBad = new Color(230, 130, 130);
        public static readonly Color TextWarn = new Color(240, 205, 120);

        public const float TextScale = 0.85f;

        /// <summary>
        /// How tall a line of text actually is at <see cref="TextScale"/>, measured from the font rather than
        /// assumed, since Terraria builds its font from the loaded language and the size follows from that.
        /// </summary>
        public static float TextHeight
        {
            get { return Measure("Ag").Y; }
        }

        /// <summary>
        /// The height a row of the window is given: a line of text with room to breathe above and below it. Every
        /// list and control is sized from this, so that no box can end up shorter than the text inside it.
        /// </summary>
        public static int RowHeight
        {
            get { return (int) TextHeight + 8; }
        }

        /// <summary>
        /// The rectangle under the mouse, kept from one frame to the next so that the hover sound plays once when
        /// the mouse arrives at a control rather than on every frame it rests there.
        /// </summary>
        private static Rectangle hotNow;
        private static Rectangle hotLast;

        public static void BeginFrame()
        {
            hotNow = Rectangle.Empty;
        }

        public static void EndFrame()
        {
            if (hotNow != hotLast && hotNow != Rectangle.Empty)
                PlayHover();

            hotLast = hotNow;
        }

        #region Mouse

        public static bool Contains(Rectangle area)
        {
            return area.Contains(Main.mouseX, Main.mouseY);
        }

        /// <summary>
        /// Whether the mouse is over the area, marking it as interface so that the click does not also reach the
        /// world behind the window.
        /// </summary>
        public static bool Hover(Rectangle area)
        {
            if (!Contains(area)) return false;

            hotNow = area;
            Main.LocalPlayer.mouseInterface = true;
            return true;
        }

        /// <summary>
        /// Whether the area was left clicked, taking the click so that nothing else acts on it too.
        /// </summary>
        public static bool Click(Rectangle area)
        {
            if (!Hover(area)) return false;
            if (!Main.mouseLeft || !Main.mouseLeftRelease) return false;

            Main.mouseLeftRelease = false;
            PlayClick();
            return true;
        }

        public static void PlayHover()
        {
            SoundEngine.PlaySound(12, -1, -1, 1);
        }

        public static void PlayClick()
        {
            SoundEngine.PlaySound(12, -1, -1, 1);
        }

        #endregion

        #region Glyphs

        /// <summary>
        /// Terraria builds its font from whichever language is loaded, so a decorative character is not certain to
        /// be in it, and one that is missing is drawn as the font's stand in character. Each is asked for once and
        /// swapped for plain text where the font has nothing for it.
        /// </summary>
        private static readonly Dictionary<string, string> glyphs = new Dictionary<string, string>();

        public static string Glyph(string preferred, string fallback)
        {
            string resolved;
            if (glyphs.TryGetValue(preferred, out resolved)) return resolved;

            resolved = Supported(preferred) ? preferred : fallback;
            glyphs[preferred] = resolved;
            return resolved;
        }

        private static bool Supported(string text)
        {
            var font = FontAssets.MouseText.Value;

            foreach (var character in text)
                if (!font.IsCharacterSupported(character)) return false;

            return true;
        }

        public static string Ellipsis { get { return Glyph("…", "..."); } }
        public static string ArrowLeft { get { return Glyph("◄", "<"); } }
        public static string ArrowRight { get { return Glyph("►", ">"); } }
        public static string ArrowInto { get { return Glyph("▸", ">"); } }
        public static string Close { get { return Glyph("✕", "X"); } }
        public static string Resort { get { return Glyph("↕", "^v"); } }
        public static string Revert { get { return Glyph("↺", "R"); } }
        public static string Crumb { get { return Glyph("›", ">"); } }

        #endregion

        #region Text
		
        public static Vector2 Measure(string text, float scale = TextScale)
        {
            if (string.IsNullOrEmpty(text)) return Vector2.Zero;

            var r = FontAssets.MouseText.Value.MeasureString(text) * scale;
            return new Vector2(r.X, 16f * scale);
        }

        private static float Center(string text, Rectangle area, float scale = TextScale)
        {
            return Math.Max(0f, (area.Height - Measure(text, scale).Y) / 2f);
        }

        public static void Text(string text, Vector2 position, Color color, float scale = TextScale)
        {
            if (string.IsNullOrEmpty(text)) return;

            Utils.DrawBorderString(Main.spriteBatch, text, position, color, scale);
        }

        public static void TextLeftCentered(string text, Rectangle area, Color color, float scale = TextScale)
        {
            Text(text, new Vector2(area.X, area.Y + Center(text, area, scale)), color, scale);
        }

        public static void TextRight(string text, float right, float y, Color color, float scale = TextScale)
        {
            Text(text, new Vector2(right - Measure(text, scale).X, y), color, scale);
        }

        public static void TextCentered(string text, Rectangle area, Color color, float scale = TextScale)
        {
	        Text(text, new Vector2(area.X + (area.Width - Measure(text, scale).X) / 2f, area.Y + Center(text, area, scale)), color, scale);
        }

        /// <summary>
        /// The text cut down to fit a width, with an ellipsis where it was cut.
        /// </summary>
        public static string Fit(string text, float width, float scale = TextScale)
        {
            if (string.IsNullOrEmpty(text) || Measure(text, scale).X <= width) return text;

            var ellipsis = Ellipsis;

            for (var length = text.Length - 1; length > 0; length--)
            {
                var shortened = text.Substring(0, length) + ellipsis;
                if (Measure(shortened, scale).X <= width) return shortened;
            }

            return ellipsis;
        }

        #endregion

        #region Shapes

        public static void Fill(Rectangle area, Color color)
        {
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, area, color);
        }

        public static void Panel(Rectangle area, Color color)
        {
            Utils.DrawInvBG(Main.spriteBatch, area, color);
        }

        public static void HorizontalLine(int x, int y, int width, Color color)
        {
            Fill(new Rectangle(x, y, width, 1), color);
        }

        public static void VerticalLine(int x, int y, int height, Color color)
        {
            Fill(new Rectangle(x, y, 1, height), color);
        }

        /// <summary>
        /// The inventory tick box, which is what Terraria's own settings use for a yes or no.
        /// </summary>
        public static void Tick(Rectangle area, bool on)
        {
            var texture = (on ? TextureAssets.InventoryTickOn : TextureAssets.InventoryTickOff).Value;
            var scale = Math.Min(area.Width / (float) texture.Width, area.Height / (float) texture.Height);
            var size = new Vector2(texture.Width, texture.Height) * scale;

            Main.spriteBatch.Draw(texture,
                new Vector2(area.X + (area.Width - size.X) / 2f, area.Y + (area.Height - size.Y) / 2f),
                null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        #endregion

        #region Controls

        /// <summary>
        /// A labelled box that reports whether it was clicked. A disabled button is drawn dimmed and cannot be.
        /// </summary>
        public static bool Button(Rectangle area, string label, bool enabled = true)
        {
            var hovered = enabled && Hover(area);

            Fill(area, hovered ? RowHover : PanelInner);
            Border(area, hovered ? Divider : PanelInner);
            TextCentered(Fit(label, area.Width - 4), area, enabled ? (hovered ? TextHot : TextNormal) : TextDim);

            return enabled && Click(area);
        }

        public static void Border(Rectangle area, Color color)
        {
            HorizontalLine(area.X, area.Y, area.Width, color);
            HorizontalLine(area.X, area.Y + area.Height - 1, area.Width, color);
            VerticalLine(area.X, area.Y, area.Height, color);
            VerticalLine(area.X + area.Width - 1, area.Y, area.Height, color);
        }

        /// <summary>
        /// The arrow that puts a setting back to the value the plugin declared. Drawn faded and inert while the
        /// setting is already at it, so that the row still lines up with the others.
        /// </summary>
        public static bool ResetButton(Rectangle area, bool available, string tooltip)
        {
            var hovered = available && Hover(area);

            TextCentered(Revert, area, available ? (hovered ? TextHot : TextNormal) : new Color(90, 95, 115));

            if (hovered) Tooltip(tooltip);

            return available && Click(area);
        }

        /// <summary>
        /// Hands text to Terraria to draw as mouse text once the window is done, so that it lands on top of it.
        /// </summary>
        public static void Tooltip(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            Main.instance.MouseText(text);
        }

        #endregion
    }
}
