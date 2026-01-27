namespace TileGame.Processors
{
    public class BaseProcessor : Processor
    {
        public static new readonly string ProcessorIDLiteral = "base";
        private static bool ProcessorLocked = false;

        // Implement Tick for game loop
        public virtual void Tick(TileGame.Types.Tile tile)
        {
            // Default does nothing, override in subclasses
        }

        public void Process(dynamic tile)
        {
            if (!ProcessorLocked) {
                ProcessorLocked = true;
                throw new System.InvalidOperationException("BaseProcessor.process() is not implemented yet.");
            }
        }
    }
}
