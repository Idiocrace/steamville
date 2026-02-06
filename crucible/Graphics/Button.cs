using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Crucible.Graphics
{
    public class Button : IDisposable
    {
        public RoundedRectangleShape Shape { get; }
        public Text TextObject { get; }

        public Color NormalColor { get; set; }
        public Color HoverColor { get; set; }

        private bool _wasPressed = false; // Track previous mouse state

        public Button(Font font, string text, Vector2f position, Vector2f size, uint radius, uint cornerPointCount)
        {
            Shape = new RoundedRectangleShape(size, radius, cornerPointCount)
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

        // Returns true only on the frame when button is clicked (not held)
        public bool IsClicked(RenderWindow window)
        {
            bool isPressed = Mouse.IsButtonPressed(Mouse.Button.Left);
            var mousePos = (Vector2f)Mouse.GetPosition(window);
            FloatRect bounds = Shape.GetGlobalBounds();
            bool isInside = bounds.Contains(mousePos);

            // Detect click: mouse was not pressed last frame, is pressed now, and is inside button
            bool clicked = !_wasPressed && isPressed && isInside;
            _wasPressed = isPressed;

            return clicked;
        }

        public void UpdateHover(RenderWindow window)
        {
            var mousePos = (Vector2f)Mouse.GetPosition(window);
            FloatRect bounds = Shape.GetGlobalBounds();
            
            if (bounds.Contains(mousePos))
            {
                Shape.FillColor = HoverColor;
            }
            else
            {
                Shape.FillColor = NormalColor;
            }
        }

        public void Draw(RenderWindow window)
        {
            window.Draw(Shape);
            window.Draw(TextObject);
        }

        public void Dispose()
        {
            Shape.Dispose();
            TextObject.Dispose();
        }
    }
}