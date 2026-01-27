using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace TileGame.Graphics
{
    public class GameGUI
    {
        private GraphicsCore Graphics;

        // --- Menu Content ---
        private List<string> mainMenuItems = new List<string> { "Start Game", "Options", "Quit" };
        private int mainMenuSelectedIndex = 0;

        private List<string> optionItems = new List<string> { "Volume", "Back" };
        private int optionSelectedIndex = 0;

        // --- Slider & UI State ---
        private float volumeValue = 75f; // 0 to 100
        private bool isDraggingSlider = false;
        private bool inOptions = false;

        // --- Input Debouncing ---
        private bool keyPressed = false;
        private bool mouseWasPressed = false;

        public GameGUI(GraphicsCore graphics)
        {
            Graphics = graphics;
        }

        public void UpdateMainMenu()
        {
            // Get inputs from your InputManager
            var upPressed = InputManager.IsKeyPressed(Keyboard.Key.Up) || InputManager.IsKeyPressed(Keyboard.Key.W);
            var downPressed = InputManager.IsKeyPressed(Keyboard.Key.Down) || InputManager.IsKeyPressed(Keyboard.Key.S);
            var enterPressed = InputManager.IsKeyPressed(Keyboard.Key.Enter) || InputManager.IsKeyPressed(Keyboard.Key.Space);
            
            var mousePos = InputManager.GetMousePosition(Graphics.GameWindow);
            bool mouseIsDown = InputManager.IsMousePressed(Mouse.Button.Left);

            // --- 1. Keyboard Navigation ---
            if (!keyPressed && !isDraggingSlider)
            {
                if (upPressed)
                {
                    if (inOptions) optionSelectedIndex = (optionSelectedIndex - 1 + optionItems.Count) % optionItems.Count;
                    else mainMenuSelectedIndex = (mainMenuSelectedIndex - 1 + mainMenuItems.Count) % mainMenuItems.Count;
                    keyPressed = true;
                }
                else if (downPressed)
                {
                    if (inOptions) optionSelectedIndex = (optionSelectedIndex + 1) % optionItems.Count;
                    else mainMenuSelectedIndex = (mainMenuSelectedIndex + 1) % mainMenuItems.Count;
                    keyPressed = true;
                }
                else if (enterPressed)
                {
                    HandleSelection();
                    keyPressed = true;
                }
            }

            // Reset keyboard debounce
            if (!upPressed && !downPressed && !enterPressed) keyPressed = false;

            // --- 2. Mouse & Slider Logic ---
            if (inOptions)
            {
                for (int j = 0; j < optionItems.Count; j++)
                {
                    float xPos = Graphics.WindowSize.X / 2f - 150;
                    float yPos = 200 + j * 60;
                    
                    // Hitbox for the text labels
                    var labelRect = new FloatRect(new Vector2f(xPos, yPos), new Vector2f(150, 40));

                    if (labelRect.Contains(new Vector2f(mousePos.X, mousePos.Y)))
                    {
                        optionSelectedIndex = j;
                        if (mouseIsDown && !mouseWasPressed && j == 1) HandleSelection(); // Clicked "Back"
                    }

                    // Special logic for Volume Slider (index 0)
                    if (j == 0)
                    {
                        float sliderX = Graphics.WindowSize.X / 2f + 20;
                        float sliderWidth = 200;
                        var sliderRect = new FloatRect(new Vector2f(sliderX, yPos), new Vector2f(sliderWidth, 30));

                        // Start dragging if mouse is clicked inside slider area
                        if (mouseIsDown && sliderRect.Contains(new Vector2f(mousePos.X, mousePos.Y)))
                        {
                            isDraggingSlider = true;
                            optionSelectedIndex = 0; // Highlight volume if interacting
                        }
                    }
                }

                // Handle active dragging (allows mouse to move outside box while holding)
                if (isDraggingSlider)
                {
                    if (!mouseIsDown)
                    {
                        isDraggingSlider = false;
                    }
                    else
                    {
                        float sliderWidth = 200;
                        float sliderX = Graphics.WindowSize.X / 2f + 20;
                        float relativeX = mousePos.X - sliderX;
                        // Calculate percentage and clamp between 0 and 100
                        volumeValue = Math.Clamp((relativeX / sliderWidth) * 100f, 0f, 100f);
                    }
                }
            }
            else
            {
                // Main Menu Mouse Logic
                for (int j = 0; j < mainMenuItems.Count; j++)
                {
                    var rect = new FloatRect(new Vector2f(Graphics.WindowSize.X / 2f - 100, 200 + j * 60), new Vector2f(250, 50));
                    if (rect.Contains(new Vector2f(mousePos.X, mousePos.Y)))
                    {
                        mainMenuSelectedIndex = j;
                        if (mouseIsDown && !mouseWasPressed) HandleSelection();
                    }
                }
            }

            mouseWasPressed = mouseIsDown;
        }

        private void HandleSelection()
        {
            if (inOptions)
            {
                if (optionSelectedIndex == 1) inOptions = false; // Back to Main
            }
            else
            {
                switch (mainMenuSelectedIndex)
                {
                    case 0: Graphics.SetState(GameState.Playing); break;
                    case 1: inOptions = true; optionSelectedIndex = 0; break;
                    case 2: Graphics.GameWindow.Close(); break;
                }
            }
        }

        public void DrawMainMenu()
        {
            // Draw Title
            Text title = new Text(Graphics.GameFont, "SteamVille", 60)
            {
                Position = new Vector2f(Graphics.WindowSize.X / 2f - 150, 80),
                FillColor = Color.White
            };
            Graphics.DrawText(title);

            if (inOptions)
            {
                for (int j = 0; j < optionItems.Count; j++)
                {
                    var color = (j == optionSelectedIndex) ? Color.Yellow : Color.White;
                    float yPos = 200 + j * 60;

                    Text text = new Text(Graphics.GameFont, optionItems[j], 24)
                    {
                        Position = new Vector2f(Graphics.WindowSize.X / 2f - 150, yPos),
                        FillColor = color
                    };
                    Graphics.DrawText(text);

                    // If this is the volume row, draw the slider next to it
                    if (j == 0)
                    {
                        DrawSlider(Graphics.WindowSize.X / 2f + 20, yPos + 10, volumeValue, color);
                    }
                }
            }
            else
            {
                // Draw Main Menu List
                for (int j = 0; j < mainMenuItems.Count; j++)
                {
                    var color = (j == mainMenuSelectedIndex) ? Color.Yellow : Color.White;
                    string prefix = (j == mainMenuSelectedIndex) ? "> " : "  ";
                    
                    Text menuItem = new Text(Graphics.GameFont, prefix + mainMenuItems[j], 30)
                    {
                        Position = new Vector2f(Graphics.WindowSize.X / 2f - 100, 200 + j * 60),
                        FillColor = color
                    };
                    Graphics.DrawText(menuItem);
                }
            }
        }

        private void DrawSlider(float x, float y, float value, Color highlightColor)
        {
            float width = 200;
            float height = 15;

            // Slider Background
            RectangleShape bg = new RectangleShape(new Vector2f(width, height))
            {
                Position = new Vector2f(x, y),
                FillColor = new Color(60, 60, 60),
                OutlineColor = Color.White,
                OutlineThickness = 1
            };
            
            // Slider Progress (Fill)
            RectangleShape progress = new RectangleShape(new Vector2f((value / 100f) * width, height))
            {
                Position = new Vector2f(x, y),
                FillColor = Color.Yellow
            };

            // Percentage Text
            string percentString = ((int)value).ToString() + "%";
            Text percentText = new Text(Graphics.GameFont, percentString, 20)
            {
                Position = new Vector2f(x + width + 15, y - 6),
                FillColor = highlightColor
            };

            Graphics.GameWindow.Draw(bg);
            Graphics.GameWindow.Draw(progress);
            Graphics.DrawText(percentText);
        }
    }
}