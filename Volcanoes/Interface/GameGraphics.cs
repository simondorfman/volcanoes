﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Volcano.Game;

namespace Volcano.Interface
{
    class GameGraphics
    {
        private Panel _panel;
        private GameGraphicsSettings _settings;
        private List<GameTile> _tiles;

        public GameGraphics(Panel panel, GameGraphicsSettings settings)
        {
            _panel = panel;
            _settings = settings;

            InitializeTiles();
        }

        private void InitializeTiles()
        {
            _tiles = new List<GameTile>();

            for (int outer = 0; outer < 20; outer++)
            {
                int outerRow = outer / 5;
                int outerCol = outer % 5;

                for (int inner = 0; inner < 4; inner++)
                {
                    // Details of outer triangle
                    int x = outerCol * (_settings.TileWidth * 2 + _settings.TileHorizontalSpacing * 4) + _settings.TileSpacing + _settings.BoardSpacing;
                    int y = outerRow * (_settings.TileHeight * 2 + _settings.TileSpacing * 2 + _settings.TileHorizontalSpacing) + _settings.TileHorizontalSpacing + _settings.BoardSpacing;
                    bool upright = outerRow % 2 == 0;
                    if (outerRow >= 2)
                    {
                        x += _settings.TileWidth + _settings.TileHorizontalSpacing * 2;
                        y -= _settings.TileHeight * 2 + _settings.TileSpacing * 2;
                    }

                    // Adjust to settings of inner triangle
                    if (upright)
                    {
                        switch (inner)
                        {
                            case 0:
                                x += _settings.TileWidth / 2 + _settings.TileHorizontalSpacing;
                                y += _settings.TileHeight + _settings.TileSpacing;
                                upright = !upright;
                                break;
                            case 1:
                                x += _settings.TileWidth / 2 + _settings.TileHorizontalSpacing;
                                break;
                            case 2:
                                x += _settings.TileWidth + _settings.TileHorizontalSpacing * 2;
                                y += _settings.TileHeight + _settings.TileSpacing + _settings.TileHorizontalSpacing;
                                break;
                            case 3:
                                y += _settings.TileHeight + _settings.TileSpacing + _settings.TileHorizontalSpacing;
                                break;
                        }
                    }
                    else
                    {
                        switch (inner)
                        {
                            case 0:
                                x += _settings.TileWidth / 2 + _settings.TileHorizontalSpacing;
                                y += _settings.TileHorizontalSpacing;
                                upright = !upright;
                                break;
                            case 1:
                                x += _settings.TileWidth / 2 + _settings.TileHorizontalSpacing;
                                y += _settings.TileHeight + _settings.TileSpacing + _settings.TileHorizontalSpacing;
                                break;
                            case 2:
                                break;
                            case 3:
                                x += _settings.TileWidth + _settings.TileHorizontalSpacing * 2;
                                break;
                        }
                    }

                    PointF[] points;
                    if (upright)
                    {
                        points = new PointF[] {
                            new PointF(x + _settings.TileWidth / 2, y),
                            new PointF(x, y + _settings.TileHeight),
                            new PointF(x + _settings.TileWidth, y + _settings.TileHeight)
                        };
                    }
                    else
                    {
                        points = new PointF[] {
                            new PointF(x + _settings.TileWidth / 2, y + _settings.TileHeight),
                            new PointF(x, y),
                            new PointF(x + _settings.TileWidth, y)
                        };
                    }

                    var path = new GraphicsPath();
                    path.AddLines(points);

                    _tiles.Add(new GameTile
                    {
                        Location = new Point(x, y),
                        Upright = upright,
                        BoundingBox = new Rectangle(x, y, _settings.TileWidth, _settings.TileHeight),
                        Path = path
                    });
                }
            }
        }

        public void Resize()
        {
            _settings.UpdateBestTileWidth(_panel.Size);
            InitializeTiles();
        }

