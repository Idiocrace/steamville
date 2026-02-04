namespace SteamVille.Content;

public class Resource
{
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string Sprite { get; set; } = "";
    public string Color { get; set; } = "";
    public double Sell { get; set; } = 0.0;
    public double Buy { get; set; } = 0.0;
    public ResourceType Type { get; set; } = ResourceType.None;

    // CRITICAL for using Resource as Dictionary key
    public override bool Equals(object? obj)
        => obj is Resource other && ID == other.ID;

    public override int GetHashCode()
        => ID.GetHashCode();
}

public enum ResourceType
{
    None,
    Importable,
    Exportable,
    Both
}