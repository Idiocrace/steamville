using System.Diagnostics;

using TileGame.Types;
using TileGame.Processors;
using TileGame.Processors.Pipes;
using TileGame.Errors;

namespace TileGame;

// THIS IS NOT FINAL: MOST CODE WILL BE MOVED TO THE BASE MOD LATER
public class TileGame
{
    // Global runtime state
    public static TileGrid MainGrid = new TileGrid();
    public static List<Tile> Tiles = [];
    public static List<Pipe> Pipes = [];
    public static List<Resource> Resources = [];
    private static readonly object GameLock = new object();
    private static bool Running = true;
    private static readonly int TickRate = 20; // ticks per second
    private static List<string> ConsoleOutputBuffer = [];

    public static void FlushConsoleOutput()
    {
        lock (GameLock)
        {
            foreach (string message in ConsoleOutputBuffer)
            {
                Console.WriteLine(message);
            }
            ConsoleOutputBuffer.Clear();
        }
    }

    public static void WriteConsole(string message)
    {
        lock (GameLock)
        {
            if (!ConsoleOutputBuffer.Contains(message))
            {
                ConsoleOutputBuffer.Add(message);
            }
        }
    }

    public static List<Tile> LoadTiles(string bcpDir)
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
                Tile tile = Tile.Deserialize(jsonData);
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

    public static List<Resource> LoadResources(string bcpDir)
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
    
    public static void Initialize()
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
        if (Pipes == null) { }

        Console.WriteLine("Initialization complete.");
    }

    public static void ProcessAllTiles()
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

                    machine.Process(tile, adj);
                }
                else if (tile.Processor is BaseProcessor baseProcessor)
                {
                    baseProcessor.Process(tile);
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

    public static void ProcessAllPipes()
    {
        List<Pipe> snapshot;
        lock (GameLock)
        {
            snapshot = Pipes.ToList();
        }

        foreach (Pipe pipe in snapshot)
        {
            try
            {
                pipe.ProcessPipe();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing pipe: {ex.Message}");
            }
        }
    }

    public static void MainCycle()
    {
        // Fixed timestep loop
        var tickDuration = TimeSpan.FromSeconds(1.0 / TickRate);
        var sw = Stopwatch.StartNew();
        var previous = sw.Elapsed;
        var accumulator = TimeSpan.Zero;
        int tickCounter = 0;

        const int maxCatchUpTicks = 5; // avoid spiral of death

        while (Running)
        {
            var current = sw.Elapsed;
            var delta = current - previous;
            previous = current;
            accumulator += delta;

            int ticks = 0;
            while (accumulator >= tickDuration && ticks < maxCatchUpTicks)
            {
                // Perform a single simulation tick
                ProcessAllTiles();
                ProcessAllPipes();

                accumulator -= tickDuration;
                ticks++;
                tickCounter++;
            }

            // Flush console every second
            if (tickCounter >= TickRate)
            {
                FlushConsoleOutput();
                tickCounter = 0;
            }

            // Sleep briefly to avoid 100% CPU usage
            Thread.Sleep(1);
        }

        Console.WriteLine("MainCycle exiting.");
    }

    public static void Main()
    {
        // Entry point for testing purposes
        Console.WriteLine("TileGame");
        Console.WriteLine("Initializing...");
        Initialize();
        Console.WriteLine("Creating main cycle thread...");
        Thread mainCycleThread = new Thread(new ThreadStart(MainCycle));
        Console.WriteLine("Starting main cycle thread...");
        mainCycleThread.Start();
        Console.WriteLine("Main cycle thread started. Press Enter to stop.");
        Console.ReadLine();
        Running = false;
        mainCycleThread.Join();
        Console.WriteLine("Main cycle stopped. Exiting.");
    }
}