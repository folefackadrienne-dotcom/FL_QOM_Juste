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
    /// it instead ("pénurie = murmures et baisse de loyauté", docs/Economy.md §3) — and finally
    /// governance: "La Justice et la Foi sont les meilleurs moyens de maintenir une haute loyauté"
    /// (docs/Economy.md §3), a second, independent Loyalty source on top of being fed.
    /// </summary>
    public class KingdomTurnManager : MonoBehaviour
    {
        [SerializeField] private BuildingManager buildingManager;
        [SerializeField] private ResourceManager resourceManager;
        [SerializeField] private PopulationSystem populationSystem;

        [SerializeField] private float shortageLoyaltyPenalty = 5f;
        [SerializeField] private float wellFedLoyaltyBonus = 2f;

        [Header("Governance (docs/Economy.md §3 & §7: Justice/Foi maintain Loyalty)")]
        [SerializeField] private float devoutJusticePerCapita = 0.5f;
        [SerializeField] private float devoutFaithPerCapita = 0.5f;
        [SerializeField] private float devoutLoyaltyBonus = 1f;

        public int TurnNumber { get; private set; } = 1;

        public event Action<int> TurnAdvanced;

        public void EndTurn()
        {
            buildingManager.ProcessTurnProduction();
            ApplyPopulationUpkeep();
            ApplyGovernanceLoyalty();

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

        /// <summary>
        /// A kingdom with abundant Justice and Faith relative to its own population — not merely
        /// fed, but justly and faithfully governed — earns a further Loyalty bonus, independent of
        /// (and additive with) ApplyPopulationUpkeep's. Deliberately one-sided (no penalty for
        /// lacking Justice/Faith): Justice-producing buildings don't unlock until Age 2, so a
        /// symmetric penalty would punish every player for a shortfall the Age system itself makes
        /// unavoidable early on, not a real choice. The Wheat/Water shortage penalty already covers
        /// outright neglect; this only ever adds on top of it.
        /// </summary>
        private void ApplyGovernanceLoyalty()
        {
            if (populationSystem.Population <= 0) return;

            float justicePerCapita = resourceManager.Get(ResourceType.Justice) / populationSystem.Population;
            float faithPerCapita = resourceManager.Get(ResourceType.Faith) / populationSystem.Population;

            if (justicePerCapita >= devoutJusticePerCapita && faithPerCapita >= devoutFaithPerCapita)
            {
                populationSystem.ModifyLoyalty(devoutLoyaltyBonus);
            }
        }
    }
}
