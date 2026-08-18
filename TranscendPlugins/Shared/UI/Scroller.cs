using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace TranscendPlugins.Shared.UI
{
    /// <summary>
    /// How far down a list of evenly sized rows has been scrolled, kept in whole rows so that a row is never drawn
    /// half outside the list it belongs to.
    /// </summary>
    public class Scroller
    {
        public int Offset;

        private bool dragging;
        private int grabOffset;

        public void Reset()
        {
            Offset = 0;
            dragging = false;
        }

        public void Clamp(int rowCount, int visibleRows)
        {
            var most = Math.Max(0, rowCount - visibleRows);

            if (Offset > most) Offset = most;
            if (Offset < 0) Offset = 0;
        }

        /// <summary>
        /// Scrolls the list when the wheel is turned over it.
        /// </summary>
        public void Wheel(Rectangle area, int rowCount, int visibleRows, int notches)
        {
            if (notches != 0 && Gui.Contains(area))
                Offset -= notches;

            Clamp(rowCount, visibleRows);
        }

        /// <summary>
        /// Draws the bar down the right of a list, and lets it be dragged. Nothing is drawn while everything fits.
        /// </summary>
        public void DrawBar(Rectangle track, int rowCount, int visibleRows)
        {
            Clamp(rowCount, visibleRows);

            if (rowCount <= visibleRows)
            {
                dragging = false;
                return;
            }

            Gui.Fill(track, Gui.PanelInner);

            var most = rowCount - visibleRows;
            var thumbHeight = Math.Max(20, track.Height * visibleRows / rowCount);
            var travel = track.Height - thumbHeight;
            var thumbTop = track.Y + (most == 0 ? 0 : travel * Offset / most);
            var thumb = new Rectangle(track.X, thumbTop, track.Width, thumbHeight);

            var hovered = Gui.Hover(thumb);

            if (!dragging && hovered && Main.mouseLeft && Main.mouseLeftRelease)
            {
                dragging = true;
                grabOffset = Main.mouseY - thumb.Y;
                Main.mouseLeftRelease = false;
            }
            else if (dragging && Gui.Contains(track))
            {
                Main.LocalPlayer.mouseInterface = true;
            }

            if (dragging)
            {
                if (!Main.mouseLeft)
                    dragging = false;
                else if (travel > 0)
                    Offset = (int) Math.Round((Main.mouseY - grabOffset - track.Y) * most / (double) travel);

                Clamp(rowCount, visibleRows);
            }

            Gui.Fill(thumb, dragging || hovered ? Gui.RowSelected : Gui.Divider);
        }
    }
}
