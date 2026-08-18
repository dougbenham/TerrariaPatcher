using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;
using Terraria.GameInput;

namespace TranscendPlugins.Shared.UI
{
    /// <summary>
    /// The in game window for reading and changing every plugin's settings: the plugins down the left, the settings
    /// of whichever one is picked down the right.
    /// </summary>
    /// <remarks>
    /// The work is split by which of Terraria's own passes it belongs in. Typing and key presses are read in
    /// <see cref="Update"/>, from a plugin update hook, because the keys typed since the last tick can only be
    /// taken once. Everything to do with the mouse is done in <see cref="Draw"/>, from the draw hook, because that
    /// is the only point at which the mouse position and the window are measured the same way.
    /// </remarks>
    public partial class SettingsWindow
    {
        public bool IsOpen { get; private set; }

        /// <summary>
        /// The plugin the window belongs to, which is the one plugin the window will not offer to switch off, since
        /// switching it off would take the window with it.
        /// </summary>
        public PluginBase Owner;

        /// <summary>
        /// The binding that opens the window, watched directly while the window is open because plugin hotkeys are
        /// deliberately not delivered while it has the keyboard.
        /// </summary>
        public Hotkey ToggleKey;

        private readonly List<PluginBase> plugins = new List<PluginBase>();
        private PluginBase selected;

        /// <summary>
        /// What the panes are showing, worked out when the selection or the search changes rather than on every
        /// frame, since the window is drawn as often as the game is.
        /// </summary>
        private readonly List<Setting> shown = new List<Setting>();
        private readonly List<PluginBase> matched = new List<PluginBase>();
        private string matchedFor;

        private readonly List<string> described = new List<string>();
        private PluginBase describedPlugin;
        private int describedWidth = -1;

        private readonly TextBox pluginFilter = new TextBox { Placeholder = "search" };
        private readonly Scroller pluginScroll = new Scroller();
        private readonly Scroller settingScroll = new Scroller();

        private readonly SettingEditor editor = new SettingEditor();
        private readonly IdPicker picker = new IdPicker();

        /// <summary>
        /// Ticks left in which a second click on "Reset all" means it, so that a whole plugin's settings are not
        /// thrown away by one stray click.
        /// </summary>
        private int resetArmed;

        /// <summary>
        /// The wheel movement for this tick, taken before the game can spend it on the hotbar.
        /// </summary>
        private int wheelNotches;

        private Keys[] heldLastTick = new Keys[0];

        #region Opening and closing

        public void Toggle()
        {
            if (IsOpen) Close();
            else Show();
        }

        public void Show()
        {
            if (IsOpen) return;

            IsOpen = true;

            Refresh();

            // The window wants the whole keyboard, so nothing else may be reading it at the same time.
            Main.playerInventory = false;
            Main.ClosePlayerChat();
            Main.chatText = "";

            resetArmed = 0;
            heldLastTick = Main.keyState.GetPressedKeys();
        }

        public void Close()
        {
            if (!IsOpen) return;

            IsOpen = false;

            TextBox.Unfocus();
            picker.Close();
            editor.Capturing = null;

            Main.blockInput = false;

            // Stops the key that closed the window from also doing whatever the game has it bound to.
            if (ToggleKey != null && ToggleKey.Key != Keys.None)
                Main.blockKey = ToggleKey.Key.ToString();
        }

        /// <summary>
        /// Rebuilds the plugin list, which changes when a plugin throws and is dropped for the session.
        /// </summary>
        public void Refresh()
        {
            plugins.Clear();
            plugins.AddRange(Loader.GetPlugins().OrderBy(plugin => plugin.Name, StringComparer.OrdinalIgnoreCase));

            if (selected != null && !plugins.Contains(selected)) selected = null;
            if (selected == null) selected = plugins.FirstOrDefault();

            matchedFor = null;
            describedPlugin = null;

            ReadSettings();
            editor.Forget();
        }

        private void ReadSettings()
        {
            shown.Clear();

            if (selected != null) shown.AddRange(selected.ConfigurableSettings);
        }

        #endregion

        #region Update

