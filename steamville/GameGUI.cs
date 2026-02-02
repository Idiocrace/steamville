using System;
using System.Collections.Generic;
using TileEngine.Graphics;
using TileEngine.Core;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace SteamVille.Graphics
{
    /// <summary>
    /// Manages the SteamVille game GUI including menus and UI elements.
    /// </summary>
    public sealed class SteamVilleGUI : IGameGUI
    {
        #region Constants

        private const float DefaultVolumeValue = 75f;
        private const float TitleFontSize = 60;
        private const float MenuItemFontSize = 30;
        private const float OptionItemFontSize = 24;
        private const float SliderPercentFontSize = 20;
        private const float SliderWidth = 200f;
        private const float SliderHeight = 15f;
        private const float MenuItemSpacing = 60f;
        private const float TitleYPosition = 80f;
        private const float MenuStartYPosition = 200f;

        #endregion

        #region Private Fields

        private readonly SteamVilleGame _game;
        private readonly GraphicsCore _graphics;
        private bool _disposed;

        // Menu content
        private readonly List<string> _mainMenuItems = new() { "Start Game", "Options", "Quit" };
        private readonly List<string> _optionItems = new() { "Volume", "Back" };

        // Menu state
        private int _mainMenuSelectedIndex;
        private int _optionSelectedIndex;
        private float _volumeValue = DefaultVolumeValue;
        private bool _isDraggingSlider;
        private bool _inOptions;
        private bool _menuActive = true;

        // Input debouncing
        private bool _keyPressed;
        private bool _mouseWasPressed;

        // Reusable SFML objects (to prevent garbage)
        private Text? _titleText;
        private readonly List<Text> _cachedMenuTexts = new();
        private RectangleShape? _sliderBackground;
        private RectangleShape? _sliderProgress;
        private Text? _sliderPercentText;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the SteamVilleGUI class.
        /// </summary>
        /// <param name="game">The SteamVille game instance.</param>
        /// <param name="graphics">The graphics core instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public SteamVilleGUI(SteamVilleGame game, GraphicsCore graphics)
        {
            _game = game ?? throw new ArgumentNullException(nameof(game));
            _graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
            InitializeUIElements();
        }

        #endregion

        #region Initialization

        private void InitializeUIElements()
        {
            // Initialize title text
            _titleText = new Text(_graphics.GameFont, "SteamVille", (uint)TitleFontSize)
            {
                FillColor = Color.White
            };

            // Initialize slider components
            _sliderBackground = new RectangleShape(new Vector2f(SliderWidth, SliderHeight))
            {
                FillColor = new Color(60, 60, 60),
                OutlineColor = Color.White,
                OutlineThickness = 1
            };

            _sliderProgress = new RectangleShape(new Vector2f(0, SliderHeight))
            {
                FillColor = Color.Yellow
            };

            _sliderPercentText = new Text(_graphics.GameFont, "", (uint)SliderPercentFontSize)
            {
                FillColor = Color.White
            };

            // Pre-allocate menu text objects
            int maxMenuItems = Math.Max(_mainMenuItems.Count, _optionItems.Count);
            for (int i = 0; i < maxMenuItems; i++)
            {
                _cachedMenuTexts.Add(new Text(_graphics.GameFont, "", (uint)MenuItemFontSize)
                {
                    FillColor = Color.White
                });
            }
        }

        #endregion

        #region IGameGUI Implementation

        /// <summary>
        /// Updates the main menu state based on user input.
        /// </summary>
        public void UpdateMainMenu()
        {
            ThrowIfDisposed();

            if (!_menuActive)
                return;

            ProcessKeyboardInput();
            ProcessMouseInput();

            _mouseWasPressed = InputManager.IsMousePressed(Mouse.Button.Left);
        }

        /// <summary>
        /// Draws the main menu to the screen.
        /// </summary>
        public void DrawMainMenu()
        {
            ThrowIfDisposed();

            if (!_menuActive || _titleText == null)
                return;

            DrawTitle();

            if (_inOptions)
                DrawOptionsMenu();
            else
                DrawMainMenuItems();
        }

        #endregion

        #region Input Processing

        private void ProcessKeyboardInput()
        {
            var upPressed = InputManager.IsKeyPressed(Keyboard.Key.Up) || InputManager.IsKeyPressed(Keyboard.Key.W);
            var downPressed = InputManager.IsKeyPressed(Keyboard.Key.Down) || InputManager.IsKeyPressed(Keyboard.Key.S);
            var enterPressed = InputManager.IsKeyPressed(Keyboard.Key.Enter) || InputManager.IsKeyPressed(Keyboard.Key.Space);

            if (!_keyPressed && !_isDraggingSlider)
            {
                if (upPressed)
                {
                    NavigateUp();
                    _keyPressed = true;
                }
                else if (downPressed)
                {
                    NavigateDown();
                    _keyPressed = true;
                }
                else if (enterPressed)
                {
                    HandleSelection();
                    _keyPressed = true;
                }
            }

            if (!upPressed && !downPressed && !enterPressed)
                _keyPressed = false;
        }

        private void ProcessMouseInput()
        {
            var mousePos = InputManager.GetMousePosition(_graphics.GameWindow);
            var mouseIsDown = InputManager.IsMousePressed(Mouse.Button.Left);
            var mousePosF = new Vector2f(mousePos.X, mousePos.Y);

            if (_inOptions)
                ProcessOptionsMouseInput(mousePosF, mouseIsDown);
            else
                ProcessMainMenuMouseInput(mousePosF, mouseIsDown);
        }

        private void ProcessMainMenuMouseInput(Vector2f mousePos, bool mouseIsDown)
        {
            for (int i = 0; i < _mainMenuItems.Count; i++)
            {
                var rect = GetMainMenuItemRect(i);

                if (rect.Contains(mousePos))
                {
                    _mainMenuSelectedIndex = i;

                    if (mouseIsDown && !_mouseWasPressed)
                        HandleSelection();
                }
            }
        }

        private void ProcessOptionsMouseInput(Vector2f mousePos, bool mouseIsDown)
        {
            for (int i = 0; i < _optionItems.Count; i++)
            {
                var labelRect = GetOptionItemRect(i);

                if (labelRect.Contains(mousePos))
                {
                    _optionSelectedIndex = i;

                    if (mouseIsDown && !_mouseWasPressed && i == 1) // Back button
                        HandleSelection();
                }

                // Volume slider
                if (i == 0)
                    ProcessSliderInput(mousePos, mouseIsDown, i);
            }

            UpdateSliderDrag(mousePos, mouseIsDown);
        }

        private void ProcessSliderInput(Vector2f mousePos, bool mouseIsDown, int itemIndex)
        {
            float sliderX = _graphics.WindowSize.X / 2f + 20;
            float sliderY = MenuStartYPosition + itemIndex * MenuItemSpacing;
            var sliderRect = new FloatRect(new Vector2f(sliderX, sliderY), new Vector2f(SliderWidth, 30));

            if (mouseIsDown && sliderRect.Contains(mousePos))
            {
                _isDraggingSlider = true;
                _optionSelectedIndex = 0;
            }
        }

        private void UpdateSliderDrag(Vector2f mousePos, bool mouseIsDown)
        {
            if (_isDraggingSlider)
            {
                if (!mouseIsDown)
                {
                    _isDraggingSlider = false;
                }
                else
                {
                    float sliderX = _graphics.WindowSize.X / 2f + 20;
                    float relativeX = mousePos.X - sliderX;
                    _volumeValue = Math.Clamp((relativeX / SliderWidth) * 100f, 0f, 100f);
                }
            }
        }

        #endregion

        #region Navigation

        private void NavigateUp()
        {
            if (_inOptions)
                _optionSelectedIndex = (_optionSelectedIndex - 1 + _optionItems.Count) % _optionItems.Count;
            else
                _mainMenuSelectedIndex = (_mainMenuSelectedIndex - 1 + _mainMenuItems.Count) % _mainMenuItems.Count;
        }

        private void NavigateDown()
        {
            if (_inOptions)
                _optionSelectedIndex = (_optionSelectedIndex + 1) % _optionItems.Count;
            else
                _mainMenuSelectedIndex = (_mainMenuSelectedIndex + 1) % _mainMenuItems.Count;
        }

        private void HandleSelection()
        {
            if (_inOptions)
            {
                if (_optionSelectedIndex == 1) // Back
                    _inOptions = false;
            }
            else
            {
                switch (_mainMenuSelectedIndex)
                {
                    case 0: // Start Game
                        StartGame();
                        break;

                    case 1: // Options
                        OpenOptions();
                        break;

                    case 2: // Quit
                        QuitGame();
                        break;
                }
            }
        }

        private void StartGame()
        {
            _game.SetGameState(GameState.Playing, "SteamVille - Playing");
            _menuActive = false;
            _keyPressed = true;
            _mouseWasPressed = true;
            _inOptions = false;

            Console.WriteLine("Game started from menu.");
        }

        private void OpenOptions()
        {
            _inOptions = true;
            _optionSelectedIndex = 0;
        }

        private void QuitGame()
        {
            Console.WriteLine("Quit selected from menu.");
            _graphics.GameWindow.Close();
        }

        #endregion

        #region Drawing

        private void DrawTitle()
        {
            if (_titleText == null) return;

            _titleText.Position = new Vector2f(
                _graphics.WindowSize.X / 2f - 150,
                TitleYPosition
            );

            _graphics.DrawText(_titleText);
        }

        private void DrawMainMenuItems()
        {
            for (int i = 0; i < _mainMenuItems.Count; i++)
            {
                var text = _cachedMenuTexts[i];
                var isSelected = i == _mainMenuSelectedIndex;

                text.DisplayedString = (isSelected ? "> " : "  ") + _mainMenuItems[i];
                text.FillColor = isSelected ? Color.Yellow : Color.White;
                text.CharacterSize = (uint)MenuItemFontSize;
                text.Position = new Vector2f(
                    _graphics.WindowSize.X / 2f - 100,
                    MenuStartYPosition + i * MenuItemSpacing
                );

                _graphics.DrawText(text);
            }
        }

        private void DrawOptionsMenu()
        {
            for (int i = 0; i < _optionItems.Count; i++)
            {
                var isSelected = i == _optionSelectedIndex;
                var color = isSelected ? Color.Yellow : Color.White;
                float yPos = MenuStartYPosition + i * MenuItemSpacing;

                var text = _cachedMenuTexts[i];
                text.DisplayedString = _optionItems[i];
                text.FillColor = color;
                text.CharacterSize = (uint)OptionItemFontSize;
                text.Position = new Vector2f(_graphics.WindowSize.X / 2f - 150, yPos);

                _graphics.DrawText(text);

                // Draw slider for volume option
                if (i == 0)
                    DrawSlider(_graphics.WindowSize.X / 2f + 20, yPos + 10, _volumeValue, color);
            }
        }

        private void DrawSlider(float x, float y, float value, Color highlightColor)
        {
            if (_sliderBackground == null || _sliderProgress == null || _sliderPercentText == null)
                return;

            // Update positions
            _sliderBackground.Position = new Vector2f(x, y);
            _sliderProgress.Position = new Vector2f(x, y);
            _sliderProgress.Size = new Vector2f((value / 100f) * SliderWidth, SliderHeight);

            _sliderPercentText.DisplayedString = $"{(int)value}%";
            _sliderPercentText.FillColor = highlightColor;
            _sliderPercentText.Position = new Vector2f(x + SliderWidth + 15, y - 6);

            // Draw
            _graphics.GameWindow.Draw(_sliderBackground);
            _graphics.GameWindow.Draw(_sliderProgress);
            _graphics.DrawText(_sliderPercentText);
        }

        #endregion

        #region Helper Methods

        private FloatRect GetMainMenuItemRect(int index)
        {
            return new FloatRect(
                new Vector2f(_graphics.WindowSize.X / 2f - 100, MenuStartYPosition + index * MenuItemSpacing),
                new Vector2f(250, 50)
            );
        }

        private FloatRect GetOptionItemRect(int index)
        {
            float xPos = _graphics.WindowSize.X / 2f - 150;
            float yPos = MenuStartYPosition + index * MenuItemSpacing;
            return new FloatRect(new Vector2f(xPos, yPos), new Vector2f(150, 40));
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(SteamVilleGUI));
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Releases all resources used by the SteamVilleGUI.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and optionally managed resources.
        /// </summary>
        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Dispose all SFML objects
                _titleText?.Dispose();
                _sliderBackground?.Dispose();
                _sliderProgress?.Dispose();
                _sliderPercentText?.Dispose();

                foreach (var text in _cachedMenuTexts)
                    text?.Dispose();

                _cachedMenuTexts.Clear();

                Console.WriteLine("SteamVilleGUI disposed.");
            }

            _disposed = true;
        }

        /// <summary>
        /// Finalizer to ensure resources are cleaned up if Dispose is not called.
        /// </summary>
        ~SteamVilleGUI()
        {
            Dispose(false);
        }

        #endregion
    }
}