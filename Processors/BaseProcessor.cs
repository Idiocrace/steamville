using TileGame.Types;

namespace TileGame.Processors;

public class BaseProcessor : Processor
{
    public static new readonly string ProcessorIDLiteral = "base";
    private static bool ProcessorLocked = false;
    public new void Process(TileGame game, Tile tile, Dictionary<string, Tile> adjacentTiles)
    {
        if (!ProcessorLocked) {
            ProcessorLocked = true;
            throw new InvalidOperationException("BaseProcessor.process() is not implemented yet.");
        }
    }
}
