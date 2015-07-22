using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace GTRPlugins.UI
{
    public class Button
    {
        public enum ButtonPosX { Left, Right, Center };
        public enum ButtonPosY { Top, Bottom, Center };

        public string Label;
        public ButtonPosX AnchorX = ButtonPosX.Left;
        public ButtonPosY AnchorY = ButtonPosY.Top;
        public ButtonPosX RelateX = ButtonPosX.Left;
        public ButtonPosY RelateY = ButtonPosY.Top;
        public Vector2 Position;
        public float Scale = 1;
        public Color Color = Color.Silver;
        public Color HoverColor = Color.White;
        public Color StrokeColor = Color.Black;
        public int StrokeWidth = 2;
        public event EventHandler MouseDown;
        private bool _hover = false;

        public Button(string label, Vector2 position, EventHandler mouseDown)
        {
            Label = label;
            Position = position;
            MouseDown += mouseDown;
        }

        public void Draw()
        {
            Vector2 size = Main.fontMouseText.MeasureString(Label) * Scale;
            float x = Position.X;
            float y = Position.Y;
            switch (RelateX)
            {
                case ButtonPosX.Right:
                    x = Main.screenWidth - Position.X;
                    break;

                case ButtonPosX.Center:
                    x = Main.screenWidth / 2 + Position.X;
                    break;
            }
            switch (RelateY)
            {
                case ButtonPosY.Bottom:
                    y = Main.screenHeight - Position.Y;
                    break;

                case ButtonPosY.Center:
                    y = Main.screenHeight / 2 + Position.Y;
                    break;
            }
            float anchorX = 0;
            float anchorY = 0;
            switch (AnchorX)
            {
                case ButtonPosX.Right:
                    anchorX = size.X;
                    break;

                case ButtonPosX.Center:
                    anchorX = size.X / 2;
                    break;
            }
            switch (AnchorY)
            {
                case ButtonPosY.Bottom:
                    anchorY = size.Y;
                    break;

                case ButtonPosY.Center:
                    anchorY = size.Y / 2;
                    break;
            }
            Vector2 origin = new Vector2(anchorX, anchorY);
            for (int i = 0; i < 5; i++)
            {
                int strokeX = 0;
                int strokeY = 0;
                Color color = StrokeColor;
                switch (i)
                {
                    case 0:
                        strokeX = -StrokeWidth;
                        break;

                    case 1:
                        strokeX = StrokeWidth;
                        break;

                    case 2:
                        strokeY = -StrokeWidth;
                        break;

                    case 3:
                        strokeY = StrokeWidth;
                        break;

                    case 4:
                        float pulse = (float)Main.mouseTextColor / 255;
                        color = _hover ? new Color((int)(HoverColor.R * pulse), (int)(HoverColor.G * pulse), (int)(HoverColor.B * pulse)) : new Color((int)(Color.R * pulse), (int)(Color.G * pulse), (int)(Color.B * pulse));
                        break;
                }
                Main.spriteBatch.DrawString(Main.fontMouseText, Label, new Vector2((float)(x + strokeX), (float)(y + strokeY)), color, 0f, origin, Scale, SpriteEffects.None, 0f);
            }
            if ((Main.mouseX > (x - anchorX - 3 * Scale)) && (Main.mouseX < (x + size.X - anchorX + 3 * Scale)) && (Main.mouseY > (y - anchorY) - 2 * Scale) && (Main.mouseY < (y + size.Y - anchorY - 7 * Scale)))
            {
                if (!_hover)
                {
                    Main.PlaySound(12, -1, -1, 1);
                }
                _hover = true;
                Main.player[Main.myPlayer].mouseInterface = true;
                if (Main.mouseLeftRelease && Main.mouseLeft)
                {
                    Main.PlaySound(12, -1, -1, 1);
                    Main.mouseLeftRelease = false;
                    if (MouseDown != null)
                    {
                        MouseDown(this, EventArgs.Empty);
                    }
                }
            }
            else
            {
                _hover = false;
            }
        }
    }
}
