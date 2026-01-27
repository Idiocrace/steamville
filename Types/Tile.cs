using System;
using System.Collections.Generic;
using SFML.System;
using System.Text.Json;

namespace TileGame.Types
{
    public class Tile
    {
        public string ID { get; set; } = Guid.NewGuid().ToString();
        public Vector3 Position { get; set; } = new Vector3(0, 0, 0);

        public string DisplayName { get; set; } = "Tile";
        public Vector2i GridPosition { get; set; } = new Vector2i(0, 0); // grid coords
        public Vector2 BoundingBox { get; set; } = new Vector2(1, 1);
        public Dictionary<string, Container> TileContainers { get; set; } = new();
        public Action<Tile>? Processor { get; set; } // procedural processor
        public Dictionary<object, object>? ProcessorData { get; set; } = new();
        public Dictionary<string, object> Data { get; set; } = new();

        // --- Serialize / Deserialize ---
        public string Serialize()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }

        public static Tile Deserialize(string json)
        {
            return JsonSerializer.Deserialize<Tile>(json)!;
        }
    }
}
