using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

using TileGame.Processors;
using TileGame.Types;
using TileGame.Graphics;

public static class GameRender
{
    public static ResourceGrid ResourceGrid = new();
    public static TileGrid? Grid;

    private const int TILE_SIZE = 32;
    private static bool wasMouseDownLastFrame = false;

    public static Tile ExtractorTile = new Tile
    {
        ID = Guid.NewGuid().ToString(),
        Processor = (Action<Tile>)ExtractorProcessor.Process, // Use method group
        DisplayName = "Extractor",
        BoundingBox = new Vector2(1, 1),
        Position = new Vector3(0, 0, 0),
        TileContainers = new Dictionary<string, Container>
        {
            { "output", new Container(10) }
        },
        Data = new Dictionary<string, object>
        {
            { "container", new Container(10) }
        },
        ProcessorData = new Dictionary<object, object>
        {
            { "extraction", new Dictionary<object, object>
                {
                    { "case1", new Dictionary<object, object>
                        {
                            { "container", "output" },
                            { "amount", 1 }
                        }
                    }
                }
            },
            { "requirements", new Dictionary<object, object>() }
        }
    };

    private static readonly object _logLock = new();
    public static List<string> LogLines = new();

    public static void AddLog(string message)
    {
        lock (_logLock)
        {
            LogLines.Add(message);
            if (LogLines.Count > 10)
                LogLines.RemoveAt(0);
        }
    }

    public static void UpdateAndDraw(GraphicsCore gfx)
    {
        if (Grid == null)
            Grid = new TileGrid();

        // Toggle pause on Esc
        if (Keyboard.IsKeyPressed(Keyboard.Key.Escape))
        {
            if (gfx.CurrentState == GameState.Playing)
                gfx.SetState(GameState.Paused);
            else if (gfx.CurrentState == GameState.Paused)
                gfx.SetState(GameState.Playing);
            // Add a small delay to prevent rapid toggling
            System.Threading.Thread.Sleep(200);
        }

        if (gfx.CurrentState == GameState.Paused)
        {
            DrawPauseMenu(gfx);
            return;
        }

        HandleInput(gfx);
        DrawTiles(gfx);
    }

    private static void HandleInput(GraphicsCore gfx)
    {
        bool isLeftMouseDown = Mouse.IsButtonPressed(Mouse.Button.Left);
        bool isRightMouseDown = Mouse.IsButtonPressed(Mouse.Button.Right);

        var mousePos = Mouse.GetPosition(gfx.GameWindow);
        var tilePos = new Vector2(
            (int)(mousePos.X / TILE_SIZE),
            (int)(mousePos.Y / TILE_SIZE)
        );

        // Place extractor on left click
        if (isLeftMouseDown && !wasMouseDownLastFrame)
        {
            if (Grid!.GetTile(tilePos) == null)
            {
                var placedTile = CreateExtractor(tilePos);
                Grid.PlaceTile(placedTile, tilePos);
            }
        }

        // Remove extractor on right click
        if (isRightMouseDown && !wasMouseDownLastFrame)
        {
            var tile = Grid!.GetTile(tilePos);
            if (tile != null && tile.Processor is Action<Tile> action && action.Method.DeclaringType == typeof(ExtractorProcessor))
            {
                Grid.RemoveTile(tilePos);
                GameRender.AddLog($"Removed extractor at {tilePos.X},{tilePos.Y}");
            }
        }

        wasMouseDownLastFrame = isLeftMouseDown || isRightMouseDown;
    }

    private static Tile CreateExtractor(Vector2 tilePos)
    {
        return new Tile
        {
            ID = Guid.NewGuid().ToString(),
            Processor = (Action<Tile>)ExtractorProcessor.Process,
            DisplayName = ExtractorTile.DisplayName,
            BoundingBox = ExtractorTile.BoundingBox,
            Position = new Vector3((int)tilePos.X, (int)tilePos.Y, 0),
            TileContainers = new Dictionary<string, Container>
            {
                { "output", new Container(10) }
            },
            Data = new Dictionary<string, object>
            {
                { "container", new Container(10) }
            },
            ProcessorData = ExtractorTile.ProcessorData
        };
    }

