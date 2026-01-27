using System;
using System.Collections.Generic;
using System.Linq;

namespace TileGame.Types
{
    public class Resource
    {
        public string ID { get; set; } = "resource";
        public string Name { get; set; } = "Resource";
        public string Unit { get; set; } = "u";
        public string UnitFull { get; set; } = "unit(s)";
        public string Sprite { get; set; } = "default.png";
        public string Color { get; set; } = "#ff00ff";
        public double Value { get; set; } = 0.0;
        public ResourceType Type { get; set; } = ResourceType.None;

        // CRITICAL for using Resource as Dictionary key
        public override bool Equals(object? obj)
            => obj is Resource other && ID == other.ID;

        public override int GetHashCode()
            => ID.GetHashCode();

        public string Serialize()
        {
            var types = new List<string>();
            if (Type is ResourceType.Importable or ResourceType.Both) types.Add("importable");
            if (Type is ResourceType.Exportable or ResourceType.Both) types.Add("exportable");

            var data = new Dictionary<string, object>
            {
                ["id"] = ID,
                ["name"] = Name,
                ["unit"] = Unit,
                ["unitFull"] = UnitFull,
                ["sprite"] = Sprite,
                ["color"] = Color,
                ["value"] = Value,
                ["types"] = types
            };

            return System.Text.Json.JsonSerializer.Serialize(data);
        }

        public static Resource Deserialize(string json)
        {
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("id", out var idEl))
                throw new ArgumentException("Resource JSON must contain 'id'.");

            var resource = new Resource
            {
                ID = idEl.GetString()!,
                Name = root.TryGetProperty("name", out var n) ? n.GetString()! : "Resource",
                Unit = root.TryGetProperty("unit", out var u) ? u.GetString()! : "u",
                UnitFull = root.TryGetProperty("unitFull", out var uf) ? uf.GetString()! : "unit(s)",
                Sprite = root.TryGetProperty("sprite", out var s) ? s.GetString()! : "default.png",
                Color = root.TryGetProperty("color", out var c) ? c.GetString()! : "#ff00ff"
            };

            if (root.TryGetProperty("value", out var valEl))
            {
                if (valEl.TryGetDouble(out var v)) resource.Value = v;
            }

            // ✅ FIXED TYPE PARSING
            if (root.TryGetProperty("types", out var typeEl) && typeEl.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                bool imp = false, exp = false;

                foreach (var t in typeEl.EnumerateArray())
                {
                    var str = t.GetString()?.ToLower();
                    if (str == "importable") imp = true;
                    if (str == "exportable") exp = true;
                }

                resource.Type = (imp, exp) switch
                {
                    (true, true) => ResourceType.Both,
                    (true, false) => ResourceType.Importable,
                    (false, true) => ResourceType.Exportable,
                    _ => ResourceType.None
                };
            }

            return resource;
        }
    }

    public enum ResourceType
    {
        None,
        Importable,
        Exportable,
        Both
    }
}
