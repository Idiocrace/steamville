// Credits to me(APersonIThink/APersonIThink12) because I dont wanna code the fucking graphics engine from scratch, and the input manager

using System;
using System.Collections.Generic;
using SFML.Window;
using SFML.System;
using SFML.Graphics;

namespace TileGame.Graphics
{
    /// <summary>
    /// Manages input state tracking for keyboard and mouse with frame-based detection.
    /// Provides utilities for detecting key/button presses, releases, and held states.
    /// </summary>
    public static class InputManager
    {
        #region Private Fields

        private static readonly HashSet<Keyboard.Key> _keysPressed = new();
        private static readonly HashSet<Keyboard.Key> _keysPressedLastFrame = new();
        
        private static readonly HashSet<Mouse.Button> _buttonsPressed = new();
        private static readonly HashSet<Mouse.Button> _buttonsPressedLastFrame = new();

        private static Vector2i _currentMousePosition;
        private static Vector2i _previousMousePosition;

        private static bool _initialized;

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the input manager. Should be called once at startup.
        /// </summary>
        public static void Initialize()
        {
            if (_initialized)
            {
                Console.WriteLine("Warning: InputManager already initialized.");
                return;
            }

            _keysPressed.Clear();
            _keysPressedLastFrame.Clear();
            _buttonsPressed.Clear();
            _buttonsPressedLastFrame.Clear();
            _currentMousePosition = new Vector2i(0, 0);
            _previousMousePosition = new Vector2i(0, 0);

            _initialized = true;
            Console.WriteLine("InputManager initialized.");
        }

        /// <summary>
        /// Updates the input state. Should be called once per frame before processing input.
        /// </summary>
        /// <param name="window">The render window to get mouse position from.</param>
        /// <exception cref="ArgumentNullException">Thrown when window is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when not initialized.</exception>
        public static void Update(RenderWindow window)
        {
            if (!_initialized)
                throw new InvalidOperationException("InputManager must be initialized before use. Call Initialize() first.");

            if (window == null)
                throw new ArgumentNullException(nameof(window));

            // Update keyboard state
            _keysPressedLastFrame.Clear();
            foreach (var key in _keysPressed)
                _keysPressedLastFrame.Add(key);

            _keysPressed.Clear();
            foreach (Keyboard.Key key in Enum.GetValues(typeof(Keyboard.Key)))
            {
                if (Keyboard.IsKeyPressed(key))
                    _keysPressed.Add(key);
            }

            // Update mouse button state
            _buttonsPressedLastFrame.Clear();
            foreach (var button in _buttonsPressed)
                _buttonsPressedLastFrame.Add(button);

            _buttonsPressed.Clear();
            foreach (Mouse.Button button in Enum.GetValues(typeof(Mouse.Button)))
            {
                if (Mouse.IsButtonPressed(button))
                    _buttonsPressed.Add(button);
            }

            // Update mouse position
            _previousMousePosition = _currentMousePosition;
            _currentMousePosition = Mouse.GetPosition(window);
        }

        #endregion

        #region Keyboard Input

        /// <summary>
        /// Checks if a key is currently pressed (held down).
        /// </summary>
        /// <param name="key">The keyboard key to check.</param>
        /// <returns>True if the key is currently pressed.</returns>
        public static bool IsKeyPressed(Keyboard.Key key)
        {
            return Keyboard.IsKeyPressed(key);
        }

        /// <summary>
        /// Checks if a key was just pressed this frame (not held from previous frame).
        /// </summary>
        /// <param name="key">The keyboard key to check.</param>
        /// <returns>True if the key was just pressed this frame.</returns>
        /// <exception cref="InvalidOperationException">Thrown when not initialized or Update not called.</exception>
        public static bool IsKeyJustPressed(Keyboard.Key key)
        {
            EnsureInitialized();
            return _keysPressed.Contains(key) && !_keysPressedLastFrame.Contains(key);
        }

        /// <summary>
        /// Checks if a key was just released this frame.
        /// </summary>
        /// <param name="key">The keyboard key to check.</param>
        /// <returns>True if the key was just released this frame.</returns>
        /// <exception cref="InvalidOperationException">Thrown when not initialized or Update not called.</exception>
        public static bool IsKeyJustReleased(Keyboard.Key key)
        {
            EnsureInitialized();
            return !_keysPressed.Contains(key) && _keysPressedLastFrame.Contains(key);
        }

        /// <summary>
        /// Checks if a key is being held down (pressed for multiple frames).
        /// </summary>
        /// <param name="key">The keyboard key to check.</param>
        /// <returns>True if the key is being held down.</returns>
        /// <exception cref="InvalidOperationException">Thrown when not initialized or Update not called.</exception>
        public static bool IsKeyHeld(Keyboard.Key key)
        {
            EnsureInitialized();
            return _keysPressed.Contains(key) && _keysPressedLastFrame.Contains(key);
        }

