using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;

namespace GTRPlugins.Utils
{
    public static class Input
    {
        private static KeyboardState _lastKeyState;
        private static MouseState _mouseState;
        private static MouseState _lastMouseState;
        public static bool PlayerHasKeyControl
        {
            get
            {
                return !Main.blockInput && !Main.editChest && !Main.editSign && !Main.chatMode;
            }
        }
        public static Vector2 MousePosition
        {
            get
            {
                return new Vector2((float)Main.mouseX, (float)Main.mouseY);
            }
        }
        public static float ScrollDelta
        {
            get
            {
                return (float)(Input._mouseState.ScrollWheelValue - Input._lastMouseState.ScrollWheelValue);
            }
        }
        public static bool KeyPressed(Keys key, bool ignoreIfPlayerDoesNotHaveControl = true)
        {
            return (!ignoreIfPlayerDoesNotHaveControl || Input.PlayerHasKeyControl) && Main.keyState.IsKeyDown(key) && Input._lastKeyState.IsKeyUp(key);
        }
        public static bool KeyReleased(Keys key, bool ignoreIfPlayerDoesNotHaveControl = true)
        {
            return (ignoreIfPlayerDoesNotHaveControl && !Input.PlayerHasKeyControl) || (Main.keyState.IsKeyUp(key) && Input._lastKeyState.IsKeyDown(key));
        }
        public static bool IsKeyDown(Keys key, bool ignoreIfPlayerDoesNotHaveControl = true)
        {
            return (!ignoreIfPlayerDoesNotHaveControl || Input.PlayerHasKeyControl) && Main.keyState.IsKeyDown(key);
        }
        public static bool IsKeyUp(Keys key, bool ignoreIfPlayerDoesNotHaveControl = true)
        {
            return (!ignoreIfPlayerDoesNotHaveControl || Input.PlayerHasKeyControl) && Main.keyState.IsKeyUp(key);
        }
        public static bool MouseButtonPressed(MouseButton button)
        {
            return Input.GetMouseButton(Input._mouseState, button) && !Input.GetMouseButton(Input._lastMouseState, button);
        }
        public static bool MouseButtonReleased(MouseButton button)
        {
            return !Input.GetMouseButton(Input._mouseState, button) && Input.GetMouseButton(Input._lastMouseState, button);
        }
        public static bool IsMouseButtonDown(MouseButton button)
        {
            return Input.GetMouseButton(Input._mouseState, button);
        }
        public static bool IsMouseButtonUp(MouseButton button)
        {
            return !Input.IsMouseButtonDown(button);
        }
        private static bool GetMouseButton(MouseState state, MouseButton button)
        {
            bool result;
            switch (button)
            {
                case MouseButton.Left:
                    result = (state.LeftButton == ButtonState.Pressed);
                    break;
                case MouseButton.Right:
                    result = (state.RightButton == ButtonState.Pressed);
                    break;
                case MouseButton.Middle:
                    result = (state.MiddleButton == ButtonState.Pressed);
                    break;
                default:
                    result = false;
                    break;
            }
            return result;
        }
        internal static void Update()
        {
            Input._lastKeyState = Main.keyState;
            Input._lastMouseState = Input._mouseState;
            Input._mouseState = Mouse.GetState();
        }
    }
}
