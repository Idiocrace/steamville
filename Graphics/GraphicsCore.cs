using System;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace TileGame.Graphics
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused
    }

    public class GraphicsCore : IDisposable
    {
        public RenderWindow GameWindow;
        public View GameView;
        public Font GameFont;
        public GameState CurrentState { get; private set; } = GameState.MainMenu;
        public bool IsRunning => GameWindow?.IsOpen ?? false;

        public GraphicsCore()
        {
            uint width = 800;
            uint height = 600;

            // Fixed VideoMode: Vector2u
            GameWindow = new RenderWindow(new VideoMode(new Vector2u(width, height)), "SteamVille");
            GameWindow.SetFramerateLimit(60);

            // Fixed FloatRect constructor
            GameView = new View(new FloatRect(new Vector2f(0f, 0f), new Vector2f((float)width, (float)height)));

            // Load font
            try
            {
                GameFont = new Font("Assets/Fonts/font_a.ttf");
            }
            catch
            {
                GameFont = new Font("/System/Library/Fonts/Supplemental/Arial.ttf");
            }
        }

        public void SetState(GameState state)
        {
            CurrentState = state;
        }

        public void PollEvents() => GameWindow.DispatchEvents();

        public void BeginFrame()
        {
            if (CurrentState == GameState.MainMenu)
                GameWindow.Clear(new Color(20, 30, 50));
            else
                GameWindow.Clear(Color.Cyan);
        }

        public void EndFrame() => GameWindow.Display();

        public void DrawText(Text text) => GameWindow.Draw(text);

        public Vector2i MousePosition => Mouse.GetPosition(GameWindow);
        public Vector2u WindowSize => GameWindow.Size;

        public void Dispose()
        {
            GameFont?.Dispose();
            GameWindow?.Dispose();
        }
    }
}
