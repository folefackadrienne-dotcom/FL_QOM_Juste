using System;
using System.Collections.Generic;
using KingdomOfGod.Alliance;
using KingdomOfGod.Grid;
using KingdomOfGod.Population;
using KingdomOfGod.Resources;
using KingdomOfGod.UI;
using TMPro;
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
        [SerializeField] private UIThemeData theme;

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

        /// <summary>
        /// Instantiates BuildingData.prefab at the cell's world position — or, until real building
        /// art exists (none of the 39 BuildingData assets have a prefab assigned), a code-generated
        /// placeholder: a primitive shaped/sized/colored by BuildingCategory via UITheme, plus a
        /// camera-facing name label, the same UITheme-instead-of-art discipline already used by
        /// HexGridRenderer/BuildingPaletteUI. Swapping in real art later is just assigning
        /// BuildingData.prefab — no code change needed, this path stops being reached automatically.
        /// </summary>
        private void SpawnVisual(BuildingInstance instance)
        {
            var worldPosition = instance.Position.ToWorldPosition(grid.HexSize);

            if (instance.Data.prefab != null)
            {
                Instantiate(instance.Data.prefab, worldPosition, Quaternion.identity, transform);
                return;
            }

            SpawnPlaceholderVisual(instance.Data, worldPosition);
        }

        private void SpawnPlaceholderVisual(BuildingData data, Vector3 worldPosition)
        {
            GetPlaceholderShape(data.category, out var primitiveType, out float height, out var color);
            float footprint = grid.HexSize * 0.75f;

            var anchor = new GameObject($"{data.displayName} (Placeholder)");
            anchor.transform.SetParent(transform, false);
            anchor.transform.position = worldPosition;

            var visual = GameObject.CreatePrimitive(primitiveType);
            visual.transform.SetParent(anchor.transform, false);
            visual.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
            visual.transform.localScale = new Vector3(
                footprint,
                primitiveType == PrimitiveType.Cylinder ? height * 0.5f : height,
                footprint);

            var collider = visual.GetComponent<Collider>();
            if (collider != null) Destroy(collider);

            var renderer = visual.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = CreateFlatMaterial(color);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            var labelGO = new GameObject("NameLabel");
            labelGO.transform.SetParent(anchor.transform, false);
            labelGO.transform.localPosition = new Vector3(0f, height + 0.35f, 0f);
            labelGO.transform.localScale = Vector3.one * 0.15f;
            var label = labelGO.AddComponent<TextMeshPro>();
            label.text = data.displayName;
            label.fontSize = 4f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = theme != null ? theme.panelText : new Color(0.2f, 0.133f, 0.078f);
            labelGO.AddComponent<BillboardLabel>();
        }

        /// <summary>Shape/height/color per BuildingCategory — squat pale cubes for Habitat, wider ochre cylinders for Production, tall dark cubes for Military, tall gold cylinders for Spiritual, a distinct blue sphere for Special — enough to tell building types apart on sight without any produced art.</summary>
        private void GetPlaceholderShape(BuildingCategory category, out PrimitiveType type, out float height, out Color color)
        {
            switch (category)
            {
                case BuildingCategory.Habitat:
                    type = PrimitiveType.Cube;
                    height = 0.9f;
                    color = theme != null ? theme.ivoryWhite : new Color(0.961f, 0.941f, 0.882f);
                    break;
                case BuildingCategory.Military:
                    type = PrimitiveType.Cube;
                    height = 2.0f;
                    color = theme != null ? theme.panelText : new Color(0.2f, 0.133f, 0.078f);
                    break;
                case BuildingCategory.Spiritual:
                    type = PrimitiveType.Cylinder;
                    height = 2.4f;
                    color = theme != null ? theme.warmGold : new Color(0.831f, 0.627f, 0.090f);
                    break;
                case BuildingCategory.Special:
                    type = PrimitiveType.Sphere;
                    height = 1.7f;
                    color = theme != null ? theme.deepBlue : new Color(0.090f, 0.196f, 0.310f);
                    break;
                case BuildingCategory.Production:
                default:
                    type = PrimitiveType.Cylinder;
                    height = 1.3f;
                    color = theme != null ? theme.ochre : new Color(0.800f, 0.467f, 0.133f);
                    break;
            }
        }

        private static Material CreateFlatMaterial(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Unlit/Color")
                ?? Shader.Find("Sprites/Default")
                ?? Shader.Find("Standard");
            var material = new Material(shader) { name = "BuildingPlaceholderFlatColor" };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            return material;
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
