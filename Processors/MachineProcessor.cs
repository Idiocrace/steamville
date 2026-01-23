using TileGame.Types;

namespace TileGame.Processors;

public class MachineProcessor : Processor
{
    public static new readonly string ProcessorIDLiteral = "machine";
    private static bool processorLocked = false;
    private static readonly Dictionary<string, Tile> adjacentTiles = [];
    public new void Process(TileGame game, dynamic tile, Dictionary<string, Tile> adjacentTiles)
    {
        if (processorLocked)
        {
            return;
        }

        if (tile.ProcessorData is not Dictionary<object, object> data)
        {
            throw new ProcessorDataException("MachineProcessor requires ProcessorData as a dictionary.");
        }

        if (!data.ContainsKey("consumption"))
        {
            throw new ProcessorDataException("MachineProcessor requires 'consumption' data.");
        }
        if (!data.ContainsKey("production"))
        {
            throw new ProcessorDataException("MachineProcessor requires 'production' data.");
        }

        var consumptionCases = (Dictionary<string, Dictionary<object, object>>)data["consumption"];
        var productionCases = (Dictionary<string, Dictionary<object, object>>)data["production"];
        var consumptionCaseNames = consumptionCases.Keys.ToList();
        var productionCaseNames = productionCases.Keys.ToList();

        // Check that each case has a matching case in the other section
        foreach (string caseName in consumptionCaseNames)
        {
            if (!productionCaseNames.Contains(caseName))
            {
                processorLocked = true;
                return;
            }
        }

        // Process first case that is fulfillable
        foreach (KeyValuePair<string, Dictionary<object, object>> caseEntry in consumptionCases)
        {
            var equivalentProductionCase = productionCases[caseEntry.Key];

            bool allContainersFulfilled = true;
            // Check each requested container for this case
            foreach (var requestedContainerObj in caseEntry.Value.Values)
            {
                var requestedContainer = (Dictionary<object, object>)requestedContainerObj;
                var containerName = (string)requestedContainer["container"];
                var resourcesRequested = (Dictionary<Resource, int>)requestedContainer["resources"];

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
                foreach (var requestedContainerObj in caseEntry.Value.Values)
                {
                    var requestedContainer = (Dictionary<object, object>)requestedContainerObj;
                    var containerName = (string)requestedContainer["container"];
                    var resourcesRequested = (Dictionary<Resource, int>)requestedContainer["resources"];
                    var targetContainer = tile.TileContainers[containerName];

                    foreach (var resourceRequest in resourcesRequested)
                    {
                        targetContainer.RemoveResource(resourceRequest.Key, resourceRequest.Value);
                    }
                }

                // Produce outputs
                foreach (var productionContainerObj in equivalentProductionCase.Values)
                {
                    var productionContainer = (Dictionary<object, object>)productionContainerObj;
                    var containerName = (string)productionContainer["container"];
                    var resourcesProduced = (Dictionary<Resource, int>)productionContainer["resources"];

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