using TileGame.Types;

namespace TileGame.Processors;

public class PipeProcessor : Processor
{
    public static new readonly string ProcessorIDLiteral = "pipe";
    private static bool ProcessorLocked = false;
    public new void Process(TileGame game, Tile tile, Dictionary<string, Tile> adjacentTiles)
    {
        // Check if the tile has a wierd non-1x1 bounding box
        Vector2 BoundingBox = tile.BoundingBox;
        if (!(BoundingBox == new Vector2(1, 1)))
        {
            // Throw an error because we don't support non-1x1 pipes yet (by that i mean they arent ever gonna be supported)
            throw new ProcessorDataException("PipeProcessor only supports 1x1 bounding boxes.");
        }

        // Now, we process the pipe
        // Get our resources
        Dictionary<Resource, int> resourcesInPipe = new Dictionary<Resource, int>();
        foreach (var container in tile.TileContainers.Values)
        {
            foreach (var resourceEntry in container.Contents)
            {
                if (resourcesInPipe.ContainsKey(resourceEntry.Key))
                {
                    resourcesInPipe[resourceEntry.Key] += resourceEntry.Value;
                }
                else
                {
                    resourcesInPipe[resourceEntry.Key] = resourceEntry.Value;
                }
            }
        }
        // Now, we check what sides have ports
        List<Side> Sides = [];
        if (tile.ProcessorData is not PipeProcessorData data)
        {
            ProcessorLocked = true;
            throw new ProcessorDataException("PipeProcessor requires ProcessorData as PipeProcessorData.");
        }
        // Apply rotation to ports (rotation is in quarter turns, 0-3)
        int rotation = tile.Position.r % 4;
        foreach (var port in data.Ports)
        {
            Sides.Add(RotateSide(port, rotation));
        }
        
        // Now we distribute resources to adjacent tiles with ports on the matching side
        // Build list of valid output tiles (those with matching ports)
        List<Tile> outputTiles = [];
        
        foreach (var adjacentEntry in adjacentTiles)
        {
            string direction = adjacentEntry.Key;
            Tile adjacentTile = adjacentEntry.Value;
            
            // Convert direction string to Side enum
            Side currentSide = DirectionToSide(direction);
            
            // Check if this pipe has a port on this side
            if (!Sides.Contains(currentSide))
            {
                continue;
            }
            
            // Check if adjacent tile is also a pipe
            if (adjacentTile.Processor is not PipeProcessor)
            {
                continue;
            }
            
            // Check if adjacent tile has processor data
            if (adjacentTile.ProcessorData is not PipeProcessorData adjacentData)
            {
                continue;
            }
            
            // Get opposite side (what the adjacent tile sees)
            Side oppositeSide = GetOppositeSide(currentSide);
            
            // Check if adjacent tile has a port on the opposite side (accounting for its rotation)
            int adjacentRotation = adjacentTile.Position.r % 4;
            List<Side> adjacentSides = [];
            foreach (var port in adjacentData.Ports)
            {
                adjacentSides.Add(RotateSide(port, adjacentRotation));
            }
            
            if (adjacentSides.Contains(oppositeSide))
            {
                outputTiles.Add(adjacentTile);
            }
        }
        
        // Distribute resources evenly to all connected output tiles
        if (outputTiles.Count > 0 && resourcesInPipe.Count > 0)
        {
            foreach (var resource in resourcesInPipe)
            {
                int amountPerTile = resource.Value / outputTiles.Count;
                int remainder = resource.Value % outputTiles.Count;
                
                // Remove resources from current pipe
                foreach (var container in tile.TileContainers.Values)
                {
                    if (container.Contents.ContainsKey(resource.Key))
                    {
                        int toRemove = Math.Min(resource.Value, container.Contents[resource.Key]);
                        if (toRemove > 0)
                        {
                            container.RemoveResource(resource.Key, toRemove);
                        }
                    }
                }
                
                // Distribute to output tiles
                for (int i = 0; i < outputTiles.Count; i++)
                {
                    int amountToAdd = amountPerTile + (i < remainder ? 1 : 0);
                    if (amountToAdd > 0)
                    {
                        // Add to first available container
                        var targetContainer = outputTiles[i].TileContainers.Values.FirstOrDefault();
                        if (targetContainer != null && targetContainer.CanAddResource(resource.Key, amountToAdd))
                        {
                            targetContainer.AddResource(resource.Key, amountToAdd);
                        }
                    }
                }
            }
        }
    }
    
    // Helper method to rotate a side by quarter turns
    private static Side RotateSide(Side side, int quarterTurns)
    {
        // Normalize all side variants to Top/Right/Bottom/Left
        Side normalized = NormalizeSide(side);
        
        // Rotate the normalized side
        for (int i = 0; i < quarterTurns; i++)
        {
            normalized = normalized switch
            {
                Side.Top => Side.Right,
                Side.Right => Side.Bottom,
                Side.Bottom => Side.Left,
                Side.Left => Side.Top,
                _ => normalized
            };
        }
        
        return normalized;
    }
    
    // Helper method to normalize side enum variants
    private static Side NormalizeSide(Side side)
    {
        return side switch
        {
            Side.Top or Side.T or Side.top or Side.t or Side.North or Side.north => Side.Top,
            Side.Right or Side.R or Side.right or Side.r or Side.East or Side.east => Side.Right,
            Side.Bottom or Side.B or Side.bottom or Side.b or Side.South or Side.south => Side.Bottom,
            Side.Left or Side.L or Side.left or Side.l or Side.West or Side.west => Side.Left,
            _ => side
        };
    }
    
    // Helper method to convert direction string to Side
    private static Side DirectionToSide(string direction)
    {
        return direction.ToLower() switch
        {
            "up" or "top" or "north" => Side.Top,
            "right" or "east" => Side.Right,
            "down" or "bottom" or "south" => Side.Bottom,
            "left" or "west" => Side.Left,
            _ => Side.Top
        };
    }
    
    // Helper method to get opposite side
    private static Side GetOppositeSide(Side side)
    {
        return side switch
        {
            Side.Top => Side.Bottom,
            Side.Right => Side.Left,
            Side.Bottom => Side.Top,
            Side.Left => Side.Right,
            _ => side
        };
    }
}