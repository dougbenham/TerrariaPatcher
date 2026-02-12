using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PluginLoader;
using Terraria;
using Terraria.Map;
using Terraria.Testing;

namespace TranscendPlugins
{
    public class Reveal : MarshalByRefObject, IPlugin, IPluginChatCommand, IPluginUpdate
    {
        private Keys revealKey;

        private enum RevealState { Idle, Scanning, Waiting, Updating }
        private RevealState _state = RevealState.Idle;

        // Scanning: spoof player position across the map so the server pushes tile sections
        private int _scanTileX;
        private int _scanTileY;
        private int _scanJumpX;
        private int _scanJumpY;
        private int _totalScans;
        private int _scansDone;

        // Waiting for tile data to arrive
        private int _waitFrames;

        // Map update phase
        private int _updateCol;
        private int _colsPerFrame;

        private int _lastPct;

        public Reveal()
        {
            if (!Keys.TryParse(IniAPI.ReadIni("Reveal", "RevealKey", "L", writeIt: true), out revealKey))
                revealKey = Keys.L;

            if (!int.TryParse(IniAPI.ReadIni("Reveal", "ColumnsPerFrame", "100", writeIt: true), out _colsPerFrame) || _colsPerFrame < 1)
                _colsPerFrame = 100;

            // How many tiles to jump per scan step (server sends ~5x3 sections around player)
            if (!int.TryParse(IniAPI.ReadIni("Reveal", "ScanJumpX", "400", writeIt: true), out _scanJumpX) || _scanJumpX < 100)
                _scanJumpX = 400;
            if (!int.TryParse(IniAPI.ReadIni("Reveal", "ScanJumpY", "200", writeIt: true), out _scanJumpY) || _scanJumpY < 100)
                _scanJumpY = 200;

            Loader.RegisterHotkey(DoReveal, revealKey);
        }

        private void DoReveal()
        {
            if (!Main.mapFullscreen || Main.Map == null)
                return;

            if (Main.netMode == 1)
            {
                if (_state != RevealState.Idle)
                {
                    Main.NewText("Already revealing map...", 255, 200, 0);
                    return;
                }
                BeginMultiplayerReveal();
            }
            else
            {
                RevealNow();
            }
        }

        private void RevealNow()
        {
            Main.clearMap = true;
            DebugOptions.unlockMap = 1;
            Main.refreshMap = true;
        }

        private void BeginMultiplayerReveal()
        {
            _scanTileX = 100;
            _scanTileY = 100;
            _scansDone = 0;
            _lastPct = -1;

            int hSteps = (Main.maxTilesX / _scanJumpX) + 1;
            int vSteps = (Main.maxTilesY / _scanJumpY) + 1;
            _totalScans = hSteps * vSteps;

            _state = RevealState.Scanning;
            Main.NewText("Scanning map (" + _totalScans + " positions)...", 0, 200, 255);
        }

        public void OnUpdate()
        {
            switch (_state)
            {
                case RevealState.Scanning:
                    UpdateScanning();
                    break;
                case RevealState.Waiting:
                    UpdateWaiting();
                    break;
                case RevealState.Updating:
                    UpdateMap();
                    break;
            }
        }

        private void UpdateScanning()
        {
            Player player = Main.player[Main.myPlayer];
            Vector2 realPos = player.position;

            // Scan 2 positions per frame
            for (int i = 0; i < 2 && _scanTileY < Main.maxTilesY; i++)
            {
                // Clamp to world bounds
                int tx = _scanTileX;
                int ty = _scanTileY;
                if (tx >= Main.maxTilesX) tx = Main.maxTilesX - 10;
                if (ty >= Main.maxTilesY) ty = Main.maxTilesY - 10;

                // Spoof position so server sends us tile sections around this point
                player.position.X = tx * 16f;
                player.position.Y = ty * 16f;
                NetMessage.SendData(13, -1, -1, null, Main.myPlayer, 0f, 0f, 0f, 0, 0, 0);

                _scansDone++;

                // Advance scan grid
                _scanTileX += _scanJumpX;
                if (_scanTileX >= Main.maxTilesX)
                {
                    _scanTileX = 100;
                    _scanTileY += _scanJumpY;
                }
            }

            // Restore real position immediately (local player doesn't actually move)
            player.position = realPos;

            // Progress
            int pct = _totalScans > 0 ? (_scansDone * 100 / _totalScans) : 100;
            if (pct / 25 > _lastPct / 25 && pct < 100)
            {
                Main.NewText("Scanning: " + pct + "%", 0, 200, 255);
                _lastPct = pct;
            }

            if (_scanTileY >= Main.maxTilesY)
            {
                // Restore real position on server
                NetMessage.SendData(13, -1, -1, null, Main.myPlayer, 0f, 0f, 0f, 0, 0, 0);

                _waitFrames = 300; // ~5 seconds for tile data to arrive
                _state = RevealState.Waiting;
                _lastPct = -1;
                Main.NewText("Waiting for tile data from server...", 0, 200, 255);
            }
        }

        private void UpdateWaiting()
        {
            _waitFrames--;
            if (_waitFrames <= 0)
            {
                _updateCol = 0;
                _state = RevealState.Updating;
                _lastPct = -1;
                Main.NewText("Rendering map...", 0, 200, 255);
            }
        }

        private void UpdateMap()
        {
            int maxX = Main.maxTilesX - 1;
            int maxY = Main.maxTilesY - 1;
            int startX = 1;
            int startY = 1;

            if (_updateCol < startX)
                _updateCol = startX;

            for (int c = 0; c < _colsPerFrame && _updateCol < maxX; c++, _updateCol++)
            {
                for (int y = startY; y < maxY; y++)
                {
                    try
                    {
                        if (Main.tile[_updateCol, y] != null)
                            Main.Map.Update(_updateCol, y, 255);
                    }
                    catch { }
                }
            }

            int total = maxX - startX;
            int done = _updateCol - startX;
            int pct = total > 0 ? (done * 100 / total) : 100;
            if (pct / 25 > _lastPct / 25 && pct < 100)
            {
                Main.NewText("Rendering: " + pct + "%", 0, 200, 255);
                _lastPct = pct;
            }

            if (_updateCol >= maxX)
            {
                _state = RevealState.Idle;
                Main.refreshMap = true;
                Main.NewText("Map fully revealed!", 0, 255, 0);
            }
        }

        public bool OnChatCommand(string command, string[] args)
        {
            if (command != "reveal")
                return false;

            if (!Main.mapFullscreen || Main.Map == null)
            {
                Main.NewText("Open the map and use /reveal to uncover everything.", 0, 200, 255);
                return true;
            }

            DoReveal();
            return true;
        }
    }
}
