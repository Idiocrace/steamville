namespace Crucible.Types;

// Tile class
public abstract class Entity
{
    /* ====== Core Entity Members ====== */
    public required string ID { get; set; }
    public required Vector2f BoundingBox { get; set; } = new Vector2f(1f, 1f); // width,height
    public required Vector3f? Position { get; set; } = new Vector3f(0f, 0f, 0f); // x,y,rotation (whole numbers 0-3 for quarter turns
    public readonly bool ReferenceObject = false; // Whether this tile is a reference object (not placed in the world, just used for reference)

    // Ref. object override: Initialize with only ID and defaulted bounding box
    public Entity(string id, Vector2f boundingBox = null!)
    {
        ID = id;
        if (boundingBox == null)
        {
            boundingBox = new Vector2f(1f, 1f);
        }
        BoundingBox = boundingBox;
        Position = null;
        ReferenceObject = true;
    }

    // Full instance override: Initialize with all parameters
    public Entity(string id, Vector2f boundingBox, Vector3f position)
    {
        ID = id;
        BoundingBox = boundingBox;
        Position = position;
        ReferenceObject = false;
    }

    /* ====== Overridable Members ====== */

    // Called when the entity is placed in the world
    // Return bool: whether placement was successful
    public virtual bool OnPlace(Map map, Vector3f position) { return true; }

    // Called when the entity is removed from the world
    // Return bool: whether removal was successful
    public virtual bool OnRemove(Map map, Vector3f position) { return true; }

    // Called every time the entity is updated whether it be in the main cycle or not
    public virtual void OnUpdate(Map map, float deltaTime) { }

}