        #endregion

        #region Mouse Input

        /// <summary>
        /// Checks if a mouse button is currently pressed (held down).
        /// </summary>
        /// <param name="button">The mouse button to check.</param>
        /// <returns>True if the button is currently pressed.</returns>
        public static bool IsMousePressed(Mouse.Button button)
        {
            return Mouse.IsButtonPressed(button);
        }

        /// <summary>
        /// Checks if a mouse button was just pressed this frame (not held from previous frame).
        /// </summary>
        /// <param name="button">The mouse button to check.</param>
        /// <returns>True if the button was just pressed this frame.</returns>
        /// <exception cref="InvalidOperationException">Thrown when not initialized or Update not called.</exception>
        public static bool IsMouseJustPressed(Mouse.Button button)
        {
            EnsureInitialized();
            return _buttonsPressed.Contains(button) && !_buttonsPressedLastFrame.Contains(button);
        }

        /// <summary>
        /// Checks if a mouse button was just released this frame.
        /// </summary>
        /// <param name="button">The mouse button to check.</param>
        /// <returns>True if the button was just released this frame.</returns>
        /// <exception cref="InvalidOperationException">Thrown when not initialized or Update not called.</exception>
        public static bool IsMouseJustReleased(Mouse.Button button)
        {
            EnsureInitialized();
            return !_buttonsPressed.Contains(button) && _buttonsPressedLastFrame.Contains(button);
        }

        /// <summary>
        /// Checks if a mouse button is being held down (pressed for multiple frames).
        /// </summary>
        /// <param name="button">The mouse button to check.</param>
        /// <returns>True if the button is being held down.</returns>
        /// <exception cref="InvalidOperationException">Thrown when not initialized or Update not called.</exception>
        public static bool IsMouseHeld(Mouse.Button button)
        {
            EnsureInitialized();
            return _buttonsPressed.Contains(button) && _buttonsPressedLastFrame.Contains(button);
        }

        /// <summary>
        /// Gets the current mouse position relative to the window.
        /// </summary>
        /// <param name="window">The render window.</param>
        /// <returns>The current mouse position.</returns>
        /// <exception cref="ArgumentNullException">Thrown when window is null.</exception>
        public static Vector2i GetMousePosition(RenderWindow window)
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));

            return Mouse.GetPosition(window);
        }

        /// <summary>
        /// Gets the mouse position from the last Update() call.
        /// </summary>
        /// <returns>The cached mouse position.</returns>
        /// <exception cref="InvalidOperationException">Thrown when not initialized or Update not called.</exception>
        public static Vector2i GetCachedMousePosition()
        {
            EnsureInitialized();
            return _currentMousePosition;
        }

        /// <summary>
        /// Gets the mouse movement delta since last frame.
        /// </summary>
        /// <returns>The mouse movement as a vector.</returns>
        /// <exception cref="InvalidOperationException">Thrown when not initialized or Update not called.</exception>
        public static Vector2i GetMouseDelta()
        {
            EnsureInitialized();
            return new Vector2i(
                _currentMousePosition.X - _previousMousePosition.X,
                _currentMousePosition.Y - _previousMousePosition.Y
            );
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Checks if any key is currently pressed.
        /// </summary>
        /// <returns>True if any key is pressed.</returns>
        /// <exception cref="InvalidOperationException">Thrown when not initialized or Update not called.</exception>
        public static bool IsAnyKeyPressed()
        {
            EnsureInitialized();
            return _keysPressed.Count > 0;
        }

        /// <summary>
        /// Checks if any mouse button is currently pressed.
        /// </summary>
        /// <returns>True if any mouse button is pressed.</returns>
        /// <exception cref="InvalidOperationException">Thrown when not initialized or Update not called.</exception>
        public static bool IsAnyMouseButtonPressed()
        {
            EnsureInitialized();
            return _buttonsPressed.Count > 0;
        }

        /// <summary>
        /// Resets all input state. Useful for clearing input between scenes.
        /// </summary>
        public static void Reset()
        {
            _keysPressed.Clear();
            _keysPressedLastFrame.Clear();
            _buttonsPressed.Clear();
            _buttonsPressedLastFrame.Clear();
            _currentMousePosition = new Vector2i(0, 0);
            _previousMousePosition = new Vector2i(0, 0);

            Console.WriteLine("InputManager reset.");
        }

        private static void EnsureInitialized()
        {
            if (!_initialized)
                throw new InvalidOperationException("InputManager must be initialized before use. Call Initialize() first.");
        }

        #endregion
    }
}