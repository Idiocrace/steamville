using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Crucible.Graphics
{
    public class Button : IDisposable
    {
        public RectangleShape Shape { get; }
        public Text TextObject { get; }

        public Color NormalColor { get; set; }
        public Color HoverColor { get; set; }

        public Button(Font font, string text, Vector2f position, Vector2f size)
        {
            Shape = new RectangleShape(size)
            {
                Position = position,
                FillColor = Color.Blue
            };

            TextObject = new Text(font, text, 20)
            {
                Position = position + new Vector2f(10, 5),
                FillColor = Color.White
            };

            NormalColor = Color.Blue;
            HoverColor = Color.Cyan;
        }

        // Returns true if left mouse button clicked inside this button
// Returns true if left mouse button clicked inside this button
        public bool IsClicked(RenderWindow window)
        {
            if (Mouse.IsButtonPressed(Mouse.Button.Left))
            {
                var mousePos = (Vector2f)Mouse.GetPosition(window); // get mouse relative to window
                FloatRect bounds = Shape.GetGlobalBounds();
                if (bounds.Contains(mousePos)) // <-- Pass Vector2f, not two floats
                {
                    return true;
                }
            }
            return false;
        }


        public void Dispose()
        {
            Shape.Dispose();
            TextObject.Dispose();
        }
    }
}
