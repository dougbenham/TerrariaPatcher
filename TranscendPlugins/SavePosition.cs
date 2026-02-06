using System;
using System.IO;
using Terraria;
using PluginLoader;
using Terraria.IO;

namespace TranscendPlugins
{
    public class SavePosition : MarshalByRefObject, IPluginPlayerLoad, IPluginPlayerSave, IPluginPlayerSpawn
    {
        private bool justLoadedIn = false;

        public void OnPlayerSave(PlayerFileData playerFileData, Player player, BinaryWriter binaryWriter)
        {
            if (justLoadedIn) return;

            if (Main.worldID == 0) return;
            if (player.position.X == 0f && player.position.Y == 0f) return;

            IniAPI.WriteIni("SavePosition", Main.worldID + "," + player.name, player.position.ToString());
        }

        public void OnPlayerLoad(PlayerFileData playerFileData, Player player, BinaryReader binaryReader)
        {
            justLoadedIn = true;
        }
		
        public void OnPlayerSpawn(Player player)
        {
            if (player.whoAmI != Main.myPlayer || !justLoadedIn) return;

            var vector = IniAPI.ReadIni("SavePosition", Main.worldID + "," + Main.player[Main.myPlayer].name, null);
            if (!string.IsNullOrEmpty(vector))
            {
                int startIndX = vector.IndexOf("X:") + 2;
                int startIndY = vector.IndexOf("Y:") + 2;
                var x = float.Parse(vector.Substring(startIndX, vector.IndexOf(" Y") - startIndX));
                var y = float.Parse(vector.Substring(startIndY, vector.IndexOf("}") - startIndY));

                player.position.X = x;
                player.position.Y = y;
                player.fallStart = (int)(player.position.Y / 16f);
                player.fallStart2 = player.fallStart;
                player.oldPosition = player.position;
                Main.screenPosition.X = player.position.X + player.width / 2 - Main.screenWidth / 2;
                Main.screenPosition.Y = player.position.Y + player.height / 2 - Main.screenHeight / 2;
            }

            justLoadedIn = false;
        }
    }
}
