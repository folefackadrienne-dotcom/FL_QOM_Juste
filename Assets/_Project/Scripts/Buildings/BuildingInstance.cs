using KingdomOfGod.Grid;

namespace KingdomOfGod.Buildings
{
    /// <summary>A placed building: its data plus runtime state (level, position).</summary>
    public class BuildingInstance
    {
        public BuildingData Data { get; }
        public HexCoordinates Position { get; }
        public int Level { get; private set; } = 1;

        public BuildingInstance(BuildingData data, HexCoordinates position)
        {
            Data = data;
            Position = position;
        }

        public void SetLevel(int level) => Level = level;
    }
}
