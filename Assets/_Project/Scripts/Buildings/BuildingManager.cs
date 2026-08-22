using System;
using System.Collections.Generic;
using KingdomOfGod.Alliance;
using KingdomOfGod.Core;
using KingdomOfGod.Core.Vfx;
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
        [SerializeField] private List<BuildingData> allBuildingTypes = new List<BuildingData>();

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
            OneShotParticles.Burst(instance.Position.ToWorldPosition(grid.HexSize) + Vector3.up * 0.3f,
                theme != null ? theme.ochre : new Color(0.800f, 0.467f, 0.133f), count: 16, size: 0.16f, speed: 1.2f);

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
        /// Instantiates BuildingData.prefab at the cell's world position if one is assigned; failing
        /// that, a camera-facing sprite billboard from BuildingData.icon (25 of 39 buildings now have
        /// one — the AI-rendered isometric scene art already used for the palette button); failing
        /// that too, a code-generated flat-color primitive placeholder. Swapping in real 3D art later
        /// is just assigning BuildingData.prefab — no code change needed, these paths stop being
        /// reached automatically.
        /// </summary>
        private void SpawnVisual(BuildingInstance instance)
        {
            var worldPosition = instance.Position.ToWorldPosition(grid.HexSize);
            var data = instance.Data;

            if (data.prefab != null)
            {
                Instantiate(data.prefab, worldPosition, Quaternion.identity, transform);
                return;
            }

            if (data.icon != null)
            {
                SpawnIconBillboard(data, worldPosition);
                return;
            }

            SpawnPlaceholderVisual(data, worldPosition);
        }

        /// <summary>
        /// A camera-facing SpriteRenderer using BuildingData.icon in place of a flat-color primitive.
        /// Height matches GetPlaceholderShape's per-category tuning so buildings keep their
        /// established relative scale on the hex grid; width follows Sprite.bounds' own aspect ratio
        /// rather than the fixed footprint, since these renders are wide establishing shots of a whole
        /// building compound, not square icons.
        /// </summary>
        private void SpawnIconBillboard(BuildingData data, Vector3 worldPosition)
        {
            GetPlaceholderShape(data.category, out _, out float height, out _);

            var anchor = new GameObject(data.displayName);
            anchor.transform.SetParent(transform, false);
            anchor.transform.position = worldPosition;

            var spriteGO = new GameObject("Sprite");
            spriteGO.transform.SetParent(anchor.transform, false);
            spriteGO.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);

            var spriteRenderer = spriteGO.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = data.icon;
            spriteRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            spriteRenderer.receiveShadows = false;

            float nativeHeight = data.icon.bounds.size.y;
            float scale = nativeHeight > 0f ? height / nativeHeight : 1f;
            spriteGO.transform.localScale = Vector3.one * scale;
            spriteGO.AddComponent<BillboardLabel>();
            spriteGO.AddComponent<PopInScale>();
            GroundShadow.Attach(anchor.transform, grid.HexSize * 0.4f);

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
            visual.AddComponent<PopInScale>();
            GroundShadow.Attach(anchor.transform, grid.HexSize * 0.4f);

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

        /// <summary>
        /// Re-places buildings loaded from a save file, matched by BuildingData.displayName against
        /// allBuildingTypes: applies the same cell occupation, visual spawn, and storage/population
        /// capacity bonuses as TryPlace, but skips CanPlace's checks and resourceManager.TrySpend —
        /// the save's resource stock already reflects every building's cost having been paid once,
        /// at its original placement. Call before ResourceManager.RestoreStock so the storage-bonus
        /// caps this raises are already in effect when stock is restored.
        /// </summary>
        public void RestoreFromSave(IEnumerable<(string buildingName, int q, int r, int level)> saved)
        {
            foreach (var entry in saved)
            {
                var data = allBuildingTypes.Find(b => b.displayName == entry.buildingName);
                if (data == null)
                {
                    Debug.LogWarning($"Kingdom of God save: building '{entry.buildingName}' no longer exists, skipping.");
                    continue;
                }

                var position = new HexCoordinates(entry.q, entry.r);
                if (!grid.TryGetCell(position, out var cell)) continue;

                var instance = new BuildingInstance(data, position);
                instance.SetLevel(entry.level);
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
            }
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
