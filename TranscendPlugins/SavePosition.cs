using System;
using System.Globalization;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using PluginLoader;
using Terraria.IO;

namespace TranscendPlugins
{
    [PluginDescription("Remembers where you were when you left a world and puts you back there instead of at your spawn point.")]
    public class SavePosition : PluginBase, IPluginPlayerLoad, IPluginPlayerSave, IPluginPlayerSpawn
    {
        private const string Section = "SavePosition";

        private bool justLoadedIn = false;

        public SavePosition() : base(toggleKey: Keys.None)
        { }

        public void OnPlayerSave(PlayerFileData playerFileData, Player player, BinaryWriter binaryWriter)
        {
            if (justLoadedIn) return;

            if (Main.worldID == 0) return;
            if (player.position.X == 0f && player.position.Y == 0f) return;

            var value = player.position.X.ToString("R", CultureInfo.InvariantCulture) + " " +
                        player.position.Y.ToString("R", CultureInfo.InvariantCulture);

            IniAPI.WriteIni(Section, GetKey(player.name), value);
        }

        public void OnPlayerLoad(PlayerFileData playerFileData, Player player, BinaryReader binaryReader)
        {
            justLoadedIn = true;
        }

        public void OnPlayerSpawn(Player player)
        {
            if (player.whoAmI != Main.myPlayer || !justLoadedIn) return;

            justLoadedIn = false;

            var value = IniAPI.ReadIni(Section, GetKey(Main.player[Main.myPlayer].name), null);
            Vector2 position;
            if (!TryParsePosition(value, out position)) return;
            if (!IsInsideWorld(position)) return;

            Player.Teleport(position, 1, 0);
            Player.velocity = Vector2.Zero;
            NetMessage.SendData(65, -1, -1, null, 0, Player.whoAmI, position.X, position.Y, 1, 0, 0);
        }

        private static string GetKey(string playerName)
        {
            return Main.worldID.ToString(CultureInfo.InvariantCulture) + "," + playerName;
        }

        private static bool IsInsideWorld(Vector2 position)
        {
            return position.X >= 0f && position.X <= Main.maxTilesX * 16f
                && position.Y >= 0f && position.Y <= Main.maxTilesY * 16f;
        }

        private static bool TryParsePosition(string value, out Vector2 position)
        {
            position = Vector2.Zero;
            if (string.IsNullOrEmpty(value)) return false;

            // Accepts "x y" as well as the Vector2.ToString() form "{X:x Y:y}" written by earlier versions.
            var text = value.Replace("{", " ").Replace("}", " ").Replace("X:", " ").Replace("Y:", " ").Replace(",", " ");
            var parts = text.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) return false;

            return TryParseCoordinate(parts[0], out position.X)
                && TryParseCoordinate(parts[1], out position.Y);
        }

        private static bool TryParseCoordinate(string text, out float value)
        {
            return float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }
    }
}
