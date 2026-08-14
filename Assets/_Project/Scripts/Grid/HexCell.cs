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

        public HexCell(HexCoordinates coordinates, TerrainType terrain = TerrainType.Plain)
        {
            Coordinates = coordinates;
            Terrain = terrain;
        }

        public bool IsBuildable => Building == null && Terrain != TerrainType.Mountain && Terrain != TerrainType.River;
    }
}
