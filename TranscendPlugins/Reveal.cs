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

        private enum RevealState { Idle, ScanPass1, Wait1, ScanPass2, Wait2, Updating }
        private RevealState _state = RevealState.Idle;

        // Scanning
        private int _scanTileX;
        private int _scanTileY;
        private int _scanJumpX;
        private int _scanJumpY;
        private int _totalScans;
        private int _scansDone;
        private int _scanStartX;
        private int _scanStartY;
        private int _scansPerFrame;

        // Waiting
        private int _waitFrames;

        // Map update
        private int _updateCol;
        private int _colsPerFrame;

        private int _lastPct;

        public Reveal()
        {
            if (!Keys.TryParse(IniAPI.ReadIni("Reveal", "RevealKey", "L", writeIt: true), out revealKey))
                revealKey = Keys.L;

            if (!int.TryParse(IniAPI.ReadIni("Reveal", "ColumnsPerFrame", "100", writeIt: true), out _colsPerFrame) || _colsPerFrame < 1)
                _colsPerFrame = 100;

            // Hardcoded for best results - half a section size for guaranteed overlap
            _scanJumpX = 100;
            _scanJumpY = 75;

            Loader.RegisterHotkey(DoReveal, revealKey);
        }

        private void DoReveal()
        {
            DoRevealWithSpeed(1);
        }

        private void DoRevealWithSpeed(int speed)
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
                _scansPerFrame = speed;
                BeginPass1();
            }
            else
            {
                Main.clearMap = true;
                DebugOptions.unlockMap = 1;
                Main.refreshMap = true;
            }
        }

        private void StartScan(int startX, int startY)
        {
            _scanStartX = startX;
            _scanStartY = startY;
            _scanTileX = startX;
            _scanTileY = startY;
            _scansDone = 0;
            _lastPct = -1;

            int hSteps = ((Main.maxTilesX - startX) / _scanJumpX) + 1;
            int vSteps = ((Main.maxTilesY - startY) / _scanJumpY) + 1;
            _totalScans = hSteps * vSteps;
        }

        private void BeginPass1()
        {
            StartScan(10, 10);
            _state = RevealState.ScanPass1;
            Main.NewText("Pass 1: Scanning map (" + _totalScans + " positions)...", 0, 200, 255);
        }

        private void BeginPass2()
        {
            // Offset by half the jump to fill gaps between pass 1 points
            StartScan(10 + _scanJumpX / 2, 10 + _scanJumpY / 2);
            _state = RevealState.ScanPass2;
            Main.NewText("Pass 2: Filling gaps (" + _totalScans + " positions)...", 0, 200, 255);
        }

        public void OnUpdate()
        {
            switch (_state)
            {
                case RevealState.ScanPass1:
                    DoScan(RevealState.Wait1);
                    break;
                case RevealState.Wait1:
                    DoWait(true);
                    break;
                case RevealState.ScanPass2:
                    DoScan(RevealState.Wait2);
                    break;
                case RevealState.Wait2:
                    DoWait(false);
                    break;
                case RevealState.Updating:
                    UpdateMap();
                    break;
            }
        }

        private void DoScan(RevealState nextWaitState)
        {
            Player player = Main.player[Main.myPlayer];
            Vector2 realPos = player.position;

            for (int i = 0; i < _scansPerFrame && _scanTileY < Main.maxTilesY; i++)
            {
                int tx = _scanTileX;
                int ty = _scanTileY;
                if (tx >= Main.maxTilesX) tx = Main.maxTilesX - 10;
                if (ty >= Main.maxTilesY) ty = Main.maxTilesY - 10;
                if (tx < 1) tx = 1;
                if (ty < 1) ty = 1;

                player.position.X = tx * 16f;
                player.position.Y = ty * 16f;
                NetMessage.SendData(13, -1, -1, null, Main.myPlayer, 0f, 0f, 0f, 0, 0, 0);

                _scansDone++;

                _scanTileX += _scanJumpX;
                if (_scanTileX >= Main.maxTilesX)
                {
                    _scanTileX = _scanStartX;
                    _scanTileY += _scanJumpY;
                }
            }

            player.position = realPos;

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

                _waitFrames = 420;
                _state = nextWaitState;
                _lastPct = -1;
                Main.NewText("Waiting for tile data...", 0, 200, 255);
            }
        }

        private void DoWait(bool startPass2After)
        {
            _waitFrames--;
            if (_waitFrames <= 0)
            {
                if (startPass2After)
                {
                    BeginPass2();
                }
                else
                {
                    _updateCol = 0;
                    _state = RevealState.Updating;
                    _lastPct = -1;
                    Main.NewText("Rendering map...", 0, 200, 255);
                }
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
                Main.NewText("Open the map and use /reveal [speed] to uncover everything.", 0, 200, 255);
                Main.NewText("Speed: 1 = safe (default), 2 = fast, 3 = turbo", 0, 200, 255);
                return true;
            }

            int speed = 1;
            if (args.Length > 0)
            {
                int.TryParse(args[0], out speed);
                if (speed < 1) speed = 1;
                if (speed > 5) speed = 5;
            }

            if (speed > 1)
                Main.NewText("Reveal speed: " + speed + "x", 0, 255, 200);

            DoRevealWithSpeed(speed);
            return true;
        }
    }
}
