using System;
using System.Collections.Generic;
using KingdomOfGod.Core;
using KingdomOfGod.Grid;
using KingdomOfGod.Miracles;
using KingdomOfGod.UI;
using TMPro;
using UnityEngine;

namespace KingdomOfGod.Battle
{
    public enum BattleOutcome
    {
        InProgress,
        Victory,
        Defeat
    }

    /// <summary>Orchestrates a single tactical battle: unit spawning, attack resolution, miracle casting, victory checks.</summary>
    public class BattleManager : MonoBehaviour
    {
        [SerializeField] private BattleGrid battleGrid;
        [SerializeField] private VictoryCondition victoryCondition;
        [SerializeField] private MiracleManager miracleManager;
        [SerializeField] private UIThemeData theme;

        private readonly List<UnitInstance> units = new List<UnitInstance>();
        private readonly Dictionary<UnitInstance, GameObject> unitVisuals = new Dictionary<UnitInstance, GameObject>();
        private TurnController turnController;
        private bool miracleUsedThisBattle;

        public BattleOutcome Outcome { get; private set; } = BattleOutcome.InProgress;
        public event Action<BattleOutcome> BattleEnded;

        private void Awake()
        {
            // miracleManager lives on the persistent Bootstrap GameManager, in a different
            // scene from this battle — Inspector references can't cross scenes, so fall back
            // to the running singleton when this field was left unassigned.
            if (miracleManager == null && GameManager.Instance != null)
            {
                miracleManager = GameManager.Instance.Miracles;
            }

            turnController = new TurnController(units);
            turnController.TurnAdvanced += OnTurnAdvanced;
        }

        /// <summary>Overrides the edit-time default victoryCondition — MissionBattleSetup calls this with the active mission's own condition before any unit spawns.</summary>
        public void Configure(VictoryCondition condition) => victoryCondition = condition;

        public UnitInstance SpawnUnit(UnitData data, Allegiance allegiance, HexCoordinates position)
        {
            var unit = new UnitInstance(data, allegiance, position);
            unit.Died += OnUnitDied;
            units.Add(unit);
            battleGrid.PlaceUnit(unit, position);
            SpawnVisual(unit);
            GameManager.Instance?.Audio.PlaySfx(data.antagonist != null
                ? "Antagonistes - Entrée en Scène du Boss"
                : "Bataille - Cri de Guerre");
            return unit;
        }

        public bool TryAttack(UnitInstance attacker, UnitInstance defender)
        {
            if (attacker.HasActedThisTurn) return false;
            if (attacker.Position.DistanceTo(defender.Position) > attacker.Data.attackRange) return false;

            int terrainBonus = battleGrid.GetTerrainDefenseBonus(defender.Position);
            defender.TakeDamage(attacker.Data.attack - terrainBonus);
            attacker.HasActedThisTurn = true;
            GameManager.Instance?.Audio.PlaySfx("Bataille - Impact de Métal");

            // "Pendant le temps de prière, le joueur est vulnérable" (GDD) — an enemy landing a
            // hit on a praying player's unit disrupts the ritual in progress.
            if (defender.Allegiance == Allegiance.Player && miracleManager != null && miracleManager.IsPraying)
            {
                miracleManager.InterruptPrayer();
            }

            CheckBattleEnd();
            return true;
        }

        public bool TryMove(UnitInstance unit, HexCoordinates destination)
        {
            if (battleGrid.IsOccupied(destination)) return false;
            if (unit.Position.DistanceTo(destination) > unit.Data.movement) return false;

            battleGrid.PlaceUnit(unit, destination);
            MoveVisual(unit);
            return true;
        }

        /// <summary>Begins the prayer ritual for one miracle for the battle (GDD: "1 miracle par bataille, coût en Foi") — resolves after its prayer duration in turns, and can be interrupted by enemy attacks in the meantime.</summary>
        public bool TryCastMiracle(MiracleData miracle)
        {
            if (miracleUsedThisBattle) return false;
            if (!miracleManager.BeginPrayer(miracle)) return false;

            miracleUsedThisBattle = true;
            return true;
        }

        public void EndPlayerPhase() => turnController.EndPhase();

        private void OnTurnAdvanced(int turnNumber)
        {
            if (miracleManager != null && miracleManager.IsPraying)
            {
                miracleManager.AdvancePrayerTurn();
            }
        }

        private void OnUnitDied(UnitInstance unit)
        {
            battleGrid.RemoveUnit(unit);
            DespawnVisual(unit);
            GameManager.Instance?.Audio.PlaySfx(unit.Data.antagonist != null
                ? "Antagonistes - Boss Vaincu"
                : "Bataille - Mort d'une Unité");
            CheckBattleEnd();
        }

        /// <summary>
        /// Instantiates UnitData.prefab at the cell's world position — or, until real unit art
        /// exists (none of the 11 UnitData assets have a prefab assigned), a code-generated
        /// placeholder: a primitive shaped by UnitClass, colored by Allegiance (blue=Player,
        /// red=Enemy, olive=Ally), scaled up and darkened for a boss (UnitData.antagonist set),
        /// plus a camera-facing name label — the same UITheme-instead-of-art discipline already
        /// used by BuildingManager's placeholder buildings. Swapping in real art later is just
        /// assigning UnitData.prefab — no code change needed, this path stops being reached
        /// automatically.
        /// </summary>
        private void SpawnVisual(UnitInstance unit)
        {
            var worldPosition = unit.Position.ToWorldPosition(battleGrid.Grid.HexSize);

            if (unit.Data.prefab != null)
            {
                unitVisuals[unit] = Instantiate(unit.Data.prefab, worldPosition, Quaternion.identity, battleGrid.transform);
                return;
            }

            unitVisuals[unit] = SpawnPlaceholderVisual(unit, worldPosition);
        }

