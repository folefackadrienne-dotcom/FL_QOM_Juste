using System;
using System.Collections.Generic;
using KingdomOfGod.Alliance;
using KingdomOfGod.Grid;
using KingdomOfGod.Population;
using KingdomOfGod.Resources;
using UnityEngine;

namespace KingdomOfGod.Buildings
{
    /// <summary>Validates and executes building placement on the territory HexGrid.</summary>
    public class BuildingManager : MonoBehaviour
    {
        [SerializeField] private HexGrid grid;
        [SerializeField] private ResourceManager resourceManager;
        [SerializeField] private AllianceSystem allianceSystem;
        [SerializeField] private PopulationSystem populationSystem;

        private readonly List<BuildingInstance> buildings = new List<BuildingInstance>();

        public IReadOnlyList<BuildingInstance> Buildings => buildings;
        public event Action<BuildingInstance> BuildingPlaced;

        public bool CanPlace(BuildingData data, HexCoordinates position)
        {
            if (!grid.TryGetCell(position, out var cell) || !cell.IsBuildable) return false;
            if (!resourceManager.CanAfford(data.buildCost)) return false;

            float faith = resourceManager.Get(ResourceType.Faith);
            float justice = resourceManager.Get(ResourceType.Justice);
            return data.MeetsRequirements(faith, justice);
        }

        public bool TryPlace(BuildingData data, HexCoordinates position, out BuildingInstance instance)
        {
            instance = null;
            if (!CanPlace(data, position)) return false;
            if (!resourceManager.TrySpend(data.buildCost)) return false;

            instance = new BuildingInstance(data, position);
            grid.TryGetCell(position, out var cell);
            cell.Building = instance;
            buildings.Add(instance);
            SpawnVisual(instance);

            foreach (var bonus in data.storageCapacityBonus)
            {
                resourceManager.SetCap(bonus.type, resourceManager.GetCap(bonus.type) + bonus.amount);
            }

            if (data.populationCapacityBonus != 0 && populationSystem != null)
            {
                populationSystem.IncreaseCapacity(data.populationCapacityBonus);
            }

            BuildingPlaced?.Invoke(instance);
            return true;
        }

        /// <summary>Instantiates BuildingData.prefab at the cell's world position — a deliberate no-op until a prefab is assigned (no building art exists yet).</summary>
        private void SpawnVisual(BuildingInstance instance)
        {
            if (instance.Data.prefab == null) return;

            var worldPosition = instance.Position.ToWorldPosition(grid.HexSize);
            Instantiate(instance.Data.prefab, worldPosition, Quaternion.identity, transform);
        }

        /// <summary>Applies every placed building's per-turn production to the resource pool, scaled by PopulationSystem.ProductionMultiplier (docs/Economy.md §3: loyalty raises or lowers output).</summary>
        public void ProcessTurnProduction()
        {
            float multiplier = populationSystem != null ? populationSystem.ProductionMultiplier : 1f;
            foreach (var building in buildings)
            {
                foreach (var production in building.Data.productionPerTurn)
                {
                    resourceManager.Add(production.type, production.amount * multiplier);
                }
            }
        }
    }
}
