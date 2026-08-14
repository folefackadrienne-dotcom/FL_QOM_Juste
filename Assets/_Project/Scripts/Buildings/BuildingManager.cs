using System;
using System.Collections.Generic;
using KingdomOfGod.Alliance;
using KingdomOfGod.Grid;
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

            foreach (var bonus in data.storageCapacityBonus)
            {
                resourceManager.SetCap(bonus.type, resourceManager.GetCap(bonus.type) + bonus.amount);
            }

            BuildingPlaced?.Invoke(instance);
            return true;
        }

        /// <summary>Applies every placed building's per-turn production to the resource pool.</summary>
        public void ProcessTurnProduction()
        {
            foreach (var building in buildings)
            {
                foreach (var production in building.Data.productionPerTurn)
                {
                    resourceManager.Add(production.type, production.amount);
                }
            }
        }
    }
}
