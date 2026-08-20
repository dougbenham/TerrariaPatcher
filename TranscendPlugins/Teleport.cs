using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;

namespace TranscendPlugins
{
    [PluginDescription("Teleports you to your cursor with a hotkey, or to anywhere you right click on the fullscreen map. " +
                       "/teleport also finds the nearest Plantera Bulb or Strange Plant on the explored map.")]
    public class Teleport : PluginBase, IPluginInitialize, IPluginUpdate, IPluginChatCommand
    {
	    /// <inheritdoc />
	    public override bool RequiresRestart
	    {
		    get { return false; }
	    }

	    private const int NoSearch = 0, PlanteraSearch = 1, StrangePlantSearch = 2;
        private const int CellsPerFrame = 200000;

        private static readonly HotkeySetting TeleportKey = new Hotkey { Key = Keys.F, Action = TeleportToCursor };

        private int planteraBulbTileLookup, plant1Lookup, plant2Lookup, plant3Lookup, plant4Lookup;
        private int searchMode, searchX;

        private static void TeleportToCursor()
        {
            TeleportTo(new Vector2(Main.mouseX + Main.screenPosition.X, Main.mouseY + Main.screenPosition.Y));
        }

        /// <summary>
        /// Moves the player through <see cref="Terraria.Player.Teleport"/> and tells the server about it with message
        /// 65, which the server applies and relays to the other clients.
        /// </summary>
        private static void TeleportTo(Vector2 position)
        {
            Player.Teleport(position, 1, 0);
            Player.velocity = Vector2.Zero;
            NetMessage.SendData(65, -1, -1, null, 0, Player.whoAmI, position.X, position.Y, 1, 0, 0);
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
                Vector2 vector = new Vector2((float)PlayerInput.MouseX, (float)PlayerInput.MouseY);
                vector.X -= (float)(PlayerInput.RealScreenWidth / 2);
                vector.Y -= (float)(PlayerInput.RealScreenHeight / 2);
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
                TeleportTo(vector2);
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

            var option = args.Length > 0 ? args[0].ToLower() : "";

            if (args.Length < 1 || args.Length > 1 || option == "help")
            {
                usage();
                return true;
            }

            switch (option)
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

                    TeleportTo(new Vector2(searchX * 16, j * 16));
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
