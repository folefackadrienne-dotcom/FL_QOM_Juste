using KingdomOfGod.Buildings;

namespace KingdomOfGod.Grid
{
    public enum TerrainType
    {
        Plain,
        Desert,
        Hill,
        Mountain,
        Forest,
        River,
        Coast,
        Ruins
    }

    /// <summary>A single hex tile: its terrain, and whatever is built on it.</summary>
    public class HexCell
    {
        public HexCoordinates Coordinates { get; }
        public TerrainType Terrain { get; set; }
        public BuildingInstance Building { get; set; }
        public bool IsPassable => Terrain != TerrainType.Mountain;

        /// <summary>Set by TempleVisualController on the map center cell (0,0) — the Temple isn't a BuildingInstance (it's not placed by the player, doesn't come from BuildingData, and TempleSystem tracks its own level independently), so it can't block the cell just by occupying Building the way a normal building does.</summary>
        public bool IsReserved { get; set; }

        public HexCell(HexCoordinates coordinates, TerrainType terrain = TerrainType.Plain)
        {
            Coordinates = coordinates;
            Terrain = terrain;
        }

        public bool IsBuildable => Building == null && !IsReserved && Terrain != TerrainType.Mountain && Terrain != TerrainType.River;
    }
}
