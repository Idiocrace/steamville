using System.Collections.Generic;
using SFML.System;

namespace TileGame.Types;

public class ResourceGrid
{
    private readonly Dictionary<Vector2i, ResourceNode> nodes = new();

    public void AddNode(Vector2i pos, ResourceNode node)
    {
        nodes[pos] = node;
    }

    public ResourceNode? GetNode(Vector2i pos)
    {
        nodes.TryGetValue(pos, out var node);
        return node;
    }

    public IReadOnlyDictionary<Vector2i, ResourceNode> GetAllNodes() => nodes;

    public void RemoveNode(Vector2i pos)
    {
        nodes.Remove(pos);
    }
}
