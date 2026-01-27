using TileGame.Types;

namespace TileGame.Processors;

public abstract class Processor
{
    public static readonly string ProcessorIDLiteral = "processor";
    private static bool ProcessorLocked = false;
    private static Dictionary<string, Tile> adjacentTiles = [];
    public void Process(TileGame game, Tile tile, Dictionary<string, Tile> adjacentTiles)
    {
        if (!ProcessorLocked)
        {
            ProcessorLocked = true;
            throw new InvalidOperationException("Processor.process() is not implemented yet.");
        }
    }
}  

public class ProcessorDataException(string message) : Exception(message) { }