using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.Window;
using SFML.System;

using TileGame.Types;
using System.Numerics;

namespace TileGame.Graphics;

public enum GameState
{
    MainMenu,
    Playing,
    Paused
}

public class GraphicsCore : IDisposable
{
    // Fields
    public RenderWindow GameWindow;
    public View GameView;
    public Font GameFont;

    public Config WindowConfig = new Config();

    public bool IsRunning => GameWindow?.IsOpen ?? false;
    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    // Menu fields
    private List<string> menuItems = new List<string> { "Start Game", "Options", "Quit" };
    private int selectedMenuIndex = 0;
    private bool keyPressed = false;

    // Class constructor
    public GraphicsCore(Config config)
    {
        // Load config
        WindowConfig = config;

        // Try to load custom font, fall back to system font if not found
        try
        {
            GameFont = new Font("bcp/fonts/font_a.ttf");
            Console.WriteLine("Loaded custom font from bcp/fonts/font_a.ttf");
        }
        catch (SFML.LoadingFailedException)
        {
            // Try common macOS system font paths and Windows Arial font path
            string[] systemFontPaths = {
                "/System/Library/Fonts/Supplemental/Arial.ttf",
                "/System/Library/Fonts/Helvetica.ttc",
                "/Library/Fonts/Arial.ttf",
                "C:\\Windows\\Fonts\\arial.ttf"
            };

            bool fontLoaded = false;
            foreach (string fontPath in systemFontPaths)
            {
                if (File.Exists(fontPath))
                {
                    try
                    {
                        GameFont = new Font(fontPath);
                        Console.WriteLine($"Loaded system font from {fontPath}");
                        fontLoaded = true;
                        break;
                    }
                    catch { }
                }
            }

            if (!fontLoaded)
            {
                throw new Exception("Could not load any font. Please create Assets/Fonts/font_a.ttf or install Arial font.");
            }
        }

        // Create the window
        uint width = config.GetSetting<uint>("window_width", 800);
        uint height = config.GetSetting<uint>("window_height", 600);
        VideoMode videoMode = new VideoMode(new Vector2u(width, height));

        GameWindow = new RenderWindow(videoMode, "SteamVille - Main Menu");
        GameWindow.SetFramerateLimit(WindowConfig.GetSetting<uint>("framerate_limit", 60));

        // Create the view
        GameView = new View(new FloatRect(new Vector2f(0f, 0f), new Vector2f((float)width, (float)height)));
        GameWindow.SetView(GameView);
    }

    public void PollEvents()
    {
        GameWindow.DispatchEvents();
    }

    public void UpdateMenu()
    {
        // Handle menu navigation
        bool upPressed = Keyboard.IsKeyPressed(Keyboard.Key.Up) || Keyboard.IsKeyPressed(Keyboard.Key.W);
        bool downPressed = Keyboard.IsKeyPressed(Keyboard.Key.Down) || Keyboard.IsKeyPressed(Keyboard.Key.S);
        bool enterPressed = Keyboard.IsKeyPressed(Keyboard.Key.Enter) || Keyboard.IsKeyPressed(Keyboard.Key.Space);

        if (!keyPressed)
        {
            if (upPressed)
            {
                selectedMenuIndex--;
                if (selectedMenuIndex < 0)
                    selectedMenuIndex = menuItems.Count - 1;
                keyPressed = true;
            }
            else if (downPressed)
            {
                selectedMenuIndex++;
                if (selectedMenuIndex >= menuItems.Count)
                    selectedMenuIndex = 0;
                keyPressed = true;
            }
            else if (enterPressed)
            {
                HandleMenuSelection();
                keyPressed = true;
            }
        }

        // Reset key pressed state when keys are released
        if (!upPressed && !downPressed && !enterPressed)
        {
            keyPressed = false;
        }
    }

    private void HandleMenuSelection()
    {
        switch (selectedMenuIndex)
        {
            case 0: // Start Game
                CurrentState = GameState.Playing;
                GameWindow.SetTitle("SteamVille - Playing");
                break;
            case 1: // Options
                // You can add options menu here later
                Console.WriteLine("Options selected (not implemented yet)");
                break;
            case 2: // Quit
                GameWindow.Close();
                break;
        }
    }

    public void DrawMenu()
    {
        // Draw title
        Text title = new Text(GameFont, "SteamVille")
        {
            CharacterSize = 60,
            Position = new Vector2f(WindowSize.X / 2f - 150f, 100f),
            FillColor = Color.White
        };
        GameWindow.Draw(title);

        // Draw menu items
        for (int i = 0; i < menuItems.Count; i++)
        {
            Color itemColor = (i == selectedMenuIndex) ? Color.Yellow : Color.White;
            string prefix = (i == selectedMenuIndex) ? "> " : "  ";

            Text menuItem = new Text(GameFont, prefix + menuItems[i])
            {
                CharacterSize = 30,
                Position = new Vector2f(WindowSize.X / 2f - 100f, 250f + (i * 60f)),
                FillColor = itemColor
            };
            GameWindow.Draw(menuItem);
        }

        // Draw controls hint
        Text hint = new Text(GameFont, "Use Arrow Keys/WASD to navigate, Enter/Space to select")
        {
            CharacterSize = 16,
            Position = new Vector2f(WindowSize.X / 2f - 250f, WindowSize.Y - 50f),
            FillColor = new Color(200, 200, 200)
        };
        GameWindow.Draw(hint);
    }

    public void BeginFrame()
    {
        if (CurrentState == GameState.MainMenu)
        {
            GameWindow.Clear(new Color(20, 30, 50)); // Dark blue for menu
        }
        else
        {
            GameWindow.Clear(Color.Cyan); // Original cyan for game
        }
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
        GameFont?.Dispose();
        GameWindow?.Dispose();
    }
}