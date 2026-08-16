using System.Collections.Generic;
using KingdomOfGod.Core;
using KingdomOfGod.Missions;
using UnityEngine;

namespace KingdomOfGod.Progression
{
    /// <summary>A legendary leader (David, Salomon, Élie...), unlockable, with a narrative arc and unique gameplay hooks.</summary>
    [CreateAssetMenu(fileName = "Leader_", menuName = "Kingdom of God/Leader", order = 70)]
    public class LeaderData : ScriptableObject
    {
        public string displayName;
        public Age age;
        public Sprite portrait;

        [Header("Narrative")]
        [TextArea] public string narrativeArc;
        [TextArea] public string appearance;
        [TextArea] public string personality;

        [Header("Gameplay")]
        [TextArea] public string gameplayRole;
        public string signatureAbilityName;
        [TextArea] public string signatureAbilityDescription;
        public string uniqueMechanicName;
        [TextArea] public string uniqueMechanicDescription;

        [Header("Relationships")]
        public List<string> keyRelationships = new List<string>();

        [TextArea] public string unlockCondition;

        [Tooltip("If set, LeaderManager unlocks this leader when this mission is completed. If left "
            + "empty, it unlocks as soon as `age` is unlocked instead — matches unlockCondition's "
            + "two phrasings (\"Débloqué à la mission «...»\" vs \"Débloqué au début de l'Âge N\").")]
        public MissionData unlockMission;
    }
}
