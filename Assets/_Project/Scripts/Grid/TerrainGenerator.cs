using System.Collections.Generic;
using UnityEngine;

namespace KingdomOfGod.Grid
{
    /// <summary>
    /// Simple relief generator for a freshly built HexGrid — before this, HexGrid.GenerateHexagonalMap
    /// left every cell at its HexCell default (TerrainType.Plain), so none of HexGridRenderer's other
    /// 7 terrain colors/textures (including the 6 illustrations wired into TerrainTileSet) ever had a
    /// cell to appear on. Classifies each cell from four independent hand-rolled value-noise channels
    /// (elevation, moisture, river, coast — a lattice of random values bilinearly interpolated and
    /// smoothstepped, not Unity's Mathf.PerlinNoise, so the exact thresholds below could be calibrated
    /// against a Python simulation of the real distribution rather than guessed) plus a per-cell hash
    /// for sparse ruins. Deterministic for a given seed: HexGrid only calls this once per play session
    /// (it lives on the persistent Bootstrap GameManager, so Awake never re-fires on scene reload), and
    /// map layout isn't part of SaveData yet (no placed building is either — see SaveData.cs), so a
    /// fresh app launch with the same seed reproduces the same map, but nothing persists it across app
    /// restarts beyond that.
    /// </summary>
    public static class TerrainGenerator
    {
        // Thresholds below were calibrated by simulating this exact algorithm across 40
        // radius/seed combinations (5 and 10, 20 seeds each) so Mountain/Coast never dominate a
        // map and Plain always stays the largest single type — see the "no live Editor" note in
        // the project's validation discipline: numeric constants get simulated, not guessed.
        private const float CellSize = 1.8f;
        private const int LatticeSize = 50;

        private const float MountainThreshold = 0.831f;
        private const float HillThreshold = 0.693f;
        private const float EdgeRatioThreshold = 0.8f;
        private const float CoastNoiseThreshold = 0.4f;
        private const float DesertMoistureThreshold = 0.32f;
        private const float ForestMoistureThreshold = 0.68f;
        private const float RiverBandwidth = 0.03f;
        private const float RuinsChance = 0.02f;

        // Applied to every sample coordinate so cells near (0,0) don't all land in the noise
        // lattice's own origin corner.
        private const float SampleOffset = 1000f;

        public static void Apply(IDictionary<HexCoordinates, HexCell> cells, int seed, int mapRadius)
        {
            var elevationLattice = BuildLattice(seed);
            var moistureLattice = BuildLattice(seed + 1);
            var riverLattice = BuildLattice(seed + 2);
            var coastLattice = BuildLattice(seed + 3);
            var ruinsRandom = new System.Random(seed + 999);

            foreach (var pair in cells)
            {
                var coords = pair.Key;
                float x = coords.q + SampleOffset;
                float y = coords.r + SampleOffset;

                float elevation = ValueNoise(x, y, elevationLattice, CellSize);
                float moisture = ValueNoise(x, y, moistureLattice, CellSize * 0.8f);
                float river = ValueNoise(x, y, riverLattice, CellSize * 0.6f);
                float coast = ValueNoise(x, y, coastLattice, CellSize * 0.7f);
                float edgeRatio = mapRadius > 0 ? coords.DistanceTo(new HexCoordinates(0, 0)) / (float)mapRadius : 0f;

                // MissionBattleSetup.SpawnEdge places every Battle unit on the q == ±mapRadius
                // column — Mountain is the only TerrainType BattleGrid treats as impassable
                // (HexCell.IsPassable), so keeping it off that exact column guarantees no unit
                // ever spawns on terrain it could never move off of. The map center (0,0) gets
                // the same guard: it's the one hardcoded VictoryConditionType.CapturePoint
                // coordinate in the mission set (Mission_Age5_03_DavidRoiAHebronPuisJerusalem),
                // and Mountain there would make that mission's objective permanently unreachable.
                bool isSpawnEdgeColumn = Mathf.Abs(coords.q) >= mapRadius;
                bool isMapCenter = coords.q == 0 && coords.r == 0;

                TerrainType terrain;
                if (elevation > MountainThreshold && !isSpawnEdgeColumn && !isMapCenter) terrain = TerrainType.Mountain;
                else if (elevation > HillThreshold) terrain = TerrainType.Hill;
                else if (edgeRatio > EdgeRatioThreshold && coast > CoastNoiseThreshold) terrain = TerrainType.Coast;
                else if (Mathf.Abs(river - 0.5f) < RiverBandwidth) terrain = TerrainType.River;
                else if (moisture < DesertMoistureThreshold) terrain = TerrainType.Desert;
                else if (moisture > ForestMoistureThreshold) terrain = TerrainType.Forest;
                else terrain = TerrainType.Plain;

                if ((terrain == TerrainType.Plain || terrain == TerrainType.Hill) && ruinsRandom.NextDouble() < RuinsChance)
                {
                    terrain = TerrainType.Ruins;
                }

                pair.Value.Terrain = terrain;
            }
        }

        private static float[,] BuildLattice(int seed)
        {
            var random = new System.Random(seed);
            var lattice = new float[LatticeSize, LatticeSize];
            for (int i = 0; i < LatticeSize; i++)
            {
                for (int j = 0; j < LatticeSize; j++)
                {
                    lattice[i, j] = (float)random.NextDouble();
                }
            }
            return lattice;
        }

        /// <summary>Bilinear interpolation of a wrapped random lattice, smoothstepped so the result is continuous — the classic "value noise" construction, deliberately not Mathf.PerlinNoise so its output distribution is fully known from the calibration simulation above.</summary>
        private static float ValueNoise(float x, float y, float[,] lattice, float cellSize)
        {
            float gx = x / cellSize;
            float gy = y / cellSize;
            int ix = Mathf.FloorToInt(gx);
            int iy = Mathf.FloorToInt(gy);
            float fx = gx - ix;
            float fy = gy - iy;

            float v00 = SampleLattice(lattice, ix, iy);
            float v10 = SampleLattice(lattice, ix + 1, iy);
            float v01 = SampleLattice(lattice, ix, iy + 1);
            float v11 = SampleLattice(lattice, ix + 1, iy + 1);

            float sx = Smooth(fx);
            float sy = Smooth(fy);
            float a = Mathf.Lerp(v00, v10, sx);
            float b = Mathf.Lerp(v01, v11, sx);
            return Mathf.Lerp(a, b, sy);
        }

        private static float SampleLattice(float[,] lattice, int x, int y)
        {
            int size = lattice.GetLength(0);
            int xi = ((x % size) + size) % size;
            int yi = ((y % size) + size) % size;
            return lattice[xi, yi];
        }

        private static float Smooth(float t) => t * t * (3f - 2f * t);
    }
}
