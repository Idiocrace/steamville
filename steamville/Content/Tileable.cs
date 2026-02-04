using Crucible.Types;

namespace SteamVille.Content;

public class Tileable : Entity
{
    // Processor stuff
    // public readonly TileProcessor? Processor = null;
    // public readonly string ProcessorID = Processor.ID;
    // Stupid bullshit constructors so it actually works
    public Tileable(string id, Vector2f boundingBox) : base(id, boundingBox) { }
    public Tileable(string id, Vector2f boundingBox, Vector3f position) : base(id, boundingBox, position) { }

    // Check if this tileable is on a valid tile position (i.e., aligned to the grid)
    public bool IsOnValidTilePosition()
    {
        if (Position == null)
        {
            return false; // Reference objects are not placed in the world
        }

        // Check if X and Y are whole numbers
        bool isXValid = Position.X % 1 == 0;
        bool isYValid = Position.Y % 1 == 0;

        return isXValid && isYValid;
    }

    public override bool OnPlace(Map map, Vector3f position)
    {
        if (!IsOnValidTilePosition())
        {
            return false; // Invalid placement
        }

        return base.OnPlace(map, position);
    }

    public override void OnUpdate(Map map, float deltaTime)
    {
        // Will eventually make calls to the processor here

        
    }
}