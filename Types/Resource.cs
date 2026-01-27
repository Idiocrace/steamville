using System.Diagnostics.Contracts;
using System.Linq;

namespace TileGame.Types; 

public class Resource
{
    public string ID = "resource";
    public string Name = "Resource";
    public string Unit = "u";
    public string UnitFull = "unit(s)";
    public string Sprite = "default.png";
    public string Color = "#ff00ff";
    public double Value = 0.0;
    public ResourceType Type = ResourceType.None;

    // Won't get used in anything but the SteamVille Modding Toolchain for easy generation of resource JSON files
    public string Serialize()
    {
        // Returns a full JSON serialization of the Resource (good for generating a JSON resource file)
        // First, we properly format resourcetypes
        List<string> RTypes = [];
        switch (Type)
        {
            case ResourceType.None:
                break;
            case ResourceType.Importable:
                RTypes.Add("importable");
                break;
            case ResourceType.Exportable:
                RTypes.Add("exportable");
                break;
            case ResourceType.Both:
                RTypes.Add("importable");
                RTypes.Add("exportable");
                break;
        }

        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "id", ID },
            { "name", Name },
            { "unit", Unit },
            { "unitFull", UnitFull },
            { "sprite", Sprite },
            { "color", Color },
            { "value", Value },
            { "types", RTypes }
        };
        return System.Text.Json.JsonSerializer.Serialize(data);
    }

    // Will get used for loading up resource files from JSON
    public static Resource Deserialize(string json)
    {
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (!root.TryGetProperty("id", out var idEl) || idEl.ValueKind != System.Text.Json.JsonValueKind.String)
        {
            throw new ArgumentException("Resource JSON data must contain an 'id' field.");
        }

        List<string> fields = ["id", "name", "sprite", "color", "types", "value"];

        // Check for unexpected fields and list them explicitly
        var unexpected = root.EnumerateObject().Select(p => p.Name).Where(n => !fields.Contains(n)).ToList();
        if (unexpected.Count > 0)
        {
            Console.WriteLine($"Warning: Resource '{idEl.GetString()}' contains unexpected fields: {string.Join(", ", unexpected)}.");
        }

        // Warn if there are missing fields
        foreach (var field in fields)
        {
            if (!root.TryGetProperty(field, out var _))
            {
                Console.WriteLine($"Warning: Resource '{idEl.GetString()}' is missing expected field '{field}'.");
            }
        }

        var resource = new Resource
        {
            ID = idEl.GetString()!,
            Name = root.TryGetProperty("name", out var nameEl) && nameEl.ValueKind == System.Text.Json.JsonValueKind.String ? nameEl.GetString()! : "Resource",
            Sprite = root.TryGetProperty("sprite", out var spEl) && spEl.ValueKind == System.Text.Json.JsonValueKind.String ? spEl.GetString()! : "default.png",
            Color = root.TryGetProperty("color", out var colorEl) && colorEl.ValueKind == System.Text.Json.JsonValueKind.String ? colorEl.GetString()! : "#ff00ff"
        };

        if (root.TryGetProperty("value", out var valEl))
        {
            if (valEl.ValueKind == System.Text.Json.JsonValueKind.Number && valEl.TryGetDouble(out var v)) resource.Value = v;
            else if (valEl.ValueKind == System.Text.Json.JsonValueKind.String && double.TryParse(valEl.GetString(), out var vs)) resource.Value = vs;
        }

        if (root.TryGetProperty("types", out var typeEl) && typeEl.ValueKind == System.Text.Json.JsonValueKind.Array)
        {
            foreach (var item in typeEl.EnumerateArray())
            {
                if (Enum.TryParse<ResourceType>(item.GetString()!, out var rt))
                {
                    resource.Type = rt;
                    break;
                }
            }
        }
        else
        {
            resource.Type = ResourceType.None;
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
