namespace Crucible.Types;

// Class which represents any ingame map
public class Map
{
    private readonly Dictionary<Vector2f, Entity> entities = [];
    private Vector2 bounds = new(100, 100); // Default bounds, can be changed later

    public void PlaceEntity(Entity entity, Vector2f position)
    {
        // Check if position is within bounds
        if (position.X < 0 || position.X >= bounds.X || position.Y < 0 || position.Y >= bounds.Y)
        {
            throw new ArgumentOutOfRangeException($"Position {position} is out of bounds {bounds}.");
        }
        
        entities[position] = entity;
    }

    public void RemoveEntity(Vector2f position)
    {
        entities.Remove(position);
    }

    public Entity? GetEntity(Vector2f position)
    {
        entities.TryGetValue(position, out var entity);
        return entity;
    }

    public void EditEntity(Vector2f position, Entity newEntity)
    {
        if (entities.ContainsKey(position))
        {
            entities[position] = newEntity;
        }
    }

    public Dictionary<Vector2f, Entity> GetAllEntities()
    {
        return entities;
    }

}