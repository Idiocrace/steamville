using System;

public sealed class Game : IDisposable
{
    private readonly GUI _gui;
    private bool _disposed;

    public Game()
    {
        _gui = new GUI();
    }

    public void Run()
    {
        while (!_disposed && _gui.IsRunning)
        {
            _gui.PollEvents();
            Update();
            _gui.Render();
        }
    }

    private void Update()
    {
        // future game logic
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _gui.Dispose();
    }
}
