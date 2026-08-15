using System;
using UnityEngine;

namespace KingdomOfGod.Population
{
    /// <summary>
    /// Total population and loyalty (0-100%). Low loyalty triggers murmurs/rebellions; high
    /// loyalty boosts production. Population/Loyalty change over time via KingdomTurnManager,
    /// which each turn spends WheatUpkeep/WaterUpkeep against ResourceManager (a shortage lowers
    /// Loyalty instead — "pénurie = murmures et baisse de loyauté", docs/Economy.md §3) and, when
    /// fed, grows Population by ComputeGrowth() up to Capacity (raised by BuildingData.
    /// populationCapacityBonus on Habitat buildings via BuildingManager.TryPlace).
    /// </summary>
    public class PopulationSystem : MonoBehaviour
    {
        [SerializeField] private int startingPopulation = 100;
        [SerializeField] private float startingLoyalty = 60f;
        [SerializeField] private float murmurThreshold = 30f;
        [SerializeField] private float rebellionThreshold = 10f;
        [SerializeField] private float productionBonusThreshold = 80f;

        [Header("Per-turn upkeep (docs/Economy.md §3: \"Chaque habitant consomme du Blé et de l'Eau\") — 0.05 validated by simulation against the 39 authored BuildingData.productionPerTurn values: only 3 Wheat- and 3 Water-producing buildings exist across all 7 ages (~17/turn each at full roster), so a higher per-capita rate outgrows food supply once population nears housing capacity and Loyalty never recovers")]
        [SerializeField] private float wheatPerCapita = 0.05f;
        [SerializeField] private float waterPerCapita = 0.05f;

        [Header("Growth & housing")]
        [SerializeField] private float growthRate = 0.02f;
        [SerializeField] private int baseCapacity = 100;

        public int Population { get; private set; }
        public float Loyalty { get; private set; }
        public int Capacity { get; private set; }

        public event Action LoyaltyLow;
        public event Action LoyaltyCritical;
        public event Action<float> LoyaltyChanged;
        public event Action<int> PopulationChanged;

        public float WheatUpkeep => Population * wheatPerCapita;
        public float WaterUpkeep => Population * waterPerCapita;

        /// <summary>
        /// docs/Economy.md §3: "Haute loyauté → +20 à +40% de production ; Basse loyauté → grèves,
        /// sabotage, baisse de production, risque de révolte" — a simple banded formula off the
        /// same thresholds already driving LoyaltyLow/LoyaltyCritical, rather than a separate
        /// invented scale.
        /// </summary>
        public float ProductionMultiplier
        {
            get
            {
                if (Loyalty <= rebellionThreshold) return 0.5f;
                if (Loyalty <= murmurThreshold) return 0.7f;
                if (Loyalty >= productionBonusThreshold)
                {
                    return Mathf.Lerp(1.2f, 1.4f, Mathf.InverseLerp(productionBonusThreshold, 100f, Loyalty));
                }
                return 1f;
            }
        }

        private void Awake()
        {
            Population = startingPopulation;
            Loyalty = startingLoyalty;
            Capacity = baseCapacity;
        }

        public void ModifyLoyalty(float delta)
        {
            Loyalty = Mathf.Clamp(Loyalty + delta, 0f, 100f);
            LoyaltyChanged?.Invoke(Loyalty);

            if (Loyalty <= rebellionThreshold) LoyaltyCritical?.Invoke();
            else if (Loyalty <= murmurThreshold) LoyaltyLow?.Invoke();
        }

        public void Grow(int amount)
        {
            Population = Mathf.Max(0, Population + amount);
            PopulationChanged?.Invoke(Population);
        }

        /// <summary>How many people would join this turn if fed: a small percentage of the current population, held back below murmurThreshold and capped by remaining housing Capacity.</summary>
        public int ComputeGrowth()
        {
            if (Loyalty <= murmurThreshold) return 0;

            int headroom = Mathf.Max(0, Capacity - Population);
            int growth = Mathf.FloorToInt(Population * growthRate);
            return Mathf.Min(growth, headroom);
        }

        /// <summary>Applied once at placement by BuildingManager.TryPlace, mirroring ResourceManager.SetCap for BuildingData.storageCapacityBonus.</summary>
        public void IncreaseCapacity(int amount)
        {
            Capacity += amount;
        }
    }
}