        public void Draw(VolcanoGame game, Point mouseLocation, int moveNumber, bool highlightLastMove)
        {
            Resize();

            if (_settings.IdealPanelWidth <= 100 || _settings.IdealPanelHeight <= 20)
            {
                using (Graphics g3 = _panel.CreateGraphics())
                {
                    g3.Clear(SystemColors.ControlDark);
                    g3.DrawString(":)", new Font("Tahoma", 8f, FontStyle.Regular), Brushes.White, new Point(1, 1));
                }

                return;
            }

            bool reviewMode = game.MoveHistory.Count > 0 && moveNumber != game.MoveHistory.Count - 1;

            int hoverTile = GetTileIndex(mouseLocation);
            Board gameState = reviewMode ? game.GetPreviousState(moveNumber) : game.CurrentState;

            int lastPlayIndex = game.MoveHistory.Count >= moveNumber && moveNumber - 1 >= 0 ? game.MoveHistory[moveNumber - 1] : -1;

            Color background = reviewMode ? _settings.ReviewBackgroundColor : _settings.BackgroundColor;

            using (Bitmap b = new Bitmap(_settings.IdealPanelWidth, _settings.IdealPanelHeight))
            using (Graphics g = Graphics.FromImage(b))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                g.Clear(background);

                for (int i = 0; i < 80; i++)
                {
                    DrawTile(g, gameState, i, hoverTile, lastPlayIndex, highlightLastMove);

                    if (gameState.Tiles[i].Owner == Player.Empty)
                    {
                        if (_settings.ShowTileNames)
                        {
                            DrawTileCenterText(g, i, gameState.Tiles[i].Name);
                        }
                        else if (_settings.ShowTileIndexes)
                        {
                            DrawTileMainText(g, i, gameState.Tiles[i].Index.ToString());
                        }

                        //DrawTileMainText(g, i, Constants.FastestPaths[i].Length.ToString());
                    }
                    else
                    {
                        int value = gameState.Tiles[i].Value;

                        if (gameState.Tiles[i].Type == TileType.MagmaChamber)
                        {
                            DrawTileSubText(g, i, "Chamber");
                        }
                        else
                        {
                            value -= VolcanoGame.Settings.MaxMagmaChamberLevel;
                            DrawTileSubText(g, i, "Volcano");
                        }

                        DrawTileMainText(g, i, value.ToString());
                    }
                }

                using (Bitmap b2 = new Bitmap(_panel.Width, _panel.Height))
                using (Graphics g2 = Graphics.FromImage(b2))
                using (Graphics g3 = _panel.CreateGraphics())
                {
                    g2.Clear(background);
                    g2.DrawImage(b, (_panel.Width - _settings.IdealPanelWidth) / 2, (_panel.Height - _settings.IdealPanelHeight) / 2, _settings.IdealPanelWidth, _settings.IdealPanelHeight);

                    Color playerColor = gameState.Player == Player.One ? _settings.PlayerOneVolcanoTileColor : _settings.PlayerTwoVolcanoTileColor;
                    g2.DrawString("Turn " + gameState.Turn, new Font("Tahoma", 12f, FontStyle.Bold), new SolidBrush(playerColor), new Point(5, 5));
                    if (gameState.State == GameState.GameOver)
                    {
                        g2.DrawString("Game Over!", new Font("Tahoma", 12f, FontStyle.Bold), new SolidBrush(playerColor), new Point(5, 25));
                    }
                    g2.DrawString(game.NodesPerSecond.ToString() + " NPS", new Font("Tahoma", 12f, FontStyle.Bold), Brushes.Gray, new Point(5, _panel.Height - 25));

                    // Double buffering
                    g3.DrawImage(b2, 0, 0, _panel.Width, _panel.Height);
                }
            }
        }

