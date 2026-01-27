using TileGame.Types;

namespace TileGame.Processors;

public class MachineProcessor : Processor
{
    public static new readonly string ProcessorIDLiteral = "machine";
    private static bool ProcessorLocked = false;
    private static readonly Dictionary<string, Tile> adjacentTiles = [];
    public new void Process(TileGame game, Tile tile, Dictionary<string, Tile> adjacentTiles)
    {
        if (ProcessorLocked)
        {
            return;
        }

        if (tile.ProcessorData is not MachineProcessorData data)
        {
            throw new ProcessorDataException("MachineProcessor requires ProcessorData as MachineProcessorData.");
        }

        var consumptionCases = data.Consumption;
        var productionCases = data.Production;
        var consumptionCaseNames = consumptionCases.Keys.ToList();
        var productionCaseNames = productionCases.Keys.ToList();

        // Check that each case has a matching case in the other section
        foreach (string caseName in consumptionCaseNames)
        {
            if (!productionCaseNames.Contains(caseName))
            {
                ProcessorLocked = true;
                return;
            }
        }

        // Process first case that is fulfillable
        foreach (KeyValuePair<string, MachineCase> caseEntry in consumptionCases)
        {
            var equivalentProductionCase = productionCases[caseEntry.Key];

            bool allContainersFulfilled = true;
            // Check each requested container for this case
            foreach (var containerSpec in caseEntry.Value.Containers.Values)
            {
                var containerName = containerSpec.ContainerName;
                var resourcesRequested = containerSpec.Resources;

                if (!tile.TileContainers.ContainsKey(containerName))
                {
                    allContainersFulfilled = false;
                    break;
                }

                var targetContainer = tile.TileContainers[containerName];
                foreach (var resourceRequest in resourcesRequested)
                {
                    if (!targetContainer.Contents.ContainsKey(resourceRequest.Key) || targetContainer.Contents[resourceRequest.Key] < resourceRequest.Value)
                    {
                        allContainersFulfilled = false;
                        break;
                    }
                }
                if (!allContainersFulfilled) break;
            }

            if (allContainersFulfilled)
            {
                // Consume resources
                foreach (var containerSpec in caseEntry.Value.Containers.Values)
                {
                    var containerName = containerSpec.ContainerName;
                    var resourcesRequested = containerSpec.Resources;
                    var targetContainer = tile.TileContainers[containerName];

                    foreach (var resourceRequest in resourcesRequested)
                    {
                        targetContainer.RemoveResource(resourceRequest.Key, resourceRequest.Value);
                    }
                }

                // Produce outputs
                foreach (var containerSpec in equivalentProductionCase.Containers.Values)
                {
                    var containerName = containerSpec.ContainerName;
                    var resourcesProduced = containerSpec.Resources;

                    if (!tile.TileContainers.ContainsKey(containerName))
                    {
                        // skip if production container doesn't exist
                        continue;
                    }

                    var targetContainer = tile.TileContainers[containerName];
                    foreach (var resourceProduction in resourcesProduced)
                    {
                        targetContainer.AddResource(resourceProduction.Key, resourceProduction.Value);
                    }
                }

                // Exit after processing one case
                break;
            }
        }
    }
}