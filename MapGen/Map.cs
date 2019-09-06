using ConsoleGameEngine;
using System;
using System.Collections.Generic;

namespace MapGen
{
    public class Map
    {
        public CellData[,] Cells;
        public int Height;
        public Random Random = new Random();
        public int Width;

        public Map(int width, int height)
        {
            Cells = new CellData[width, height];
            Width = width;
            Height = height;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Cells[x, y] = new CellData(x, y) { Tile = " " };
                }
            }
        }

        public CellData GetRandomCell()
        {
            return Cells[Random.Next(0, Width), Random.Next(0, Height)];
        }

        public void AddCellIfValid(int x, int y, List<CellData> cells)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                cells.Add(Cells[x, y]);
            }
        }

        public int Clamp(int value, int min, int max)
        {
            if (value > max)
                return max;

            if (value < min)
                return min;

            return value;
        }

        public void Draw(ConsoleEngine engine)
        {

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    engine.SetPixel(new Point(x, y), 15, ConsoleCharacter.Full);

                }
            }
        }

        public CellData GetCellAtCoordinate(int x, int y)
        {
            if (x < 0
                || y < 0
                || x >= Width
                || y >= Height)
            {
                return null;
            }

            return Cells[x, y];
        }

        public List<CellData> GetCircle(CellData cell, int radius)
        {
            var cells = new List<CellData>();

            for (var x = cell.X - radius; x <= cell.X; x++)
            {
                for (var y = cell.Y - radius; y <= cell.Y; y++)
                {
                    if ((x - cell.X) * (x - cell.X) + (y - cell.Y) * (y - cell.Y) <= radius * radius)
                    {
                        // calculate and add cell for each of the four points
                        AddCellIfValid(cell.X - (x - cell.X), cell.Y - (y - cell.Y), cells);
                        AddCellIfValid(cell.X + (x - cell.X), cell.Y + (y - cell.Y), cells);
                        AddCellIfValid(cell.X - (x - cell.X), cell.Y + (y - cell.Y), cells);
                        AddCellIfValid(cell.X + (x - cell.X), cell.Y - (y - cell.Y), cells);
                    }
                }
            }

            return cells;
        }

        public List<CellData> GetLine(CellData a, CellData b)
        {
            // Bresenham line algorithm [https://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm]
            // https://stackoverflow.com/questions/11678693/all-cases-covered-bresenhams-line-algorithm

            var line = new List<CellData>();

            var x = a.X;
            var y = a.Y;
            var x2 = b.X;
            var y2 = b.Y;

            var w = x2 - x;
            var h = y2 - y;

            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;

            var longest = Math.Abs(w);
            var shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            var numerator = longest >> 1;
            for (var i = 0; i <= longest; i++)
            {
                line.Add(Cells[x, y]);
                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                }
            }

            return line;
        }

        public List<CellData> GetNaiveLine(CellData a, CellData b)
        {
            var line = new List<CellData>();
            var dx = b.X - a.X;
            var dy = b.Y - a.Y;

            var y = 0;
            for (int x = a.X; x < b.X; x++)
            {
                y = a.Y + dy * (x - a.X) / dx;
                line.Add(Cells[x, y]);
            }

            return line;
        }

        internal CellData GetCellAttRadian((int X, int Y) center, int radius, int angle)
        {
            var mineX = Clamp((int)(center.X + (radius * Math.Cos(angle))), 0, Width);
            var mineY = Clamp((int)(center.Y + (radius * Math.Sin(angle))), 0, Height);

            return GetCellAtCoordinate(mineX, mineY);
        }

        internal float GetDegreesBetweenPoints(CellData point1, CellData point2)
        {
            var deltaX = point1.X - point2.X;
            var deltaY = point1.Y - point2.Y;

            var radAngle = Math.Atan2(deltaY, deltaX);
            var degreeAngle = radAngle * 180.0 / Math.PI;

            return (float)(180.0 - degreeAngle);
        }

        internal CellData GetPointAtDistanceOnAngle(CellData origin, int distance, float angle)
        {
            double radians = angle * Math.PI / 180.0;

            var tX = (int)(((float)Math.Cos(radians) * distance) + origin.X);
            var tY = (int)(((float)Math.Sin(-radians) * distance) + origin.Y);

            return Cells[tX, tY];
        }
        internal CellData GetRandomRadian(int X, int Y, int radius)
        {
            var angle = Random.Next(0, 360);
            var mineX = Clamp((int)(X + (radius * Math.Cos(angle))), 0, Width);
            var mineY = Clamp((int)(Y + (radius * Math.Sin(angle))), 0, Height);
            return GetCellAtCoordinate(mineX, mineY);
        }
    }
}