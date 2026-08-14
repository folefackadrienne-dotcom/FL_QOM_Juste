using System;
using System.Collections.Generic;
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
        private TurnController turnController;
        private bool miracleUsedThisBattle;

        public BattleOutcome Outcome { get; private set; } = BattleOutcome.InProgress;
        public event Action<BattleOutcome> BattleEnded;

        private void Awake()
        {
            turnController = new TurnController(units);
        }

        public UnitInstance SpawnUnit(UnitData data, Allegiance allegiance, HexCoordinates position)
        {
            var unit = new UnitInstance(data, allegiance, position);
            unit.Died += OnUnitDied;
            units.Add(unit);
            battleGrid.PlaceUnit(unit, position);
            return unit;
        }

        public bool TryAttack(UnitInstance attacker, UnitInstance defender)
        {
            if (attacker.HasActedThisTurn) return false;
            if (attacker.Position.DistanceTo(defender.Position) > attacker.Data.attackRange) return false;

            int terrainBonus = battleGrid.GetTerrainDefenseBonus(defender.Position);
            defender.TakeDamage(attacker.Data.attack - terrainBonus);
            attacker.HasActedThisTurn = true;

            CheckVictory();
            return true;
        }

        public bool TryMove(UnitInstance unit, HexCoordinates destination)
        {
            if (battleGrid.IsOccupied(destination)) return false;
            if (unit.Position.DistanceTo(destination) > unit.Data.movement) return false;

            battleGrid.PlaceUnit(unit, destination);
            return true;
        }

        /// <summary>Casts one miracle for the battle (GDD: "1 miracle par bataille, coût en Foi").</summary>
        public bool TryCastMiracle(MiracleData miracle)
        {
            if (miracleUsedThisBattle) return false;
            if (!miracleManager.TryCast(miracle)) return false;

            miracleUsedThisBattle = true;
            return true;
        }

        public void EndPlayerPhase() => turnController.EndPhase();

        private void OnUnitDied(UnitInstance unit)
        {
            battleGrid.RemoveUnit(unit);
            CheckVictory();
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
