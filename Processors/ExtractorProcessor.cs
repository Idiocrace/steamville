using TileGame.Types;

namespace TileGame.Processors;

// Used for extractors
public class ExtractorProcessor : Processor
{
    public static new readonly string ProcessorIDLiteral = "extractor";
    private static bool ProcessorLocked = false;
    public new void Process(TileGame game, Tile tile, Dictionary<string, Tile> adjacentTiles)
    {
        if (ProcessorLocked) {
            return;
        }

        if (tile.ProcessorData is not ExtractorProcessorData data)
        {
            ProcessorLocked = true;
            throw new ProcessorDataException("ExtractorProcessor requires ProcessorData as ExtractorProcessorData.");
        }

        // Check if the requirements are met
        bool requirementsMet = true;
        foreach (var requirement in data.Requirements)
        {
            // Check if the container has enough resources
            var containerName = requirement.Container;
            if (!tile.TileContainers.ContainsKey(containerName))
            {
                requirementsMet = false;
                break;
            }
            
            var totalInContainer = tile.TileContainers[containerName].Contents.Values.Sum();
            if (totalInContainer < requirement.Amount)
            {
                requirementsMet = false;
                break;
            }
        }

        if (!requirementsMet)
        {
            return;
        }

        // Now that we know there is sufficient resources, we process!
        foreach (var extraction in data.Extraction)
        {
            var containerName = extraction.Container;
            var amount = extraction.Amount;
            
            if (!tile.TileContainers.ContainsKey(containerName))
            {
                // Skip if container doesn't exist
                continue;
            }
            
            // Note: This implementation assumes extractors work with a single resource type
            // You may need to adjust based on your game's requirements
            var container = tile.TileContainers[containerName];
            if (container.Contents.Any())
            {
                var resource = container.Contents.First().Key;
                container.AddResource(resource, amount);
            }
        }
    }
}
