using System;
using System.Collections.Generic;
using System.Threading;
using TileGame.Types;
using TileGame.Graphics;
using TileGame.Processors;

namespace TileGame
{
    public class TileGameApp
    {
        public static List<object> Resources = new();

        private static GraphicsCore? Graphics;
        private static GameGUI? GUI;
        private static bool Running = true;

        public static void MainCycle()
        {
            while (Running && Graphics != null && Graphics.IsRunning)
            {
                if (GameRender.Grid != null)
                {
                    foreach (var kv in GameRender.Grid.GetAllTiles().ToList())
                    {
                        var tile = kv.Value;
                        object? proc = tile.Processor;

                        if (proc == null) continue;

                        if (proc is BaseProcessor p)
                            p.Tick(tile);
                        else if (proc is Action<Tile> action)
                            action(tile);
                    }
                }

                Thread.Sleep(1000);
            }
        }

        public static void RenderLoop()
        {
            if (Graphics == null) return;

            while (Graphics.IsRunning)
            {
                Graphics.PollEvents();
                Graphics.BeginFrame();

                if (Graphics.CurrentState == GameState.MainMenu)
                {
                    GUI?.UpdateMainMenu();
                    GUI?.DrawMainMenu();
                }
                else if (Graphics.CurrentState == GameState.Playing)
                {
                    GameRender.UpdateAndDraw(Graphics);
                }

                Graphics.EndFrame();
            }

            Running = false;
        }

        public static void Main()
        {
            Graphics = new GraphicsCore();
            GUI = new GameGUI(Graphics);

            GameRender.Grid = new TileGrid();
            GameRender.ResourceGrid = new ResourceGrid();

            Thread gameThread = new Thread(MainCycle);
            gameThread.Start();

            RenderLoop();
    
            gameThread.Join();
            Graphics.Dispose();
        }
    }
}
