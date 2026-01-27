namespace TileGame.Types;

public class ResourceNode
{
    public Resource ResourceType { get; set; }
    public int Richness { get; set; }

    public ResourceNode(Resource resource, int richness)
    {
        ResourceType = resource;
        Richness = richness;
    }
}
