using SFML.Window;
using SFML.System;
using SFML.Graphics;

namespace TileGame.Graphics
{
    public static class InputManager
    {
        // Keyboard
        public static bool IsKeyPressed(Keyboard.Key key) => Keyboard.IsKeyPressed(key);

        // Mouse
        public static bool IsMousePressed(Mouse.Button button) => Mouse.IsButtonPressed(button);

        // Mouse position relative to window
        public static Vector2i GetMousePosition(RenderWindow window) => Mouse.GetPosition(window);
    }
}
