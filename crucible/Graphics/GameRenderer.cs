using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;

namespace Crucible.Graphics;

/// <summary>
/// Handles rendering of game objects and UI elements.
/// </summary>
public class GameRenderer : IDisposable
{
    private readonly GraphicsCore _graphics;
    private readonly Text _testText;
    private bool _disposed;
    
    // Configuration constants
    private const uint DefaultFontSize = 60;
    private const float TextHorizontalOffset = 150f;
    private const float TextVerticalPosition = 80f;

    /// <summary>
    /// Initializes a new instance of the GameRenderer class.
    /// </summary>
    /// <param name="graphics">The graphics core instance for rendering operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when graphics is null.</exception>
    public GameRenderer(GraphicsCore graphics)
    {
        _graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
        
        // Initialize reusable text object
        // Correct order: Text(Font, string, uint)
        _testText = new Text(_graphics.GameFont, "Test Text", DefaultFontSize)
        {
            FillColor = Color.White
        };
    }

    /// <summary>
    /// Renders all game elements for the current frame.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if called after disposal.</exception>
    public void Draw()
    {
        ThrowIfDisposed();
        
        // Apply game view transformation for world-space rendering
        _graphics.GameWindow.SetView(_graphics.GameView);
        
        // Center text horizontally with offset
        _testText.Position = CalculateTextPosition();
        
        _graphics.DrawText(_testText);
    }

    /// <summary>
    /// Calculates the centered position for the test text.
    /// </summary>
    /// <returns>The calculated position vector.</returns>
    private Vector2f CalculateTextPosition()
    {
        float centeredX = (_graphics.WindowSize.X / 2f) - TextHorizontalOffset;
        return new Vector2f(centeredX, TextVerticalPosition);
    }

    /// <summary>
    /// Updates the text content displayed on screen.
    /// </summary>
    /// <param name="text">The new text to display.</param>
    /// <exception cref="ArgumentNullException">Thrown when text is null.</exception>
    /// <exception cref="ObjectDisposedException">Thrown if called after disposal.</exception>
    public void UpdateText(string text)
    {
        ThrowIfDisposed();
        
        if (text == null)
            throw new ArgumentNullException(nameof(text));
            
        _testText.DisplayedString = text;
    }

    /// <summary>
    /// Updates the text color.
    /// </summary>
    /// <param name="color">The new color for the text.</param>
    /// <exception cref="ObjectDisposedException">Thrown if called after disposal.</exception>
    public void UpdateTextColor(Color color)
    {
        ThrowIfDisposed();
        _testText.FillColor = color;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(GameRenderer));
    }

    /// <summary>
    /// Releases all resources used by the GameRenderer.
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
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // Dispose managed resources
            _testText?.Dispose();
        }

        _disposed = true;
    }

    /// <summary>
    /// Finalizer to ensure resources are cleaned up if Dispose is not called.
    /// </summary>
    ~GameRenderer()
    {
        Dispose(false);
    }
}
