using System;
using System.Collections.Generic;
using System.Linq;

namespace KingdomOfGod.Battle
{
    public enum TurnPhase
    {
        PlayerTurn,
        EnemyTurn,
        AllyTurn
    }

    /// <summary>Drives turn order for a tactical battle: whose phase it is, and resetting per-unit action flags.</summary>
    public class TurnController
    {
        private readonly List<UnitInstance> allUnits;
        public TurnPhase CurrentPhase { get; private set; } = TurnPhase.PlayerTurn;
        public int TurnNumber { get; private set; } = 1;

        public event Action<TurnPhase> PhaseChanged;
        public event Action<int> TurnAdvanced;

        public TurnController(List<UnitInstance> allUnits)
        {
            this.allUnits = allUnits;
        }

        public void EndPhase()
        {
            ResetActedFlags(CurrentPhase);

            CurrentPhase = CurrentPhase switch
            {
                TurnPhase.PlayerTurn => TurnPhase.EnemyTurn,
                TurnPhase.EnemyTurn => TurnPhase.AllyTurn,
                _ => TurnPhase.PlayerTurn
            };

            if (CurrentPhase == TurnPhase.PlayerTurn)
            {
                TurnNumber++;
                TurnAdvanced?.Invoke(TurnNumber);
            }

            PhaseChanged?.Invoke(CurrentPhase);
        }

        private Allegiance PhaseAllegiance(TurnPhase phase) => phase switch
        {
            TurnPhase.PlayerTurn => Allegiance.Player,
            TurnPhase.EnemyTurn => Allegiance.Enemy,
            _ => Allegiance.Ally
        };

        private void ResetActedFlags(TurnPhase phase)
        {
            foreach (var unit in allUnits.Where(u => u.IsAlive && u.Allegiance == PhaseAllegiance(phase)))
            {
                unit.HasActedThisTurn = false;
            }
        }

        public bool IsPhaseComplete(TurnPhase phase)
        {
            return allUnits.Where(u => u.IsAlive && u.Allegiance == PhaseAllegiance(phase)).All(u => u.HasActedThisTurn);
        }
    }
}
