using System;
using KingdomOfGod.Grid;
using UnityEngine;

namespace KingdomOfGod.Battle
{
    public enum VictoryConditionType
    {
        AnnihilateEnemy,
        SurviveTurns,
        ProtectUnit,
        CapturePoint
    }

    /// <summary>Data-only description of how a mission's battle is won (see GDD "Conditions de victoire variables").</summary>
    [Serializable]
    public class VictoryCondition
    {
        public VictoryConditionType type;

        [Tooltip("Used by SurviveTurns.")]
        public int turnsToSurvive;

        [Tooltip("Used by ProtectUnit: the unit that must not die.")]
        public string protectedUnitId;

        [Tooltip("Used by CapturePoint.")]
        public HexCoordinates capturePoint;

        public bool IsMet(BattleGrid battleGrid, TurnController turnController)
        {
            return type switch
            {
                VictoryConditionType.AnnihilateEnemy => !HasLivingUnitsOfAllegiance(battleGrid, Allegiance.Enemy),
                VictoryConditionType.SurviveTurns => turnController.TurnNumber > turnsToSurvive,
                VictoryConditionType.CapturePoint => battleGrid.GetUnitAt(capturePoint)?.Allegiance == Allegiance.Player,
                VictoryConditionType.ProtectUnit => true, // failure is checked separately via UnitInstance.Died
                _ => false
            };
        }

        private static bool HasLivingUnitsOfAllegiance(BattleGrid battleGrid, Allegiance allegiance)
        {
            foreach (var cell in battleGrid.Grid.Cells)
            {
                var unit = battleGrid.GetUnitAt(cell.Key);
                if (unit != null && unit.IsAlive && unit.Allegiance == allegiance) return true;
            }
            return false;
        }
    }
}
