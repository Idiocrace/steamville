using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using TileGame.Types;
using TileGame.Processors;
using TileGame.Graphics;
using TileGame.Errors;
using SFML.Graphics;
using SFML.Window;

namespace TileGame
{
    /// <summary>
    /// Main game controller managing initialization, game loop, and rendering.
    /// </summary>
    public sealed class TileGame : IDisposable
    {
        #region Constants
        
        private const int DefaultTickRate = 20; // ticks per second
        private const int MaxCatchUpTicks = 5; // prevent spiral of death
        private const string BaseContentPackDirectory = "bcp/";
        
        #endregion

        #region Game State
        
        public TileGrid MainGrid { get; private set; }
        public IReadOnlyList<Tile> Tiles => _tiles.AsReadOnly();
        public IReadOnlyList<Resource> Resources => _resources.AsReadOnly();
        public string Phase { get; set; } = "No Phase";
        public int Money { get; set; }
        public IReadOnlyList<string> ActiveCheats => _activeCheats.AsReadOnly();

        private readonly List<Tile> _tiles = new();
        private readonly List<Resource> _resources = new();
        private readonly List<string> _activeCheats = new();
        
        #endregion

        #region Threading & Synchronization
        
        private readonly object _gameLock = new();
        private readonly object _runningLock = new();
        private volatile bool _running;
        private Thread? _gameThread;
        private readonly ManualResetEventSlim _shutdownEvent = new(false);
        
        #endregion

        #region Components
        
        private GraphicsCore? _graphics;
        private GameGUI? _gui;
        private GameRenderer? _renderer;
        private readonly RichPresenceManager _richPresence = new();
        
        #endregion

        #region Configuration
        
        private readonly int _tickRate;
        
        #endregion

        #region Disposal
        
        private bool _disposed;
        
        #endregion

        /// <summary>
        /// Initializes a new instance of the TileGame class.
        /// </summary>
        /// <param name="tickRate">Target tick rate for game logic updates.</param>
        public TileGame(int tickRate = DefaultTickRate)
        {
            if (tickRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(tickRate), "Tick rate must be positive.");

            _tickRate = tickRate;
            MainGrid = new TileGrid();
        }

        #region Initialization

        /// <summary>
        /// Initializes all game systems including content loading and graphics.
        /// </summary>
        /// <exception cref="DirectoryNotFoundException">Thrown when the base content pack directory doesn't exist.</exception>
        public void Initialize()
        {
            ThrowIfDisposed();

            Console.WriteLine($"BCP Directory set to {BaseContentPackDirectory}");
            
            if (!Directory.Exists(BaseContentPackDirectory))
                throw new DirectoryNotFoundException($"BCP directory not found: {BaseContentPackDirectory}");

            Console.WriteLine("Loading game content...");
            LoadGameContent();

            Console.WriteLine("Initializing graphics...");
            InitializeGraphics();

            Console.WriteLine("Initializing Rich Presence...");
            InitializeRichPresence();

            Console.WriteLine("Initialization complete.");
        }

        private void LoadGameContent()
        {
            _resources.Clear();
            _resources.AddRange(LoadResources(BaseContentPackDirectory));
            
            foreach (var resource in _resources)
                Console.WriteLine($"Loaded Resource: {resource.ID}");

            lock (_gameLock)
            {
                _tiles.Clear();
                _tiles.AddRange(LoadTiles(BaseContentPackDirectory));
                
                foreach (var tile in _tiles)
                    Console.WriteLine($"Loaded Tile: {tile.ID}");
            }
        }

        private void InitializeGraphics()
        {
            var graphicsConfig = new Config();
            _graphics = new GraphicsCore();
            _gui = new GameGUI(_graphics);
            _renderer = new GameRenderer(_graphics);
        }

        private void InitializeRichPresence()
        {
            _richPresence.Enable();
            _richPresence.UpdatePresence(new Dictionary<string, object>
            {
                { "details", "In Main Menu" },
                { "state", "Idle" }
            });
        }

        #endregion

        #region Content Loading

