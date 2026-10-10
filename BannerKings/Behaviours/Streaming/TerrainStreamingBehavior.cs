using System.Collections.Generic;
using System.Diagnostics;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace BannerKings.Behaviours.Streaming
{
    /// <summary>
    /// Streams the tiles of a <see cref="StreamingRegion"/> in and out of the mission scene
    /// based on the main agent's position. Tiles are regular prefabs, so this only relies on
    /// GameEntity.Instantiate / Remove, which the engine already exposes to C#.
    /// </summary>
    public class TerrainStreamingBehavior : MissionLogic
    {
        // Arbitrary id passed to GameEntity.Remove, shows up in engine logs as the remove reason.
        private const int RemoveReason = 7310;

        // Max tiles instantiated per frame once the mission is running, spreads hitches over frames.
        private const int MaxLoadsPerTick = 1;

        private readonly StreamingRegion region;
        private readonly Dictionary<(int x, int y), GameEntity> loadedTiles = new();
        private readonly HashSet<(int x, int y)> navMeshImported = new();
        private readonly List<(int x, int y)> toLoad = new();
        private readonly List<(int x, int y)> toUnload = new();
        private bool initialLoadDone;

        public TerrainStreamingBehavior(StreamingRegion region)
        {
            this.region = region;
        }

        public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

        public static bool DebugMessages { get; set; } = true;

        public int LoadedTileCount => loadedTiles.Count;

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            Agent main = Agent.Main;
            if (main == null || !main.IsActive())
            {
                return;
            }

            Vec2 position = main.Position.AsVec2;
            CollectChanges(position);

            foreach (var tile in toUnload)
            {
                UnloadTile(tile);
            }

            // The first load happens all at once so the player never spawns above missing ground.
            int budget = initialLoadDone ? MaxLoadsPerTick : int.MaxValue;
            for (int i = 0; i < toLoad.Count && i < budget; i++)
            {
                LoadTile(toLoad[i]);
            }

            initialLoadDone = true;
        }

        private void CollectChanges(Vec2 position)
        {
            toLoad.Clear();
            toUnload.Clear();

            for (int x = 0; x < region.TilesX; x++)
            {
                for (int y = 0; y < region.TilesY; y++)
                {
                    float distance = region.DistanceToTile(x, y, position);
                    bool loaded = loadedTiles.ContainsKey((x, y));
                    if (!loaded && distance <= region.LoadRadius)
                    {
                        toLoad.Add((x, y));
                    }
                    else if (loaded && distance > region.UnloadRadius)
                    {
                        toUnload.Add((x, y));
                    }
                }
            }

            // Closest tiles first, so the ground under the player is never the one waiting on the budget.
            toLoad.Sort((a, b) => region.DistanceToTile(a.x, a.y, position)
                .CompareTo(region.DistanceToTile(b.x, b.y, position)));
        }

        private void LoadTile((int x, int y) tile)
        {
            Scene scene = Mission.Scene;
            string prefab = region.GetTilePrefab(tile.x, tile.y);
            var timer = Stopwatch.StartNew();

            GameEntity entity = GameEntity.Instantiate(scene, prefab, true);
            if (entity == null)
            {
                Log($"Streaming: prefab {prefab} not found", Colors.Red);
                // Mark as loaded anyway so a missing prefab is not retried every frame.
                loadedTiles[tile] = null;
                return;
            }

            Vec2 corner = region.GetTileCorner(tile.x, tile.y);
            MatrixFrame frame = MatrixFrame.Identity;
            frame.origin = new Vec3(corner.x, corner.y, 0f);
            entity.SetFrame(ref frame);
            loadedTiles[tile] = entity;

            // Navmesh is imported once per tile and kept: there is no exposed call to remove imported
            // navmesh faces, and keeping it is cheap compared to the render/physics data of the tile.
            string navMesh = region.GetNavMeshPrefab(tile.x, tile.y);
            if (navMesh != null && navMeshImported.Add(tile))
            {
                scene.ImportNavigationMeshPrefab(navMesh, GetNavMeshGroupShift(tile));
            }

            timer.Stop();
            Log($"Streaming: loaded {prefab} in {timer.ElapsedMilliseconds}ms ({loadedTiles.Count} tiles)", Colors.Green);
        }

        private void UnloadTile((int x, int y) tile)
        {
            if (loadedTiles.TryGetValue(tile, out GameEntity entity) && entity != null)
            {
                var timer = Stopwatch.StartNew();
                entity.Remove(RemoveReason);
                timer.Stop();
                Log($"Streaming: unloaded {region.GetTilePrefab(tile.x, tile.y)} in {timer.ElapsedMilliseconds}ms", Colors.Yellow);
            }

            loadedTiles.Remove(tile);
        }

        // Each tile's navmesh face group ids are shifted into their own range so tiles do not collide.
        private int GetNavMeshGroupShift((int x, int y) tile) => (tile.y * region.TilesX + tile.x + 1) * 1000;

        private static void Log(string text, Color color)
        {
            if (DebugMessages)
            {
                InformationManager.DisplayMessage(new InformationMessage(text, color));
            }
        }
    }
}
