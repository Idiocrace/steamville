// Credits to me(APersonIThink/APersonIThink12) because I dont wanna code the fucking graphics engine

using System;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Crucible.Types;

namespace Crucible.Graphics;

/// <summary>
/// Represents the current state of the game.
/// </summary>
public enum GameState
{
    MainMenu,
    Playing,
    Paused
}

/// <summary>
/// Core graphics system managing the SFML render window, views, and rendering operations.
/// </summary>
public sealed class GraphicsCore : IDisposable
{
    #region Constants

    private const uint DefaultWidth = 800;
    private const uint DefaultHeight = 600;
    private const uint DefaultFramerateLimit = 60;
    private Color _defaultBackgroundColor = Color.Black; // default
    private const string DefaultWindowTitle = "Crucible Game";
    private const string PrimaryFontPath = "bcp/assets/font_a.ttf";
    private const string FallbackFontPath = "/System/Library/Fonts/Supplemental/Arial.ttf";

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets the main render window.
    /// </summary>
    public RenderWindow GameWindow { get; private set; } = null!;

    /// <summary>
    /// Gets the game world view used for rendering game objects.
    /// </summary>
    public View GameView { get; private set; } = null!;

    /// <summary>
    /// Gets the font used for rendering text.
    /// </summary>
    public Font GameFont { get; private set; } = null!;
    

    /// <summary>
    /// Gets the current game state.
    /// </summary>
    public GameState CurrentState
    {
        get
        {
            lock (_stateLock)
            {
                return _currentState;
            }
        }
        private set
        {
            lock (_stateLock)
            {
                _currentState = value;
            }
        }
    }

    /// <summary>
    /// Gets whether the graphics system is running.
    /// </summary>
    public bool IsRunning => GameWindow?.IsOpen ?? false;

    /// <summary>
    /// Gets the current mouse position relative to the window.
    /// </summary>
    public Vector2i MousePosition
    {
        get
        {
            ThrowIfDisposed();
            return Mouse.GetPosition(GameWindow);
        }
    }

    /// <summary>
    /// Gets the current window size.
    /// </summary>
    public Vector2u WindowSize
    {
        get
        {
            ThrowIfDisposed();
            return GameWindow.Size;
        }
    }

    #endregion

    #region Private Fields

    private GameState _currentState = GameState.MainMenu;
    private readonly object _stateLock = new();
    private bool _disposed;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the GraphicsCore class with default settings.
    /// </summary>
    public GraphicsCore()
        : this(DefaultWidth, DefaultHeight, DefaultWindowTitle, DefaultFramerateLimit)
    {
    }

    /// <summary>
    /// Initializes a new instance of the GraphicsCore class with custom settings.
    /// </summary>
    /// <param name="width">Window width in pixels.</param>
    /// <param name="height">Window height in pixels.</param>
    /// <param name="title">Window title.</param>
    /// <param name="framerateLimit">Maximum framerate limit.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when width, height, or framerate is invalid.</exception>
    /// <exception cref="ArgumentNullException">Thrown when title is null.</exception>
    public GraphicsCore(uint width, uint height, string title, uint framerateLimit)
    {
        if (width == 0)
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be greater than zero.");
        if (height == 0)
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be greater than zero.");
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentNullException(nameof(title), "Title cannot be null or empty.");
        if (framerateLimit == 0)
            throw new ArgumentOutOfRangeException(nameof(framerateLimit), "Framerate limit must be greater than zero.");

        InitializeWindow(width, height, title, framerateLimit);
        InitializeView(width, height);
        InitializeFont();
        RegisterEventHandlers();

        Console.WriteLine($"GraphicsCore initialized: {width}x{height} @ {framerateLimit}fps");
    }

    #endregion

    #region Initialization

    private void InitializeWindow(uint width, uint height, string title, uint framerateLimit)
    {
        var videoMode = new VideoMode(new Vector2u(width, height));
        GameWindow = new RenderWindow(videoMode, title);
        GameWindow.SetFramerateLimit(framerateLimit);
    }

    private void InitializeView(uint width, uint height)
    {
        var viewRect = new FloatRect(
            new SFML.System.Vector2f(0f, 0f),
            new SFML.System.Vector2f(width, height)
        );
        GameView = new View(viewRect);
    }

    private void InitializeFont()
    {
        // Try primary font first
        if (TryLoadFont(PrimaryFontPath, out var font))
        {
            GameFont = font;
            Console.WriteLine($"Loaded font from: {PrimaryFontPath}");
            return;
        }

        // Fallback to system font
        Console.WriteLine($"Warning: Could not load primary font '{PrimaryFontPath}', falling back to system font.");
        
        if (TryLoadFont(FallbackFontPath, out font))
        {
            GameFont = font;
            Console.WriteLine($"Loaded fallback font from: {FallbackFontPath}");
            return;
        }

        throw new InvalidOperationException(
            $"Failed to load both primary font '{PrimaryFontPath}' and fallback font '{FallbackFontPath}'.");
    }

