using TileGame.Types;

namespace TileGame.Processors;

public class MachineProcessor : Processor
{
    public static new readonly string ProcessorIDLiteral = "machine";
    public void Process(dynamic tile)
    {
        dynamic data = tile.ProcessorData;
        if (!data.ContainsKey("consumption"))
        {
            throw new ProcessorDataException("MachineProcessor requires 'consumption' data.");
        }
        if (!data.ContainsKey("production"))
        {
            throw new ProcessorDataException("MachineProcessor requires 'production' data.");
        }
        // The consumption-production logic works where there are recipe cases, where if a case is fulfilled, it will process for that case.
        // This is for things like the generators so they can take either normal or high pressure steam and produce different outputs.
        Dictionary<string, Dictionary<object, object>> consumptionCases = (Dictionary<string, Dictionary<object, object>>)data["consumption"];
        Dictionary<string, Dictionary<object, object>> productionCases = (Dictionary<string, Dictionary<object, object>>)data["production"];
        List<string> consumptionCaseNames = [.. consumptionCases.Keys];
        List<string> productionCaseNames = [.. productionCases.Keys];
        // Check that each case has a matching case in the other section
        foreach (string caseName in consumptionCaseNames)
        {
            if (!productionCaseNames.Contains(caseName))
            {
                throw new ProcessorDataException($"MachineProcessor consumption case '{caseName}' has no matching production case.");
            }
        }
        // Now, we process!
        // There is a miniscule chance that there could be two cases that have their requirements fulfilled at the same time, but regardless, it will prefer the first case
        foreach (KeyValuePair<string, Dictionary<object, object>> caseEntry in consumptionCases)
        {
            Dictionary<object, object> equivalentProductionCase = productionCases[caseEntry.Key];
            // Check if we have enough resources this tick in the specified containers to fulfill this case
            // Also, for future me or if i get other devs on this proj, this code is incredibly fuckin janky
            // Theres a lot of room for stuff to desync and for example, the tile to process when its not supposed to, resulting in no resource consumption
            List<Dictionary<object, object>> requestedContainers = [.. caseEntry.Value.Values.Cast<Dictionary<object, object>>()];
            Dictionary<Dictionary<object, object>, bool> containerFulfillments = []; // the unholy dictionary! using a dict as a key has gotta be the most braindead thing i've ever done
            foreach (Dictionary<object, object> requestedContainer in requestedContainers)
            {
                Dictionary<Resource, int> resourcesRequested = (Dictionary<Resource, int>)requestedContainer["resources"];
                Dictionary<Resource, bool> resourceFulfillments = new Dictionary<Resource, bool>();
                foreach (KeyValuePair<Resource, int> resourceRequest in resourcesRequested)
                {
                    Dictionary<string, object> containerHunk = tile.Container[(string)requestedContainer["container"]];
                    Dictionary<Resource, int> containerResources = (Dictionary<Resource, int>)containerHunk["contents"];
                    if (containerResources.ContainsKey(resourceRequest.Key) && containerResources[resourceRequest.Key] >= resourceRequest.Value)
                    {
                        resourceFulfillments[resourceRequest.Key] = true;
                    }
                    else
                    {
                        resourceFulfillments[resourceRequest.Key] = false;
                    }
                }
                // Check if all resources were fulfilled
                bool allResourcesFulfilled = false;
                foreach (bool fulfilled in resourceFulfillments.Values)
                {
                    if (fulfilled)
                    {
                        allResourcesFulfilled = true;
                    }
                    else
                    {
                        allResourcesFulfilled = false;
                        break;
                    }
                }
                if (allResourcesFulfilled)
                {
                    containerFulfillments[requestedContainer] = true;
                }
            }
            // Check if all containers were fulfilled
            bool allContainersFulfilled = false;
            foreach (bool fulfilled in containerFulfillments.Values)
            {
                if (fulfilled)
                {
                    allContainersFulfilled = true;
                }
                else
                {
                    allContainersFulfilled = false;
                    break;
                }
            }
            if (allContainersFulfilled)
            {
                // Process production for this case
                List<Dictionary<object, object>> productionContainers = [.. equivalentProductionCase.Values.Cast<Dictionary<object, object>>()];
                foreach (Dictionary<object, object> productionContainer in productionContainers)
                {
                    Dictionary<Resource, int> resourcesProduced = (Dictionary<Resource, int>)productionContainer["resources"];
                    foreach (KeyValuePair<Resource, int> resourceProduction in resourcesProduced)
                    {
                        if (!tile.Container[(string)productionContainer["container"]].ContainsKey(resourceProduction.Key))
                        {
                            tile.Container[(string)productionContainer["container"]][resourceProduction.Key] = 0;
                        }
                        tile.Container[(string)productionContainer["container"]][resourceProduction.Key] += resourceProduction.Value;
                    }
                }
                // Exit after processing one case
                break;
            }
        }
    }
}