        /// <summary>
        /// Loads all tile definitions from the content pack directory.
        /// </summary>
        /// <param name="bcpDir">Base content pack directory path.</param>
        /// <returns>List of loaded tiles.</returns>
        private List<Tile> LoadTiles(string bcpDir)
        {
            var tilesDirectory = Path.Combine(bcpDir, "tiles");
            var tileFilePaths = Directory.GetFiles(tilesDirectory, "*.json");
            var loadedTiles = new List<Tile>(tileFilePaths.Length);

            foreach (var filePath in tileFilePaths)
            {
                try
                {
                    var jsonData = File.ReadAllText(filePath);
                    var tile = Tile.Deserialize(this, jsonData);
                    loadedTiles.Add(tile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to deserialize tile '{filePath}': {ex.GetType().Name}: {ex.Message}");
                    throw new ContentPackError($"Failed to load tile from '{filePath}'. {ex.Message}");
                }
            }

            return loadedTiles;
        }

        /// <summary>
        /// Loads all resource definitions from the content pack directory.
        /// </summary>
        /// <param name="bcpDir">Base content pack directory path.</param>
        /// <returns>List of loaded resources.</returns>
        private List<Resource> LoadResources(string bcpDir)
        {
            var resourcesDirectory = Path.Combine(bcpDir, "resources");
            var resourceFilePaths = Directory.GetFiles(resourcesDirectory, "*.json");
            var loadedResources = new List<Resource>(resourceFilePaths.Length);

            foreach (var filePath in resourceFilePaths)
            {
                try
                {
                    var jsonData = File.ReadAllText(filePath);
                    var resource = Resource.Deserialize(jsonData);
                    loadedResources.Add(resource);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to deserialize resource '{filePath}': {ex.GetType().Name}: {ex.Message}");
                    throw new ContentPackError($"Failed to load resource from '{filePath}'. {ex.Message}");
                }
            }

            return loadedResources;
        }

        #endregion

        #region Game Loop

        /// <summary>
        /// Starts the game, initializing systems and running the main loops.
        /// </summary>
        public void Start()
        {
            ThrowIfDisposed();

            Console.WriteLine("TileGame - Starting...");
            Initialize();

            lock (_runningLock)
            {
                _running = true;
            }

            // Start game logic thread
            _gameThread = new Thread(MainCycle)
            {
                Name = "GameLogicThread",
                IsBackground = false
            };
            _gameThread.Start();

            // Run render loop on main thread (required for SFML)
            RenderLoop();

            // Wait for game thread to finish
            _gameThread.Join();
            
            Console.WriteLine("Game shutdown complete.");
        }

        /// <summary>
        /// Main game logic loop with fixed timestep.
        /// </summary>
        private void MainCycle()
        {
            var tickDuration = TimeSpan.FromSeconds(1.0 / _tickRate);
            var stopwatch = Stopwatch.StartNew();
            var previousTime = stopwatch.Elapsed;
            var accumulator = TimeSpan.Zero;
            var tickCounter = 0;

            while (IsRunning())
            {
                var currentTime = stopwatch.Elapsed;
                var deltaTime = currentTime - previousTime;
                previousTime = currentTime;
                accumulator += deltaTime;

                // Process accumulated time in fixed steps
                var ticksProcessed = 0;
                while (accumulator >= tickDuration && ticksProcessed < MaxCatchUpTicks)
                {
                    if (GetCurrentGameState() == GameState.Playing)
                    {
                        ProcessAllTiles();
                    }

                    accumulator -= tickDuration;
                    ticksProcessed++;
                    tickCounter++;
                }

                // Update rich presence every second
                if (tickCounter >= _tickRate)
                {
                    UpdateRichPresence();
                    tickCounter = 0;
                }

                // Prevent CPU thrashing
                Thread.Sleep(1);
            }

            Console.WriteLine("MainCycle exiting.");
            _shutdownEvent.Set();
        }

        /// <summary>
        /// Rendering loop running on the main thread.
        /// </summary>
        private void RenderLoop()
        {
            if (_graphics == null || _gui == null || _renderer == null)
            {
                Console.WriteLine("Graphics systems not initialized.");
                return;
            }

            while (_graphics.IsRunning && IsRunning())
            {
                // Capture state once at frame start to prevent race conditions
                var currentState = _graphics.CurrentState;

                // 1. Process window events
                _graphics.PollEvents();

                // 2. Update active state
                if (currentState == GameState.MainMenu)
                    _gui.UpdateMainMenu();

                // 3. Begin rendering
                _graphics.BeginFrame();

                // 4. Render game world
                if (currentState == GameState.Playing)
                {
                    RenderGameWorld();
                }

                // 5. Render UI
                RenderUI(currentState);

                // 6. Present frame
                _graphics.EndFrame();
            }

            // Signal shutdown
            lock (_runningLock)
            {
                _running = false;
            }
        }

        private void RenderGameWorld()
        {
            if (_graphics == null || _renderer == null) return;

            _graphics.GameWindow.SetView(_graphics.GameView);

            // Render tiles
            lock (_gameLock)
            {
                foreach (var kvp in MainGrid.GetAllTiles())
                {
                    // TODO: Implement tile rendering
                }
            }

            _renderer.Draw();
        }

        private void RenderUI(GameState currentState)
        {
            if (_graphics == null || _gui == null) return;

            _graphics.GameWindow.SetView(_graphics.GameWindow.DefaultView);

            switch (currentState)
            {
                case GameState.MainMenu:
                    _gui.DrawMainMenu();
                    break;
                case GameState.Paused:
                    // TODO: Implement pause menu
                    // _gui.DrawPauseMenu();
                    break;
            }
        }

        #endregion

        #region Tile Processing

        /// <summary>
        /// Processes all tiles in the grid for one game tick.
        /// </summary>
        private void ProcessAllTiles()
        {
            // Create thread-safe snapshot
            Dictionary<Vector2, Tile> snapshot;
            lock (_gameLock)
            {
                snapshot = MainGrid.GetAllTiles().ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            foreach (var kvp in snapshot)
            {
                ProcessSingleTile(kvp.Key, kvp.Value);
            }
        }

        private void ProcessSingleTile(Vector2 position, Tile tile)
        {
            try
            {
                switch (tile.Processor)
                {
                    case MachineProcessor machine:
                        ProcessMachineTile(position, tile, machine);
                        break;
                    case BaseProcessor:
                        // Base processor requires no processing
                        break;
                    default:
                        throw new ContentPackError(
                            $"Unknown processor type '{tile.Processor?.GetType().Name}' for tile at ({position.X},{position.Y})");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing tile at ({position.X},{position.Y}): {ex.Message}");
            }
        }

        private void ProcessMachineTile(Vector2 position, Tile tile, MachineProcessor machine)
        {
            var adjacentTiles = BuildAdjacentTilesMap(position);
            machine.Process(this, tile, adjacentTiles);
        }

        private Dictionary<string, Tile> BuildAdjacentTilesMap(Vector2 position)
        {
            var adjacentByPosition = MainGrid.GetAdjacentTiles(position);
            var adjacentByDirection = new Dictionary<string, Tile>();

            var positions = new Dictionary<string, Vector2>
            {
                ["left"] = new Vector2(position.X - 1, position.Y),
                ["right"] = new Vector2(position.X + 1, position.Y),
                ["up"] = new Vector2(position.X, position.Y - 1),
                ["down"] = new Vector2(position.X, position.Y + 1)
            };

            foreach (var kvp in positions)
            {
                if (adjacentByPosition.TryGetValue(kvp.Value, out var tile))
                    adjacentByDirection[kvp.Key] = tile;
            }

            return adjacentByDirection;
        }

        #endregion

        #region Rich Presence

        private void UpdateRichPresence()
        {
            var currentState = GetCurrentGameState();

            var presenceData = currentState switch
            {
                GameState.MainMenu => new Dictionary<string, object>
                {
                    { "details", "Idling" },
                    { "state", "Main Menu" }
                },
                GameState.Playing => new Dictionary<string, object>
                {
                    { "details", $"${Money} | {Phase}" },
                    { "state", "In Game" }
                },
                GameState.Paused => new Dictionary<string, object>
                {
                    { "details", $"${Money} | {Phase}" },
                    { "state", "Paused" }
                },
                _ => new Dictionary<string, object>
                {
                    { "details", $"${Money} | {Phase}" },
                    { "state", "Idling" }
                }
            };

            _richPresence.UpdatePresence(presenceData);
        }

        #endregion

        #region Helper Methods

        private bool IsRunning()
        {
            lock (_runningLock)
            {
                return _running && (_graphics?.IsRunning ?? false);
            }
        }

        private GameState GetCurrentGameState()
        {
            return _graphics?.CurrentState ?? GameState.MainMenu;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(TileGame));
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Releases all resources used by the TileGame.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Signal shutdown
                lock (_runningLock)
                {
                    _running = false;
                }

                // Wait for game thread to finish
                if (_gameThread != null && _gameThread.IsAlive)
                {
                    _shutdownEvent.Wait(TimeSpan.FromSeconds(5));
                }

                // Dispose managed resources
                _renderer?.Dispose();
                _graphics?.Dispose();
                // RichPresenceManager doesn't implement IDisposable, so we just leave it
                _shutdownEvent?.Dispose();
            }

            _disposed = true;
        }

        /// <summary>
        /// Finalizer to ensure cleanup if Dispose is not called.
        /// </summary>
        ~TileGame()
        {
            Dispose(false);
        }

        #endregion
    }
}