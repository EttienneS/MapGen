using ConsoleGameEngine;
using System;

namespace MapGen
{
    internal class Program : ConsoleGame
    {
        private const int centerX = Width / 2;

        private const int centerY = Height / 2;

        private const int Height = 64;

        private const int Width = 128;

        private readonly Map Map = new Map(Width, Height);

        private readonly Random Random = new Random();

        private CellData Center;

        public override void Create()
        {
            Engine.SetPalette(Palettes.Pico8);
            Engine.Borderless();

            TargetFramerate = 60;

            Center = Map.Cells[centerX, centerY];
            

            for (int i = 0; i < 36; i++)
            {
                var target = Map.GetPointAtDistanceOnAngle(Center, 20, i * 10);
                target.Tile = "+";
                foreach (var cell in Map.GetLine(Center, target))
                {
                    cell.Tile = "+";
                }
            }

            for (int i = 0; i < 10; i++)
            {
                var point1 = Map.Cells[Random.Next(0, Width), Random.Next(0, Height)];
                var point2 = Map.Cells[Random.Next(0, Width), Random.Next(0, Height)];
                foreach (var cell in Map.GetLine(point1, point2))
                {
                    cell.Tile = "|";
                }
            }
        }

        public override void Render()
        {
            Engine.ClearBuffer();

            foreach (var cell in Map.Cells)
            {
                Engine.WriteText(new Point(cell.X, cell.Y), cell.Tile, cell.Color);
            }

            Engine.DisplayBuffer();
        }

        public override void Update()
        {
            //var cCol = Random.Next(0, 15);

            //foreach (var cell in Map.GetCircle(Map.GetRandomCell(), Random.Next(5,10)))
            //{
            //    cell.Tile = "+";
            //    cell.Color = cCol;
            //}

            //var point1 = Map.Cells[Random.Next(0, Width), Random.Next(0, Height)];
            //var point2 = Map.Cells[Random.Next(0, Width), Random.Next(0, Height)];
            //var col = Random.Next(0, 15);
            //foreach (var cell in Map.GetLine(point1, point2))
            //{
            //    cell.Tile = "O";
            //    cell.Color = col;
            //}
        }

        private static void Main(string[] args)
        {
            new Program().Construct(Width, Height, 10, 10, FramerateMode.MaxFps);
        }
    }
}