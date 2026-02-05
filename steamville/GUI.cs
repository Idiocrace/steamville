// ISTG DONT CHANGE MY FUCKING CODE

using System;
using SFML.Graphics;
using SFML.System;
using Crucible.Graphics;

public sealed class GUI : IDisposable
{
    private readonly GraphicsCore _graphics;
    private readonly Text _welcomeText;
    private readonly RectangleShape _rectangle;
    private readonly Button _testButton;
    private readonly RoundedRectangleShape rounded;
    private bool _disposed;

    public bool IsRunning => !_disposed && _graphics.IsRunning;

    public GUI()
    {
        _graphics = new GraphicsCore(800, 600, "Crucible Game", 60);

        _welcomeText = new Text(_graphics.GameFont, "Welcome to Crucible!", 30)
        {
            FillColor = Color.White,
            Position = new SFML.System.Vector2f(100, 100)
        };

        _rectangle = new RectangleShape()
        {
            Size = new SFML.System.Vector2f(200, 100),
            FillColor = Color.Green,
            Position = new SFML.System.Vector2f(300, 250)
        };

        _testButton = new Button(
            _graphics.GameFont,           // font
            "Click Me",                   // text
            new SFML.System.Vector2f(325, 400),       // position
            new SFML.System.Vector2f(150, 50)         // size (width, height)
        );

        // Set BG color to purple
        _graphics.SetBackgroundColor(new Color(149, 0, 255));

        var rounded = new RoundedRectangleShape(new SFML.System.Vector2f(220, 90), 20, 12);
        rounded.Position = new SFML.System.Vector2f(300, 200);
        rounded.FillColor = Color.Blue;
        rounded.OutlineThickness = 2f;
        rounded.OutlineColor = Color.Blue;
        this.rounded = rounded;
    }

    public void PollEvents()
    {
        if (_disposed) return;
        _graphics.PollEvents();

        // Check if button is clicked
        if (_testButton.IsClicked(_graphics.GameWindow))
        {
            Console.WriteLine("Button was clicked!");
        }

    }

    public void Render()
    {
        if (_disposed) return;

        _graphics.BeginFrame();
        _graphics.DrawText(_welcomeText);
        _graphics.DrawShape(_rectangle);
        _graphics.DrawButton(_testButton);
        _graphics.DrawShape(rounded);
        _graphics.EndFrame();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _welcomeText.Dispose();   // GPU resource
        _rectangle.Dispose();     // GPU resource
        _testButton.Dispose();    // GPU resource
        rounded.Dispose();        // GPU resource
        _graphics.Dispose();      // Window + event hooks
    }
}
