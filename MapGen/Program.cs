using ConsoleGameEngine;
using System;

namespace MapGen
{
    internal class Program : ConsoleGame
    {
        public Random Random;
        public int Seed = 1;
        

        private const int Height = 64;

        private const int Width = 128;

        private Map Map;
        private bool Paused = true;

        public override void Create()
        {
            Engine.SetPalette(Palettes.Pico8);
            Engine.Borderless();

            TargetFramerate = 3;

            MakeTown(Seed);
        }

        public override void Render()
        {
            Engine.ClearBuffer();

            foreach (var cell in Map.Cells)
            {
                Engine.WriteText(new Point(cell.X, cell.Y), cell.Tile, cell.Color);
            }

            var seedString = Seed.ToString();
            for (int i = 0; i < seedString.Length; i++)
            {
                Engine.WriteText(new Point(i + 1, 1), seedString.Substring(i, 1), 6);
            }

            Engine.DisplayBuffer();
        }

        public override void Update()
        {
            if (Engine.GetKeyDown(ConsoleKey.Spacebar))
            {
                Paused = !Paused;
            }

            if (!Paused || Engine.GetKeyDown(ConsoleKey.X))
            {
                Seed++;
                Map.Clear();
                var passed = MakeTown(Seed);
                
                while (!Paused && !passed)
                {
                    passed = MakeTown(Seed);
                    Seed++;
                }
            }
        }

        private static void Main(string[] args)
        {
            new Program().Construct(Width, Height, 10, 10, FramerateMode.MaxFps);
        }

        public const string Error = "░";

       
        private bool MakeTown(int seed)
        {
            try
            {
                Random = new Random(seed);

                Map = new Map(Width, Height, Random);

                Map.CreateTown();
            }
            catch (Exception ex)
            {
                Map.Fill(Error, 8);
                return false;
            }

            return true;
        }
    }
}