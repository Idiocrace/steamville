using TileGame.Types;
using SFML.System;

namespace TileGame.Processors
{
    public static class ExtractorProcessor
    {
        public static void Process(Tile tile)
        {
            // Use the "output" container by default
            if (!tile.TileContainers.TryGetValue("output", out var container))
            {
                GameRender.AddLog($"[Extractor @ {tile.GridPosition.X},{tile.GridPosition.Y}] ❌ Missing output container.");
                return;
            }

            Vector2i pos = tile.GridPosition;

            var node = GameRender.ResourceGrid.GetNode(pos);
            if (node == null)
            {
                GameRender.AddLog($"[Extractor @ {pos.X},{pos.Y}] ❌ No resource under extractor.");
                return;
            }

            if (node.Richness <= 0)
            {
                GameRender.AddLog($"[Extractor @ {pos.X},{pos.Y}] 💀 Resource depleted.");
                GameRender.ResourceGrid.RemoveNode(pos);
                return;
            }

            var resource = node.ResourceType;
            int amount = 1;

            GameRender.AddLog($"[Extractor @ {pos.X},{pos.Y}] Attempting to mine {resource.Name}");

            if (!container.CanAddResource(resource, amount))
            {
                GameRender.AddLog("  ⚠ Container full.");
                return;
            }

            container.AddResource(resource, amount);
            node.Richness -= amount;

            GameRender.AddLog($"  ⛏ Mined {amount} {resource.Name}");

            GameRender.AddLog($"  🪨 Remaining in ground: {node.Richness}");
        }
    }
}
