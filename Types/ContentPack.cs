namespace TileGame.Types;

// Content pack to hold class
public class ContentPack
{
    public PackMeta Meta { get; set; } = new PackMeta();
    public List<Resource> Resources { get; set; } = [];
    public List<Tile> Tiles { get; set; } = [];
}

public record PackMeta
{
    public string Version { get; set; } = "0";
    public string UpdateName { get; set; } = "Unknown";
    public string ID { get; set; } = "unknownPack";
    public string Name { get; set; } = "Unknown Pack";
    public List<string> Authors { get; set; } = [];
    public string Description { get; set; } = "No Description";
    public List<string> Dependencies { get; set; } = [];
    public string UpdateURL { get; set; } = "none";
}