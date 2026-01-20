using System;
using SFML.Graphics;
using SFML.Window;
using SFML.System;

using TileGame.Types;
using System.Numerics;

namespace TileGame.Graphics;

public class GraphicsCore : IDisposable
{
    // Fields
    public required RenderWindow GameWindow;
    public required View GameView;
    public Font GameFont = new Font("Assets/Fonts/font_a.ttf");

    public Config WindowConfig = new Config();

    public bool IsRunning => GameWindow.IsOpen;

    // Class constructor
    public GraphicsCore(Config config)
    {
        // Load config
        WindowConfig = config;

        // Create the window
        uint width = config.GetSetting<uint>("window_width", 800);
        uint height = config.GetSetting<uint>("window_height", 600);
        VideoMode videoMode = new VideoMode(new Vector2u(width, height));

        GameWindow = new RenderWindow(videoMode, "SteamVille - No Status Set");
        GameWindow.SetFramerateLimit(WindowConfig.GetSetting<uint>("framerate_limit", 60));

        // Create the view
        GameView = new View(new FloatRect(new Vector2f(0f, 0f), new Vector2f((float)width, (float)height)));
        GameWindow.SetView(GameView);
    }

    public void PollEvents()
    {
        GameWindow.DispatchEvents();
    }

    public void BeginFrame()
    {
        GameWindow.Clear(Color.Cyan);
    }

    public Texture LoadTexture(string filePath)
    {
        return new Texture(filePath);
    }


    public void DrawTexture(Texture texture, FloatRect sourceRect, FloatRect destRect)
    {
        Sprite sprite = new Sprite(texture)
        {
            TextureRect = new IntRect(new Vector2i((int)sourceRect.Left, (int)sourceRect.Top), new Vector2i((int)sourceRect.Width, (int)sourceRect.Height)),
            Position = new Vector2f(destRect.Left, destRect.Top),
            Scale = new Vector2f(destRect.Width / sourceRect.Width, destRect.Height / sourceRect.Height)
        };

        GameWindow.Draw(sprite);
    }

    public void DrawSprite(Sprite sprite)
    {
        GameWindow.Draw(sprite);
    }

    public bool IsKeyPressed(Keyboard.Key key)
    {
        return Keyboard.IsKeyPressed(key);
    }

    public Vector2i MousePosition => new Vector2i(Mouse.GetPosition(GameWindow).X, Mouse.GetPosition(GameWindow).Y);

    public Vector2u WindowSize => GameWindow.Size;

    public void EndFrame()
    {
        GameWindow.Display();
    }

    public void Dispose()
    {
        // Cleanup resources here
    }
}
