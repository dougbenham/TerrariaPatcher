using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;
using Terraria.ID;

namespace TranscendPlugins
{
    public class Teleport : MarshalByRefObject, IPluginInitialize, IPluginUpdate, IPluginChatCommand
    {
        private int planteraBulbTileLookup, plant1Lookup, plant2Lookup, plant3Lookup, plant4Lookup;
        private Keys teleportKey;
        private bool hotkeyEnabled;
        private bool fullscreenMapEnabled;

        public Teleport()
        {
            if (!bool.TryParse(IniAPI.ReadIni("Teleport", "HotkeyEnabled", "true", writeIt: true), out hotkeyEnabled))
                hotkeyEnabled = true;
            if (!bool.TryParse(IniAPI.ReadIni("Teleport", "FullscreenMapEnabled", "true", writeIt: true), out fullscreenMapEnabled))
                fullscreenMapEnabled = true;

            if (!Keys.TryParse(IniAPI.ReadIni("Teleport", "TeleportKey", "F", writeIt: true), out teleportKey))
                teleportKey = Keys.F;

            Loader.RegisterHotkey(() =>
            {
                if (!hotkeyEnabled) return;
                var player = Main.player[Main.myPlayer];
                var vector = new Vector2(Main.mouseX + Main.screenPosition.X, Main.mouseY + Main.screenPosition.Y);
                player.Teleport(vector, 1, 0);
                player.velocity = Vector2.Zero;
                NetMessage.SendData(65, -1, -1, null, 0, player.whoAmI, vector.X, vector.Y, 1, 0, 0);
            }, teleportKey);
        }

        public void OnInitialize()
        {
            planteraBulbTileLookup = Terraria.Map.MapHelper.TileToLookup(TileID.PlanteraBulb, 0);
            plant1Lookup = Terraria.Map.MapHelper.TileToLookup(TileID.DyePlants, 8);
            plant2Lookup = Terraria.Map.MapHelper.TileToLookup(TileID.DyePlants, 9);
            plant3Lookup = Terraria.Map.MapHelper.TileToLookup(TileID.DyePlants, 10);
            plant4Lookup = Terraria.Map.MapHelper.TileToLookup(TileID.DyePlants, 11);
        }

        public void OnUpdate()
        {
            if (!fullscreenMapEnabled) return;

            if (Main.mapFullscreen && Main.mouseRight && Main.keyState.IsKeyUp(Keys.LeftControl))
            {
                int num = Main.maxTilesX * 16;
                int num2 = Main.maxTilesY * 16;
                Vector2 vector = new Vector2((float)Main.mouseX, (float)Main.mouseY);
                vector.X -= (float)(Main.screenWidth / 2);
                vector.Y -= (float)(Main.screenHeight / 2);
                Vector2 mapFullscreenPos = Main.mapFullscreenPos;
                Vector2 vector2 = mapFullscreenPos;
                vector /= 16f;
                vector *= 16f / Main.mapFullscreenScale;
                vector2 += vector;
                vector2 *= 16f;
                Player player = Main.player[Main.myPlayer];
                vector2.Y -= (float)player.height;
                if (vector2.X < 0f)
                {
                    vector2.X = 0f;
                }
                else if (vector2.X + (float)player.width > (float)num)
                {
                    vector2.X = (float)(num - player.width);
                }
                if (vector2.Y < 0f)
                {
                    vector2.Y = 0f;
                }
                else if (vector2.Y + (float)player.height > (float)num2)
                {
                    vector2.Y = (float)(num2 - player.height);
                }
                player.position = vector2;
                player.velocity = Vector2.Zero;
                player.fallStart = (int)(player.position.Y / 16f);
                NetMessage.SendData(13, -1, -1, null, Main.myPlayer, 0f, 0f, 0f, 0, 0, 0);
            }
        }

        public bool OnChatCommand(string command, string[] args)
        {
            if (command != "teleport") return false;

            Action usage = () =>
            {
                Main.NewText("Usage:");
                Main.NewText("  /teleport plantera");
                Main.NewText("  /teleport strangeplant");
                Main.NewText("  /teleport cursor");
                Main.NewText("  /teleport togglehotkey");
                Main.NewText("  /teleport togglemap");
            };

            if (args.Length < 1 || args.Length > 1 || args[0] == "help")
            {
                usage();
                return true;
            }

            switch (args[0])
            {
                case "plantera":
                    for (int i = 0; i < Main.Map.MaxWidth; i++)
                    {
                        for (int j = 0; j < Main.Map.MaxHeight; j++)
                        {
                            if (Main.Map[i, j].Type == planteraBulbTileLookup)
                            {
                                Player player = Main.player[Main.myPlayer];
                                player.position = new Vector2(i * 16, j * 16);
                                player.velocity = Vector2.Zero;
                                player.fallStart = (int)(player.position.Y / 16f);
                                NetMessage.SendData(13, -1, -1, null, Main.myPlayer, 0f, 0f, 0f, 0, 0, 0);
                                return true;
                            }
                        }
                    }
                    return true;
                case "strangeplant":
                    for (int i = 0; i < Main.Map.MaxWidth; i++)
                    {
                        for (int j = 0; j < Main.Map.MaxHeight; j++)
                        {
                            var type = Main.Map[i, j].Type;
                            if (type == plant1Lookup ||
                                type == plant2Lookup ||
                                type == plant3Lookup ||
                                type == plant4Lookup)
                            {
                                Player player = Main.player[Main.myPlayer];
                                player.position = new Vector2(i * 16, j * 16);
                                player.velocity = Vector2.Zero;
                                player.fallStart = (int)(player.position.Y / 16f);
                                NetMessage.SendData(13, -1, -1, null, Main.myPlayer, 0f, 0f, 0f, 0, 0, 0);
                                return true;
                            }
                        }
                    }
                    return true;
                case "cursor":
                    TeleportToCursor();
                    return true;
                case "togglehotkey":
                    hotkeyEnabled = !hotkeyEnabled;
                    IniAPI.WriteIni("Teleport", "HotkeyEnabled", hotkeyEnabled.ToString());
                    Main.NewText("Teleport hotkey " + (hotkeyEnabled ? "enabled" : "disabled") + (hotkeyEnabled ? "." : " (F to toggle when enabled)"));
                    return true;
                case "togglemap":
                    fullscreenMapEnabled = !fullscreenMapEnabled;
                    IniAPI.WriteIni("Teleport", "FullscreenMapEnabled", fullscreenMapEnabled.ToString());
                    Main.NewText("Fullscreen map teleport " + (fullscreenMapEnabled ? "enabled" : "disabled") + ".");
                    return true;
                default:
                    usage();
                    return true;
            }
        }

        private void TeleportToCursor()
        {
            var player = Main.player[Main.myPlayer];
            var vector = new Vector2(Main.mouseX + Main.screenPosition.X, Main.mouseY + Main.screenPosition.Y);
            player.Teleport(vector, 1, 0);
            player.velocity = Vector2.Zero;
            NetMessage.SendData(65, -1, -1, null, 0, player.whoAmI, vector.X, vector.Y, 1, 0, 0);
        }
    }
}
