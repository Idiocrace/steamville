using System.Diagnostics;

using TileGame.Types;
using TileGame.Processors;
using TileGame.Graphics;
using TileGame.Errors;

namespace TileGame;

// THIS IS NOT FINAL: MOST CODE WILL BE MOVED TO THE BASE MOD LATER
public class TileGame
{
    // Global runtime state
    public TileGrid MainGrid = new TileGrid();
    public List<Tile> Tiles = [];
    public List<Resource> Resources = [];
    private readonly object GameLock = new object();
    private bool Running = true;
    private readonly int TickRate = 20; // ticks per second
    
    // Graphics core
    private GraphicsCore? Graphics;

    public List<Tile> LoadTiles(string bcpDir)
    {
        // Get all file paths in the bcpDir/tiles/ directory
        List<string> tileFilePaths = [.. Directory.GetFiles(Path.Combine(bcpDir, "tiles"), "*.json")];
        // Initialize a list to hold loaded Tile objects
        List<Tile> loadedTiles = [];
        // Load each tile file and deserialize into Tile objects
        foreach (string filePath in tileFilePaths)
        {
            string jsonData = File.ReadAllText(filePath);
            try
            {
                Tile tile = Tile.Deserialize(this, jsonData);
                // Add tile to a list or dictionary as needed
                loadedTiles.Add(tile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to deserialize tile '{filePath}': {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }
        return loadedTiles; // Return the list of loaded tiles
    }

    public List<Resource> LoadResources(string bcpDir)
    {
        // Get all file paths in the bcpDir/resources/ directory
        List<string> resourceFilePaths = [.. Directory.GetFiles(Path.Combine(bcpDir, "resources"), "*.json")];
        // Initialize a list to hold loaded Resource objects
        List<Resource> loadedResources = [];
        // Load each resource file and deserialize into Resource objects
        foreach (string filePath in resourceFilePaths)
        {
            string jsonData = File.ReadAllText(filePath);
            try
            {
                Resource resource = Resource.Deserialize(jsonData);
                // Add resource to a list or dictionary as needed
                loadedResources.Add(resource);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to deserialize resource '{filePath}': {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }
        return loadedResources; // Return the list of loaded resources
    }
    
    public void Initialize()
    {
        // Create all necessary game-instance values
        string BaseContentPackDir = "bcp/";
        Console.WriteLine("BCP Directory set to " + BaseContentPackDir);
        if (!Directory.Exists(BaseContentPackDir))
        {
            throw new DirectoryNotFoundException("BCP directory not found.");
        }
        Console.WriteLine("Initializing TileMap and PipeMap...");
        // Load resources and tiles into global runtime lists
        Resources = LoadResources(BaseContentPackDir);
        foreach (Resource resource in Resources)
        {
            Console.WriteLine("Loaded Resource: " + resource.ID);
        }

        Tiles = LoadTiles(BaseContentPackDir);
        lock (GameLock)
        {
            foreach (Tile tile in Tiles)
            {
                Console.WriteLine("Loaded Tile: " + tile.ID);
            }
        }
        

        Console.WriteLine("Initializing bits and bobs...");
        int Money = 0; // Player's money

        // Temp code to make compiler shut up
        if (Money == -1) { }
        if (Tiles == null) { }

        Console.WriteLine("Initialization complete.");
    }

    public void ProcessAllTiles()
    {
        // Create a thread-safe snapshot of the grid so processing doesn't hold the game lock
        Dictionary<Vector2, Tile> snapshot;
        lock (GameLock)
        {
            snapshot = MainGrid.GetAllTiles().ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        foreach (KeyValuePair<Vector2, Tile> kvp in snapshot)
        {
            Vector2 position = kvp.Key;
            Tile tile = kvp.Value;
            try
            {
                // Dispatch to concrete processor implementations
                if (tile.Processor is MachineProcessor machine)
                {
                    // Build adjacent map with string directions expected by processors
                    var adjByPos = MainGrid.GetAdjacentTiles(position);
                    var adj = new Dictionary<string, Tile>();
                    var leftPos = new Vector2(position.X - 1, position.Y);
                    var rightPos = new Vector2(position.X + 1, position.Y);
                    var upPos = new Vector2(position.X, position.Y - 1);
                    var downPos = new Vector2(position.X, position.Y + 1);
                    if (adjByPos.TryGetValue(leftPos, out var left)) adj["left"] = left;
                    if (adjByPos.TryGetValue(rightPos, out var right)) adj["right"] = right;
                    if (adjByPos.TryGetValue(upPos, out var up)) adj["up"] = up;
                    if (adjByPos.TryGetValue(downPos, out var down)) adj["down"] = down;

                    // To make things easier, we make this a daemonized thread
                    
                    machine.Process(this, tile, adj);
                }
                else if (tile.Processor is BaseProcessor baseProcessor)
                {
                    // Skip processing baseprocessor
                }
                else
                {
                    // Throw a content pack error
                    throw new ContentPackError($"Unknown processor type for tile at {position.X},{position.Y}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing tile at {position.X},{position.Y}: {ex.Message}");
            }
        }
    }

    public void MainCycle()
    {
        // Fixed timestep loop
        var tickDuration = TimeSpan.FromSeconds(1.0 / TickRate);
        var sw = Stopwatch.StartNew();
        var previous = sw.Elapsed;
        var accumulator = TimeSpan.Zero;
        int tickCounter = 0;

        const int maxCatchUpTicks = 5; // avoid spiral of death

        while (Running && Graphics != null && Graphics.IsRunning)
        {
            var current = sw.Elapsed;
            var delta = current - previous;
            previous = current;
            accumulator += delta;

            int ticks = 0;
            while (accumulator >= tickDuration && ticks < maxCatchUpTicks)
            {
                // Only process game logic if we're actually playing
                if (Graphics.CurrentState == GameState.Playing)
                {
                    ProcessAllTiles();
                }

                accumulator -= tickDuration;
                ticks++;
                tickCounter++;
            }

            // Example loop
            if (tickCounter >= TickRate && Graphics.CurrentState == GameState.Playing)
            {
                tickCounter = 0;
            }

            // Sleep briefly to avoid 100% CPU usage
            Thread.Sleep(1);
        }

        Console.WriteLine("MainCycle exiting.");
    }

    public void RenderLoop()
    {
        if (Graphics == null) return;

        while (Graphics.IsRunning)
        {
            // Handle window events
            Graphics.PollEvents();

            // Begin rendering
            Graphics.BeginFrame();

            // Handle different game states
            if (Graphics.CurrentState == GameState.MainMenu)
            {
                Graphics.UpdateMenu();
                Graphics.DrawMenu();
            }
            else if (Graphics.CurrentState == GameState.Playing)
            {
                // TODO: Draw game world here
                // For now, just show a placeholder
                // Graphics.DrawTexture(...);
                // You'll add your tile rendering code here
            }

            // End rendering
            Graphics.EndFrame();
        }

        Running = false;
        Console.WriteLine("RenderLoop exiting.");
    }

    public void Start()
    {
        // Entry point
        Console.WriteLine("TileGame");
        Console.WriteLine("Initializing...");
        Initialize();

        // Initialize graphics
        Console.WriteLine("Initializing graphics...");
        Config graphicsConfig = new Config();
        Graphics = new GraphicsCore(graphicsConfig);

        Console.WriteLine("Creating game threads...");
        
        // Create main cycle thread for game logic
        Thread mainCycleThread = new Thread(new ThreadStart(MainCycle));
        mainCycleThread.Start();
        Console.WriteLine("Main cycle thread started.");

        // Run render loop on main thread (required for SFML on macOS)
        Console.WriteLine("Starting render loop on main thread...");
        RenderLoop();

        // Wait for main cycle to finish
        Console.WriteLine("Waiting for main cycle to finish...");
        mainCycleThread.Join();
        
        // Cleanup
        Graphics?.Dispose();
        Console.WriteLine("Game stopped. Exiting.");
    }
}