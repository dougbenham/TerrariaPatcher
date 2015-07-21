using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace GTRPlugins.UI
{
    public class Button
    {
        public enum ButtonPosX
        {
            Left,
            Right,
            Center
        }
        public enum ButtonPosY
        {
            Top,
            Bottom,
            Center
        }
        public string Label;
        public ButtonPosX AnchorX;
        public ButtonPosY AnchorY;
        public ButtonPosX RelateX;
        public ButtonPosY RelateY;
        public Vector2 Position;
        public float Scale = 1f;
        public Color Color = Color.Silver;
        public Color HoverColor = Color.White;
        public Color StrokeColor = Color.Black;
        public int StrokeWidth = 2;
        private bool _hover;
        public event EventHandler MouseDown;
        public Button(string label, Vector2 position, EventHandler mouseDown)
        {
            Label = label;
            Position = position;
            MouseDown += mouseDown;
        }
        public void Draw()
        {
            Vector2 vector = Main.fontMouseText.MeasureString(Label) * Scale;
            float num = Position.X;
            float num2 = Position.Y;
            switch (RelateX)
            {
                case ButtonPosX.Right:
                    num = (float)Main.screenWidth - Position.X;
                    break;
                case ButtonPosX.Center:
                    num = (float)(Main.screenWidth / 2) + Position.X;
                    break;
            }
            switch (RelateY)
            {
                case ButtonPosY.Bottom:
                    num2 = (float)Main.screenHeight - Position.Y;
                    break;
                case ButtonPosY.Center:
                    num2 = (float)(Main.screenHeight / 2) + Position.Y;
                    break;
            }
            float num3 = 0f;
            float num4 = 0f;
            switch (AnchorX)
            {
                case ButtonPosX.Right:
                    num3 = vector.X;
                    break;
                case ButtonPosX.Center:
                    num3 = vector.X / 2f;
                    break;
            }
            switch (AnchorY)
            {
                case ButtonPosY.Bottom:
                    num4 = vector.Y;
                    break;
                case ButtonPosY.Center:
                    num4 = vector.Y / 2f;
                    break;
            }
            Vector2 origin = new Vector2(num3, num4);
            for (int i = 0; i < 5; i++)
            {
                int num5 = 0;
                int num6 = 0;
                Color color = StrokeColor;
                switch (i)
                {
                    case 0:
                        num5 = -StrokeWidth;
                        break;
                    case 1:
                        num5 = StrokeWidth;
                        break;
                    case 2:
                        num6 = -StrokeWidth;
                        break;
                    case 3:
                        num6 = StrokeWidth;
                        break;
                    case 4:
                        {
                            float num7 = (float)Main.mouseTextColor / 255f;
                            color = (_hover ? new Color((int)((float)HoverColor.R * num7), (int)((float)HoverColor.G * num7), (int)((float)HoverColor.B * num7)) : new Color((int)((float)Color.R * num7), (int)((float)Color.G * num7), (int)((float)Color.B * num7)));
                            break;
                        }
                }
                Main.spriteBatch.DrawString(Main.fontMouseText, Label, new Vector2(num + (float)num5, num2 + (float)num6), color, 0f, origin, Scale, SpriteEffects.None, 0f);
            }
            if ((float)Main.mouseX > num - num3 - 3f * Scale && (float)Main.mouseX < num + vector.X - num3 + 3f * Scale && (float)Main.mouseY > num2 - num4 - 2f * Scale && (float)Main.mouseY < num2 + vector.Y - num4 - 7f * Scale)
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
                        MouseDown(this, EventArgs.Empty);
                }
            }
            else
            {
                _hover = false;
            }
        }
    }
}