    private static void DrawTiles(GraphicsCore gfx)
    {
        foreach (var kv in Grid!.GetAllTiles().ToList())
        {
            var pos = kv.Key;
            var tile = kv.Value;

            RectangleShape rect = new RectangleShape(new Vector2f(TILE_SIZE, TILE_SIZE))
            {
                Position = new Vector2f(pos.X * TILE_SIZE, pos.Y * TILE_SIZE),
                FillColor = tile.Processor is Action<Tile> action && action.Method.DeclaringType == typeof(ExtractorProcessor)
                    ? Color.Yellow
                    : Color.White,
                OutlineColor = Color.Black,
                OutlineThickness = 1
            };

            gfx.GameWindow.Draw(rect);

            Text text = new Text(gfx.GameFont, tile.DisplayName, 10)
            {
                Position = new Vector2f(pos.X * TILE_SIZE +  2, pos.Y * TILE_SIZE + 2),
                FillColor = Color.Black
            };

            gfx.GameWindow.Draw(text);
        }

        // Draw resource nodes
        foreach (var kv in ResourceGrid.GetAllNodes().ToList())
        {
            var pos = kv.Key;
            var node = kv.Value;
            var color = ParseHexColor(node.ResourceType.Color);

            CircleShape resourceCircle = new CircleShape(TILE_SIZE / 4f)
            {
                Position = new Vector2f(pos.X * TILE_SIZE + TILE_SIZE / 4f, pos.Y * TILE_SIZE + TILE_SIZE / 4f),
                FillColor = color
            };
            gfx.GameWindow.Draw(resourceCircle);

            // Draw richness as text
            Text richnessText = new Text(gfx.GameFont, node.Richness.ToString(), 10)
            {
                Position = new Vector2f(pos.X * TILE_SIZE + TILE_SIZE / 2f - 6, pos.Y * TILE_SIZE + TILE_SIZE / 2f - 6),
                FillColor = Color.White
            };
            gfx.GameWindow.Draw(richnessText);
        }

        // Draw logs at bottom
        List<string> logsToDraw;
        lock (_logLock)
        {
            logsToDraw = LogLines.ToList();
        }
        for (int i = 0; i < logsToDraw.Count; i++)
        {
            var line = logsToDraw[i];
            Text logText = new Text(gfx.GameFont, line, 12)
            {
                Position = new Vector2f(10, gfx.WindowSize.Y - 140 + i * 14),
                FillColor = Color.White
            };
            gfx.GameWindow.Draw(logText);
        }
    }
    public static void GenerateResources(int width, int height)
    {
        var random = new Random();
        string[] resourceNames = { "Iron", "Copper", "Coal" };
        string[] resourceColors = { "#b0b0b0", "#c87333", "#222222" };

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 20% chance to spawn a resource node
                if (random.NextDouble() < 0.2)
                {
                    int idx = random.Next(resourceNames.Length);
                    var resource = new Resource
                    {
                        ID = resourceNames[idx].ToLower(),
                        Name = resourceNames[idx],
                        Color = resourceColors[idx],
                        Value = 1,
                        Type = ResourceType.Both
                    };
                    var node = new ResourceNode(resource, random.Next(5, 20));
                    ResourceGrid.AddNode(new SFML.System.Vector2i(x, y), node);
                }
            }
        }
    }

    // Helper to parse hex color
    private static Color ParseHexColor(string hex)
    {
        if (hex.StartsWith("#")) hex = hex[1..];
        if (hex.Length == 6)
        {
            return new Color(
                Convert.ToByte(hex.Substring(0, 2), 16),
                Convert.ToByte(hex.Substring(2, 2), 16),
                Convert.ToByte(hex.Substring(4, 2), 16)
            );
        }
        return Color.Magenta;
    }

    // --- Pause Menu State ---
    private static List<string> pauseMenuItems = new() { "Resume", "Save Game", "Load Game", "Quit" };
    private static int pauseMenuSelectedIndex = 0;
    private static bool pauseMenuKeyPressed = false;
    private static bool pauseMenuMouseWasPressed = false;

    private static void DrawPauseMenu(GraphicsCore gfx)
    {
        // Draw semi-transparent overlay
        RectangleShape overlay = new RectangleShape(new Vector2f(gfx.WindowSize.X, gfx.WindowSize.Y))
        {
            FillColor = new Color(20, 30, 50, 220)
        };
        gfx.GameWindow.Draw(overlay);

        // Draw Title
        Text title = new Text(gfx.GameFont, "Paused", 60)
        {
            Position = new Vector2f(gfx.WindowSize.X / 2f - 120, 80),
            FillColor = Color.White
        };
        gfx.GameWindow.Draw(title);

        // Draw menu items
        for (int i = 0; i < pauseMenuItems.Count; i++)
        {
            var color = (i == pauseMenuSelectedIndex) ? Color.Yellow : Color.White;
            string prefix = (i == pauseMenuSelectedIndex) ? "> " : "  ";
            Text item = new Text(gfx.GameFont, prefix + pauseMenuItems[i], 30)
            {
                Position = new Vector2f(gfx.WindowSize.X / 2f - 100, 220 + i * 60),
                FillColor = color
            };
            gfx.GameWindow.Draw(item);
        }

        HandlePauseMenuInput(gfx);
    }

    private static void HandlePauseMenuInput(GraphicsCore gfx)
    {
        // Keyboard navigation
        bool up = Keyboard.IsKeyPressed(Keyboard.Key.Up) || Keyboard.IsKeyPressed(Keyboard.Key.W);
        bool down = Keyboard.IsKeyPressed(Keyboard.Key.Down) || Keyboard.IsKeyPressed(Keyboard.Key.S);
        bool enter = Keyboard.IsKeyPressed(Keyboard.Key.Enter) || Keyboard.IsKeyPressed(Keyboard.Key.Space);

        // Mouse navigation
        var mousePos = Mouse.GetPosition(gfx.GameWindow);
        bool mouseIsDown = Mouse.IsButtonPressed(Mouse.Button.Left);

        // Mouse hover/select
        for (int i = 0; i < pauseMenuItems.Count; i++)
        {
            var rect = new FloatRect(new Vector2f(gfx.WindowSize.X / 2f - 100, 220 + i * 60), new Vector2f(250, 50));
            if (rect.Contains(new Vector2f(mousePos.X, mousePos.Y)))
            {
                pauseMenuSelectedIndex = i;
                if (mouseIsDown && !pauseMenuMouseWasPressed)
                {
                    ActivatePauseMenuItem(gfx);
                }
            }
        }

        // Keyboard navigation with debounce
        if (!pauseMenuKeyPressed)
        {
            if (up)
            {
                pauseMenuSelectedIndex = (pauseMenuSelectedIndex - 1 + pauseMenuItems.Count) % pauseMenuItems.Count;
                pauseMenuKeyPressed = true;
            }
            else if (down)
            {
                pauseMenuSelectedIndex = (pauseMenuSelectedIndex + 1) % pauseMenuItems.Count;
                pauseMenuKeyPressed = true;
            }
            else if (enter)
            {
                ActivatePauseMenuItem(gfx);
                pauseMenuKeyPressed = true;
            }
        }
        if (!up && !down && !enter) pauseMenuKeyPressed = false;
        pauseMenuMouseWasPressed = mouseIsDown;
    }

    private static void ActivatePauseMenuItem(GraphicsCore gfx)
    {
        switch (pauseMenuItems[pauseMenuSelectedIndex])
        {
            case "Resume":
                gfx.SetState(GameState.Playing);
                break;
            case "Save Game":
                SaveGame();
                break;
            case "Load Game":
                LoadGame();
                break;
            case "Quit":
                gfx.GameWindow.Close();
                break;
        }
    }

    public static void SaveGame(string path = "savegame.json")
    {
        var saveData = new
        {
            Tiles = Grid?.GetAllTiles().Select(kv => new
            {
                X = kv.Key.X,
                Y = kv.Key.Y,
                Tile = kv.Value.Serialize()
            }).ToList(),
            Resources = ResourceGrid.GetAllNodes().Select(kv => new
            {
                X = kv.Key.X,
                Y = kv.Key.Y,
                Resource = kv.Value.ResourceType.Serialize(),
                Richness = kv.Value.Richness
            }).ToList()
        };

        var json = JsonSerializer.Serialize(saveData, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
        AddLog("Game saved.");
    }

    public static void LoadGame(string path = "savegame.json")
    {
        if (!File.Exists(path))
        {
            AddLog("No save file found.");
            return;
        }

        var json = File.ReadAllText(path);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Clear current state
        Grid = new TileGrid();
        ResourceGrid = new ResourceGrid();

        // Load tiles
        if (root.TryGetProperty("Tiles", out var tilesEl))
        {
            foreach (var tileEl in tilesEl.EnumerateArray())
            {
                int x = tileEl.GetProperty("X").GetInt32();
                int y = tileEl.GetProperty("Y").GetInt32();
                string tileJson = tileEl.GetProperty("Tile").GetString()!;
                // You need a Tile.Deserialize method for full support
                // For now, just create a basic extractor if it matches
                if (tileJson.Contains("Extractor"))
                {
                    var tile = CreateExtractor(new Vector2(x, y));
                    Grid.PlaceTile(tile, new Vector2(x, y));
                }
                // Add more tile types as needed
            }
        }

        // Load resources
        if (root.TryGetProperty("Resources", out var resEl))
        {
            foreach (var nodeEl in resEl.EnumerateArray())
            {
                int x = nodeEl.GetProperty("X").GetInt32();
                int y = nodeEl.GetProperty("Y").GetInt32();
                string resourceJson = nodeEl.GetProperty("Resource").GetString()!;
                int richness = nodeEl.GetProperty("Richness").GetInt32();
                var resource = Resource.Deserialize(resourceJson);
                var node = new ResourceNode(resource, richness);
                ResourceGrid.AddNode(new SFML.System.Vector2i(x, y), node);
            }
        }

        AddLog("Game loaded.");
    }
}
