using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;
using TranscendPlugins.Shared.UI;

namespace TranscendPlugins
{
    [PluginDescription("Opens a window in game, on pressing ~, for reading and changing every plugin's " +
                       "settings and hotkeys, switching plugins on and off, and putting any of it back to its " +
                       "default.")]
    public class SettingsMenu : PluginBase, IPluginPreUpdate, IPluginPlayerPreUpdate, IPluginPlayerSpawn, IPluginDrawUI
    {
        [SettingDescription("Opens and closes the settings window.")]
        private readonly HotkeySetting ToggleKey = new Hotkey { Key = Keys.OemTilde };

        private readonly SettingsWindow window = new SettingsWindow();

        public SettingsMenu()
        {
            ToggleKey.Value.Action = window.Toggle;

            window.Owner = this;
            window.ToggleKey = ToggleKey.Value;

            EnabledSetting.Changed += () => { if (!Enabled) window.Close(); };
        }

        public void OnPreUpdate()
        {
            window.Update();
        }

        /// <summary>
        /// Runs at the start of the player's own update, which is early enough to take the wheel before the game
        /// spends it on changing the selected item.
        /// </summary>
        public void OnPlayerPreUpdate(Player player)
        {
            if (player.whoAmI != Main.myPlayer) return;

            window.TakeScrollWheel();
        }

        /// <summary>
        /// Closes the window on the way into a world, for one left open on the way out to the main menu, where the
        /// update hook that would otherwise have closed it is not raised.
        /// </summary>
        public void OnPlayerSpawn(Player player)
        {
            if (player.whoAmI != Main.myPlayer) return;

            window.Close();
        }

        public void OnDrawUI()
        {
            window.Draw();
        }
    }
}
