using ConsoleGameEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MapGen
{
    public class Map
    {
        public const string Building = "█";
        public const string Floor = "≡";
        public const string Grass = ".";
        public const string Reserved = "-";
        public const string Road = "░";
        public CellData[,] Cells;
        public int Height;
        public Random Random;
        public int Width;

        public Map(int width, int height, Random random)
        {
            Cells = new CellData[width, height];
            Width = width;
            Height = height;
            Random = random;
            Streets = new List<List<CellData>>();
            Buildings = new List<List<CellData>>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Cells[x, y] = new CellData(x, y);
                }
            }
        }

        public enum Axis
        {
            xOnly, yOnly, Both
        }

        public List<List<CellData>> Buildings { get; set; }
        public List<List<CellData>> Streets { get; set; }

        private CellData Center
        {
            get
            {
                return Cells[Width / 2, Height / 2];
            }
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

        public List<CellData> GetBorder(List<CellData> square)
        {
            var frame = square.ToList();
            var hollow = HollowSquare(square);
            frame.RemoveAll(c => hollow.Contains(c));

            return frame;
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

        public (int minx, int maxx, int miny, int maxy) GetMinMax(List<CellData> cells)
        {
            var minx = int.MaxValue;
            var maxx = int.MinValue;

            var miny = int.MaxValue;
            var maxy = int.MinValue;

            foreach (var cell in cells)
            {
                if (cell.X > maxx)
                {
                    maxx = cell.X;
                }
                if (cell.X < minx)
                {
                    minx = cell.X;
                }
                if (cell.Y > maxy)
                {
                    maxy = cell.Y;
                }
                if (cell.Y < miny)
                {
                    miny = cell.Y;
                }
            }

            return (minx, maxx, miny, maxy);
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

        public CellData GetRandomCell()
        {
            return Cells[Random.Next(0, Width), Random.Next(0, Height)];
        }

        public List<CellData> GetRectangle(CellData cell, int width, int height)
        {
            var cells = new List<CellData>();

            var fromX = Math.Min(cell.X, cell.X + width);
            var toX = Math.Max(cell.X, cell.X + width);

            var fromY = Math.Min(cell.Y, cell.Y + height);
            var toY = Math.Max(cell.Y, cell.Y + height);

            for (var x = fromX; x < toX; x++)
            {
                for (var y = fromY; y < toY; y++)
                {
                    AddCellIfValid(x, y, cells);
                }
            }

            return cells;
        }

        public (int, int) GetWidthAndHeight(List<CellData> cells)
        {
            var minMax = GetMinMax(cells);
            return (minMax.maxx - minMax.minx, minMax.maxy - minMax.miny);
        }

        public List<CellData> HollowSquare(List<CellData> square)
        {
            var minMax = GetMinMax(square);
            return GetRectangle(new CellData(minMax.Item1 + 1,
                                             minMax.Item3 + 1),
                                             minMax.Item2 - minMax.Item1 - 1,
                                             minMax.Item4 - minMax.Item3 - 1);
        }

        internal void Clear()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Cells[x, y].SetTile(" ");
                }
            }
        }

        internal void CreateTown()
        {
            CreateStreets();

            CreateBuildings(7, 7, 3, 3);

            foreach (var cell in Cells)
            {
                if (string.IsNullOrWhiteSpace(cell.Tile) && Random.NextDouble() > 0.8)
                {
                    cell.SetTile(Grass, 3);
                }
            }
        }

        internal void Fill(string character, int color)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (string.IsNullOrWhiteSpace(Cells[x, y].Tile))
                    {
                        Cells[x, y].SetTile(character, color);
                    }
                }
            }
        }

        internal CellData GetCellAttRadian(CellData center, int radius, int angle)
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

        internal List<CellData> GetDiameterLine(CellData center, int lenght, int angle = 0)
        {
            return GetLine(GetPointAtDistanceOnAngle(center, lenght / 2, angle),
                           GetPointAtDistanceOnAngle(center, lenght / 2, angle + 180));
        }

        internal CellData GetPointAtDistanceOnAngle(CellData origin, int distance, float angle)
        {
            var radians = angle * Math.PI / 180.0;

            // cater for right angle scenarios
            var tX = origin.X;
            var tY = origin.Y;

            if (angle != 0 && angle != 180)
            {
                tY = (int)((Math.Sin(-radians) * distance) + origin.Y);
            }

            if (angle != 90 && angle != 270)
            {
                tX = (int)((Math.Cos(radians) * distance) + origin.X);
            }

            // add 1 to offset rounding errors
            return Cells[tX, tY];
        }

        internal CellData GetRandomRadian(CellData origin, int radius)
        {
            var angle = Random.Next(0, 360);
            var mineX = Clamp((int)(origin.X + (radius * Math.Cos(angle))), 0, Width);
            var mineY = Clamp((int)(origin.Y + (radius * Math.Sin(angle))), 0, Height);
            return GetCellAtCoordinate(mineX, mineY);
        }

        private void CreateBuildings(int maxWidth, int maxHeight, int minWidth, int minHeight)
        {
            foreach (var street in Streets)
            {
                for (int cellIndex = 0; cellIndex < street.Count - 1; cellIndex++)
                {
                    var cell = street[cellIndex];
                    var biggest = new List<CellData>();

                    var neighbours = new List<CellData>
                    {
                        cell,
                        new CellData(cell.X + 1, cell.Y),
                        new CellData(cell.X - 1, cell.Y),
                        new CellData(cell.X, cell.Y + 1),
                        new CellData(cell.X, cell.Y - 1),
                    };
                    foreach (var neighbour in neighbours)
                    {
                        bool found = false;
                        for (int width = -maxWidth; width < maxWidth; width++)
                        {
                            for (int height = -maxHeight; height < maxHeight; height++)
                            {
                                var structure = GetRectangle(neighbour, width, height);
                                var measure = GetWidthAndHeight(structure);

                                if (measure.Item1 < minWidth)
                                {
                                    continue;
                                }

                                if (measure.Item2 < minHeight)
                                {
                                    continue;
                                }

                                if (structure.TrueForAll(c => string.IsNullOrWhiteSpace(c.Tile)))
                                {
                                    if (Random.NextDouble() > 0.9)
                                    {
                                        biggest = structure;
                                        found = true;
                                        break;
                                    }

                                    if (structure.Count > biggest.Count)
                                    {
                                        biggest = structure;
                                        if (biggest.Count >= maxWidth * maxHeight)
                                        {
                                            // max size for space found stop searching
                                            found = true;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (found)
                            {
                                break;
                            }
                        }

                        if (found)
                        {
                            break;
                        }
                    }

                    if (biggest.Count > 0)
                    {
                        var col = Random.Next(1, 15);
                        biggest.ForEach(c => c.SetTile(Building, col));
                        Buildings.Add(biggest);

                        foreach (var buffer in Grow(biggest, Random.Next(1, 3)))
                        {
                            if (string.IsNullOrWhiteSpace(buffer.Tile))
                            {
                                buffer.SetTile(Reserved, 8);
                            }
                        }
                    }
                }
            }

            // cull buildings that are not connected
            foreach (var building in Buildings)
            {
                var v = Random.NextDouble();

                if (!Grow(building, 1).Any(c => c.Tile == Road))
                {
                    foreach (var cell in building)
                    {
                        cell.SetTile("", 1);
                    }
                }
                else
                {
                    foreach (var cell in HollowSquare(building))
                    {
                        cell.SetTile(Reserved, 1);
                    }

                    var doors = GetPossibleDoors(building);
                    if (doors.Count > 0)
                    {
                        doors[Random.Next(0, doors.Count)].SetTile(Reserved);
                    }
                    else
                    {
                        foreach (var cell in building)
                        {
                            cell.SetTile("", 3);
                        }
                    }
                }
            }
        }

        private void CreateStreets()
        {
            var mainStreet = GetDiameterLine(Center, Random.Next(30, 90), Random.Next(-10, 10));
            mainStreet.ForEach(c => c.SetTile(Road));
            Streets.Add(mainStreet);

            for (int i = 1; i < mainStreet.Count; i++)
            {
                if (Random.NextDouble() > 0.8)
                {
                    MakeStreet(mainStreet[i], Random.Next(15, 25), true, Random.NextDouble() * 2, 6);
                    i += 5;
                }
            }
        }

        private List<CellData> GetCorners(List<CellData> square)
        {
            var minMax = GetMinMax(square);

            return new List<CellData>
            {
                new CellData(minMax.minx, minMax.miny),
                new CellData(minMax.minx, minMax.maxy),
                new CellData(minMax.maxx, minMax.miny),
                new CellData(minMax.maxx, minMax.maxy)
            };
        }

        private List<CellData> GetNeigbours(CellData cell)
        {
            return Grow(new List<CellData> { cell }, 1);
        }

        private List<CellData> GetPossibleDoors(List<CellData> building)
        {
            var doors = new List<CellData>();
            var corners = GetCorners(building);
            foreach (var cell in GetBorder(building))
            {
                if (corners.Any(c => c.X == cell.X && c.Y == cell.Y))
                {
                    continue;
                }

                var neighbours = GetNeigbours(cell).Count(c => c.Tile == Road);
                if (neighbours > 1)
                {
                    doors.Add(cell);
                }
            }

            return doors;
        }

        private List<CellData> Grow(List<CellData> cells, int size)
        {
            var group = cells.ToList();

            foreach (var cell in cells)
            {
                for (int x = -size; x <= size; x++)
                {
                    for (int y = -size; y <= size; y++)
                    {
                        var tx = cell.X + x;
                        var ty = cell.Y + y;

                        if (group.Any(g => g.X == tx && g.Y == ty))
                        {
                            continue;
                        }

                        if (tx >= 0 && tx < Width && ty >= 0 && ty < Height)
                        {
                            group.Add(Cells[tx, ty]);
                        }
                    }
                }
            }

            return group.Distinct().ToList();
        }

        private void MakeStreet(CellData crossingPoint, int length, bool vertical, double momentum, int color)
        {
            var degrees = vertical ? new[] { 90, 270 } : new[] { 0, 180 };
            var angle = degrees[Random.Next(0, 2)];

            if (Random.NextDouble() > 0.7)
            {
                angle += Random.Next(-10, -10);
            }

            var street = GetLine(crossingPoint, GetPointAtDistanceOnAngle(crossingPoint, length, angle));

            foreach (var cell in street)
            {
                cell.SetTile(Road, color);
            }

            Streets.Add(street);
            momentum *= Random.NextDouble() + 1f;
            length = (int)((length * Random.NextDouble()) + 1f);

            if (momentum > 0.1f)
            {
                for (int i = (int)Math.Ceiling(length / 3f); i < street.Count; i++)
                {
                    if (Random.NextDouble() > 0.9)
                    {
                        MakeStreet(street[i], length, !vertical, momentum, color);
                        i += 5;
                    }
                }
            }
        }
    }
}