using System;
using ConsoleGameEngine;

namespace MapGen
{
    public class CellData
    {
        public CellData(int x, int y)
        {
            X = x;
            Y = y;
            _tile = " ";
            Color = 6;
        }

        private string _tile;
        public string Tile
        {
            get
            {
                return _tile;
            }
        }
        public int X { get; set; }
        public int Y { get; set; }
        public int Color { get; set; }

        internal void SetTile(string character, int color = 6)
        {
            _tile = character;
            Color = color;
        }
    }
}