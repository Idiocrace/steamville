namespace TileGame.Types;

// Class which represents any ingame tile grid.
// Used because the old list method was slow and if you wanted to find an adjacent tile you had to iterate through the entire 256 tile map
// This makes tile lookup and management easier
public class TileGrid
{
    private readonly Dictionary<Vector2, Tile> tiles = [];

    public void PlaceTile(Tile tile, Vector2 position)
    {
        tiles[position] = tile;
    }

    public void RemoveTile(Vector2 position)
    {
        tiles.Remove(position);
    }

    public Tile? GetTile(Vector2 position)
    {
        tiles.TryGetValue(position, out var tile);
        return tile;
    }

    public void EditTile(Vector2 position, Tile newTile)
    {
        if (tiles.ContainsKey(position))
        {
            tiles[position] = newTile;
        }
    }

    public Dictionary<Vector2, Tile> GetAllTiles()
    {
        return tiles;
    }

    public Dictionary<Vector2, Tile> GetAdjacentTiles(Vector2 position)
    {
        // This gets the tiles adjacent to the given position (up, down, left, right) not accounting for rotation
        var adjacentPositions = new List<Vector2>
        {
            new Vector2(position.X - 1, position.Y),
            new Vector2(position.X + 1, position.Y),
            new Vector2(position.X, position.Y - 1),
            new Vector2(position.X, position.Y + 1)
        };

        var adjacentTiles = new Dictionary<Vector2, Tile>();
        foreach (var adjacentPosition in adjacentPositions)
        {
            if (tiles.TryGetValue(adjacentPosition, out var tile))
            {
                adjacentTiles[adjacentPosition] = tile;
            }
        }

        return adjacentTiles;
    }
}