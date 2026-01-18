using TileGame.Processors;

namespace TileGame.Types;

// Tile class
public class Tile
{
    public required string ID { get; set; }
    public required Processor Processor { get; set; } = new Processors.BaseProcessor();
    public required string DisplayName { get; set; } = "Base Tile";
    public required Vector2 BoundingBox { get; set; } = new Vector2(1, 1); // width,height
    public Vector3 Position { get; set; } = new Vector3(0, 0, 0); // x,y,rotation
    // Containers are the most complicated part of tiles due to the amount of specificity needed
    public required Dictionary<string, Container> TileContainers { get; set; } = new Dictionary<string, Container>();
    public required Dictionary<object, object> ProcessorData { get; set; } = [];

    // Parameterless constructor initializes all required members so `new Tile()` is valid.
    public Tile()
    {
        ID = System.Guid.NewGuid().ToString();
        Processor = new Processors.BaseProcessor();
        DisplayName = "Base Tile";
        BoundingBox = new Vector2(1, 1);
        Position = new Vector3(0, 0, 0);
        TileContainers = new Dictionary<string, Container>();
        ProcessorData = [];
    }

    public static Tile NewTile(string id, Processor processor, string displayName, Vector2 boundingBox, Vector3 position)
    {
        return new Tile
        {
            ID = id,
            Processor = processor,
            DisplayName = displayName,
            BoundingBox = boundingBox,
            Position = position,
            TileContainers = new Dictionary<string, Container>(),
            ProcessorData = new Dictionary<object, object>()
        };
    }

    // Won't get used in anything but the SteamVille Modding Toolchain for easy generation of tile JSON files
    public string Serialize()
    {
        // Returns a full JSON serialization of the Tile (good for generating a JSON tile file)
        // (excluding instance data)
        Dictionary<string, object> data = [];
        data["id"] = ID;
        data["name"] = DisplayName;
        data["boundingBox"] = new Dictionary<string, int>
        {
            { "width", BoundingBox.X },
            { "height", BoundingBox.Y }
        };
        // Container formatting is a bit complex
        Dictionary<string, object> containerData = [];
        // We need to strip the container contents but keep like everything else with the containers
        foreach (KeyValuePair<string, Container> containerEntry in TileContainers)
        {
            // Serialize the container and manipulate the resulting object
            var containerJson = System.Text.Json.JsonSerializer.Serialize(containerEntry.Value);
            var containerDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(containerJson);
            if (containerDict != null && containerDict.ContainsKey("contents"))
            {
                containerDict.Remove("contents");
            }
            containerData[containerEntry.Key] = containerDict ?? new Dictionary<string, object>();
        }
        data["containers"] = containerData;
        data["processor"] = Processor.ProcessorIDLiteral;
        data["processorData"] = ProcessorData;

        // Serialize to JSON
        return System.Text.Json.JsonSerializer.Serialize(data);
    }

