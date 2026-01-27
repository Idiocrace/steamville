using System;
using System.Collections.Generic;
using System.Linq; // Added for LINQ methods
using System.Numerics; // Added for Vector2/Vector3
using TileGame.Processors;

namespace TileGame.Types;

// Tile class
public class Tile
{
    public required string ID { get; set; }
    public required Processor Processor { get; set; } = new Processors.BaseProcessor();
    public required string DisplayName { get; set; } = "Base Tile";
    public required Vector2 BoundingBox { get; set; } = new Vector2(1, 1); // width,height
    public Vector3 Position { get; set; } = new Vector3(0, 0, 0); // x,y,rotation (whole numbers 0-3 for quarter turns assuming top-left origin and counterclockwise rotation)
    // Containers are the most complicated part of tiles due to the amount of specificity needed
    public required Dictionary<string, Container> TileContainers { get; set; } = [];
    public ProcessorData? ProcessorData { get; set; } = null;

    // Parameterless constructor initializes all required members so `new Tile()` is valid.
    public Tile()
    {
        ID = System.Guid.NewGuid().ToString();
        Processor = new Processors.BaseProcessor();
        DisplayName = "Base Tile";
        BoundingBox = new Vector2(1, 1);
        Position = new Vector3(0, 0, 0);
        TileContainers = [];
        ProcessorData = null;
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
            TileContainers = [],
            ProcessorData = null
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
            containerData[containerEntry.Key] = containerDict ?? [];
        }
        data["containers"] = containerData;
        data["processor"] = Processor.ProcessorIDLiteral;
        
        // Serialize ProcessorData based on type
        if (ProcessorData != null)
        {
            if (ProcessorData is MachineProcessorData machineData)
            {
                var pdDict = new Dictionary<string, object>();
                var consumption = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();
                var production = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();
                
                foreach (var caseEntry in machineData.Consumption)
                {
                    var caseDict = new Dictionary<string, Dictionary<string, int>>();
                    foreach (var containerSpec in caseEntry.Value.Containers.Values)
                    {
                        var resourceDict = containerSpec.Resources.ToDictionary(
                            kvp => kvp.Key.ID,
                            kvp => kvp.Value
                        );
                        caseDict[containerSpec.ContainerName] = resourceDict;
                    }
                    consumption[caseEntry.Key] = caseDict;
                }
                
                foreach (var caseEntry in machineData.Production)
                {
                    var caseDict = new Dictionary<string, Dictionary<string, int>>();
                    foreach (var containerSpec in caseEntry.Value.Containers.Values)
                    {
                        var resourceDict = containerSpec.Resources.ToDictionary(
                            kvp => kvp.Key.ID,
                            kvp => kvp.Value
                        );
                        caseDict[containerSpec.ContainerName] = resourceDict;
                    }
                    production[caseEntry.Key] = caseDict;
                }
                
                pdDict["consumption"] = consumption;
                pdDict["production"] = production;
                data["processorData"] = pdDict;
            }
            else if (ProcessorData is ExtractorProcessorData extractorData)
            {
                var pdDict = new Dictionary<string, object>();
                pdDict["extraction"] = extractorData.Extraction.Select(e => new { e.Container, e.Amount }).ToList();
                pdDict["requirements"] = extractorData.Requirements.Select(r => new { r.Container, r.Amount }).ToList();
                data["processorData"] = pdDict;
            }
            else if (ProcessorData is PipeProcessorData pipeData)
            {
                var pdDict = new Dictionary<string, object>();
                pdDict["ports"] = pipeData.Ports.Select(p => p.ToString()).ToList();
                data["processorData"] = pdDict;
            }
        }

