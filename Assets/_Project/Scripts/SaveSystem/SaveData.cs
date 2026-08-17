using System;
using System.Collections.Generic;
using KingdomOfGod.Monetization;

namespace KingdomOfGod.SaveSystem
{
    /// <summary>A single placed building: which BuildingData (by displayName) at which hex cell, and its level.</summary>
    [Serializable]
    public class PlacedBuildingSave
    {
        public string buildingName;
        public int q;
        public int r;
        public int level;
    }

    /// <summary>Plain-data snapshot of everything needed to resume a game, serialized to JSON.</summary>
    [Serializable]
    public class SaveData
    {
        public string saveVersion = "0.1";
        public int currentAge;
        public List<int> unlockedAges = new List<int>();

        public EntitlementTier entitlementTier = EntitlementTier.Free;
        public List<string> ownedProductIds = new List<string>();

        public List<string> resourceTypes = new List<string>();
        public List<float> resourceValues = new List<float>();

        public float allianceValue;
        public int templeLevel;
        public int population;
        public float loyalty;

        public List<string> memorizedVerseIds = new List<string>();
        public List<string> ownedArtifactIds = new List<string>();
        public List<string> completedMissionIds = new List<string>();
        public List<string> unlockedTechIds = new List<string>();

        public List<PlacedBuildingSave> placedBuildings = new List<PlacedBuildingSave>();

        public List<string> unlockedLeaderIds = new List<string>();
        public string activeLeaderId = "";

        public List<string> usedOnceMiracleIds = new List<string>();

        public DateTime savedAtUtc;
    }
}
