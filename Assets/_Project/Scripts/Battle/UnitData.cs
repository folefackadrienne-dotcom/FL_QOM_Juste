using UnityEngine;

namespace KingdomOfGod.Battle
{
    public enum UnitClass
    {
        Infantry,
        Archer,
        Chariot,
        Cavalry,
        Priest,
        Prophet,
        Special // Guerriers de David, Hommes de Gédéon, Chérubins, etc.
    }

    /// <summary>Static definition of a unit type: base stats, one asset per unit.</summary>
    [CreateAssetMenu(fileName = "Unit_", menuName = "Kingdom of God/Unit", order = 20)]
    public class UnitData : ScriptableObject
    {
        public string displayName;
        public UnitClass unitClass;
        public GameObject prefab;
        public Sprite icon;

        [Header("Base Stats")]
        public int maxHealth = 10;
        public int attack = 3;
        public int defense = 2;
        public int movement = 3;
        public int attackRange = 1;

        [Header("Support")]
        public bool canHeal;
        public int healAmount;

        public bool isUnlockable;
        [TextArea] public string unlockCondition;

        [Tooltip("Set only for a boss's stat block — links back to the narrative AntagonistData so BattleManager can play a boss-specific cue instead of the regular unit spawn/death sound.")]
        public AntagonistData antagonist;
    }
}
