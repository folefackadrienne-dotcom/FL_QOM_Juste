using System.Collections.Generic;
using UnityEngine;

namespace KingdomOfGod.Grid
{
    /// <summary>
    /// Owns the map of HexCoordinates -> HexCell. Used both for the territory/management
    /// view and, with a separate instance, for tactical battle grids.
    /// </summary>
    public class HexGrid : MonoBehaviour
    {
        [SerializeField] private int radius = 10;
        [SerializeField] private float hexSize = 1f;
        [SerializeField] private int seed = 12345;

        private readonly Dictionary<HexCoordinates, HexCell> cells = new Dictionary<HexCoordinates, HexCell>();

        public float HexSize => hexSize;
        public IReadOnlyDictionary<HexCoordinates, HexCell> Cells => cells;

        private void Awake()
        {
            GenerateHexagonalMap(radius);
        }

        public void GenerateHexagonalMap(int mapRadius)
        {
            cells.Clear();
            for (int q = -mapRadius; q <= mapRadius; q++)
            {
                int r1 = Mathf.Max(-mapRadius, -q - mapRadius);
                int r2 = Mathf.Min(mapRadius, -q + mapRadius);
                for (int r = r1; r <= r2; r++)
                {
                    var coords = new HexCoordinates(q, r);
                    cells[coords] = new HexCell(coords);
                }
            }

            TerrainGenerator.Apply(cells, seed, mapRadius);
        }

        public bool TryGetCell(HexCoordinates coords, out HexCell cell) => cells.TryGetValue(coords, out cell);

        public IEnumerable<HexCell> GetNeighbors(HexCoordinates coords)
        {
            for (int direction = 0; direction < 6; direction++)
            {
                if (cells.TryGetValue(coords.Neighbor(direction), out var neighbor))
                {
                    yield return neighbor;
                }
            }
        }

        /// <summary>Cells reachable within <paramref name="range"/> steps, ignoring passability (line-of-sight range, not pathing).</summary>
        public IEnumerable<HexCell> GetCellsInRange(HexCoordinates center, int range)
        {
            for (int q = -range; q <= range; q++)
            {
                for (int r = Mathf.Max(-range, -q - range); r <= Mathf.Min(range, -q + range); r++)
                {
                    var coords = center + new HexCoordinates(q, r);
                    if (cells.TryGetValue(coords, out var cell))
                    {
                        yield return cell;
                    }
                }
            }
        }
    }
}