        private void DrawTile(Graphics g, Board gameState, int index, int hoverIndex, int lastPlayIndex, bool highlightLastMove)
        {
            Color tileColor = _settings.EmptyTileColor;

            if (gameState.Tiles[index].Owner == Player.One)
            {
                tileColor = gameState.Tiles[index].Type == TileType.Volcano ? _settings.PlayerOneVolcanoTileColor : _settings.PlayerOneMagmaChamberTileColor;
            }
            else if (gameState.Tiles[index].Owner == Player.Two)
            {
                tileColor = gameState.Tiles[index].Type == TileType.Volcano ? _settings.PlayerTwoVolcanoTileColor : _settings.PlayerTwoMagmaChamberTileColor;
            }

            // Winning path
            if (gameState.WinningPath.Count > 0 && !gameState.WinningPath.Contains(index))
            {
                tileColor = Color.FromArgb(64, tileColor);
            }

            //// Tile on the shortest unhindered path to the antipode
            //if (hoverIndex >= 0 && Constants.FastestPaths[hoverIndex].Any(x => x == index))
            //{
            //    tileColor = Color.LightBlue;
            //}

            Brush brush = new SolidBrush(tileColor);

            g.FillPath(brush, _tiles[index].Path);

            //// Recently erupted volcano
            //if (gameState.Eruptions.Contains(index))
            //{
            //    Pen pen = new Pen(_settings.RecentEruptionTileBorderColor, _settings.TileHorizontalSpacing);
            //    g.DrawPolygon(pen, _tiles[index].Path.PathPoints);
            //}

            if (highlightLastMove)
            {
                // Tile most recently played on
                if (lastPlayIndex == index || (lastPlayIndex == 80 && gameState.Tiles[index].Value > 0))
                {
                    Pen pen = new Pen(_settings.LastPlayedTileBorderColor, _settings.TileHorizontalSpacing);
                    g.DrawPolygon(pen, _tiles[index].Path.PathPoints);
                }
            }

            if (hoverIndex >= 0)
            {
                // Tile under the mouse pointer
                if (index == hoverIndex)
                {
                    Pen pen = new Pen(_settings.HoverTileBorderColor, _settings.TileHorizontalSpacing);
                    g.DrawPolygon(pen, _tiles[index].Path.PathPoints);
                }

                // Tiles directly adjacent to the tile under the mouse pointer
                if (Constants.ConnectingTiles[hoverIndex].Any(x => x == index))
                {
                    Pen pen = new Pen(_settings.HoverAdjacentTileBorderColor, _settings.TileHorizontalSpacing);
                    g.DrawPolygon(pen, _tiles[index].Path.PathPoints);
                }

                //// Tiles kitty corner to the tile under the pointer
                //if (Constants.KittyCornerTiles[hoverIndex].Any(x => x == index))
                //{
                //    Pen pen = new Pen(Color.Red, _settings.TileHorizontalSpacing);
                //    g.DrawPolygon(pen, _tiles[index].Path.PathPoints);
                //}

                // Tile on the opposite side of the board
                if (gameState.Tiles[hoverIndex].Antipode == index)
                {
                    Pen pen = new Pen(_settings.HoverAntipodeTileBorderColor, _settings.TileHorizontalSpacing);
                    g.DrawPolygon(pen, _tiles[index].Path.PathPoints);
                }
            }
        }

        private Point GetOffsetPoint(int x, int y)
        {
            return new Point(x - (_panel.Width - _settings.IdealPanelWidth) / 2, y - (_panel.Height - _settings.IdealPanelHeight) / 2);
        }

        private Point GetOffsetPoint(Point point)
        {
            return GetOffsetPoint(point.X, point.Y);
        }

        public int GetTileIndex(Point location)
        {
            Point offset = GetOffsetPoint(location);

            for (int i = 0; i < 80; i++)
            {
                if (_tiles[i].Path.IsVisible(offset))
                {
                    return i;
                }
            }

            return -1;
        }

        private void DrawTileCenterText(Graphics g, int index, string text)
        {
            DrawTileText(g, index, text, 4, _settings.MainFontSize, true);
        }

        private void DrawTileMainText(Graphics g, int index, string text)
        {
            DrawTileText(g, index, text, 600, _settings.MainFontSize, true);
        }

        private void DrawTileSubText(Graphics g, int index, string text)
        {
            DrawTileText(g, index, text, 3, _settings.SubTextFontSize, false);
        }

        private void DrawTileText(Graphics g, int index, string text, int heightOffset, int fontSize, bool bold)
        {
            int triangleAdjust = (_tiles[index].Upright ? 1 : -1) * _settings.TileHeight / heightOffset;
            Font font = new Font("Tahoma", fontSize, bold ? FontStyle.Bold : FontStyle.Regular);
            SizeF size = g.MeasureString(text, font);
            Brush brush = new SolidBrush(Color.FromArgb(128, 255, 255, 255));
            RectangleF location = new RectangleF(new PointF(_tiles[index].BoundingBox.X + _tiles[index].BoundingBox.Width / 2 - size.Width / 2, _tiles[index].BoundingBox.Y + _tiles[index].BoundingBox.Height / 2 - size.Height / 2 + triangleAdjust), size);

            g.DrawString(text, font, brush, location);
        }
    }
}