            // Serialize to JSON
            return System.Text.Json.JsonSerializer.Serialize(data);
        }

    // Will get used for loading up tile files from JSON
    public static Tile Deserialize(TileGame game, string json)
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
            TileContainers = [],
            ProcessorData = null
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
            case "pipe":
                tile.Processor = new Processors.PipeProcessor();
                break;
            default:
                tile.Processor = new Processors.BaseProcessor();
                break;
        }

        // Parse processorData (optional) into strongly-typed ProcessorData objects
        if (root.TryGetProperty("processorData", out var pdEl) && pdEl.ValueKind == System.Text.Json.JsonValueKind.Object)
        {
            switch (tile.Processor)
            {
                case Processors.MachineProcessor:
                    var machineData = new MachineProcessorData
                    {
                        Consumption = [],
                        Production = []
                    };

                    if (pdEl.TryGetProperty("consumption", out var consEl) && consEl.ValueKind == System.Text.Json.JsonValueKind.Object)
                    {
                        foreach (var caseProp in consEl.EnumerateObject())
                        {
                            var machineCase = new MachineCase { Containers = [] };
                            if (caseProp.Value.ValueKind == System.Text.Json.JsonValueKind.Object)
                            {
                                foreach (var contProp in caseProp.Value.EnumerateObject())
                                {
                                    var spec = new ContainerSpec
                                    {
                                        ContainerName = contProp.Name,
                                        Resources = []
                                    };

                                    if (contProp.Value.ValueKind == System.Text.Json.JsonValueKind.Object)
                                    {
                                        foreach (var resProp in contProp.Value.EnumerateObject())
                                        {
                                            string resId = resProp.Name;
                                            int amount = 0;
                                            if (resProp.Value.ValueKind == System.Text.Json.JsonValueKind.Number && resProp.Value.TryGetInt32(out var a)) amount = a;
                                            else if (resProp.Value.ValueKind == System.Text.Json.JsonValueKind.String && int.TryParse(resProp.Value.GetString(), out var ai)) amount = ai;

                                            var resource = game.Resources.FirstOrDefault(r => r.ID == resId);
                                            if (resource != null)
                                            {
                                                spec.Resources[resource] = amount;
                                            }
                                        }
                                    }
                                    machineCase.Containers[contProp.Name] = spec;
                                }
                            }
                            machineData.Consumption[caseProp.Name] = machineCase;
                        }
                    }

                    if (pdEl.TryGetProperty("production", out var prodEl) && prodEl.ValueKind == System.Text.Json.JsonValueKind.Object)
                    {
                        foreach (var caseProp in prodEl.EnumerateObject())
                        {
                            var machineCase = new MachineCase { Containers = [] };
                            if (caseProp.Value.ValueKind == System.Text.Json.JsonValueKind.Object)
                            {
                                foreach (var contProp in caseProp.Value.EnumerateObject())
                                {
                                    var spec = new ContainerSpec
                                    {
                                        ContainerName = contProp.Name,
                                        Resources = []
                                    };

                                    if (contProp.Value.ValueKind == System.Text.Json.JsonValueKind.Object)
                                    {
                                        foreach (var resProp in contProp.Value.EnumerateObject())
                                        {
                                            string resId = resProp.Name;
                                            int amount = 0;
                                            if (resProp.Value.ValueKind == System.Text.Json.JsonValueKind.Number && resProp.Value.TryGetInt32(out var a)) amount = a;
                                            else if (resProp.Value.ValueKind == System.Text.Json.JsonValueKind.String && int.TryParse(resProp.Value.GetString(), out var ai)) amount = ai;

                                            var resource = game.Resources.FirstOrDefault(r => r.ID == resId);
                                            if (resource != null)
                                            {
                                                spec.Resources[resource] = amount;
                                            }
                                        }
                                    }
                                    machineCase.Containers[contProp.Name] = spec;
                                }
                            }
                            machineData.Production[caseProp.Name] = machineCase;
                        }
                    }

                    tile.ProcessorData = machineData;
                    break;

                case Processors.ExtractorProcessor:
                    var extractorData = new ExtractorProcessorData
                    {
                        Extraction = [],
                        Requirements = []
                    };

                    if (pdEl.TryGetProperty("extraction", out var extrEl) && extrEl.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        foreach (var item in extrEl.EnumerateArray())
                        {
                            if (item.ValueKind == System.Text.Json.JsonValueKind.Object)
                            {
                                string? container = null;
                                int amount = 0;
                                if (item.TryGetProperty("container", out var cEl)) container = cEl.GetString();
                                if (item.TryGetProperty("amount", out var aEl) && aEl.TryGetInt32(out var a)) amount = a;
                                
                                if (container != null)
                                {
                                    extractorData.Extraction.Add(new ExtractionSpec { Container = container, Amount = amount });
                                }
                            }
                        }
                    }

                    if (pdEl.TryGetProperty("requirements", out var reqEl) && reqEl.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        foreach (var item in reqEl.EnumerateArray())
                        {
                            if (item.ValueKind == System.Text.Json.JsonValueKind.Object)
                            {
                                string? container = null;
                                int amount = 0;
                                if (item.TryGetProperty("container", out var cEl)) container = cEl.GetString();
                                if (item.TryGetProperty("amount", out var aEl) && aEl.TryGetInt32(out var a)) amount = a;
                                
                                if (container != null)
                                {
                                    extractorData.Requirements.Add(new RequirementSpec { Container = container, Amount = amount });
                                }
                            }
                        }
                    }

                    tile.ProcessorData = extractorData;
                    break;

                case Processors.PipeProcessor:
                    var pipeData = new PipeProcessorData
                    {
                        Ports = []
                    };

                    if (pdEl.TryGetProperty("ports", out var portsEl) && portsEl.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        foreach (var item in portsEl.EnumerateArray())
                        {
                            if (item.ValueKind == System.Text.Json.JsonValueKind.String)
                            {
                                string? portStr = item.GetString();
                                if (portStr != null && Enum.TryParse<Side>(portStr, true, out var side))
                                {
                                    pipeData.Ports.Add(side);
                                }
                            }
                        }
                    }

                    tile.ProcessorData = pipeData;
                    break;
            }
        }

            return tile;
        }
    }
}