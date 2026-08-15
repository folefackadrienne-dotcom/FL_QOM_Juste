using System.Collections.Generic;
using KingdomOfGod.Core;
using KingdomOfGod.Resources;
using UnityEngine;

namespace KingdomOfGod.Buildings
{
    /// <summary>Static definition of a building type (design-time data, one asset per building).</summary>
    [CreateAssetMenu(fileName = "Building_", menuName = "Kingdom of God/Building", order = 10)]
    public class BuildingData : ScriptableObject
    {
        public string displayName;
        [TextArea] public string description;
        public BuildingCategory category;
        public Age introducedInAge;
        public GameObject prefab;
        public Sprite icon;

        [Header("Cost & Requirements")]
        public List<ResourceAmount> buildCost = new List<ResourceAmount>();
        public float minFaithRequired;
        public float minJusticeRequired;

        [Header("Production (per turn, optional)")]
        public List<ResourceAmount> productionPerTurn = new List<ResourceAmount>();

        [Header("Storage (optional, applied once at placement)")]
        public List<ResourceAmount> storageCapacityBonus = new List<ResourceAmount>();

        [Header("Housing (optional, applied once at placement — PopulationSystem.IncreaseCapacity)")]
        public int populationCapacityBonus;

        public bool MeetsRequirements(float currentFaith, float currentJustice)
        {
            return currentFaith >= minFaithRequired && currentJustice >= minJusticeRequired;
        }
    }
}
