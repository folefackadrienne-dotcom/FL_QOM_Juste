using System;
using System.Collections.Generic;
using KingdomOfGod.Core;
using KingdomOfGod.Grid;
using KingdomOfGod.Miracles;
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

            CheckVictory();
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
            CheckVictory();
        }

        /// <summary>Instantiates UnitData.prefab at the cell's world position — a deliberate no-op until a prefab is assigned (no unit art exists yet).</summary>
        private void SpawnVisual(UnitInstance unit)
        {
            if (unit.Data.prefab == null) return;

            var worldPosition = unit.Position.ToWorldPosition(battleGrid.Grid.HexSize);
            unitVisuals[unit] = Instantiate(unit.Data.prefab, worldPosition, Quaternion.identity, battleGrid.transform);
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

        private void CheckVictory()
        {
            if (Outcome != BattleOutcome.InProgress) return;

            if (victoryCondition.IsMet(battleGrid, turnController))
            {
                Outcome = BattleOutcome.Victory;
                BattleEnded?.Invoke(Outcome);
            }
        }
    }
}
