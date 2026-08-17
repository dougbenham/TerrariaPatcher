using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;
using Terraria.ID;

namespace TranscendPlugins
{
    [PluginDescription("Teleports you to your cursor with a hotkey, or to anywhere you right click on the fullscreen map. " +
                       "/teleport also finds the nearest Plantera bulb or Strange Plant on the explored map.")]
    public class Teleport : PluginBase, IPluginInitialize, IPluginUpdate, IPluginChatCommand
    {
        private const int NoSearch = 0, PlanteraSearch = 1, StrangePlantSearch = 2;
        private const int CellsPerFrame = 200000;

        private static readonly HotkeySetting TeleportKey = new Hotkey { Key = Keys.F, Action = TeleportToCursor };

        private int planteraBulbTileLookup, plant1Lookup, plant2Lookup, plant3Lookup, plant4Lookup;
        private int searchMode, searchX;

        private static void TeleportToCursor()
        {
            var vector = new Vector2(Main.mouseX + Main.screenPosition.X, Main.mouseY + Main.screenPosition.Y);
            Player.Teleport(vector, 1, 0);
            Player.velocity = Vector2.Zero;
            NetMessage.SendData(65, -1, -1, null, 0, Player.whoAmI, vector.X, vector.Y, 1, 0, 0);
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
            StepSearch();

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
            };

            if (args.Length < 1 || args.Length > 1 || args[0] == "help")
            {
                usage();
                return true;
            }

            switch (args[0])
            {
                case "plantera":
                    BeginSearch(PlanteraSearch);
                    return true;
                case "strangeplant":
                    BeginSearch(StrangePlantSearch);
                    return true;
                default:
                    usage();
                    return true;
            }
        }

        private void BeginSearch(int mode)
        {
            searchMode = mode;
            searchX = 0;
            Main.NewText("Searching the map...");
        }

        private void StepSearch()
        {
            if (searchMode == NoSearch) return;

            int width = Main.Map.MaxWidth;
            int height = Main.Map.MaxHeight;
            int columns = Math.Max(1, CellsPerFrame / Math.Max(1, height));
            int limit = Math.Min(width, searchX + columns);

            for (; searchX < limit; searchX++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (!IsSearchMatch(Main.Map[searchX, j].Type)) continue;

                    Player player = Main.player[Main.myPlayer];
                    player.position = new Vector2(searchX * 16, j * 16);
                    player.velocity = Vector2.Zero;
                    player.fallStart = (int)(player.position.Y / 16f);
                    NetMessage.SendData(13, -1, -1, null, Main.myPlayer, 0f, 0f, 0f, 0, 0, 0);
                    searchMode = NoSearch;
                    return;
                }
            }

            if (searchX >= width)
            {
                Main.NewText("Nothing found on the explored map.");
                searchMode = NoSearch;
            }
        }

        private bool IsSearchMatch(int type)
        {
            if (searchMode == PlanteraSearch)
                return type == planteraBulbTileLookup;

            return type == plant1Lookup ||
                   type == plant2Lookup ||
                   type == plant3Lookup ||
                   type == plant4Lookup;
        }
    }
}