    private bool TryLoadFont(string path, out Font font)
    {
        try
        {
            font = new Font(path);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load font from '{path}': {ex.Message}");
            font = null!;
            return false;
        }
    }

    private void RegisterEventHandlers()
    {
        GameWindow.Closed += OnWindowClosed;
        GameWindow.Resized += OnWindowResized;
    }

    #endregion

    #region Event Handlers

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        GameWindow?.Close();
    }

    private void OnWindowResized(object? sender, SizeEventArgs e)
    {
        // Update the view to match the new window size
        // SizeEventArgs.Size is a Vector2u
        var newViewRect = new FloatRect(
            new SFML.System.Vector2f(0f, 0f),
            new SFML.System.Vector2f(e.Size.X, e.Size.Y)
        );
        GameView = new View(newViewRect);
        
        Console.WriteLine($"Window resized to: {e.Size.X}x{e.Size.Y}");
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Sets the current game state and optionally updates the window title.
    /// </summary>
    /// <param name="state">The new game state.</param>
    /// <param name="title">Optional window title to set.</param>
    /// <exception cref="ObjectDisposedException">Thrown if called after disposal.</exception>
    public void SetState(GameState state, string? title = null)
    {
        ThrowIfDisposed();

        CurrentState = state;
        
        if (!string.IsNullOrEmpty(title))
        {
            GameWindow.SetTitle(title);
        }

        Console.WriteLine($"Game state changed to: {state}");
    }

    /// <summary>
    /// Processes all pending window events.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if called after disposal.</exception>
    public void PollEvents()
    {
        ThrowIfDisposed();
        GameWindow.DispatchEvents();
    }

    /// <summary>
    /// Clears the window and begins a new rendering frame.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if called after disposal.</exception>
    public void BeginFrame()
    {
        ThrowIfDisposed();
        GameWindow.Clear(_defaultBackgroundColor);
    }
    /// <summary>
    /// Sets the background color for clearing the window.
    /// </summary>
    public void SetBackgroundColor(Color color)
    {
        ThrowIfDisposed();
        _defaultBackgroundColor = color;
    }


    /// <summary>
    /// Displays the rendered frame to the window.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if called after disposal.</exception>
    public void EndFrame()
    {
        ThrowIfDisposed();
        GameWindow.Display();
    }

    /// <summary>
    /// Draws text to the window.
    /// </summary>
    /// <param name="text">The text object to draw.</param>
    /// <exception cref="ArgumentNullException">Thrown when text is null.</exception>
    /// <exception cref="ObjectDisposedException">Thrown if called after disposal.</exception>
    public void DrawText(Text text) 
    {
        ThrowIfDisposed();
        
        if (text == null)
            throw new ArgumentNullException(nameof(text));

        GameWindow.Draw(text);
    }

    public void DrawShape(Shape shape)
    {
        ThrowIfDisposed();

        if (shape == null)
            throw new ArgumentNullException(nameof(shape));

        GameWindow.Draw(shape);
    }

    /// <summary>
    /// Draws a button composed of a shape and text.
    /// </summary>
    /// <param name="shape">The shape representing the button's visual form.</param>
    /// <param name="text">The text to be displayed on the button.</param>
    /// <exception cref="ArgumentNullException">Thrown when either shape or text is null.</exception>
    /// <exception cref="ObjectDisposedException">Thrown if called after disposal.</exception>
    public bool DrawButton(Button button)
    {
        ThrowIfDisposed();

        if (button == null)
            throw new ArgumentNullException(nameof(button));

        // Draw visuals
        GameWindow.Draw(button.Shape);
        GameWindow.Draw(button.TextObject);

        // --- INPUT LOGIC (what makes this a REAL button) ---
        var mousePos = Mouse.GetPosition(GameWindow);
        var mouseWorld = GameWindow.MapPixelToCoords(mousePos);

        // Fix: Pass Vector2f instead of two separate floats
        bool hovered = button.Shape.GetGlobalBounds().Contains(mouseWorld);
        bool clicked = hovered && Mouse.IsButtonPressed(Mouse.Button.Left);

        // Hover effect
        if (hovered)
            button.Shape.FillColor = button.HoverColor;
        else
            button.Shape.FillColor = button.NormalColor;

        return clicked;
    }


    #endregion

    #region Helper Methods

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(GraphicsCore));
    }

    #endregion

    #region IDisposable Implementation

    /// <summary>
    /// Releases all resources used by the GraphicsCore.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and optionally managed resources.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // Unregister event handlers to prevent memory leaks
            if (GameWindow != null)
            {
                GameWindow.Closed -= OnWindowClosed;
                GameWindow.Resized -= OnWindowResized;
            }

            // Dispose managed resources
            GameView?.Dispose();
            GameFont?.Dispose();
            GameWindow?.Dispose();

            Console.WriteLine("GraphicsCore disposed.");
        }

        _disposed = true;
    }

    /// <summary>
    /// Finalizer to ensure resources are cleaned up if Dispose is not called.
    /// </summary>
    ~GraphicsCore()
    {
        Dispose(false);
    }

    #endregion
}
