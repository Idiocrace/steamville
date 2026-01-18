namespace TileGame.Processors;

public class BaseProcessor : Processor
{
    public static new readonly string ProcessorIDLiteral = "base";
    private static bool ProcessorLocked = false;
    public void Process(dynamic tile)
    {
        if (!ProcessorLocked) {
            ProcessorLocked = true;
            throw new InvalidOperationException("BaseProcessor.process() is not implemented yet.");
        }
    }
}