        /// <summary>
        /// Takes the wheel before the game spends it on the hotbar. Called from a hook that runs early in the tick.
        /// </summary>
        public void TakeScrollWheel()
        {
            if (!IsOpen)
            {
                wheelNotches = 0;
                return;
            }

            wheelNotches = PlayerInput.ScrollWheelDelta / 120;

            PlayerInput.ScrollWheelDelta = 0;
            PlayerInput.ScrollWheelDeltaForUI = 0;
        }

        /// <summary>
        /// Reads the keyboard for this tick. Called once a tick while the window is open.
        /// </summary>
        public void Update()
        {
            if (!IsOpen) return;

            // Terraria draws its interface layers for the in game interface only, so the window cannot be seen
            // over either of these. Closing it hands the keyboard back rather than leaving it held by a window
            // that is no longer on screen.
            if (Main.gameMenu || Main.mapFullscreen)
            {
                Close();
                return;
            }

            if (resetArmed > 0) resetArmed--;

            // Keeps the game's own keys, including the plugin loader's hotkeys, off the keyboard while the window
            // has it. WritingText empties the key list PlayerInput builds its triggers from; blockInput stops the
            // inventory and creative menu keys, which are read apart from those triggers.
            Main.blockInput = true;
            PlayerInput.WritingText = true;

            var held = Main.keyState.GetPressedKeys();

            if (editor.Capturing != null) Capture(held);
            else if (!TextBox.AnyFocused && (JustPressed(held, Keys.Escape) || TogglePressed(held))) Close();

            heldLastTick = held;

            if (!IsOpen) return;

            TextBox.UpdateFocused();

            if (picker.Setting != null) picker.Update();
        }

        /// <summary>
        /// Binds a hotkey setting to whatever is pressed next.
        /// </summary>
        private void Capture(Keys[] held)
        {
            var setting = editor.Capturing;

            foreach (var key in held)
            {
                if (IsModifier(key)) continue;
                if (!JustPressed(held, key)) continue;

                editor.Capturing = null;

                if (key == Keys.Escape)
                {
                    Set(setting, "None");
                    return;
                }

                var binding =
                    (Held(held, Keys.LeftControl, Keys.RightControl) ? "Control," : "") +
                    (Held(held, Keys.LeftShift, Keys.RightShift) ? "Shift," : "") +
                    (Held(held, Keys.LeftAlt, Keys.RightAlt) ? "Alt," : "") + key;

                Set(setting, binding);

                // Stops the key it was just bound to from firing on the very press that bound it.
                Main.blockKey = key.ToString();
                return;
            }
        }

        private static void Set(Setting setting, string value)
        {
            try
            {
                setting.SetFrom(value);
            }
            catch (Exception ex)
            {
                Main.NewText("Could not set " + setting.FullName + ": " + ex.Message, 230, 130, 130);
            }
        }

        private bool TogglePressed(Keys[] held)
        {
            if (ToggleKey == null || ToggleKey.Key == Keys.None) return false;
            if (!JustPressed(held, ToggleKey.Key)) return false;

            if (ToggleKey.IgnoreModifierKeys) return true;

            return Held(held, Keys.LeftControl, Keys.RightControl) == ToggleKey.Control &&
                   Held(held, Keys.LeftShift, Keys.RightShift) == ToggleKey.Shift &&
                   Held(held, Keys.LeftAlt, Keys.RightAlt) == ToggleKey.Alt;
        }

        private bool JustPressed(Keys[] held, Keys key)
        {
            return Contains(held, key) && !Contains(heldLastTick, key);
        }

        private static bool Held(Keys[] held, Keys left, Keys right)
        {
            return Contains(held, left) || Contains(held, right);
        }

        private static bool Contains(Keys[] keys, Keys key)
        {
            for (var i = 0; i < keys.Length; i++)
                if (keys[i] == key) return true;

            return false;
        }

        private static bool IsModifier(Keys key)
        {
            return key == Keys.LeftControl || key == Keys.RightControl ||
                   key == Keys.LeftShift || key == Keys.RightShift ||
                   key == Keys.LeftAlt || key == Keys.RightAlt;
        }

        #endregion
    }
}
