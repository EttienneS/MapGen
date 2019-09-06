using ConsoleGameEngine;

namespace MapGen
{
    public class CellData
    {
        public CellData(int x, int y)
        {
            X = x;
            Y = y;
        }

        public string Tile { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Color { get; set; } = 6;
    }
}