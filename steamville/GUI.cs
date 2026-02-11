using System;
using SFML.Graphics;
using SFML.System;
using Crucible.Graphics;

public sealed class GUI : IDisposable
{
    private readonly GraphicsCore _graphics;
    private readonly Text _welcomeText;
    private Text testText;
    private readonly RectangleShape _rectangle;
    private readonly Button _testButton;
    private bool _disposed;
    private bool _updateTime;

    public bool IsRunning => !_disposed && _graphics.IsRunning;

    public GUI()
    {
        _graphics = new GraphicsCore(800, 600, "Crucible Game", 60);

        _welcomeText = new Text(_graphics.GameFont, "Welcome to Crucible!", 30)
        {
            FillColor = Color.White,
            Position = new SFML.System.Vector2f(100, 100)
        };

        testText = new Text(_graphics.GameFont, "This is a test text.", 20)
        {
            FillColor = Color.Yellow,
            Position = new SFML.System.Vector2f(100, 150)
        };

        _rectangle = new RectangleShape()
        {
            Size = new SFML.System.Vector2f(200, 100),
            FillColor = Color.Green,
            Position = new SFML.System.Vector2f(300, 250)
        };

        _testButton = new Button(_graphics.GameFont, "Click Me", new Vector2f(100, 100), new Vector2f(200, 50), 15, 8);

        // Set BG color to purple
        _graphics.SetBackgroundColor(new Color(149, 0, 255));
    }

    public void PollEvents()
    {
        if (_disposed) return;

        _graphics.PollEvents();

        // Check if button is clicked
        if (_testButton.IsClicked(_graphics.GameWindow))
        {
            Console.WriteLine("Button was clicked!");
            _updateTime = !_updateTime;

            // Ensure that this null item cannot be trashed
            if (testText != null)
            {
                _graphics.TrashItem(testText);
                testText = null;
            }
        }
    }

    public void Render()
    {
        if (_disposed) return;

        // Update time every frame if flag is set
        if (_updateTime)
        {
            if (_welcomeText != null)
            {
                _graphics.UpdateText(_welcomeText, "Welcome to Crucible! " + DateTime.Now.ToLongTimeString());
            }
        }
        else
        {
            if (_welcomeText != null)
            {
                _graphics.UpdateText(_welcomeText, "Welcome to Crucible! Button not clicked.");
            }
        }

        _graphics.BeginFrame();
        
        if (_welcomeText != null)
        {
            _graphics.DrawText(_welcomeText);
        }
        
        if (testText != null)
        {
            _graphics.DrawText(testText);
        }
        
        if (_rectangle != null)
        {
            _graphics.DrawShape(_rectangle);
        }
        
        if (_testButton != null)
        {
            _graphics.DrawButton(_testButton);
        }
        
        _graphics.EndFrame();
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;

        _welcomeText.Dispose();
        testText?.Dispose(); // Safe disposal - only if not already trashed
        _rectangle.Dispose();
        _testButton.Dispose();
        _graphics.Dispose();
    }
}