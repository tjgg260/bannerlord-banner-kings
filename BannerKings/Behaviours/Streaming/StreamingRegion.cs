using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Library;

namespace BannerKings.Behaviours.Streaming
{
    /// <summary>
    /// A grid of terrain tiles that is streamed in and out around the player while a mission
    /// runs in <see cref="SceneName"/>. Each tile is a prefab (static mesh + collision body)
    /// authored in the Modding Kit and named with <see cref="TilePrefabFormat"/>.
    /// </summary>
    public class StreamingRegion
    {
        public StreamingRegion(string sceneName, string tilePrefabFormat, int tilesX, int tilesY, float tileSize,
            Vec2 origin, float loadRadius, float unloadRadius, string navMeshPrefabFormat = null)
        {
            SceneName = sceneName;
            TilePrefabFormat = tilePrefabFormat;
            TilesX = tilesX;
            TilesY = tilesY;
            TileSize = tileSize;
            Origin = origin;
            LoadRadius = loadRadius;
            UnloadRadius = unloadRadius;
            NavMeshPrefabFormat = navMeshPrefabFormat;
        }

        // Mission scene the region is active in. Usually a tiny base scene that only holds the spawn points.
        public string SceneName { get; }

        // string.Format pattern for tile prefab ids, receives (x, y). e.g. "bk_tile_{0}_{1}"
        public string TilePrefabFormat { get; }

        public int TilesX { get; }
        public int TilesY { get; }

        // Edge length of one square tile in meters.
        public float TileSize { get; }

        // World position of the south-west corner of tile (0, 0).
        public Vec2 Origin { get; }

        // Tiles whose closest edge is within LoadRadius of the player are loaded,
        // and only unloaded once beyond UnloadRadius, so walking along a border does not thrash.
        public float LoadRadius { get; }
        public float UnloadRadius { get; }

        // Optional string.Format pattern for per-tile navmesh prefabs, receives (x, y).
        // Null disables navmesh import, agents will then only be able to walk on the base scene navmesh.
        public string NavMeshPrefabFormat { get; }

        public string GetTilePrefab(int x, int y) => string.Format(TilePrefabFormat, x, y);

        public string GetNavMeshPrefab(int x, int y) => NavMeshPrefabFormat == null ? null : string.Format(NavMeshPrefabFormat, x, y);

        public Vec2 GetTileCorner(int x, int y) => new Vec2(Origin.x + x * TileSize, Origin.y + y * TileSize);

        public float DistanceToTile(int x, int y, Vec2 position)
        {
            Vec2 min = GetTileCorner(x, y);
            float dx = MathF.Max(0f, MathF.Max(min.x - position.x, position.x - (min.x + TileSize)));
            float dy = MathF.Max(0f, MathF.Max(min.y - position.y, position.y - (min.y + TileSize)));
            return MathF.Sqrt(dx * dx + dy * dy);
        }
    }

    public static class StreamingRegions
    {
        // Prototype test region: a 4x4 grid of 500m tiles. Build a scene called "bk_stream_test"
        // with a spawn point near (1000, 1000) and tile prefabs bk_tile_0_0 ... bk_tile_3_3.
        public static readonly List<StreamingRegion> All = new()
        {
            new StreamingRegion("bk_stream_test", "bk_tile_{0}_{1}", 4, 4, 500f,
                Vec2.Zero, 600f, 900f, "bk_tile_navmesh_{0}_{1}")
        };

        public static StreamingRegion GetForScene(string sceneName) => All.FirstOrDefault(r => r.SceneName == sceneName);
    }
}
