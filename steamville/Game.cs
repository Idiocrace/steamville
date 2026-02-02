using System;
using SFML.Graphics;
using SFML.System;
using Crucible.Graphics;

namespace Crucible
{
    public class Game : IDisposable
    {
        private readonly GraphicsCore _graphics;
        private readonly Text _welcomeText;

        public Game()
        {
            // Initialize graphics core
            _graphics = new GraphicsCore(800, 600, "Crucible Game", 60);

            // Initialize text once
            _welcomeText = new Text(_graphics.GameFont, "Welcome to Crucible!", 30)
            {
                FillColor = Color.White,
                Position = new Vector2f(100, 100)
            };
        }

        public void Run()
        {
            while (_graphics.IsRunning)
            {
                _graphics.PollEvents();
                _graphics.BeginFrame();

                // Draw the reusable text
                _graphics.DrawText(_welcomeText);

                _graphics.EndFrame();
            }
        }

        public void Dispose()
        {
            // Dispose all disposable objects
            _welcomeText.Dispose();
            _graphics.Dispose();
        }

        static void Main()
        {
            using var game = new Game(); // Dispose prolly
            game.Run();
        }
    }
}
