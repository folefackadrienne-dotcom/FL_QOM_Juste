using KingdomOfGod.Core;
using KingdomOfGod.Miracles;
using UnityEngine;

namespace KingdomOfGod.Battle
{
    /// <summary>Shape of a major boss encounter (GDD: "mécaniques de combat détaillées des antagonistes majeurs").</summary>
    public enum EncounterType
    {
        MultiPhaseBattle,
        LegendaryDuel,
        PoliticalCorruption,
        Siege,
        Harassment
    }

    /// <summary>
    /// A major campaign antagonist (Pharaon, Goliath, Jézabel...), narratively paired against a
    /// LeaderData hero. Mirrors LeaderData's narrative fields, plus the boss encounter mechanics
    /// specific to antagonists: how the fight unfolds, its named unique mechanic, its victory
    /// condition, and — where the design calls for one — the specific miracle that resolves it.
    /// </summary>
    [CreateAssetMenu(fileName = "Antagonist_", menuName = "Kingdom of God/Antagonist", order = 26)]
    public class AntagonistData : ScriptableObject
    {
        public string displayName;
        public Age age;
        [TextArea] public string role;
        public Sprite portrait;

        [Header("Narrative")]
        [TextArea] public string appearance;
        [TextArea] public string personality;
        [TextArea] public string heroicContrast;

        [Header("Encounter")]
        public EncounterType encounterType;
        [TextArea] public string encounterDescription;
        public string uniqueMechanicName;
        [TextArea] public string uniqueMechanicDescription;
        [TextArea] public string victoryCondition;
        public MiracleData requiredMiracle;
    }
}