    // Will get used for loading up tile files from JSON
    public static Tile Deserialize(string json)
    {
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (!root.TryGetProperty("id", out var idEl) || idEl.ValueKind != System.Text.Json.JsonValueKind.String)
        {
            throw new ArgumentException("Tile JSON data must contain an 'id' field.");
        }
        string id = idEl.GetString()!;

        string displayName = root.TryGetProperty("name", out var nameEl) && nameEl.ValueKind == System.Text.Json.JsonValueKind.String
            ? nameEl.GetString()!
            : "Base Tile";

        int width = 1, height = 1;
        if (root.TryGetProperty("boundingBox", out var bboxEl))
        {
            if (bboxEl.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                var arr = bboxEl.EnumerateArray().ToArray();
                if (arr.Length >= 1 && arr[0].TryGetInt32(out var w)) width = w;
                if (arr.Length >= 2 && arr[1].TryGetInt32(out var h)) height = h;
            }
            else if (bboxEl.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                if (bboxEl.TryGetProperty("width", out var wEl) && wEl.TryGetInt32(out var w)) width = w;
                if (bboxEl.TryGetProperty("height", out var hEl) && hEl.TryGetInt32(out var h)) height = h;
            }
        }

        var tile = new Tile
        {
            ID = id,
            Processor = new Processors.BaseProcessor(),
            DisplayName = displayName,
            BoundingBox = new Vector2(width, height),
            TileContainers = new Dictionary<string, Container>(),
            ProcessorData = new Dictionary<object, object>()
        };

        // Parse containers (capacity + optional filter list)
        if (root.TryGetProperty("containers", out var containersEl) && containersEl.ValueKind == System.Text.Json.JsonValueKind.Object)
        {
            foreach (var contProp in containersEl.EnumerateObject())
            {
                string contName = contProp.Name;
                int cap = 0;
                List<string>? filter = null;
                var contObj = contProp.Value;
                if (contObj.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    if (contObj.TryGetProperty("capacity", out var capEl) && capEl.TryGetInt32(out var c)) cap = c;
                    if (contObj.TryGetProperty("filter", out var filterEl) && filterEl.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        filter = filterEl.EnumerateArray().Where(e => e.ValueKind == System.Text.Json.JsonValueKind.String).Select(e => e.GetString()!).ToList();
                    }
                }
                tile.TileContainers[contName] = new Container(cap, filter);
            }
        }

        // Set processor
        switch (root.TryGetProperty("processor", out var procEl) && procEl.ValueKind == System.Text.Json.JsonValueKind.String
            ? procEl.GetString()!
            : "")
        {
            case "base":
                tile.Processor = new Processors.BaseProcessor();
                break;
            case "machine":
                tile.Processor = new Processors.MachineProcessor();
                break;
            default:
                tile.Processor = new Processors.BaseProcessor();
                break;
        }

        // Parse processorData (optional) into the structure expected by processors
        if (root.TryGetProperty("processorData", out var pdEl) && pdEl.ValueKind == System.Text.Json.JsonValueKind.Object)
        {
            var processorData = new Dictionary<object, object>();

            // Helper to parse a cases object (consumption/production)
            void ParseCases(System.Text.Json.JsonElement casesEl, Dictionary<string, Dictionary<object, object>> dst)
            {
                foreach (var caseProp in casesEl.EnumerateObject())
                {
                    var caseDict = new Dictionary<object, object>();
                    var caseObj = caseProp.Value;
                    if (caseObj.ValueKind == System.Text.Json.JsonValueKind.Object)
                    {
                        foreach (var contProp in caseObj.EnumerateObject())
                        {
                            var spec = new Dictionary<object, object>();
                            spec["container"] = contProp.Name; // e.g. "container0"

                            // Parse resources map (resourceId -> amount) into Dictionary<Resource,int>
                            var resDict = new Dictionary<Resource, int>();
                            if (contProp.Value.ValueKind == System.Text.Json.JsonValueKind.Object)
                            {
                                foreach (var resProp in contProp.Value.EnumerateObject())
                                {
                                    string resId = resProp.Name;
                                    int amount = 0;
                                    if (resProp.Value.ValueKind == System.Text.Json.JsonValueKind.Number && resProp.Value.TryGetInt32(out var a)) amount = a;
                                    else if (resProp.Value.ValueKind == System.Text.Json.JsonValueKind.String && int.TryParse(resProp.Value.GetString(), out var ai)) amount = ai;

                                    var resource = TileGame.Resources.FirstOrDefault(r => r.ID == resId);
                                    if (resource == null)
                                    {
                                        throw new ArgumentException($"Unknown resource '{resId}' in processorData.");
                                    }

                                    resDict[resource] = amount;
                                }
                            }

                            spec["resources"] = resDict;
                            caseDict[contProp.Name] = spec;
                        }
                    }
                    dst[caseProp.Name] = caseDict;
                }
            }

            if (pdEl.TryGetProperty("consumption", out var consEl) && consEl.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                var consCases = new Dictionary<string, Dictionary<object, object>>();
                ParseCases(consEl, consCases);
                processorData["consumption"] = consCases;
            }

            if (pdEl.TryGetProperty("production", out var prodEl) && prodEl.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                var prodCases = new Dictionary<string, Dictionary<object, object>>();
                ParseCases(prodEl, prodCases);
                processorData["production"] = prodCases;
            }

            tile.ProcessorData = processorData;
        }

        return tile;
    }
}