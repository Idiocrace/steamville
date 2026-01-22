namespace TileGame.Processors;

// Used for extractors
public class ExtractorProcessor : Processor
{
    public static new readonly string ProcessorIDLiteral = "extractor";
    private static bool ProcessorLocked = false;
    public void Process(dynamic tile)
    {
        if (ProcessorLocked) {
            return;
        }

        if (tile.ProcessorData is not Dictionary<object, object> tileData)
        {
            ProcessorLocked = true;
            throw new ProcessorDataException("ExtractorProcessor requires ProcessorData as a dictionary.");
        }

        if (!tileData.ContainsKey("extraction"))
        {
            ProcessorLocked = true;
            throw new ProcessorDataException("ExtractorProcessor requires 'extraction' data.");
        }

        if (!tileData.ContainsKey("requirements"))
        {
            ProcessorLocked = true;
            throw new ProcessorDataException("ExtractorProcessor requires 'requirements' data.");
        }

        object extractionData = tileData["extraction"];
        object requirementsData = tileData["requirements"];

        // Check if the requirements are met
        bool requirementsMet = true;
        foreach (var requirement in ((Dictionary<object, object>)requirementsData).Values)
        {
            // Get the requested container
            var container = ((Dictionary<object, object>)requirement)["container"];
            var amount = ((Dictionary<object, object>)requirement)["amount"];
            // Check if the container has enough resources
            if (!tileData.ContainsKey(container) || (int)tileData[container] < (int)amount)
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
        foreach (var extraction in ((Dictionary<object, object>)extractionData).Values)
        {
            var container = ((Dictionary<object, object>)extraction)["container"];
            var amount = ((Dictionary<object, object>)extraction)["amount"];
            if (!tileData.ContainsKey(container))
            {
                tileData[container] = 0;
            }
            tileData[container] = (int)tileData[container] + (int)amount;
        }
    }
}
