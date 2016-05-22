using System;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;

namespace MrBlueSLPlugins
{
    public class Flashlight : MarshalByRefObject, IPluginPlayerUpdate
    {
        private bool flashlight = false;
        private Keys flashlightKey;

        public Flashlight()
        {
            if (!Keys.TryParse(IniAPI.ReadIni("Flashlight", "ToggleKey", "U", writeIt: true), out flashlightKey))
                flashlightKey = Keys.U;

            Loader.RegisterHotkey(() =>
            {
                flashlight = !flashlight;
                Main.NewText("Flashlight " + (flashlight ? "Enabled" : "Disabled"), 150, 150, 150);
            }, flashlightKey);
        }

        public void OnPlayerUpdate(Player player)
        {
            if (flashlight)
            {
                Lighting.AddLight((int)(Main.mouseX + Main.screenPosition.X + (double)(Player.defaultWidth / 2)) / 16, (int)(Main.mouseY + Main.screenPosition.Y + (double)(Player.defaultHeight / 2)) / 16, 1f, 1f, 1f);
            }
        }
    }
}