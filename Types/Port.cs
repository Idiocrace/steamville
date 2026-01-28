namespace TileGame.Types;

// Enum for port types
public enum PortType
{
    Output = 0,
    Input = 1
}

// Port class for tile ports that pipes can interface with
public class Port
{
    public Vector2 Position { get; set; } = new Vector2(0, 0);
    public PortType Type { get; set; } = PortType.Output;
    public List<string> Containers { get; set; } = [];

    public Port()
    {
        Position = new Vector2(0, 0);
        Type = PortType.Output;
        Containers = [];
    }

    public Port(Vector2 position, PortType type, List<string> containers)
    {
        Position = position;
        Type = type;
        Containers = containers;
    }

    // Check if this port is an input port
    public bool IsInput => Type == PortType.Input;

    // Check if this port is an output port
    public bool IsOutput => Type == PortType.Output;
}
