using System.Collections.Generic;
using System.Linq;
using KingdomOfGod.Grid;
using UnityEngine;

namespace KingdomOfGod.Battle
{
    /// <summary>A dedicated hex grid for a single mission's tactical battle, tracking unit occupancy and terrain bonuses.</summary>
    public class BattleGrid : MonoBehaviour
    {
        [SerializeField] private HexGrid grid;

        private readonly Dictionary<HexCoordinates, UnitInstance> occupants = new Dictionary<HexCoordinates, UnitInstance>();

        public HexGrid Grid => grid;

        public bool IsOccupied(HexCoordinates coords) => occupants.ContainsKey(coords);

        public UnitInstance GetUnitAt(HexCoordinates coords) => occupants.TryGetValue(coords, out var unit) ? unit : null;

        public void PlaceUnit(UnitInstance unit, HexCoordinates coords)
        {
            occupants.Remove(unit.Position);
            occupants[coords] = unit;
            unit.Position = coords;
        }

        public void RemoveUnit(UnitInstance unit) => occupants.Remove(unit.Position);

        /// <summary>Defense terrain bonus applied to units standing on the given cell (e.g. hills, walls).</summary>
        public int GetTerrainDefenseBonus(HexCoordinates coords)
        {
            if (!grid.TryGetCell(coords, out var cell)) return 0;

            return cell.Terrain switch
            {
                TerrainType.Hill => 2,
                TerrainType.Forest => 1,
                TerrainType.Ruins => 1,
                _ => 0
            };
        }

        public IEnumerable<HexCell> GetReachableCells(UnitInstance unit)
        {
            return grid.GetCellsInRange(unit.Position, unit.Data.movement)
                .Where(cell => cell.IsPassable && !IsOccupied(cell.Coordinates));
        }

        public IEnumerable<UnitInstance> GetUnitsInRange(HexCoordinates center, int range)
        {
            return grid.GetCellsInRange(center, range)
                .Select(cell => GetUnitAt(cell.Coordinates))
                .Where(unit => unit != null);
        }
    }
}
