using System;
using SFML.Graphics;
using SFML.System;
using Crucible.Graphics;

namespace Crucible
{
    public class Game
    {
        private readonly GraphicsCore _graphics;

        public Game()
        {
            // Initialize the graphics core (window opens here)
            _graphics = new GraphicsCore(800, 600, "Crucible Game", 60);
        }

        public void Run()
        {
            // Main game loop
            while (_graphics.IsRunning)
            {
                // Handle window events
                _graphics.PollEvents();

                // Clear the window for this frame
                _graphics.BeginFrame();

                // Example: Draw some text
                var text = new Text(_graphics.GameFont, "Welcome to Crucible!", 30)
                {
                    FillColor = Color.White,
                    Position = new Vector2f(100, 100)
                };
                _graphics.DrawText(text);

                // Display the frame
                _graphics.EndFrame();
            }

            // Dispose graphics core when done
            _graphics.Dispose();
        }

        static void Main()
        {
            var game = new Game();
            game.Run();
        }
    }
}
