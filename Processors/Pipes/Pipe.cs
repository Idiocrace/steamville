// Typeclass for a pipe connection
using TileGame.Types;

namespace TileGame.Processors.Pipes;

public record Pipe(Container From, Container To)
{
    public bool Transfer(Resource item, int amount)
    {
        // Remove the item from the source container
        if (From.CanRemoveResource(item, amount) && To.CanAddResource(item, amount))
        {
            From.RemoveResource(item, amount);
            To.AddResource(item, amount);
        }
        else
        {
            return false;
        }
        return true;
    }

    public void ProcessPipe()
    {
        // Add the item to the destination container
        foreach (KeyValuePair<Resource, int> content in From.Contents)
        {
            if (To.Filter != null && !To.Filter.Contains(content.Key.ID))
                continue;
            Transfer(content.Key, content.Value);
        }
    }
}