using System;
using System.Threading;
using TileGame.Graphics;

namespace TileGame
{
    public class TileGame
    {
        // Static resources (fixes missing Resources error)
        public static System.Collections.Generic.List<object> Resources = new System.Collections.Generic.List<object>();

        private static GraphicsCore? Graphics;
        private static GameGUI? GUI;
        private static bool Running = true;

        public static void MainCycle()
        {
            while (Running && Graphics != null && Graphics.IsRunning)
            {
                // placeholder for game logic
                Thread.Sleep(16); // ~60 ticks/sec
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
                    // placeholder for game drawing
                }

                Graphics.EndFrame();
            }

            Running = false;
        }

        public static void Main()
        {
            Graphics = new GraphicsCore();
            GUI = new GameGUI(Graphics);

            Thread gameThread = new Thread(MainCycle);
            gameThread.Start();

            RenderLoop();

            gameThread.Join();
            Graphics.Dispose();
        }
    }
}
