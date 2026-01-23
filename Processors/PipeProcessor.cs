using TileGame.Types;

namespace TileGame.Processors;

public class PipeProcessor : Processor
{
    public static new readonly string ProcessorIDLiteral = "pipe";
    private static bool ProcessorLocked = false;
    public new void Process(TileGame game, dynamic tile, Dictionary<string, Tile> adjacentTiles)
    {
        // Check if the tile has a wierd non-1x1 bounding box
        Vector2 BoundingBox = tile.BoundingBox;
        if (!(BoundingBox == new Vector2(1, 1)))
        {
            // Throw an error because we don't support non-1x1 pipes yet (by that i mean they arent ever gonna be supported)
            throw new ProcessorDataException("PipeProcessor only supports 1x1 bounding boxes.");
        }

        // Now, we process the pipe
        
    }
}