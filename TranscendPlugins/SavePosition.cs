using System;
using System.IO;
using Terraria;
using PluginLoader;
using Terraria.IO;

namespace TranscendPlugins
{
    public class SavePosition : MarshalByRefObject, IPluginPlayerLoad, IPluginPlayerSave, IPluginPlayerPreSpawn, IPluginPlayerSpawn
    {
        private bool justLoadedIn = false;
        private int originalSpawnX, originalSpawnY;

        public void OnPlayerSave(PlayerFileData playerFileData, BinaryWriter binaryWriter)
        {
            IniAPI.WriteIni("SavePosition", Main.worldID + "," + Main.player[Main.myPlayer].name, (playerFileData.Player.position / 16f).ToString());
        }

        public void OnPlayerLoad(PlayerFileData playerFileData, Player player, BinaryReader binaryReader)
        {
            justLoadedIn = true;
        }

        public void OnPlayerPreSpawn(Player player)
        {
            if (player.whoAmI != Main.myPlayer || !justLoadedIn) return;
            
            var vector = IniAPI.ReadIni("SavePosition", Main.worldID + "," + Main.player[Main.myPlayer].name, null);
            if (!string.IsNullOrEmpty(vector))
            {
                player.FindSpawn();
                originalSpawnX = player.SpawnX;
                originalSpawnY = player.SpawnY;

                int startIndX = vector.IndexOf("X:") + 2;
                int startIndY = vector.IndexOf("Y:") + 2;
                player.ChangeSpawn(
                    (int) float.Parse(vector.Substring(startIndX, vector.IndexOf(" Y") - startIndX)), 
                    (int) float.Parse(vector.Substring(startIndY, vector.IndexOf("}") - startIndY)));
            }

            justLoadedIn = false;
        }

        public void OnPlayerSpawn(Player player)
        {
            player.SpawnX = originalSpawnX;
            player.SpawnY = originalSpawnY;
        }
    }
}
