using TileGame.Types;

namespace TileGame.Processors;

public abstract class Processor
{
    public static readonly string ProcessorIDLiteral = "processor";
    private static bool processorLocked = false;
    private static Dictionary<string, Tile> adjacentTiles = [];
    public void Process()
    {
        if (!processorLocked)
        {
            processorLocked = true;
            throw new InvalidOperationException("Processor.process() is not implemented yet.");
        }
    }
}  

public class ProcessorDataException(string message) : Exception(message) { }