        private GameObject SpawnPlaceholderVisual(UnitInstance unit, Vector3 worldPosition)
        {
            GetPlaceholderShape(unit.Data.unitClass, out var primitiveType, out float height);
            bool isBoss = unit.Data.antagonist != null;
            if (isBoss) height *= 1.5f;
            float footprint = battleGrid.Grid.HexSize * 0.6f;

            var anchor = new GameObject($"{unit.Data.displayName} (Placeholder)");
            anchor.transform.SetParent(battleGrid.transform, false);
            anchor.transform.position = worldPosition;

            var visual = GameObject.CreatePrimitive(primitiveType);
            visual.transform.SetParent(anchor.transform, false);
            visual.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
            visual.transform.localScale = new Vector3(
                footprint,
                primitiveType == PrimitiveType.Cylinder || primitiveType == PrimitiveType.Capsule ? height * 0.5f : height,
                footprint);

            var collider = visual.GetComponent<Collider>();
            if (collider != null) Destroy(collider);

            var renderer = visual.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = CreateFlatMaterial(GetPlaceholderColor(unit.Allegiance, isBoss));
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            var labelGO = new GameObject("NameLabel");
            labelGO.transform.SetParent(anchor.transform, false);
            labelGO.transform.localPosition = new Vector3(0f, height + 0.35f, 0f);
            labelGO.transform.localScale = Vector3.one * 0.15f;
            var label = labelGO.AddComponent<TextMeshPro>();
            label.text = unit.Data.displayName;
            label.fontSize = 4f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = theme != null ? theme.ivoryWhite : Color.white;
            labelGO.AddComponent<BillboardLabel>();

            return anchor;
        }

        /// <summary>Shape/height per UnitClass — enough to tell unit types apart on sight without any produced art.</summary>
        private static void GetPlaceholderShape(UnitClass unitClass, out PrimitiveType type, out float height)
        {
            switch (unitClass)
            {
                case UnitClass.Archer:
                    type = PrimitiveType.Cylinder;
                    height = 1.0f;
                    break;
                case UnitClass.Chariot:
                    type = PrimitiveType.Cube;
                    height = 0.6f;
                    break;
                case UnitClass.Cavalry:
                    type = PrimitiveType.Capsule;
                    height = 1.3f;
                    break;
                case UnitClass.Priest:
                    type = PrimitiveType.Cylinder;
                    height = 1.1f;
                    break;
                case UnitClass.Prophet:
                    type = PrimitiveType.Cylinder;
                    height = 1.2f;
                    break;
                case UnitClass.Special:
                    type = PrimitiveType.Sphere;
                    height = 1.0f;
                    break;
                case UnitClass.Infantry:
                default:
                    type = PrimitiveType.Cube;
                    height = 1.0f;
                    break;
            }
        }

        private Color GetPlaceholderColor(Allegiance allegiance, bool isBoss)
        {
            if (isBoss) return theme != null ? theme.panelText : new Color(0.2f, 0.133f, 0.078f);

            switch (allegiance)
            {
                case Allegiance.Player: return theme != null ? theme.deepBlue : new Color(0.090f, 0.196f, 0.310f);
                case Allegiance.Ally: return theme != null ? theme.oliveGreen : new Color(0.420f, 0.478f, 0.227f);
                case Allegiance.Enemy:
                default: return theme != null ? theme.crisisRed : new Color(0.361f, 0.102f, 0.102f);
            }
        }

        private static Material CreateFlatMaterial(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Unlit/Color")
                ?? Shader.Find("Sprites/Default")
                ?? Shader.Find("Standard");
            var material = new Material(shader) { name = "UnitPlaceholderFlatColor" };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            return material;
        }

        private void MoveVisual(UnitInstance unit)
        {
            if (unitVisuals.TryGetValue(unit, out var visual))
            {
                visual.transform.position = unit.Position.ToWorldPosition(battleGrid.Grid.HexSize);
            }
        }

        private void DespawnVisual(UnitInstance unit)
        {
            if (unitVisuals.TryGetValue(unit, out var visual))
            {
                Destroy(visual);
                unitVisuals.Remove(unit);
            }
        }

        /// <summary>Was only ever called CheckVictory and only ever able to set Victory — a battle could never actually be lost. Now also declares Defeat once every Player unit that ever spawned is dead.</summary>
        private void CheckBattleEnd()
        {
            if (Outcome != BattleOutcome.InProgress) return;

            if (victoryCondition.IsMet(battleGrid, turnController))
            {
                Outcome = BattleOutcome.Victory;
                BattleEnded?.Invoke(Outcome);
                return;
            }

            bool hadPlayerUnits = false;
            bool anyPlayerAlive = false;
            foreach (var unit in units)
            {
                if (unit.Allegiance != Allegiance.Player) continue;
                hadPlayerUnits = true;
                if (unit.IsAlive) anyPlayerAlive = true;
            }

            if (hadPlayerUnits && !anyPlayerAlive)
            {
                Outcome = BattleOutcome.Defeat;
                BattleEnded?.Invoke(Outcome);
            }
        }
    }
}
