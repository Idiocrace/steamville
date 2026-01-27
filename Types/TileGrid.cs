using System.Collections.Generic;

namespace TileGame.Types
{
    public class TileGrid
    {
        // IMPORTANT: Explicit type, not [] shorthand (can break older compilers)
        private readonly Dictionary<Vector2, Tile> tiles = new();

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
                tiles[position] = newTile;
        }

        // Return IReadOnlyDictionary to prevent accidental modification
        public IReadOnlyDictionary<Vector2, Tile> GetAllTiles()
        {
            return tiles;
        }

        public Dictionary<Vector2, Tile> GetAdjacentTiles(Vector2 position)
        {
            var adjacentTiles = new Dictionary<Vector2, Tile>();

            Vector2[] offsets =
            {
                new Vector2(position.X - 1, position.Y),
                new Vector2(position.X + 1, position.Y),
                new Vector2(position.X, position.Y - 1),
                new Vector2(position.X, position.Y + 1)
            };

            foreach (var pos in offsets)
            {
                if (tiles.TryGetValue(pos, out var tile))
                    adjacentTiles[pos] = tile;
            }

            return adjacentTiles;
        }
    }
}
