using System;
using KingdomOfGod.Grid;

namespace KingdomOfGod.Battle
{
    public enum Allegiance
    {
        Player,
        Enemy,
        Ally
    }

    /// <summary>A unit on the battle grid: its data plus runtime combat state.</summary>
    public class UnitInstance
    {
        public UnitData Data { get; }
        public Allegiance Allegiance { get; }
        public HexCoordinates Position { get; set; }
        public int CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0;
        public bool HasActedThisTurn { get; set; }

        public event Action<UnitInstance> Died;

        public UnitInstance(UnitData data, Allegiance allegiance, HexCoordinates position)
        {
            Data = data;
            Allegiance = allegiance;
            Position = position;
            CurrentHealth = data.maxHealth;
        }

        /// <summary>Resolves incoming damage, factoring in this unit's defense stat.</summary>
        public void TakeDamage(int rawAttack)
        {
            int damage = Math.Max(1, rawAttack - Data.defense);
            CurrentHealth = Math.Max(0, CurrentHealth - damage);
            if (CurrentHealth == 0) Died?.Invoke(this);
        }

        public void Heal(int amount) => CurrentHealth = Math.Min(Data.maxHealth, CurrentHealth + amount);
    }
}
