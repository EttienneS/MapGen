namespace MapGen
{
    public class CellData
    {
        private string _tile;

        public CellData(int x, int y)
        {
            X = x;
            Y = y;
            _tile = " ";
            Color = 6;
        }

        public int Color { get; set; }

        public string Tile
        {
            get
            {
                return _tile;
            }
        }

        public int X { get; set; }
        public int Y { get; set; }

        public override string ToString()
        {
            return $"{X}:{Y} ({Tile})";
        }

        internal void SetTile(string character, int color = 6)
        {
            _tile = character;
            Color = color;
        }
    }
}