using System;
using KingdomOfGod.Buildings;
using KingdomOfGod.Population;
using KingdomOfGod.Resources;
using UnityEngine;

namespace KingdomOfGod.Core
{
    /// <summary>
    /// The missing heartbeat of the Kingdom scene: BuildingManager.ProcessTurnProduction,
    /// PopulationSystem.Grow and the Loyalty consequence of a food/water shortage all existed as
    /// real methods with nothing ever calling them — Kingdom had no notion of a turn at all, and
    /// PopulationSystem.ModifyLoyalty had no caller anywhere with a positive delta either, which a
    /// simulation showed turns any shortage into a one-way ratchet to permanent 0 Loyalty. EndTurn()
    /// (bound to the HUD's "Fin de Tour" button) is the fix: building production lands first so
    /// this turn's Blé/Or/etc. are available to pay upkeep from, then population upkeep — fed grows
    /// Population via PopulationSystem.ComputeGrowth and nudges Loyalty back up, a shortfall lowers
    /// it instead ("pénurie = murmures et baisse de loyauté", docs/Economy.md §3).
    /// </summary>
    public class KingdomTurnManager : MonoBehaviour
    {
        [SerializeField] private BuildingManager buildingManager;
        [SerializeField] private ResourceManager resourceManager;
        [SerializeField] private PopulationSystem populationSystem;

        [SerializeField] private float shortageLoyaltyPenalty = 5f;
        [SerializeField] private float wellFedLoyaltyBonus = 2f;

        public int TurnNumber { get; private set; } = 1;

        public event Action<int> TurnAdvanced;

        public void EndTurn()
        {
            buildingManager.ProcessTurnProduction();
            ApplyPopulationUpkeep();

            TurnNumber++;
            TurnAdvanced?.Invoke(TurnNumber);
        }

        private void ApplyPopulationUpkeep()
        {
            var upkeep = new[]
            {
                new ResourceAmount { type = ResourceType.Wheat, amount = populationSystem.WheatUpkeep },
                new ResourceAmount { type = ResourceType.Water, amount = populationSystem.WaterUpkeep }
            };

            if (resourceManager.TrySpend(upkeep))
            {
                populationSystem.ModifyLoyalty(wellFedLoyaltyBonus);
                populationSystem.Grow(populationSystem.ComputeGrowth());
            }
            else
            {
                populationSystem.ModifyLoyalty(-shortageLoyaltyPenalty);
            }
        }
    }
}
